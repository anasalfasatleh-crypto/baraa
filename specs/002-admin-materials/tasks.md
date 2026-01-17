# Tasks: Admin Materials Management

**Input**: Design documents from `/specs/002-admin-materials/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Not requested - no test tasks included.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

- **Backend**: `backend/src/ResearchPlatform.Api/`
- **Frontend**: `frontend/src/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create base DTOs and API client methods used across all user stories

- [x] T001 [P] Add AdminMaterialDto record to backend/src/ResearchPlatform.Api/DTOs/AdminDtos.cs
- [x] T002 [P] Add AdminMaterialDetailDto record to backend/src/ResearchPlatform.Api/DTOs/AdminDtos.cs
- [x] T003 [P] Add Material TypeScript interfaces to frontend/src/lib/api/admin.ts
- [x] T004 Add GetAllMaterialsAsync method to backend/src/ResearchPlatform.Api/Services/MaterialService.cs (returns all materials including inactive)

**Checkpoint**: Base types defined - ready for endpoint and UI implementation

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T005 Create admin materials page skeleton at frontend/src/routes/admin/materials/+page.svelte with basic layout (header, loading state, error state)
- [x] T006 [P] Add getMaterials API method to frontend/src/lib/api/admin.ts (GET /admin/materials)
- [x] T007 Add GET /admin/materials endpoint to backend/src/ResearchPlatform.Api/Controllers/AdminController.cs (list all materials with access counts)

**Checkpoint**: Foundation ready - admin can navigate to /admin/materials and see empty/loading state. User story implementation can now begin.

---

## Phase 3: User Story 1 - Upload New Study Material (Priority: P1) 🎯 MVP

**Goal**: Enable admins to upload PDFs, videos, and text documents with title/description

**Independent Test**: Login as admin, navigate to /admin/materials, click Upload, fill form, select file, verify it appears in list with correct metadata

### Backend Implementation for User Story 1

- [x] T008 [P] [US1] Add UploadMaterialRequest record to backend/src/ResearchPlatform.Api/DTOs/AdminDtos.cs
- [x] T009 [P] [US1] Add UploadMaterialRequestValidator to backend/src/ResearchPlatform.Api/Validators/AdminValidators.cs (title required max 200, description max 1000, file type validation, max 1GB)
- [x] T010 [US1] Add POST /admin/materials/upload endpoint to backend/src/ResearchPlatform.Api/Controllers/AdminController.cs (multipart/form-data, calls MaterialService.CreateMaterialAsync)

### Frontend Implementation for User Story 1

- [x] T011 [P] [US1] Create MaterialForm.svelte component at frontend/src/lib/components/MaterialForm.svelte (title input, description textarea, type select, file input, progress bar)
- [x] T012 [P] [US1] Add uploadMaterial API method to frontend/src/lib/api/admin.ts with XMLHttpRequest for progress tracking
- [x] T013 [US1] Add upload modal/dialog trigger and state management to frontend/src/routes/admin/materials/+page.svelte
- [x] T014 [US1] Wire MaterialForm.svelte to uploadMaterial API call with success/error alerts in frontend/src/routes/admin/materials/+page.svelte
- [x] T015 [US1] Add client-side file type validation in MaterialForm.svelte (immediate feedback before upload)

**Checkpoint**: At this point, User Story 1 should be fully functional - admin can upload materials that appear in list

---

## Phase 4: User Story 2 - View and Manage Materials List (Priority: P2)

**Goal**: Display all materials in searchable, sortable table with full metadata

**Independent Test**: Navigate to /admin/materials and verify all uploaded materials appear with title, type, file size, access count, last updated; test search and sort functions

### Backend Implementation for User Story 2

- [x] T016 [US2] Add GET /admin/materials/{id} endpoint to backend/src/ResearchPlatform.Api/Controllers/AdminController.cs (returns AdminMaterialDetailDto with signed URL)

### Frontend Implementation for User Story 2

