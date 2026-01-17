# Tasks: Research Data Collection Platform

**Input**: Design documents from `/specs/001-research-data-platform/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/openapi.yaml

**Tests**: Tests are not explicitly requested - implementation tasks only.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Backend**: `backend/src/ResearchPlatform.Api/`
- **Frontend**: `frontend/src/`
- **Docker**: `docker/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create backend .NET 9 solution in backend/ResearchPlatform.sln
- [X] T002 Create ResearchPlatform.Api project in backend/src/ResearchPlatform.Api/
- [X] T003 [P] Add NuGet packages: EF Core 9, Npgsql, FluentValidation, JWT Bearer in backend/src/ResearchPlatform.Api/ResearchPlatform.Api.csproj
- [X] T004 [P] Initialize SvelteKit project with TypeScript in frontend/
- [X] T005 [P] Add npm packages: shadcn-svelte, tailwindcss, chart.js in frontend/package.json
- [X] T006 [P] Configure TailwindCSS in frontend/app.css (v4 CSS-first approach)
- [X] T007 [P] Initialize shadcn-svelte components in frontend/src/lib/components/ui/
- [X] T008 [P] Create docker-compose.yml with PostgreSQL, MinIO services in docker/docker-compose.yml
- [X] T009 [P] Create Dockerfile for backend in docker/Dockerfile.backend
- [X] T010 [P] Create Dockerfile for frontend in docker/Dockerfile.frontend
- [X] T011 [P] Create nginx reverse proxy config in docker/nginx.conf

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**CRITICAL**: No user story work can begin until this phase is complete

### Database & Models

- [X] T012 Create ApplicationDbContext in backend/src/ResearchPlatform.Api/Data/ApplicationDbContext.cs
- [X] T013 [P] Create User entity model in backend/src/ResearchPlatform.Api/Models/User.cs
- [X] T014 [P] Create Role enum (Admin, Evaluator, Student) in backend/src/ResearchPlatform.Api/Models/Enums/Role.cs
- [X] T015 [P] Create UserStatus enum (Active, Inactive) in backend/src/ResearchPlatform.Api/Models/Enums/UserStatus.cs
- [X] T016 [P] Create AuditLog entity model in backend/src/ResearchPlatform.Api/Models/AuditLog.cs
- [X] T017 Create initial EF Core migration in backend/src/ResearchPlatform.Api/Data/Migrations/
- [X] T018 Configure EF Core entity relationships and indexes in backend/src/ResearchPlatform.Api/Data/ApplicationDbContext.cs

### Authentication & Authorization

- [X] T019 Configure JWT authentication in backend/src/ResearchPlatform.Api/Program.cs
- [X] T020 [P] Create JwtSettings configuration class in backend/src/ResearchPlatform.Api/Auth/JwtSettings.cs
- [X] T021 [P] Create TokenService for JWT generation in backend/src/ResearchPlatform.Api/Auth/TokenService.cs
- [X] T022 [P] Create RefreshToken entity model in backend/src/ResearchPlatform.Api/Models/RefreshToken.cs
- [X] T023 Implement AuthController (login, refresh, logout) in backend/src/ResearchPlatform.Api/Controllers/AuthController.cs
- [X] T024 [P] Create role-based authorization policies in backend/src/ResearchPlatform.Api/Auth/AuthorizationPolicies.cs
- [X] T025 [P] Create AuditService for logging actions in backend/src/ResearchPlatform.Api/Services/AuditService.cs

### Frontend Foundation

- [X] T026 [P] Create auth store for token management in frontend/src/lib/stores/auth.ts
- [X] T027 [P] Create API client base with auth headers in frontend/src/lib/api/client.ts
- [X] T028 [P] Create auth API functions (login, logout, refresh) in frontend/src/lib/api/auth.ts
- [X] T029 Create login page in frontend/src/routes/login/+page.svelte
- [X] T030 [P] Create auth guard hook in frontend/src/hooks.server.ts
- [X] T031 [P] Create layout with role-based navigation in frontend/src/routes/+layout.svelte
- [X] T032 [P] Create change password component in frontend/src/lib/components/ChangePassword.svelte

