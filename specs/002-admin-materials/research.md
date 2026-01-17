# Research: Admin Materials Management

**Feature**: 002-admin-materials | **Date**: 2026-01-10

## Overview

This document captures research findings and technical decisions for implementing admin materials management. The key finding is that most infrastructure already exists - the implementation primarily involves creating new API endpoints and frontend pages.

---

## Research Item 1: File Upload Strategy

### Context
Need to handle file uploads up to 1GB for PDFs, videos, and text documents.

### Decision
Use multipart/form-data with streaming upload to MinIO via existing StorageService.

### Rationale
- StorageService already implements `UploadFileAsync(stream, objectName, contentType)`
- MinIO SDK handles large files efficiently with streaming
- Existing pattern from user CSV import in AdminController shows FormData handling

### Alternatives Considered
1. **Base64 encoding in JSON**: Rejected - doubles file size, not suitable for large files
2. **Chunked upload with client-side splitting**: Rejected - adds unnecessary complexity, MinIO handles large files well
3. **Pre-signed PUT URLs for direct client-to-MinIO upload**: Considered for future - more complex but better for very large files

### Implementation Notes
```csharp
// Pattern from existing codebase (AdminController.cs ImportUsers)
[HttpPost("materials/upload")]
[Consumes("multipart/form-data")]
public async Task<ActionResult<MaterialDto>> UploadMaterial(
    [FromForm] string title,
    [FromForm] string? description,
    [FromForm] string type,
    IFormFile file)
{
    using var stream = file.OpenReadStream();
    var material = await _materialService.CreateMaterialAsync(
        title, description, Enum.Parse<MaterialType>(type),
        stream, file.FileName, file.ContentType);
    return Ok(MapToDto(material));
}
```

---

## Research Item 2: File Type Validation

### Context
Need to validate file types both client-side (UX) and server-side (security).

### Decision
Use file extension checking combined with content-type validation.

### Rationale
- Simple and effective for the expected use case
- Extension mapping already available in file.FileName
- Content-type provided by browser in IFormFile

### Supported Types
| Extension | Content-Type | MaterialType |
|-----------|-------------|--------------|
| .pdf | application/pdf | Pdf |
| .mp4 | video/mp4 | Video |
| .avi | video/x-msvideo | Video |
| .mov | video/quicktime | Video |
| .wmv | video/x-ms-wmv | Video |
| .txt | text/plain | Text |
| .doc | application/msword | Text |
| .docx | application/vnd.openxmlformats-officedocument.wordprocessingml.document | Text |

### Implementation Notes
```csharp
// FluentValidation rule
public class UploadMaterialRequestValidator : AbstractValidator<UploadMaterialRequest>
{
    private static readonly string[] AllowedExtensions =
        { ".pdf", ".mp4", ".avi", ".mov", ".wmv", ".txt", ".doc", ".docx" };

    public UploadMaterialRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.File)
            .NotNull()
            .Must(f => f.Length <= 1_073_741_824) // 1GB
            .WithMessage("File size must not exceed 1GB")
            .Must(f => AllowedExtensions.Contains(
                Path.GetExtension(f.FileName).ToLowerInvariant()))
            .WithMessage("File type not supported");
    }
}
```

---

## Research Item 3: Frontend Upload Progress

### Context
Need to show upload progress for large files (>10MB).

### Decision
Use native XMLHttpRequest with progress events.

### Rationale
- fetch() API doesn't support upload progress
- XMLHttpRequest provides `upload.onprogress` event
- Simpler than adding third-party library like @uppy/core

### Alternatives Considered
1. **@uppy/core library**: Rejected for MVP - adds dependency, overkill for single file upload
2. **Server-Sent Events**: Not applicable - need upload progress, not download
3. **WebSocket progress**: Rejected - unnecessary complexity

### Implementation Notes
```typescript
// Upload with progress tracking
async function uploadMaterial(
    file: File,
    title: string,
    description: string | undefined,
    type: string,
    onProgress?: (percent: number) => void
): Promise<MaterialDto> {
    return new Promise((resolve, reject) => {
        const xhr = new XMLHttpRequest();
        const formData = new FormData();

        formData.append('file', file);
        formData.append('title', title);
        formData.append('type', type);
        if (description) formData.append('description', description);

        xhr.upload.onprogress = (e) => {
            if (e.lengthComputable && onProgress) {
                onProgress(Math.round((e.loaded / e.total) * 100));
            }
        };

        xhr.onload = () => {
            if (xhr.status >= 200 && xhr.status < 300) {
                resolve(JSON.parse(xhr.responseText));
            } else {
                reject(new Error(xhr.responseText || 'Upload failed'));
            }
        };

        xhr.onerror = () => reject(new Error('Network error'));

        xhr.open('POST', `${API_URL}/admin/materials/upload`);
        xhr.setRequestHeader('Authorization', `Bearer ${getToken()}`);
        xhr.send(formData);
    });
}
```