- [x] T017 [P] [US2] Create MaterialTable.svelte component at frontend/src/lib/components/MaterialTable.svelte (columns: title, type, file size, access count, created date, actions) - Implemented directly in +page.svelte using Table component
- [x] T018 [P] [US2] Add getMaterialDetail API method to frontend/src/lib/api/admin.ts (GET /admin/materials/{id})
- [x] T019 [US2] Integrate MaterialTable.svelte into frontend/src/routes/admin/materials/+page.svelte replacing placeholder - Table integrated directly in page
- [ ] T020 [US2] Add search input with client-side filtering in frontend/src/routes/admin/materials/+page.svelte
- [ ] T021 [US2] Add column header click sorting in MaterialTable.svelte (title, type, size, access count, date)
- [x] T022 [US2] Add type filter dropdown (All, Pdf, Video, Text) in frontend/src/routes/admin/materials/+page.svelte
- [x] T023 [US2] Add View action button in MaterialTable.svelte that opens signed URL in new tab

**Checkpoint**: At this point, User Stories 1 AND 2 should both work - upload and view/search/sort/filter materials

---

## Phase 5: User Story 3 - Update Material Information (Priority: P3)

**Goal**: Allow admins to edit material title, description, type, and order without re-uploading

**Independent Test**: Click Edit on existing material, change title/description, save, verify changes persist in list

### Backend Implementation for User Story 3

- [x] T024 [P] [US3] Add UpdateMaterialRequest record to backend/src/ResearchPlatform.Api/DTOs/AdminDtos.cs
- [x] T025 [P] [US3] Add UpdateMaterialRequestValidator to backend/src/ResearchPlatform.Api/Validators/AdminValidators.cs
- [x] T026 [US3] Add PUT /admin/materials/{id} endpoint to backend/src/ResearchPlatform.Api/Controllers/AdminController.cs (calls MaterialService.UpdateMaterialAsync)

### Frontend Implementation for User Story 3

- [x] T027 [P] [US3] Add updateMaterial API method to frontend/src/lib/api/admin.ts (PUT /admin/materials/{id})
- [x] T028 [US3] Add edit mode to MaterialForm.svelte (pre-populate fields, hide file input for edit mode)
- [x] T029 [US3] Add Edit action button in MaterialTable.svelte that triggers edit modal with material data
- [x] T030 [US3] Wire edit form submission to updateMaterial API with success/error handling in frontend/src/routes/admin/materials/+page.svelte

**Checkpoint**: At this point, User Stories 1, 2, AND 3 should work - upload, view, and edit materials

---

## Phase 6: User Story 4 - Delete Materials (Priority: P3)

**Goal**: Allow admins to soft-delete materials with confirmation dialog

**Independent Test**: Click Delete on material, confirm in dialog, verify material no longer appears in list or student view; verify access logs preserved

### Backend Implementation for User Story 4

- [x] T031 [US4] Add DELETE /admin/materials/{id} endpoint to backend/src/ResearchPlatform.Api/Controllers/AdminController.cs (soft delete via MaterialService - sets IsActive=false)

### Frontend Implementation for User Story 4

- [x] T032 [P] [US4] Add deleteMaterial API method to frontend/src/lib/api/admin.ts (DELETE /admin/materials/{id})
- [x] T033 [US4] Add Delete action button in MaterialTable.svelte with confirmation dialog
- [x] T034 [US4] Wire delete confirmation to deleteMaterial API with success/error handling and list refresh in frontend/src/routes/admin/materials/+page.svelte

