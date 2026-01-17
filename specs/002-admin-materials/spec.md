# Feature Specification: Admin Materials Management

**Feature Branch**: `002-admin-materials`
**Created**: 2026-01-10
**Status**: Draft
**Input**: User description: "Implement admin materials management feature for uploading and managing study materials"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Upload New Study Material (Priority: P1)

As an admin, I need to upload study materials (PDFs, videos, or text documents) to the platform so that students can access them for their research study participation.

**Why this priority**: This is the core functionality - without the ability to upload materials, admins cannot provide content to students. This is the foundation that all other admin material features depend on.

**Independent Test**: Can be fully tested by logging in as admin, navigating to materials management, selecting a file, providing title/description, and confirming the file appears in the materials list. Delivers immediate value by enabling content distribution.

**Acceptance Scenarios**:

1. **Given** I am logged in as an admin, **When** I upload a PDF file with title "Research Protocol" and description "Study guidelines", **Then** the material is saved to MinIO storage and appears in the materials list with correct metadata
2. **Given** I am uploading a video file, **When** the file is larger than 100MB, **Then** I see a progress indicator showing upload percentage
3. **Given** I upload a material, **When** the upload completes successfully, **Then** I see a success message and the material appears immediately in the list
4. **Given** I attempt to upload a file without providing a title, **When** I click submit, **Then** I see a validation error requiring a title

---

### User Story 2 - View and Manage Materials List (Priority: P2)

As an admin, I need to view all uploaded materials in a searchable, sortable table so that I can quickly find and manage existing content.

**Why this priority**: Once materials can be uploaded, admins need to see what's available. This provides visibility and is essential for ongoing management, but the system can function with just upload capability.

**Independent Test**: Can be tested by navigating to admin materials page and verifying all previously uploaded materials appear with correct details (title, type, size, access count). Delivers value by providing content inventory visibility.

**Acceptance Scenarios**:

1. **Given** there are 50 materials in the system, **When** I navigate to admin materials page, **Then** I see a table showing all materials with title, type, file size, and access count
2. **Given** I am viewing the materials list, **When** I type "Protocol" in the search box, **Then** only materials with "Protocol" in the title or description are displayed
3. **Given** I am viewing the materials list, **When** I click the "Type" column header, **Then** materials are sorted alphabetically by type (Pdf, Text, Video)
4. **Given** I am viewing a material in the list, **When** I click the "View" button, **Then** I see the material detail page with full metadata and access statistics

---

### User Story 3 - Update Material Information (Priority: P3)

As an admin, I need to edit material titles, descriptions, and types so that I can correct errors or improve content organization without re-uploading files.

**Why this priority**: This is a quality-of-life improvement. Admins can work around missing edit functionality by deleting and re-uploading, but editing is more efficient. Lower priority than core upload/view features.

**Independent Test**: Can be tested by opening an existing material's edit form, changing the title/description, saving, and verifying changes persist. Delivers value by reducing administrative overhead.

**Acceptance Scenarios**:

1. **Given** I am viewing a material with title "Old Title", **When** I click "Edit", change the title to "New Title", and save, **Then** the material displays "New Title" in the materials list
2. **Given** I am editing a material, **When** I clear the title field and attempt to save, **Then** I see a validation error preventing the save
3. **Given** I am editing a material, **When** I change the type from "Pdf" to "Text", **Then** the type is updated and reflected in the materials list

---

### User Story 4 - Delete Materials (Priority: P3)

As an admin, I need to delete materials that are no longer needed so that students don't see outdated or incorrect content.

**Why this priority**: Deletion is important for content curation but not essential for initial launch. Admins can manually track which materials to ignore if deletion isn't available. Lower priority than creation and viewing.

**Independent Test**: Can be tested by clicking delete on a material, confirming the action, and verifying it no longer appears in the materials list or student view. Delivers value by enabling content lifecycle management.

**Acceptance Scenarios**:

1. **Given** I am viewing a material in the list, **When** I click "Delete" and confirm the action, **Then** the material is removed from the database and MinIO storage
2. **Given** I click "Delete" on a material, **When** the confirmation dialog appears, **Then** I can click "Cancel" to abort the deletion without changes
3. **Given** I delete a material that has been accessed by students, **When** the deletion completes, **Then** student access logs are preserved for research data integrity

---

### Edge Cases

- What happens when an admin uploads a file with the same name as an existing material?
  - System should allow it - each material has a unique ID, filename conflicts are handled by MinIO with unique object keys
- How does the system handle uploads that fail mid-transfer?
  - Display error message, remove partial upload from MinIO, allow retry
- What happens when an admin tries to delete a material that is currently being viewed by a student?
  - Deletion should succeed - student viewing uses signed URL that remains valid until expiration
- How does the system handle very large files (500MB+)?
  - Display upload progress, use chunked upload to MinIO, set reasonable file size limit (e.g., 1GB) with clear error message
- What happens when MinIO is unavailable during upload?
  - Display connection error message, suggest retry, do not create database record without successful storage