### Error Handling & Configuration

- [X] T033 [P] Configure global exception handling middleware in backend/src/ResearchPlatform.Api/Middleware/ExceptionMiddleware.cs
- [X] T034 [P] Create appsettings.json with connection strings in backend/src/ResearchPlatform.Api/appsettings.json
- [X] T035 [P] Create appsettings.Development.json in backend/src/ResearchPlatform.Api/appsettings.Development.json
- [X] T036 [P] Configure CORS for frontend in backend/src/ResearchPlatform.Api/Program.cs
- [X] T037 [P] Create .env file for frontend API URL in frontend/.env

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Student Completes Pre-Test Assessment (Priority: P1) MVP

**Goal**: Students can log in, complete multi-step pre-test questionnaire with auto-save, and submit

**Independent Test**: Create student account, log in, complete multi-step questionnaire, verify submission lock

### Backend Models for US1

- [ ] T038 [P] [US1] Create Questionnaire entity model in backend/src/ResearchPlatform.Api/Models/Questionnaire.cs
- [ ] T039 [P] [US1] Create QuestionnaireType enum (Pretest, Posttest) in backend/src/ResearchPlatform.Api/Models/Enums/QuestionnaireType.cs
- [ ] T040 [P] [US1] Create Question entity model in backend/src/ResearchPlatform.Api/Models/Question.cs
- [ ] T041 [P] [US1] Create QuestionType enum in backend/src/ResearchPlatform.Api/Models/Enums/QuestionType.cs
- [ ] T042 [P] [US1] Create Answer entity model in backend/src/ResearchPlatform.Api/Models/Answer.cs
- [ ] T043 [P] [US1] Create StepTiming entity model in backend/src/ResearchPlatform.Api/Models/StepTiming.cs
- [ ] T044 [P] [US1] Create Score entity model in backend/src/ResearchPlatform.Api/Models/Score.cs
- [ ] T045 [US1] Create EF migration for questionnaire entities in backend/src/ResearchPlatform.Api/Data/Migrations/
- [ ] T046 [US1] Add database trigger for immutable answers in backend/src/ResearchPlatform.Api/Data/Migrations/

### Backend Services for US1

- [ ] T047 [US1] Create QuestionnaireService in backend/src/ResearchPlatform.Api/Services/QuestionnaireService.cs
- [ ] T048 [US1] Create AnswerService with auto-save logic in backend/src/ResearchPlatform.Api/Services/AnswerService.cs
- [ ] T049 [US1] Create ScoreCalculationService in backend/src/ResearchPlatform.Api/Services/ScoreCalculationService.cs
- [ ] T050 [US1] Create StepTimingService in backend/src/ResearchPlatform.Api/Services/StepTimingService.cs

### Backend API for US1

- [ ] T051 [US1] Create StudentController with status endpoint in backend/src/ResearchPlatform.Api/Controllers/StudentController.cs
- [ ] T052 [US1] Add GET /student/pretest endpoint in backend/src/ResearchPlatform.Api/Controllers/StudentController.cs
- [ ] T053 [US1] Add POST /student/pretest/save endpoint in backend/src/ResearchPlatform.Api/Controllers/StudentController.cs
- [ ] T054 [US1] Add POST /student/pretest/submit endpoint in backend/src/ResearchPlatform.Api/Controllers/StudentController.cs
- [ ] T055 [US1] Add POST /student/step-timing endpoint in backend/src/ResearchPlatform.Api/Controllers/StudentController.cs

### Backend DTOs for US1

