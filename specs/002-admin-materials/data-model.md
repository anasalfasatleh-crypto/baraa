# Data Model: Admin Materials Management

**Feature**: 002-admin-materials | **Date**: 2026-01-10

## Overview

This feature uses existing data models with minimal modifications. The Material and MaterialAccess entities already exist and provide all necessary fields.

---

## Entities

### Material (Existing)

**Location**: `backend/src/ResearchPlatform.Api/Models/Material.cs`

**Purpose**: Represents a study material (PDF, video, or text document) available to students.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | Guid | PK, Auto-generated | Unique identifier |
| Title | string | Required, Max 200 | Display title |
| Description | string? | Optional, Max 1000 | Detailed description |
| Type | MaterialType | Required, Enum | Content type (Pdf, Video, Text) |
| StorageKey | string | Required | MinIO object key |
| FileExtension | string? | Optional | File extension (.pdf, .mp4, etc.) |
| FileSizeBytes | long? | Optional | File size in bytes |
| OrderIndex | int | Default 0 | Display ordering |
| IsActive | bool | Default true | Soft delete flag |
| CreatedAt | DateTime | Auto-set | Creation timestamp |
| UpdatedAt | DateTime | Auto-updated | Last modification timestamp |

**Relationships**:
- Has many `MaterialAccess` (one-to-many)

**Indexes** (defined in ApplicationDbContext):
- `Type` - Filter by content type
- `IsActive` - Filter active/deleted
- `OrderIndex` - Sorting

**No Changes Required**: Entity has all fields needed for admin materials management.

---

### MaterialAccess (Existing)

**Location**: `backend/src/ResearchPlatform.Api/Models/MaterialAccess.cs`

**Purpose**: Tracks when students access materials for analytics and research data.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | Guid | PK, Auto-generated | Unique identifier |
| UserId | Guid | FK, Required | Student who accessed |
| MaterialId | Guid | FK, Required | Material accessed |
| AccessedAt | DateTime | Auto-set | When access started |
| DurationSeconds | int? | Optional | Time spent viewing |
| Completed | bool | Default false | If student completed viewing |

**Relationships**:
- Belongs to `User` (many-to-one)
- Belongs to `Material` (many-to-one)

**Indexes**:
- `(UserId, MaterialId)` - Composite for lookup
- `AccessedAt` - Time-based queries

**No Changes Required**: Access logs are preserved when materials are soft-deleted.

---

### MaterialType (Existing Enum)

**Location**: `backend/src/ResearchPlatform.Api/Models/Enums/MaterialType.cs`

| Value | Description | File Extensions |
|-------|-------------|----------------|
| Pdf | PDF documents | .pdf |
| Video | Video content | .mp4, .avi, .mov, .wmv |
| Text | Text documents | .txt, .doc, .docx |

**No Changes Required**: Enum covers all specified file types.

---

## DTOs (To Be Created)

### AdminMaterialDto

**Purpose**: Response DTO for material list and detail views.

```csharp
public record AdminMaterialDto(
    Guid Id,
    string Title,
    string? Description,
    MaterialType Type,
    string? FileExtension,
    long? FileSizeBytes,
    int OrderIndex,
    bool IsActive,
    int AccessCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
```

### AdminMaterialDetailDto

**Purpose**: Response DTO with signed URL for preview.

```csharp
public record AdminMaterialDetailDto(
    Guid Id,
    string Title,
    string? Description,
    MaterialType Type,
    string? FileExtension,
    long? FileSizeBytes,
    int OrderIndex,
    bool IsActive,
    string SignedUrl,
    int AccessCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
```

### UploadMaterialRequest

**Purpose**: Request DTO for file upload (via multipart/form-data).

```csharp
public record UploadMaterialRequest(
    string Title,
    string? Description,
    MaterialType Type,
    IFormFile File
);
```

### UpdateMaterialRequest

**Purpose**: Request DTO for metadata update.

```csharp
public record UpdateMaterialRequest(
    string Title,
    string? Description,
    MaterialType Type,
    int OrderIndex,
    bool IsActive
);
```

---

## Validation Rules (To Be Created)

### UploadMaterialRequestValidator

```csharp
public class UploadMaterialRequestValidator : AbstractValidator<UploadMaterialRequest>
{
    private static readonly string[] AllowedExtensions =
        { ".pdf", ".mp4", ".avi", ".mov", ".wmv", ".txt", ".doc", ".docx" };

    private const long MaxFileSize = 1_073_741_824; // 1GB

    public UploadMaterialRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid material type");

        RuleFor(x => x.File)
            .NotNull().WithMessage("File is required")
            .Must(f => f != null && f.Length <= MaxFileSize)
            .WithMessage("File size must not exceed 1GB")
            .Must(f => f != null && AllowedExtensions.Contains(
                Path.GetExtension(f.FileName).ToLowerInvariant()))
            .WithMessage($"File type not supported. Allowed: {string.Join(", ", AllowedExtensions)}");
    }
}
```

### UpdateMaterialRequestValidator

```csharp
public class UpdateMaterialRequestValidator : AbstractValidator<UpdateMaterialRequest>
{
    public UpdateMaterialRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid material type");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("Order index must be non-negative");
    }
}
```

---

## Database Changes

**Required Changes**: None

The existing Material entity and database schema already support all required fields:
- Title, Description, Type - content metadata
- StorageKey, FileExtension, FileSizeBytes - file info
- OrderIndex - display ordering
- IsActive - soft delete
- CreatedAt, UpdatedAt - timestamps

**Migrations**: Not needed

---

## MinIO Storage

**Bucket**: `research-materials` (already configured)

**Object Key Pattern**: `materials/{id}.{extension}`

Example keys:
- `materials/a1b2c3d4-e5f6-7890-abcd-ef1234567890.pdf`
- `materials/b2c3d4e5-f6a7-8901-bcde-f23456789012.mp4`

**Signed URL Expiration**: 1 hour (default from StorageService)

---

## Entity Relationship Diagram

```
┌─────────────────────────────────────────────────┐
│                    Material                      │
├─────────────────────────────────────────────────┤
│ PK  Id: Guid                                    │
│     Title: string                               │
│     Description: string?                        │
│     Type: MaterialType (Pdf|Video|Text)         │
│     StorageKey: string                          │
│     FileExtension: string?                      │
│     FileSizeBytes: long?                        │
│     OrderIndex: int                             │
│     IsActive: bool                              │
│     CreatedAt: DateTime                         │
│     UpdatedAt: DateTime                         │
└─────────────────────────────────────────────────┘
                         │
                         │ 1:N
                         ▼
┌─────────────────────────────────────────────────┐
│                 MaterialAccess                   │
├─────────────────────────────────────────────────┤
│ PK  Id: Guid                                    │
│ FK  UserId: Guid ─────────────────► User        │
│ FK  MaterialId: Guid                            │
│     AccessedAt: DateTime                        │
│     DurationSeconds: int?                       │
│     Completed: bool                             │
└─────────────────────────────────────────────────┘
```
