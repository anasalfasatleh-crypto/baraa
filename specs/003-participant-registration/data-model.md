# Data Model: Participant Registration System

**Feature**: 003-participant-registration
**Date**: 2026-01-17

## Entity Relationship Diagram

```
┌─────────────────────────────┐
│        Participant          │
├─────────────────────────────┤
│ Id: Guid (PK)               │
│ Code: string (unique)       │
│ LoginIdentifier: string (u) │
│ PhoneNumber: string?        │
│ PasswordHash: string        │
│ MustChangePassword: bool    │
│ FailedLoginAttempts: int    │
│ LockoutEnd: DateTime?       │
│ CreatedAt: DateTime         │
│ UpdatedAt: DateTime         │
│ LastLoginAt: DateTime?      │
└──────────────┬──────────────┘
               │
               │ 1:N
               ▼
┌─────────────────────────────┐
│  ParticipantRefreshToken    │
├─────────────────────────────┤
│ Id: Guid (PK)               │
│ ParticipantId: Guid (FK)    │
│ Token: string               │
│ ExpiresAt: DateTime         │
│ CreatedAt: DateTime         │
│ RevokedAt: DateTime?        │
└─────────────────────────────┘

┌─────────────────────────────┐
│  ParticipantCodeSequence    │
├─────────────────────────────┤
│ Id: int (PK)                │
│ CurrentPrefix: string       │  // "A", "B", ..., "Z", "AA", "AB", ...
│ CurrentNumber: int          │  // 1-99
│ UpdatedAt: DateTime         │
└─────────────────────────────┘
```

## Entities

### Participant

The main entity representing a registered participant in the system.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | `Guid` | PK, auto-generated | Unique identifier |
| `Code` | `string` | Unique, max 10 chars | Auto-generated code (A1-Z99, AA1-ZZ99, etc.) |
| `LoginIdentifier` | `string` | Unique, max 255 chars | Username or email used for login |
| `PhoneNumber` | `string?` | Optional, max 20 chars | Optional contact number |
| `PasswordHash` | `string` | Required, max 255 chars | BCrypt hashed password |
| `MustChangePassword` | `bool` | Default: false | Force password change on next login |
| `FailedLoginAttempts` | `int` | Default: 0 | Counter for lockout mechanism |
| `LockoutEnd` | `DateTime?` | Nullable | UTC timestamp when lockout expires |
| `CreatedAt` | `DateTime` | Auto-set | UTC registration timestamp |
| `UpdatedAt` | `DateTime` | Auto-updated | UTC last modification timestamp |
| `LastLoginAt` | `DateTime?` | Nullable | UTC last successful login |

**Indexes:**
- `IX_participants_code` (unique)
- `IX_participants_login_identifier` (unique, case-insensitive)
- `IX_participants_created_at`

**Validation Rules:**
- `LoginIdentifier`:
  - If contains `@`: Must be valid email format
  - If no `@`: Must be 3-50 alphanumeric characters, underscores allowed
- `PasswordHash`: Minimum 8 characters before hashing
- `PhoneNumber`: Optional, if provided must be valid phone format

---

### ParticipantRefreshToken

Stores refresh tokens for participant JWT authentication.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | `Guid` | PK, auto-generated | Unique identifier |
| `ParticipantId` | `Guid` | FK → Participant.Id | Owner of the token |
| `Token` | `string` | Required, max 500 chars | The refresh token value |
| `ExpiresAt` | `DateTime` | Required | UTC expiration timestamp |
| `CreatedAt` | `DateTime` | Auto-set | UTC creation timestamp |
| `RevokedAt` | `DateTime?` | Nullable | UTC revocation timestamp (if revoked) |

**Indexes:**
- `IX_participant_refresh_tokens_token`
- `IX_participant_refresh_tokens_participant_id`
- `IX_participant_refresh_tokens_expires_at`

**Relationships:**
- `Participant` → `ParticipantRefreshToken` (1:N, cascade delete)

---

### ParticipantCodeSequence