- [ ] T056 [P] [US1] Create StudentStatusDto in backend/src/ResearchPlatform.Api/DTOs/StudentStatusDto.cs
- [ ] T057 [P] [US1] Create QuestionnaireWithAnswersDto in backend/src/ResearchPlatform.Api/DTOs/QuestionnaireWithAnswersDto.cs
- [ ] T058 [P] [US1] Create SaveAnswersRequest/Response DTOs in backend/src/ResearchPlatform.Api/DTOs/SaveAnswersDto.cs
- [ ] T059 [P] [US1] Create SubmissionResponseDto in backend/src/ResearchPlatform.Api/DTOs/SubmissionResponseDto.cs

### Frontend for US1

- [ ] T060 [P] [US1] Create student API functions in frontend/src/lib/api/student.ts
- [ ] T061 [P] [US1] Create questionnaire store with auto-save in frontend/src/lib/stores/questionnaire.ts
- [ ] T062 [P] [US1] Create LikertScale question component in frontend/src/lib/components/questions/LikertScale.svelte
- [ ] T063 [P] [US1] Create TrueFalse question component in frontend/src/lib/components/questions/TrueFalse.svelte
- [ ] T064 [P] [US1] Create MultipleChoice question component in frontend/src/lib/components/questions/MultipleChoice.svelte
- [ ] T065 [P] [US1] Create Dropdown question component in frontend/src/lib/components/questions/Dropdown.svelte
- [ ] T066 [P] [US1] Create TextField question component in frontend/src/lib/components/questions/TextField.svelte
- [ ] T067 [US1] Create QuestionRenderer component in frontend/src/lib/components/QuestionRenderer.svelte
- [ ] T068 [US1] Create StepNavigation component in frontend/src/lib/components/StepNavigation.svelte
- [ ] T069 [US1] Create pre-test page with multi-step navigation in frontend/src/routes/student/pretest/+page.svelte
- [ ] T070 [US1] Create submission confirmation component in frontend/src/lib/components/SubmissionConfirmation.svelte
- [ ] T071 [US1] Create student dashboard/status page in frontend/src/routes/student/+page.svelte

**Checkpoint**: User Story 1 complete - Students can complete pre-test assessment

---

## Phase 4: User Story 2 - Student Accesses Educational Materials (Priority: P2)

**Goal**: Students who completed pre-test can access PDFs, videos, and text materials with engagement tracking

**Independent Test**: Upload materials, have student access multiple times, verify access tracking

### Backend Models for US2

- [ ] T072 [P] [US2] Create Material entity model in backend/src/ResearchPlatform.Api/Models/Material.cs
- [ ] T073 [P] [US2] Create MaterialType enum (Pdf, Video, Text) in backend/src/ResearchPlatform.Api/Models/Enums/MaterialType.cs
- [ ] T074 [P] [US2] Create MaterialAccess entity model in backend/src/ResearchPlatform.Api/Models/MaterialAccess.cs
- [ ] T075 [US2] Create EF migration for material entities in backend/src/ResearchPlatform.Api/Data/Migrations/

### Backend Services for US2

- [ ] T076 [US2] Create MaterialService in backend/src/ResearchPlatform.Api/Services/MaterialService.cs
- [ ] T077 [US2] Create StorageService for S3/MinIO in backend/src/ResearchPlatform.Api/Services/StorageService.cs
- [ ] T078 [US2] Create MaterialAccessService for tracking in backend/src/ResearchPlatform.Api/Services/MaterialAccessService.cs

### Backend API for US2

- [ ] T079 [US2] Add GET /student/materials endpoint in backend/src/ResearchPlatform.Api/Controllers/StudentController.cs
- [ ] T080 [US2] Add GET /student/materials/{id} endpoint with signed URL in backend/src/ResearchPlatform.Api/Controllers/StudentController.cs
- [ ] T081 [US2] Add POST /student/materials/{id}/track endpoint in backend/src/ResearchPlatform.Api/Controllers/StudentController.cs

### Frontend for US2

