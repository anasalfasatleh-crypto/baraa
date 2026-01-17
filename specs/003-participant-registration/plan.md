# Implementation Plan: Participant Registration System

**Branch**: `003-participant-registration` | **Date**: 2026-01-17 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/003-participant-registration/spec.md`

## Summary

Implement a dedicated participant registration and authentication system separate from the existing user (Student/Admin/Evaluator) flow. Participants will have auto-generated sequential codes (A1-Z99, then AA1+), a single login identifier field (username or email), optional phone number, and their own login/registration pages. Admins will be able to search participants and reset their passwords.

## Technical Context

**Language/Version**: TypeScript 5.9.3 (Frontend - SvelteKit), C# .NET 9.0 (Backend)
**Primary Dependencies**: SvelteKit 2.x, ASP.NET Core 9, Entity Framework Core, JWT Authentication
**Storage**: PostgreSQL (existing ApplicationDbContext)
**Testing**: Vitest (Frontend), xUnit (Backend)
**Target Platform**: Web application (browser-based)
**Project Type**: Web (frontend + backend separation)
**Performance Goals**: Registration < 2 minutes, Login < 1 second response
**Constraints**: Session duration 24 hours, 5 failed login attempts = 1 minute lockout
**Scale/Scope**: Support 2,574+ unique participant codes (A1-Z99 minimum)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The constitution template has not been customized for this project. Applying general best practices:

| Principle | Status | Notes |
|-----------|--------|-------|
| Separation of Concerns | PASS | Participant entity separate from User entity |
| Security | PASS | Password hashing, account lockout, session management |
| Testability | PASS | Clear acceptance criteria defined |
| Simplicity | PASS | Minimal registration fields, sequential code generation |

## Project Structure

### Documentation (this feature)

```text
specs/003-participant-registration/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
backend/
├── src/ResearchPlatform.Api/
│   ├── Models/
│   │   ├── Participant.cs              # New entity
│   │   └── ParticipantCodeSequence.cs  # New entity
│   ├── DTOs/
│   │   └── ParticipantDtos.cs          # New DTOs
│   ├── Services/
│   │   ├── ParticipantService.cs       # Registration, code generation
│   │   └── ParticipantAuthService.cs   # Login, session, lockout
│   ├── Controllers/
│   │   └── ParticipantController.cs    # Public registration/login endpoints
│   ├── Validators/
│   │   └── ParticipantValidators.cs    # FluentValidation rules
│   └── Migrations/
│       └── [timestamp]_AddParticipantEntities.cs
└── tests/

frontend/
├── src/
│   ├── routes/
│   │   └── participant/
│   │       ├── +page.svelte            # Participant dashboard
│   │       ├── login/+page.svelte      # Dedicated login page
│   │       └── register/+page.svelte   # Dedicated registration page
│   ├── lib/
│   │   ├── stores/
│   │   │   └── participantAuth.ts      # Participant auth state
│   │   └── services/
│   │       └── participantApi.ts       # API client for participant endpoints
└── tests/
```

**Structure Decision**: Web application structure with separate backend API and SvelteKit frontend. Participant routes under `/participant/` namespace to keep them distinct from existing `/student/`, `/admin/`, `/evaluator/` routes.

## Complexity Tracking

No constitution violations requiring justification. Implementation follows existing patterns in the codebase.
