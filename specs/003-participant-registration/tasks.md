# Tasks: Participant Registration System

**Input**: Design documents from `/specs/003-participant-registration/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/participant-api.yaml

**Tests**: Not explicitly requested - test tasks omitted. Add them by running `/speckit.tasks` with test flag if needed.

**Organization**: Tasks grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4)
- Exact file paths included in descriptions

## Path Conventions

- **Backend**: `backend/src/ResearchPlatform.Api/`
- **Frontend**: `frontend/src/`
- Following existing project structure from plan.md

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and database structure for participants

- [x] T001 Create Participant model entity in backend/src/ResearchPlatform.Api/Models/Participant.cs
- [x] T002 [P] Create ParticipantRefreshToken model in backend/src/ResearchPlatform.Api/Models/ParticipantRefreshToken.cs
- [x] T003 [P] Create ParticipantCodeSequence model in backend/src/ResearchPlatform.Api/Models/ParticipantCodeSequence.cs
- [x] T004 Add Participant DbSets to ApplicationDbContext in backend/src/ResearchPlatform.Api/Data/ApplicationDbContext.cs
- [x] T005 Add Participant entity configurations (indexes, constraints) to ApplicationDbContext OnModelCreating
- [ ] T006 Create and run EF migration AddParticipantEntities in backend/src/ResearchPlatform.Api/Migrations/
- [x] T007 Add seed data for ParticipantCodeSequence (initial A, 0) in backend/src/ResearchPlatform.Api/Data/SeedData.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core services and DTOs that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T008 Create ParticipantDtos (register request/response, login request/response, profile) in backend/src/ResearchPlatform.Api/DTOs/ParticipantDtos.cs
- [x] T009 [P] Create ParticipantValidators with FluentValidation rules in backend/src/ResearchPlatform.Api/Validators/ParticipantValidators.cs
- [x] T010 Implement ParticipantCodeService with sequential code generation algorithm (A1→Z99→AA1) in backend/src/ResearchPlatform.Api/Services/ParticipantCodeService.cs
- [x] T011 Implement ParticipantTokenService extending TokenService pattern for JWT in backend/src/ResearchPlatform.Api/Services/ParticipantTokenService.cs
- [x] T012 Create ParticipantController scaffold with route prefix /api/v1/participants in backend/src/ResearchPlatform.Api/Controllers/ParticipantController.cs
- [x] T013 [P] Create participantApi service stub in frontend/src/lib/services/participantApi.ts
- [x] T014 [P] Create participantAuth store for frontend state management in frontend/src/lib/stores/participantAuth.ts

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Participant Self-Registration (Priority: P1) 🎯 MVP

**Goal**: New participant registers, receives auto-generated code, can view registration success

**Independent Test**: Navigate to /participant/register, fill form, verify unique code assigned

### Backend Implementation for User Story 1

- [x] T015 [US1] Implement RegisterParticipant method in ParticipantService with code generation in backend/src/ResearchPlatform.Api/Services/ParticipantService.cs
- [x] T016 [US1] Add POST /participants/register endpoint to ParticipantController in backend/src/ResearchPlatform.Api/Controllers/ParticipantController.cs
- [x] T017 [US1] Add duplicate login identifier validation (409 conflict) to registration endpoint
- [x] T018 [US1] Implement password hashing using BCrypt for participant registration

### Frontend Implementation for User Story 1

- [x] T019 [P] [US1] Create participant registration page at frontend/src/routes/participant/register/+page.svelte
- [x] T020 [US1] Implement registration form with loginIdentifier, password, optional phone fields
- [x] T021 [US1] Add form validation (8+ char password, valid email/username format)
- [x] T022 [US1] Display success message with assigned participant code after registration
- [x] T023 [US1] Implement register API call in frontend/src/lib/services/participantApi.ts
- [x] T024 [US1] Store tokens in sessionStorage via participantAuth store after registration

**Checkpoint**: User Story 1 complete - participant can register and receive their code

---

## Phase 4: User Story 2 - Participant Login (Priority: P1)

**Goal**: Registered participant logs in with username/email and password

**Independent Test**: Navigate to /participant/login, enter valid credentials, verify redirect to dashboard

### Backend Implementation for User Story 2

- [x] T025 [US2] Implement LoginParticipant method in ParticipantAuthService with lockout logic in backend/src/ResearchPlatform.Api/Services/ParticipantAuthService.cs
- [x] T026 [US2] Add POST /participants/login endpoint to ParticipantController
- [x] T027 [US2] Implement account lockout (5 failed attempts = 1 min lock) in login logic
- [x] T028 [US2] Return 423 AccountLocked response with remaining seconds when locked
- [x] T029 [US2] Implement POST /participants/refresh endpoint for token refresh
- [x] T030 [US2] Implement POST /participants/logout endpoint to revoke refresh token

### Frontend Implementation for User Story 2

- [x] T031 [P] [US2] Create participant login page at frontend/src/routes/participant/login/+page.svelte
- [x] T032 [US2] Implement login form with loginIdentifier and password fields
- [x] T033 [US2] Display error messages for invalid credentials (generic, no field hints)
- [x] T034 [US2] Display lockout countdown when account is locked
- [x] T035 [US2] Implement login API call in frontend/src/lib/services/participantApi.ts
- [x] T036 [US2] Redirect to participant dashboard on successful login
- [x] T037 [US2] Store tokens in sessionStorage (clears on browser close)

**Checkpoint**: User Stories 1 AND 2 complete - registration and login flow working

---

## Phase 5: User Story 3 - Admin Password Reset (Priority: P2)

**Goal**: Admin searches for participant and resets their password

**Independent Test**: Admin logs in, searches participant by code, resets password, participant uses temp password

### Backend Implementation for User Story 3

- [x] T038 [US3] Implement SearchParticipants method in ParticipantService
- [x] T039 [US3] Implement ResetParticipantPassword method generating temp password in ParticipantService
- [x] T040 [US3] Add GET /admin/participants endpoint with pagination in AdminController extension
- [x] T041 [US3] Add GET /admin/participants/search endpoint to AdminController
- [x] T042 [US3] Add GET /admin/participants/{id} endpoint to AdminController
- [x] T043 [US3] Add POST /admin/participants/{id}/reset-password endpoint to AdminController
- [x] T044 [US3] Log password reset action to AuditLog with admin ID and participant code
- [x] T045 [US3] Set MustChangePassword flag when admin resets password

### Frontend Implementation for User Story 3

- [x] T046 [P] [US3] Create participant management section in admin area at frontend/src/routes/admin/participants/+page.svelte
- [x] T047 [US3] Implement participant search by code, email, or username
- [x] T048 [US3] Display participant list with pagination
- [x] T049 [US3] Add "Reset Password" button for each participant row
- [x] T050 [US3] Display modal with temporary password after reset (copy button)
- [x] T051 [US3] Implement admin participant API calls in frontend/src/lib/services/participantApi.ts

**Checkpoint**: Admin can now manage participant passwords

---

## Phase 6: User Story 4 - View Participant Code & Profile (Priority: P3)

**Goal**: Logged-in participant views their dashboard showing their code

**Independent Test**: Participant logs in, views dashboard, sees their code displayed prominently

### Backend Implementation for User Story 4

- [x] T052 [US4] Add GET /participants/me endpoint returning ParticipantProfile in ParticipantController
- [x] T053 [US4] Implement GetCurrentParticipant method in ParticipantService using JWT claims

### Frontend Implementation for User Story 4

- [x] T054 [P] [US4] Create participant dashboard at frontend/src/routes/participant/+page.svelte
- [x] T055 [US4] Display participant code prominently on dashboard
- [x] T056 [US4] Show login identifier and phone number on profile section
- [x] T057 [US4] Implement profile fetch API call in participantApi.ts
- [x] T058 [US4] Add route guard to redirect unauthenticated users to login

### Password Change Flow (linked to US3)

- [x] T059 [US4] Add POST /participants/me/change-password endpoint to ParticipantController
- [x] T060 [US4] Implement ChangePassword method validating current password
- [x] T061 [P] [US4] Create change password page at frontend/src/routes/participant/change-password/+page.svelte
- [x] T062 [US4] Redirect to change-password page when mustChangePassword is true after login
- [x] T063 [US4] Clear MustChangePassword flag after successful password change

**Checkpoint**: All user stories complete - full participant flow working

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final improvements affecting multiple stories

- [x] T064 [P] Add participant routes to main navigation/routing config
- [x] T065 [P] Style participant pages to be visually distinct from admin/student pages
- [x] T066 Verify session expires after 24 hours (JWT expiration config)
- [x] T067 Add loading states to all participant forms
- [x] T068 Add error boundary handling for participant routes
- [ ] T069 Run through quickstart.md validation scenarios
- [x] T070 Update CLAUDE.md with participant feature documentation

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational phase completion
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

| Story | Priority | Depends On | Can Start After |
|-------|----------|------------|-----------------|
| US1: Registration | P1 | Foundational only | Phase 2 complete |
| US2: Login | P1 | Foundational only | Phase 2 complete |
| US3: Admin Reset | P2 | Foundational + some US1/US2 for testing | Phase 2 complete |
| US4: Dashboard | P3 | US2 (login required) | US2 complete |

### Within Each User Story

- Backend models → services → controllers
- Frontend API client → pages → integration
- Each story should be independently testable after completion

### Parallel Opportunities

**Phase 1 (Setup):**
```
T002 ParticipantRefreshToken model ─┬─ parallel
T003 ParticipantCodeSequence model ─┘
```

**Phase 2 (Foundational):**
```
T009 Validators ─────────┬─ parallel
T013 participantApi stub ┼─ parallel
T014 participantAuth store ┘
```

**Phase 3 (US1) + Phase 4 (US2):**
```
After T018, frontend tasks can proceed:
T019 register page ─┬─ parallel with
T031 login page ────┘
```

**Phase 5 (US3) + Phase 6 (US4):**
```
T046 admin participants page ─┬─ parallel with
T054 participant dashboard ───┘
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2 Only)

