# Implementation Plan: Admin Materials Management

**Branch**: `002-admin-materials` | **Date**: 2026-01-10 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-admin-materials/spec.md`

## Summary

Implement admin materials management feature allowing administrators to upload, view, edit, and delete study materials (PDFs, videos, text documents) for student access. The implementation extends the existing Material entity, MaterialService, and StorageService (MinIO integration) by adding admin-specific API endpoints and a new frontend page at `/admin/materials`.

## Technical Context

**Language/Version**: TypeScript 5.9.3 (Frontend), C# .NET 9.0 (Backend)
**Primary Dependencies**:
- Backend: ASP.NET Core, EF Core 9.x, Npgsql, FluentValidation 11.3.1, Minio 7.0.0
- Frontend: SvelteKit 2.49.1, Svelte 5.45.6, Tailwind CSS 4.1.18, bits-ui 2.15.4, @lucide/svelte
**Storage**: PostgreSQL (metadata), MinIO (file content) - both already configured
**Testing**: No test framework currently configured (tests/ directory empty)
**Target Platform**: Web (Desktop browsers: Chrome, Firefox, Edge)
**Project Type**: Web application (backend + frontend)
**Performance Goals**:
- Upload completion <5 seconds
- Materials list load <2 seconds for 100+ items
- Search results <500ms
**Constraints**:
- File size limit 1GB
- Admin role required for all operations
- Supported formats: PDF, Video (.mp4, .avi, .mov, .wmv), Text (.txt, .doc, .docx)
**Scale/Scope**: Expected 100-1000 materials, single admin managing content

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Note**: Project constitution is using default template - no specific violations to check. Following standard patterns from existing codebase.

**Pre-Design Check**:
- [x] Reusing existing entities (Material, MaterialAccess) - no new data models needed
- [x] Reusing existing services (MaterialService, StorageService, MaterialAccessService)
- [x] Following established controller patterns (AdminController)
- [x] No new dependencies required
- [x] No architectural changes - extending existing patterns

**Post-Design Check**: (Completed after Phase 1)
- [x] API contracts follow existing REST patterns (see contracts/admin-materials-api.yaml)
- [x] Data model uses established conventions (see data-model.md)
- [x] No unnecessary complexity introduced - reusing existing entities and services

## Project Structure

### Documentation (this feature)

```text
specs/002-admin-materials/
├── plan.md              # This file
├── research.md          # Phase 0 output - technical decisions
├── data-model.md        # Phase 1 output - entity documentation
├── quickstart.md        # Phase 1 output - development setup
├── contracts/           # Phase 1 output - API specifications
│   └── admin-materials-api.yaml
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
backend/
├── src/
│   └── ResearchPlatform.Api/
│       ├── Controllers/
│       │   └── AdminController.cs     # ADD: Material endpoints here
│       ├── DTOs/
│       │   └── AdminDtos.cs           # ADD: Material DTOs
│       ├── Validators/
│       │   └── AdminValidators.cs     # ADD: Material validation rules
│       ├── Models/
│       │   └── Material.cs            # EXISTS: No changes needed
│       └── Services/
│           ├── MaterialService.cs     # EXISTS: Has all CRUD methods
│           └── StorageService.cs      # EXISTS: Has file upload/delete
└── tests/                             # Currently empty

frontend/
├── src/
│   ├── routes/
│   │   └── admin/
│   │       └── materials/
│   │           └── +page.svelte       # CREATE: Admin materials page
│   ├── lib/
│   │   ├── api/
│   │   │   └── admin.ts               # ADD: Material API methods
│   │   └── components/
│   │       ├── MaterialTable.svelte   # CREATE: Reusable table component
│   │       └── MaterialForm.svelte    # CREATE: Upload/edit form
│   └── app.css
└── tests/                             # Currently empty
```

**Structure Decision**: Using existing web application structure. Backend follows established controller/service/model pattern. Frontend follows SvelteKit file-based routing with component library. No new projects needed - extending existing codebase.

## Complexity Tracking

> No violations to justify - feature follows existing patterns

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | Feature uses existing architecture | N/A |

## Key Findings from Codebase Analysis

### Backend

1. **Material Entity** already exists at `backend/src/ResearchPlatform.Api/Models/Material.cs`:
   - Fields: Id, Title, Description, Type (enum), StorageKey, FileExtension, FileSizeBytes, OrderIndex, IsActive, CreatedAt, UpdatedAt
   - Relationships: Has many MaterialAccesses

2. **MaterialService** at `backend/src/ResearchPlatform.Api/Services/MaterialService.cs` already has:
   - `CreateMaterialAsync(title, description, type, stream, fileName, contentType)`
   - `UpdateMaterialAsync(id, title, description, type)`
   - `DeleteMaterialAsync(id)` - deletes both DB record and MinIO file
   - `GetActiveMaterialsAsync()` - returns all active materials ordered
   - `GetMaterialByIdAsync(id)` - returns single material
   - `GetMaterialSignedUrlAsync(id)` - generates MinIO signed URL

3. **StorageService** at `backend/src/ResearchPlatform.Api/Services/StorageService.cs`:
   - `UploadFileAsync(stream, objectName, contentType)`
   - `DeleteFileAsync(objectName)`
   - `GetPresignedUrlAsync(objectName, expirySeconds)` - default 1 hour

4. **AdminController** pattern at `backend/src/ResearchPlatform.Api/Controllers/AdminController.cs`:
   - Uses `[Authorize(Policy = AuthorizationPolicies.AdminOnly)]`
   - Injects services via constructor
   - Returns `ActionResult<T>` with appropriate status codes
   - Uses try-catch with InvalidOperationException for business errors

### Frontend

1. **Admin page pattern** at `frontend/src/routes/admin/users/+page.svelte`:
   - Uses Svelte 5 runes (`$state`, `$derived`)
   - Follows Card/Table/Alert component pattern
   - Loading states, error handling, success messages
   - Filters with Select components

2. **API client pattern** at `frontend/src/lib/api/admin.ts`:
   - Uses generic HTTP client from `client.ts`
   - Typed interfaces for requests/responses
   - File uploads use FormData with direct fetch()

3. **Sidebar** already includes Materials link at `/admin/materials` (line 33 of app-sidebar.svelte)

## Implementation Approach

1. **Backend**: Add new endpoints to existing AdminController
2. **Frontend**: Create new page and components following users page pattern
3. **No database migrations**: Material entity already has all required fields
4. **No service changes**: MaterialService already implements all needed operations