**Checkpoint**: All user stories should now be independently functional

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [x] T035 Add loading spinner/skeleton for table in frontend/src/routes/admin/materials/+page.svelte
- [x] T036 Add empty state message when no materials exist in frontend/src/routes/admin/materials/+page.svelte
- [x] T037 [P] Add human-readable file size formatting helper in frontend/src/lib/utils.ts - Added directly in +page.svelte
- [x] T038 [P] Add date formatting for created/updated columns in MaterialTable.svelte - Added directly in +page.svelte
- [x] T039 Verify nginx client_max_body_size supports 1GB uploads in docker/nginx.conf
- [ ] T040 Manual E2E test: Complete upload-view-edit-delete cycle as admin user
- [ ] T041 Manual test: Verify student can see uploaded material at /student/materials

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational phase completion
  - Can proceed sequentially in priority order (P1 → P2 → P3 → P3)
  - Or US3 and US4 can run in parallel after US2 (same priority)
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after US1 complete (needs materials to display) - Builds on list view from foundational
- **User Story 3 (P3)**: Can start after US2 complete (edit button in table) - Independent edit functionality
- **User Story 4 (P3)**: Can start after US2 complete (delete button in table) - Independent delete functionality

### Within Each User Story

- Backend DTOs before validators
- Backend validators before endpoints
- Frontend API methods can parallel with backend
- Frontend components before page integration
- Core implementation before UI polish

### Parallel Opportunities

**Phase 1 - All can run in parallel**:
```
T001 (AdminMaterialDto) || T002 (AdminMaterialDetailDto) || T003 (TS interfaces) || T004 (GetAllMaterialsAsync)
```

**Phase 2 - Foundation**:
```
T005 (page skeleton) → T006 (getMaterials API) || T007 (GET endpoint)
```

**User Story 1 - Backend parallel, then frontend**:
```
T008 (UploadRequest DTO) || T009 (Validator) → T010 (POST endpoint)
T011 (MaterialForm) || T012 (uploadMaterial API) → T013 → T014 → T015
```

**User Story 2 - Parallel components**:
```
T016 (GET /id endpoint)
T017 (MaterialTable) || T018 (getMaterialDetail API) → T019 → T020 || T021 || T022 → T023
```

**User Story 3 & 4 - Can run in parallel (same P3 priority)**:
```
[US3]: T024 || T025 → T026 ... || [US4]: T031 → T032 → T033 → T034
```

---

## Parallel Example: User Story 1

```bash
# Launch backend DTOs and validators together:
Task: "Add UploadMaterialRequest record to backend/src/ResearchPlatform.Api/DTOs/AdminDtos.cs"
Task: "Add UploadMaterialRequestValidator to backend/src/ResearchPlatform.Api/Validators/AdminValidators.cs"

# Launch frontend component and API together:
Task: "Create MaterialForm.svelte component at frontend/src/lib/components/MaterialForm.svelte"
Task: "Add uploadMaterial API method to frontend/src/lib/api/admin.ts"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T004)
2. Complete Phase 2: Foundational (T005-T007)
3. Complete Phase 3: User Story 1 (T008-T015)
4. **STOP and VALIDATE**: Admin can upload materials, they appear in list
5. Deploy/demo if ready - this is a functional MVP!

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy (MVP: Upload works!)
3. Add User Story 2 → Test independently → Deploy (Search/sort/filter works!)
4. Add User Story 3 → Test independently → Deploy (Edit works!)
5. Add User Story 4 → Test independently → Deploy (Delete works!)
6. Each story adds value without breaking previous stories

### Estimated Task Summary

| Phase | Story | Task Count | Key Deliverable |
|-------|-------|------------|-----------------|
| Phase 1 | Setup | 4 | Base types defined |
| Phase 2 | Foundational | 3 | Page skeleton + list endpoint |
| Phase 3 | US1 (P1) | 8 | Upload functionality |
| Phase 4 | US2 (P2) | 8 | List view with search/sort |
| Phase 5 | US3 (P3) | 7 | Edit functionality |
| Phase 6 | US4 (P3) | 4 | Delete functionality |
| Phase 7 | Polish | 7 | Final touches |
| **Total** | | **41** | Complete feature |

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Backend endpoints use existing AdminOnly authorization policy
- Frontend uses existing shadcn-svelte components (Button, Card, Table, Select, Input, Alert)
- No database migrations needed - Material entity already has all required fields
- Soft delete (IsActive=false) preserves access logs per spec requirement