- [ ] T082 [P] [US2] Create materials API functions in frontend/src/lib/api/materials.ts
- [ ] T083 [P] [US2] Create MaterialCard component in frontend/src/lib/components/MaterialCard.svelte
- [ ] T084 [P] [US2] Create PdfViewer component in frontend/src/lib/components/PdfViewer.svelte
- [ ] T085 [P] [US2] Create VideoPlayer component in frontend/src/lib/components/VideoPlayer.svelte
- [ ] T086 [US2] Create materials list page in frontend/src/routes/student/materials/+page.svelte
- [ ] T087 [US2] Create material detail page with tracking in frontend/src/routes/student/materials/[id]/+page.svelte

**Checkpoint**: User Story 2 complete - Students can access educational materials

---

## Phase 5: User Story 3 - Student Completes Post-Test Assessment (Priority: P3)

**Goal**: Students can complete post-test when batch is open, using same questionnaire system

**Independent Test**: Open post-test batch, have student complete it, verify submission and scoring

### Backend Models for US3

- [ ] T088 [P] [US3] Create PostTestBatch entity model in backend/src/ResearchPlatform.Api/Models/PostTestBatch.cs
- [ ] T089 [US3] Create EF migration for PostTestBatch in backend/src/ResearchPlatform.Api/Data/Migrations/

### Backend Services for US3

- [ ] T090 [US3] Create PostTestBatchService in backend/src/ResearchPlatform.Api/Services/PostTestBatchService.cs
- [ ] T091 [US3] Extend QuestionnaireService for post-test logic in backend/src/ResearchPlatform.Api/Services/QuestionnaireService.cs

### Backend API for US3

- [ ] T092 [US3] Add GET /student/posttest endpoint in backend/src/ResearchPlatform.Api/Controllers/StudentController.cs
- [ ] T093 [US3] Add POST /student/posttest/save endpoint in backend/src/ResearchPlatform.Api/Controllers/StudentController.cs
- [ ] T094 [US3] Add POST /student/posttest/submit endpoint in backend/src/ResearchPlatform.Api/Controllers/StudentController.cs

### Frontend for US3

- [ ] T095 [US3] Create post-test page (reuse questionnaire components) in frontend/src/routes/student/posttest/+page.svelte
- [ ] T096 [US3] Update student dashboard with post-test status in frontend/src/routes/student/+page.svelte

**Checkpoint**: User Story 3 complete - Students can complete post-test assessment

---

## Phase 6: User Story 4 - Evaluator Scores Student Responses (Priority: P4)

**Goal**: Evaluators can view assigned students, enter scores on evaluator-only fields, see auto-calculated totals

**Independent Test**: Assign student to evaluator, evaluator enters scores, verify calculation

### Backend Models for US4

- [ ] T097 [P] [US4] Create EvaluatorAssignment entity model in backend/src/ResearchPlatform.Api/Models/EvaluatorAssignment.cs
- [ ] T098 [P] [US4] Create EvaluatorScore entity model in backend/src/ResearchPlatform.Api/Models/EvaluatorScore.cs
- [ ] T099 [P] [US4] Create CombinedScore entity model in backend/src/ResearchPlatform.Api/Models/CombinedScore.cs
- [ ] T100 [US4] Create EF migration for evaluator entities in backend/src/ResearchPlatform.Api/Data/Migrations/
- [ ] T101 [US4] Add database trigger for immutable finalized scores in backend/src/ResearchPlatform.Api/Data/Migrations/

### Backend Services for US4

- [ ] T102 [US4] Create EvaluatorService in backend/src/ResearchPlatform.Api/Services/EvaluatorService.cs
- [ ] T103 [US4] Create EvaluatorScoreService with auto-calculation in backend/src/ResearchPlatform.Api/Services/EvaluatorScoreService.cs

### Backend API for US4

- [ ] T104 [US4] Create EvaluatorController in backend/src/ResearchPlatform.Api/Controllers/EvaluatorController.cs
- [ ] T105 [US4] Add GET /evaluator/students endpoint in backend/src/ResearchPlatform.Api/Controllers/EvaluatorController.cs
- [ ] T106 [US4] Add GET /evaluator/students/{id}/responses endpoint in backend/src/ResearchPlatform.Api/Controllers/EvaluatorController.cs
- [ ] T107 [US4] Add POST /evaluator/students/{id}/scores endpoint in backend/src/ResearchPlatform.Api/Controllers/EvaluatorController.cs

