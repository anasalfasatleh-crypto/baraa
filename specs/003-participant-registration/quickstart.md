# Quickstart: Participant Registration System

**Feature**: 003-participant-registration
**Date**: 2026-01-17

## Overview

This guide covers the implementation of the Participant Registration System, which provides a dedicated registration and authentication flow for participants, separate from the existing User (Admin/Evaluator/Student) system.

## Prerequisites

- .NET 9 SDK
- Node.js 18+
- PostgreSQL database (already configured)
- Existing ResearchPlatform.Api project running

## Quick Setup

### 1. Database Migration

Run the migration to create participant tables:

```bash
cd backend/src/ResearchPlatform.Api
dotnet ef migrations add AddParticipantEntities
dotnet ef database update
```

### 2. Verify Setup

After migration, verify the tables exist:

```sql
-- Check tables were created
SELECT table_name FROM information_schema.tables
WHERE table_name IN ('participants', 'participant_refresh_tokens', 'participant_code_sequences');

-- Verify sequence initialization
SELECT * FROM participant_code_sequences;
-- Should show: id=1, current_prefix='A', current_number=0
```

## Key Implementation Files

### Backend

| File | Purpose |
|------|---------|
| `Models/Participant.cs` | Participant entity |
| `Models/ParticipantCodeSequence.cs` | Code sequence tracker |
| `Models/ParticipantRefreshToken.cs` | JWT refresh tokens |
| `DTOs/ParticipantDtos.cs` | Request/response DTOs |
| `Services/ParticipantService.cs` | Registration, code generation |
| `Services/ParticipantAuthService.cs` | Login, lockout, session |
| `Controllers/ParticipantController.cs` | Public API endpoints |
| `Validators/ParticipantValidators.cs` | FluentValidation rules |

### Frontend

| File | Purpose |
|------|---------|
| `routes/participant/+page.svelte` | Dashboard |
| `routes/participant/login/+page.svelte` | Login form |
| `routes/participant/register/+page.svelte` | Registration form |
| `routes/participant/change-password/+page.svelte` | Password change |
| `lib/stores/participantAuth.ts` | Auth state management |
| `lib/services/participantApi.ts` | API client |

## API Endpoints

### Public Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/v1/participants/register` | Register new participant |
| POST | `/api/v1/participants/login` | Authenticate participant |
| POST | `/api/v1/participants/refresh` | Refresh access token |

### Authenticated Participant Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/v1/participants/me` | Get profile |
| POST | `/api/v1/participants/me/change-password` | Change password |
| POST | `/api/v1/participants/logout` | Logout |

### Admin Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/v1/admin/participants` | List participants |
| GET | `/api/v1/admin/participants/{id}` | Get participant details |
| POST | `/api/v1/admin/participants/{id}/reset-password` | Reset password |
| GET | `/api/v1/admin/participants/search?q=` | Search participants |

## Code Generation Algorithm

The participant code follows the pattern: `A1` → `A99` → `B1` → ... → `Z99` → `AA1` → ...

```csharp
// Pseudocode for code generation
string GenerateNextCode()
{
    // 1. Lock the sequence row
    var seq = GetSequenceForUpdate();

    // 2. Increment
    seq.CurrentNumber++;

    // 3. Check overflow (99 → next letter)
    if (seq.CurrentNumber > 99)
    {
        seq.CurrentNumber = 1;
        seq.CurrentPrefix = IncrementPrefix(seq.CurrentPrefix);
    }

    // 4. Return code
    return $"{seq.CurrentPrefix}{seq.CurrentNumber}";
}

string IncrementPrefix(string prefix)
{
    // A → B, Z → AA, AZ → BA, ZZ → AAA
    // Implementation handles multi-character prefixes
}
```

## Testing Scenarios

### 1. Registration Flow

```bash
# Register a new participant
curl -X POST http://localhost:5000/api/v1/participants/register \
  -H "Content-Type: application/json" \
  -d '{
    "loginIdentifier": "testuser@example.com",
    "password": "SecurePass123",
    "phoneNumber": "+1234567890"
  }'

# Expected response:
# {
#   "id": "uuid",
#   "code": "A1",
#   "loginIdentifier": "testuser@example.com",
#   "accessToken": "jwt...",
#   "refreshToken": "token...",
#   "expiresIn": 86400
# }
```

### 2. Login Flow

```bash
# Login with username/email
curl -X POST http://localhost:5000/api/v1/participants/login \
  -H "Content-Type: application/json" \
  -d '{
    "loginIdentifier": "testuser@example.com",
    "password": "SecurePass123"
  }'
```

### 3. Account Lockout Testing

```bash
# Try 5 failed logins
for i in {1..5}; do
  curl -X POST http://localhost:5000/api/v1/participants/login \
    -H "Content-Type: application/json" \
    -d '{"loginIdentifier": "testuser@example.com", "password": "wrong"}'
done

# 6th attempt should return 423 Locked
# Wait 60 seconds, then retry
```

### 4. Admin Password Reset

```bash
# As admin, reset participant password
curl -X POST http://localhost:5000/api/v1/admin/participants/{participantId}/reset-password \
  -H "Authorization: Bearer {admin_jwt}"

# Response includes temporary password
# Participant must change password on next login
```

## Frontend Routes

| Route | Purpose | Auth Required |
|-------|---------|---------------|
| `/participant/register` | Registration form | No |
| `/participant/login` | Login form | No |
| `/participant/` | Dashboard (shows code) | Yes |
| `/participant/change-password` | Password change | Yes |

## Configuration

### JWT Settings (appsettings.json)

```json
{
  "JwtSettings": {
    "Secret": "your-secret-key-min-32-chars",
    "Issuer": "ResearchPlatform",
    "Audience": "ResearchPlatform",
    "AccessTokenExpirationMinutes": 1440,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Lockout Settings

- Max failed attempts: 5
- Lockout duration: 1 minute
- Configured in `ParticipantAuthService`

## Common Issues

### Issue: Duplicate code generated

**Cause**: Race condition in code generation
**Solution**: Ensure `SELECT FOR UPDATE` is used in PostgreSQL transaction

### Issue: Login identifier case sensitivity

**Cause**: PostgreSQL case-sensitive comparison
**Solution**: Use `LOWER()` in unique constraint and queries

### Issue: Session not cleared on browser close

**Cause**: Token stored in localStorage
**Solution**: Use sessionStorage in frontend auth store

## Next Steps

After implementation:

1. Run `/speckit.tasks` to generate task breakdown
2. Create database migration
3. Implement backend services
4. Build frontend components
5. Write integration tests
6. Update admin UI for participant management