- How does the system handle file types that aren't PDF, video, or text?
  - Display validation error listing supported types, reject upload
- What happens when an admin edits a material while another admin is viewing it?
  - Last write wins - no optimistic locking required for MVP, consider adding version tracking in future

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide an admin interface at `/admin/materials` for managing study materials
- **FR-002**: System MUST allow admins to upload files with types: PDF (.pdf), Video (.mp4, .avi, .mov, .wmv), and Text (.txt, .doc, .docx)
- **FR-003**: System MUST require title (max 200 chars) and optionally accept description (max 1000 chars) when uploading materials
- **FR-004**: System MUST store uploaded files in MinIO object storage with unique keys to prevent naming conflicts
- **FR-005**: System MUST persist material metadata in PostgreSQL including: id, title, description, type, fileExtension, fileSizeBytes, createdAt, updatedAt
- **FR-006**: System MUST display a materials list table showing: title, type, file size, upload date, and access count
- **FR-007**: System MUST provide search functionality filtering materials by title or description (case-insensitive)
- **FR-008**: System MUST allow sorting the materials table by: title, type, file size, upload date, and access count
- **FR-009**: System MUST show upload progress for files larger than 10MB
- **FR-010**: System MUST validate file types on both client-side (immediate feedback) and server-side (security)
- **FR-011**: System MUST allow admins to edit material title, description, and type without re-uploading the file
- **FR-012**: System MUST allow admins to delete materials, removing both database record and MinIO object
- **FR-013**: System MUST preserve student access logs when materials are deleted (soft delete for analytics)
- **FR-014**: System MUST restrict materials management to users with Admin role only
- **FR-015**: System MUST generate success/error notifications for upload, update, and delete operations

### Key Entities

- **Material**: Represents a study material (PDF, video, or text document) available to students
  - Attributes: id (UUID), title (string), description (nullable string), type (enum: Pdf/Video/Text), fileExtension (string), fileSizeBytes (integer), createdAt (datetime), updatedAt (datetime)
  - Relationships: Has many MaterialAccessLogs (one-to-many)
  - Storage: Metadata in PostgreSQL, file content in MinIO with key pattern `materials/{id}.{extension}`

- **MaterialAccessLog**: Tracks when students view or interact with materials (existing entity)
  - Attributes: id, materialId (FK), studentId (FK), accessedAt, durationSeconds, completed
  - Note: Already exists in system for student tracking, preserved when materials deleted

- **Admin User**: User with Admin role authorized to manage materials
  - Leverages existing User entity with role='Admin'
  - No new entity needed

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Admins can upload a material (with title, description, file) and have it appear in student materials list within 5 seconds
- **SC-002**: Materials list loads and displays 100+ materials in under 2 seconds
- **SC-003**: Upload progress indicator updates at minimum every 10% for files >10MB
- **SC-004**: Search/filter functionality returns results within 500ms for datasets up to 1000 materials
- **SC-005**: 100% of uploaded files are recoverable from MinIO and accessible via signed URLs
- **SC-006**: Zero data loss during delete operations - either both database and MinIO deletion succeed, or both fail (transactional integrity)
- **SC-007**: Material edit operations complete within 1 second for metadata-only changes
- **SC-008**: System correctly rejects 100% of non-supported file types with clear error messages

## Assumptions

- MinIO is configured and accessible at the endpoint specified in backend configuration
- MinIO bucket `research-materials` already exists (created during initial setup)
- Backend API has existing authentication middleware to verify Admin role
- Frontend has existing file upload UI components or can use shadcn-svelte form components
- File size limit of 1GB is acceptable for research materials (configurable if needed)
- Admins access the platform from desktop browsers (Chrome, Firefox, Edge) - mobile admin UI not required for MVP

## Out of Scope

- **Version control for materials**: Editing replaces material, no version history tracked
- **Material categories/tags**: All materials in single flat list, no taxonomies
- **Batch upload**: Upload one file at a time only
- **Material preview/thumbnails**: No preview generation for videos or PDFs
- **Access permissions per material**: All materials visible to all students, no individual restrictions
- **Material scheduling**: No publish/unpublish dates, materials immediately available after upload
- **Material analytics dashboard**: Access count displayed but no detailed analytics graphs
- **Content moderation workflow**: No approval process, admins can publish directly
- **File conversion**: Uploaded files stored as-is, no format conversion (e.g., DOC to PDF)
- **Material duplication detection**: No automatic detection of duplicate content

## Technical Notes

- Reuse existing `materialsApi` client in `frontend/src/lib/api/materials.ts`, add admin endpoints
- Backend endpoints should follow pattern: `/api/v1/admin/materials` (parallel to existing `/api/v1/student/materials`)
- MinIO signed URLs should use same expiration time as student material access (currently configured in backend)
- Consider using `@uppy/core` library for advanced upload progress if native implementation is insufficient
- Material type enum should match existing backend model: 'Pdf' | 'Video' | 'Text'