### Frontend for US4

- [ ] T108 [P] [US4] Create evaluator API functions in frontend/src/lib/api/evaluator.ts
- [ ] T109 [P] [US4] Create StudentScoreCard component in frontend/src/lib/components/StudentScoreCard.svelte
- [ ] T110 [P] [US4] Create ScoringForm component in frontend/src/lib/components/ScoringForm.svelte
- [ ] T111 [US4] Create evaluator dashboard in frontend/src/routes/evaluator/+page.svelte
- [ ] T112 [US4] Create student scoring page in frontend/src/routes/evaluator/students/[id]/+page.svelte

**Checkpoint**: User Story 4 complete - Evaluators can score student responses

---

## Phase 7: User Story 5 - Administrator Manages Users and Study Flow (Priority: P5)

**Goal**: Admins can create/manage users, assign evaluators, control post-test batch, view dashboard

**Independent Test**: Create users, manage status, open/close post-test batch, view metrics

### Backend Services for US5

- [ ] T113 [US5] Create UserManagementService in backend/src/ResearchPlatform.Api/Services/UserManagementService.cs
- [ ] T114 [US5] Create CsvImportService for bulk user creation in backend/src/ResearchPlatform.Api/Services/CsvImportService.cs
- [ ] T115 [US5] Create DashboardService for metrics in backend/src/ResearchPlatform.Api/Services/DashboardService.cs
- [ ] T116 [US5] Create EvaluatorAssignmentService in backend/src/ResearchPlatform.Api/Services/EvaluatorAssignmentService.cs

### Backend API for US5

- [ ] T117 [US5] Create AdminController in backend/src/ResearchPlatform.Api/Controllers/AdminController.cs
- [ ] T118 [US5] Add GET /admin/dashboard endpoint in backend/src/ResearchPlatform.Api/Controllers/AdminController.cs
- [ ] T119 [US5] Add CRUD /admin/users endpoints in backend/src/ResearchPlatform.Api/Controllers/AdminController.cs
- [ ] T120 [US5] Add POST /admin/users/import endpoint in backend/src/ResearchPlatform.Api/Controllers/AdminController.cs
- [ ] T121 [US5] Add POST /admin/users/{id}/activate endpoint in backend/src/ResearchPlatform.Api/Controllers/AdminController.cs
- [ ] T122 [US5] Add POST /admin/users/{id}/deactivate endpoint in backend/src/ResearchPlatform.Api/Controllers/AdminController.cs
- [ ] T123 [US5] Add POST /admin/users/{id}/reset-password endpoint in backend/src/ResearchPlatform.Api/Controllers/AdminController.cs
- [ ] T124 [US5] Add CRUD /admin/evaluator-assignments endpoints in backend/src/ResearchPlatform.Api/Controllers/AdminController.cs
- [ ] T125 [US5] Add POST /admin/posttest-batch/open endpoint in backend/src/ResearchPlatform.Api/Controllers/AdminController.cs
- [ ] T126 [US5] Add POST /admin/posttest-batch/close endpoint in backend/src/ResearchPlatform.Api/Controllers/AdminController.cs
- [ ] T127 [US5] Add POST /admin/students/{id}/finalize endpoint in backend/src/ResearchPlatform.Api/Controllers/AdminController.cs

### Frontend for US5