1. Complete Phase 1: Setup (T001-T007)
2. Complete Phase 2: Foundational (T008-T014)
3. Complete Phase 3: User Story 1 - Registration (T015-T024)
4. Complete Phase 4: User Story 2 - Login (T025-T037)
5. **STOP and VALIDATE**: Test registration + login flow end-to-end
6. Deploy/demo if ready - participants can now register and log in!

### Incremental Delivery

| Increment | Stories Included | Value Delivered |
|-----------|------------------|-----------------|
| MVP | US1 + US2 | Participants can register and log in |
| +Admin | US3 | Admin can manage participant passwords |
| +Dashboard | US4 | Participants can view their code/profile |
| +Polish | All | Production-ready with styling |

### Task Count Summary

| Phase | Task Count | Parallel Tasks |
|-------|------------|----------------|
| Phase 1: Setup | 7 | 2 |
| Phase 2: Foundational | 7 | 3 |
| Phase 3: US1 Registration | 10 | 1 |
| Phase 4: US2 Login | 13 | 1 |
| Phase 5: US3 Admin Reset | 14 | 1 |
| Phase 6: US4 Dashboard | 12 | 2 |
| Phase 7: Polish | 7 | 2 |
| **Total** | **70** | **12** |

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently testable after its phase completes
- Commit after each task or logical group
- Stop at any checkpoint to validate the story independently
- Frontend tasks use SvelteKit with existing UI component library
- Backend follows existing ResearchPlatform.Api patterns