---

## Research Item 4: List Performance

### Context
Need materials list to load in <2 seconds with 100+ items.

### Decision
Use server-side pagination with client-side search/filter.

### Rationale
- 100-1000 materials is manageable without pagination
- Single query with ordering is efficient with proper indexes
- Existing index on Material.OrderIndex supports efficient ordering
- Client-side filtering provides instant response for search

### Implementation Notes
- Backend returns all active materials in single query (existing `GetActiveMaterialsAsync`)
- Frontend filters and sorts in memory for instant response
- Consider adding pagination if materials exceed 1000

---

## Research Item 5: Admin Materials API Pattern

### Context
Need to decide API endpoint structure for admin materials.

### Decision
Add endpoints to existing AdminController following established patterns.

### Rationale
- Keeps all admin endpoints in one controller
- Reuses existing authorization (AdminOnly policy)
- Follows REST conventions from existing endpoints
- Avoids creating new controller for small feature set

### Alternatives Considered
1. **Separate MaterialsController**: Rejected - would need separate auth setup, only 5 endpoints
2. **Nested resource under /admin/resources/materials**: Rejected - over-engineering

### API Endpoints
```
GET    /api/v1/admin/materials           # List all materials
GET    /api/v1/admin/materials/{id}      # Get material detail
POST   /api/v1/admin/materials/upload    # Upload new material
PUT    /api/v1/admin/materials/{id}      # Update metadata
DELETE /api/v1/admin/materials/{id}      # Delete material
```

---

## Research Item 6: Deletion Strategy

### Context
Need to handle material deletion with student access log preservation.

### Decision
Hard delete Material, cascade behavior preserves MaterialAccess with null reference.

### Rationale
- Spec requires preserving access logs for research data integrity
- Material entity uses cascade delete to MaterialAccess (configured in DbContext)
- Need to modify: Set MaterialAccess.MaterialId to nullable, update cascade to SetNull

### Alternative
- Soft delete (IsActive=false): Simpler but clutters database, complicates queries

### Implementation Notes
```csharp
// DbContext configuration change
modelBuilder.Entity<MaterialAccess>()
    .HasOne(ma => ma.Material)
    .WithMany(m => m.MaterialAccesses)
    .HasForeignKey(ma => ma.MaterialId)
    .OnDelete(DeleteBehavior.SetNull); // Change from Cascade

// MaterialAccess model change
public Guid? MaterialId { get; set; } // Make nullable
```

**Decision Update**: After reviewing spec requirements again, soft delete (IsActive=false) is cleaner. MaterialService.GetActiveMaterialsAsync already filters by IsActive. Students won't see deleted materials, logs preserved with valid FK.

---

## Research Item 7: Existing Service Coverage

### Context
Verify MaterialService has all needed functionality.

### Findings
MaterialService already provides:
- `CreateMaterialAsync` - handles file upload to MinIO + DB insert
- `UpdateMaterialAsync` - updates metadata only
- `DeleteMaterialAsync` - removes from MinIO + DB
- `GetActiveMaterialsAsync` - returns all active, ordered
- `GetMaterialByIdAsync` - single material by ID
- `GetMaterialSignedUrlAsync` - generates access URL

### Gap Analysis
- Need method to get ALL materials (including inactive) for admin view
- Currently `GetActiveMaterialsAsync` filters by `IsActive = true`

### Resolution
Add new method `GetAllMaterialsAsync()` to MaterialService for admin use.

---

## Decisions Summary

| Decision | Choice | Key Reason |
|----------|--------|------------|
| File upload | Multipart/form-data streaming | Existing pattern, handles large files |
| File validation | Extension + content-type | Simple, effective, secure |
| Upload progress | XMLHttpRequest | Native, no dependencies |
| List strategy | Single query, client filter | Performance sufficient for scale |
| API structure | Extend AdminController | Follows existing patterns |
| Deletion | Soft delete (IsActive) | Preserves FK integrity, simpler |
| Service gaps | Add GetAllMaterialsAsync | Minimal change, admin needs all materials |