- [ ] T128 [P] [US5] Create admin API functions in frontend/src/lib/api/admin.ts
- [ ] T129 [P] [US5] Create DashboardMetrics component in frontend/src/lib/components/DashboardMetrics.svelte
- [ ] T130 [P] [US5] Create CompletionChart component in frontend/src/lib/components/charts/CompletionChart.svelte
- [ ] T131 [P] [US5] Create ScoreComparisonChart component in frontend/src/lib/components/charts/ScoreComparisonChart.svelte
- [ ] T132 [P] [US5] Create UserTable component in frontend/src/lib/components/UserTable.svelte
- [ ] T133 [P] [US5] Create CreateUserForm component in frontend/src/lib/components/CreateUserForm.svelte
- [ ] T134 [P] [US5] Create BulkImportForm component in frontend/src/lib/components/BulkImportForm.svelte
- [ ] T135 [P] [US5] Create EvaluatorAssignmentTable component in frontend/src/lib/components/EvaluatorAssignmentTable.svelte
- [ ] T136 [US5] Create admin dashboard page in frontend/src/routes/admin/dashboard/+page.svelte
- [ ] T137 [US5] Create user management page in frontend/src/routes/admin/users/+page.svelte
- [ ] T138 [US5] Create user detail/edit page in frontend/src/routes/admin/users/[id]/+page.svelte
- [ ] T139 [US5] Create post-test batch control UI in frontend/src/routes/admin/dashboard/+page.svelte

**Checkpoint**: User Story 5 complete - Administrators can manage users and study flow

---

## Phase 8: User Story 6 - Administrator Exports Research Data (Priority: P6)

**Goal**: Admins can export all data to Excel/CSV format compatible with SPSS

**Independent Test**: Generate sample data, export to Excel/CSV, verify SPSS import

### Backend Services for US6

- [ ] T140 [US6] Create ExportService for data export in backend/src/ResearchPlatform.Api/Services/ExportService.cs
- [ ] T141 [US6] Add streaming Excel generation in backend/src/ResearchPlatform.Api/Services/ExportService.cs
- [ ] T142 [US6] Add streaming CSV generation in backend/src/ResearchPlatform.Api/Services/ExportService.cs

### Backend API for US6

- [ ] T143 [US6] Add GET /admin/export endpoint with format parameter in backend/src/ResearchPlatform.Api/Controllers/AdminController.cs

### Frontend for US6

- [ ] T144 [P] [US6] Create ExportButton component in frontend/src/lib/components/ExportButton.svelte
- [ ] T145 [US6] Create export page in frontend/src/routes/admin/export/+page.svelte

**Checkpoint**: User Story 6 complete - Administrators can export research data

---

## Phase 9: User Story 7 - Administrator Configures Questionnaires (Priority: P7)

**Goal**: Admins can create/edit questionnaires with all question types via form builder

**Independent Test**: Build questionnaire with various question types, verify student can complete it

### Backend Services for US7

- [ ] T146 [US7] Create QuestionnaireBuilderService in backend/src/ResearchPlatform.Api/Services/QuestionnaireBuilderService.cs

### Backend API for US7

- [ ] T147 [US7] Add GET /admin/questionnaires endpoint in backend/src/ResearchPlatform.Api/Controllers/AdminController.cs
- [ ] T148 [US7] Add POST /admin/questionnaires endpoint in backend/src/ResearchPlatform.Api/Controllers/AdminController.cs
- [ ] T149 [US7] Add GET /admin/questionnaires/{id} endpoint in backend/src/ResearchPlatform.Api/Controllers/AdminController.cs
- [ ] T150 [US7] Add PUT /admin/questionnaires/{id} endpoint in backend/src/ResearchPlatform.Api/Controllers/AdminController.cs

### Frontend for US7

- [ ] T151 [P] [US7] Create QuestionEditor component in frontend/src/lib/components/builder/QuestionEditor.svelte
- [ ] T152 [P] [US7] Create StepEditor component in frontend/src/lib/components/builder/StepEditor.svelte
- [ ] T153 [P] [US7] Create QuestionTypeSelector component in frontend/src/lib/components/builder/QuestionTypeSelector.svelte
- [ ] T154 [P] [US7] Create OptionsEditor component for choice questions in frontend/src/lib/components/builder/OptionsEditor.svelte
- [ ] T155 [US7] Create questionnaire list page in frontend/src/routes/admin/questionnaires/+page.svelte
- [ ] T156 [US7] Create questionnaire builder page in frontend/src/routes/admin/questionnaires/[id]/+page.svelte
- [ ] T157 [US7] Create new questionnaire page in frontend/src/routes/admin/questionnaires/new/+page.svelte