Singleton entity tracking the current position in code generation sequence.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | `int` | PK, value = 1 | Always single row |
| `CurrentPrefix` | `string` | Required, max 5 chars | Current letter prefix (A-Z, AA-ZZ, etc.) |
| `CurrentNumber` | `int` | Required, 1-99 | Current number in sequence |
| `UpdatedAt` | `DateTime` | Auto-updated | Last code generation timestamp |

**Initial Seed Data:**
```sql
INSERT INTO participant_code_sequences (id, current_prefix, current_number, updated_at)
VALUES (1, 'A', 0, NOW());
```

**Code Generation Algorithm:**
1. Lock row with `SELECT FOR UPDATE`
2. Increment `CurrentNumber`
3. If `CurrentNumber > 99`:
   - Set `CurrentNumber = 1`
   - Advance `CurrentPrefix` (A→B, Z→AA, AZ→BA, ZZ→AAA)
4. Return `CurrentPrefix + CurrentNumber` (e.g., "A1", "B42", "AA15")
5. Commit transaction

---

## Database Schema (PostgreSQL)

```sql
-- Participants table
CREATE TABLE participants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(10) NOT NULL,
    login_identifier VARCHAR(255) NOT NULL,
    phone_number VARCHAR(20),
    password_hash VARCHAR(255) NOT NULL,
    must_change_password BOOLEAN NOT NULL DEFAULT FALSE,
    failed_login_attempts INTEGER NOT NULL DEFAULT 0,
    lockout_end TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    last_login_at TIMESTAMP WITH TIME ZONE,

    CONSTRAINT uq_participants_code UNIQUE (code),
    CONSTRAINT uq_participants_login_identifier UNIQUE (LOWER(login_identifier))
);

CREATE INDEX ix_participants_created_at ON participants (created_at);

-- Participant refresh tokens table
CREATE TABLE participant_refresh_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    participant_id UUID NOT NULL REFERENCES participants(id) ON DELETE CASCADE,
    token VARCHAR(500) NOT NULL,
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    revoked_at TIMESTAMP WITH TIME ZONE
);

CREATE INDEX ix_participant_refresh_tokens_token ON participant_refresh_tokens (token);
CREATE INDEX ix_participant_refresh_tokens_participant_id ON participant_refresh_tokens (participant_id);
CREATE INDEX ix_participant_refresh_tokens_expires_at ON participant_refresh_tokens (expires_at);

-- Code sequence table (singleton)
CREATE TABLE participant_code_sequences (
    id INTEGER PRIMARY KEY CHECK (id = 1),
    current_prefix VARCHAR(5) NOT NULL DEFAULT 'A',
    current_number INTEGER NOT NULL DEFAULT 0,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Initialize sequence
INSERT INTO participant_code_sequences (id, current_prefix, current_number, updated_at)
VALUES (1, 'A', 0, NOW());
```

---

## State Transitions

### Participant Account States

```
[New Visitor]
     │
     ▼ (Register)
[Active] ←──────────────────────┐
     │                          │
     │ (5 failed logins)        │ (lockout expires OR admin unlocks)
     ▼                          │
[Locked] ───────────────────────┘
     │
     │ (admin resets password)
     ▼
[MustChangePassword] ──────────→ [Active]
                      (password changed)
```

### Lockout State Logic

| Current State | Event | New State |
|---------------|-------|-----------|
| Active | Failed login (attempts < 5) | Active (increment counter) |
| Active | Failed login (attempts = 5) | Locked (set lockout_end) |
| Locked | Time passes (lockout_end reached) | Active (counter reset) |
| Locked | Admin reset password | MustChangePassword |
| MustChangePassword | Password changed | Active |
| Any | Successful login | Active (reset counter) |

---

## Audit Integration

Password reset events are logged using the existing `AuditLog` entity:

```csharp
// When admin resets participant password
await _context.AuditLogs.AddAsync(new AuditLog
{
    UserId = adminId,  // The admin who performed the action
    Action = "ParticipantPasswordReset",
    Details = JsonSerializer.Serialize(new { ParticipantId = participantId, ParticipantCode = participant.Code }),
    CreatedAt = DateTime.UtcNow
});
```
