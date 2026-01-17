# Research: Participant Registration System

**Feature**: 003-participant-registration
**Date**: 2026-01-17

## Research Summary

This document captures technical decisions and research findings for implementing the participant registration system.

---

## 1. Participant Entity vs User Entity

### Decision
Create a **separate Participant entity** rather than extending the existing User entity with a new Role.

### Rationale
- The existing `User` entity is tightly coupled to the research platform workflow (evaluator assignments, questionnaires, scores)
- Participants have different attributes (auto-generated code, single login identifier field, different lockout mechanism)
- Keeps participant authentication isolated from existing admin/evaluator/student flows
- Allows independent evolution of participant features without affecting core platform

### Alternatives Considered
| Alternative | Rejected Because |
|-------------|------------------|
| Add `Participant` role to existing `Role` enum | Would require nullable fields for code, different validation rules, complex migrations |
| Create inheritance hierarchy (BaseUser) | Over-engineering for current requirements, adds complexity |

---

## 2. Sequential Code Generation Pattern

### Decision
Use a **database-tracked sequence** with atomic increment for code generation (A1-A99, B1-B99, ..., Z99, AA1-AA99, ...).

### Rationale
- Ensures uniqueness even under concurrent registration
- Simple algorithm: letter prefix cycles through A-Z, then AA-ZZ, etc.
- Number suffix ranges 1-99, resets with each letter change
- Database sequence prevents race conditions

### Implementation Pattern
```csharp
// ParticipantCodeSequence entity tracks current position
// Use database transaction with row-level locking for atomic increment
public async Task<string> GenerateNextCode()
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    var sequence = await _context.ParticipantCodeSequences
        .FromSqlRaw("SELECT * FROM participant_code_sequences FOR UPDATE")
        .FirstOrDefaultAsync();

    // Generate code, increment, save, commit
    // Pattern: A1-A99, B1-B99, ..., Z99, AA1-AA99, AB1-AB99, ...
}
```

### Alternatives Considered
| Alternative | Rejected Because |
|-------------|------------------|
| GUID-based codes | Not human-readable, violates spec requirement |
| Application-level sequence | Race condition risk under concurrent load |
| Database SEQUENCE | PostgreSQL sequences are numeric only, can't do letter patterns |

---

## 3. Login Identifier Field Design

### Decision
Store login identifier in a **single field** that accepts both username and email formats.

### Rationale
- Simplifies registration form (one field instead of two)
- Login logic checks if input contains '@' to determine lookup strategy
- Unique constraint ensures no duplicates regardless of format
- Matches user's mental model (they pick one identifier they'll remember)

### Validation Rules
- If contains `@`: validate as email format
- If no `@`: validate as username (alphanumeric, 3-50 chars, no spaces)
- Case-insensitive uniqueness check

### Alternatives Considered
| Alternative | Rejected Because |
|-------------|------------------|
| Separate username + email fields | User requested single field for simplicity |
| Email-only | Limits flexibility, some users prefer usernames |

---

## 4. Account Lockout Implementation

### Decision
Implement **time-based lockout** with 5 attempts and 1-minute cooldown, tracked in the Participant entity.

### Rationale
- Simple to implement with two fields: `FailedLoginAttempts`, `LockoutEnd`
- 1-minute lockout is short enough to not frustrate legitimate users
- 5 attempts is industry standard for preventing brute-force
- No need for separate lockout table

### Implementation Pattern
```csharp
// On failed login:
participant.FailedLoginAttempts++;
if (participant.FailedLoginAttempts >= 5)
{
    participant.LockoutEnd = DateTime.UtcNow.AddMinutes(1);
}

// On successful login:
participant.FailedLoginAttempts = 0;
participant.LockoutEnd = null;

// Before login attempt:
if (participant.LockoutEnd > DateTime.UtcNow)
{
    return "Account locked. Try again in X seconds.";
}
```

---

## 5. Session Management

### Decision
Use **JWT tokens with 24-hour expiration** for participant sessions, following existing platform pattern.

### Rationale
- Consistent with existing `TokenService` implementation
- 24-hour duration balances security with user convenience
- Browser close handled by not persisting token (session storage vs local storage)
- Existing refresh token mechanism can be reused

### Implementation
- Reuse existing `TokenService.GenerateAccessToken()` with participant claims
- Create new `ParticipantRefreshToken` entity (or extend existing)
- Frontend uses session storage (clears on browser close)

---

## 6. Admin Password Reset Flow

### Decision
Generate **temporary password** displayed to admin, with forced change on next login.

### Rationale
- Simple implementation without email infrastructure
- Admin can communicate password through existing channels (in-person, phone)
- `MustChangePassword` flag ensures security
- Audit trail through existing `AuditLog` entity

### Implementation Pattern
```csharp
public async Task<string> ResetParticipantPassword(Guid participantId, Guid adminId)
{
    var tempPassword = GenerateSecurePassword(12);
    participant.PasswordHash = _hasher.HashPassword(tempPassword);
    participant.MustChangePassword = true;

    await _auditService.Log(adminId, "ParticipantPasswordReset", participantId);

    return tempPassword; // Displayed to admin once
}
```

---

## 7. Frontend Route Structure

### Decision
Use `/participant/` route namespace with dedicated login and registration pages.

### Rationale
- Clear separation from existing routes (`/student/`, `/admin/`, `/evaluator/`)
- Follows existing routing conventions in the codebase
- Easy to apply route guards for participant-only access

### Route Structure
```
/participant/register  → Registration form
/participant/login     → Login form
/participant/          → Dashboard (shows code, profile)
/participant/change-password → Force change after reset
```

---

## 8. Database Migration Strategy

### Decision
Create **additive migration** with new tables, no changes to existing User table.

### Rationale
- Zero risk to existing data
- Can be rolled back independently
- New tables: `participants`, `participant_code_sequences`
- Follows existing migration naming convention

### Tables to Create
1. `participants` - Main participant data
2. `participant_code_sequences` - Tracks current code position (single row)
3. `participant_refresh_tokens` - JWT refresh tokens for participants

---

## Open Questions Resolved

All technical unknowns have been resolved through this research. No NEEDS CLARIFICATION items remain.