**Checkpoint**: User Story 7 complete - Administrators can configure questionnaires

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T158 [P] Add input validation with FluentValidation in backend/src/ResearchPlatform.Api/Validators/
- [ ] T159 [P] Add rate limiting middleware in backend/src/ResearchPlatform.Api/Middleware/RateLimitingMiddleware.cs
- [ ] T160 [P] Add request/response logging in backend/src/ResearchPlatform.Api/Middleware/RequestLoggingMiddleware.cs
- [ ] T161 [P] Add mobile-responsive CSS refinements across frontend/src/
- [ ] T162 [P] Add loading states and error handling to all frontend pages
- [ ] T163 [P] Add Swagger/OpenAPI documentation in backend/src/ResearchPlatform.Api/Program.cs
- [ ] T164 Database seed script for demo data in backend/src/ResearchPlatform.Api/Data/SeedData.cs
- [ ] T165 Run and verify quickstart.md setup instructions
- [ ] T166 Final security review: CORS, HTTPS, input sanitization

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup - BLOCKS all user stories
- **User Stories (Phase 3-9)**: All depend on Foundational phase completion
  - US1 (P1): Foundation only - MVP
  - US2 (P2): Foundation + requires Material entities
  - US3 (P3): Foundation + US1 (questionnaire system)
  - US4 (P4): Foundation + US3 (post-test must exist)
  - US5 (P5): Foundation + User model
  - US6 (P6): Foundation + all scoring complete
  - US7 (P7): Foundation + questionnaire models
- **Polish (Phase 10)**: Depends on all desired user stories

### User Story Dependencies

```
Phase 2 (Foundation) ─┬─> US1 (P1) ─┬─> US2 (P2)
                      │             │
                      │             └─> US3 (P3) ──> US4 (P4)
                      │
                      ├─> US5 (P5)
                      │
                      ├─> US6 (P6) [after US4]
                      │
                      └─> US7 (P7)
```

### Parallel Opportunities

- **Phase 1**: T003-T011 can all run in parallel
- **Phase 2**: T013-T016, T020-T022, T024-T025, T026-T028, T030-T032, T033-T037 can run in parallel
- **Per User Story**: Model tasks [P] can run in parallel, component tasks [P] can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all model tasks in parallel:
T038: Create Questionnaire entity model
T039: Create QuestionnaireType enum
T040: Create Question entity model
T041: Create QuestionType enum
T042: Create Answer entity model
T043: Create StepTiming entity model
T044: Create Score entity model

# Then launch all DTO tasks in parallel:
T056: Create StudentStatusDto
T057: Create QuestionnaireWithAnswersDto
T058: Create SaveAnswersRequest/Response DTOs
T059: Create SubmissionResponseDto

# Then launch all question component tasks in parallel:
T062: Create LikertScale component
T063: Create TrueFalse component
T064: Create MultipleChoice component
T065: Create Dropdown component
T066: Create TextField component
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Students can complete pre-test
5. Deploy if ready - this alone captures baseline research data

### Incremental Delivery

1. Setup + Foundation → Foundation ready
2. Add US1 → Pre-test works → Deploy (MVP!)
3. Add US2 → Materials work → Deploy
4. Add US3 → Post-test works → Deploy
5. Add US4 → Scoring works → Deploy
6. Add US5 → Admin dashboard → Deploy
7. Add US6 → Export works → Deploy (Research ready!)
8. Add US7 → Form builder → Deploy (Full feature)

### Parallel Team Strategy

With 3 developers after Foundation:
- Developer A: US1 → US3 → US4 (student/evaluator flow)
- Developer B: US5 → US6 (admin features)
- Developer C: US2 → US7 (materials/builder)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story
- Each user story is independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Backend and frontend tasks within a story can be parallelized by different developers
