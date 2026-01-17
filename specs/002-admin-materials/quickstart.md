# Quickstart: Admin Materials Management

**Feature**: 002-admin-materials | **Date**: 2026-01-10

## Prerequisites

- Docker and Docker Compose installed
- Node.js 18+ (for local frontend development)
- .NET 9 SDK (for local backend development)

## Quick Setup (Docker)

The recommended way to run the project is via Docker Compose:

```bash
# From repository root
cd docker

# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Access the application
# Frontend: http://localhost:3000
# Backend API: http://localhost:5001/api/v1
# MinIO Console: http://localhost:19001
```

### Services

| Service | Port | Description |
|---------|------|-------------|
| nginx | 3000 | Reverse proxy (main entry point) |
| frontend | 5174 | SvelteKit dev server |
| backend | 5001 | ASP.NET Core API |
| postgres | 54320 | PostgreSQL database |
| minio | 19000/19001 | MinIO S3 storage |

### Default Credentials

**Admin Login**:
- Email: `admin@research.edu`
- Password: `admin123`

**MinIO Console**:
- Username: `minioadmin`
- Password: `minioadmin`

**PostgreSQL**:
- Host: localhost:54320
- Database: research_platform
- User: research
- Password: research_dev

---

## Local Development

### Backend

```bash
# Navigate to backend
cd backend/src/ResearchPlatform.Api

# Restore packages
dotnet restore

# Run with hot reload
dotnet watch run

# Or run normally
dotnet run

# API available at http://localhost:5000
```

**Environment Variables** (set in launchSettings.json or environment):
```
ConnectionStrings__DefaultConnection=Host=localhost;Port=54320;Database=research_platform;Username=research;Password=research_dev
Storage__Endpoint=localhost:19000
Storage__AccessKey=minioadmin
Storage__SecretKey=minioadmin
Storage__BucketName=research-materials
Storage__UseSSL=false
```

### Frontend

```bash
# Navigate to frontend
cd frontend

# Install dependencies
npm install

# Run dev server
npm run dev

# Available at http://localhost:5173
```

**Environment Variables** (.env file):
```
PUBLIC_API_URL=http://localhost:5000/api/v1
```

---

## Feature Development

### File Locations

**Backend files to modify**:
```
backend/src/ResearchPlatform.Api/
├── Controllers/AdminController.cs      # Add material endpoints
├── DTOs/AdminDtos.cs                   # Add material DTOs
└── Validators/AdminValidators.cs       # Add validation rules
```

**Frontend files to create**:
```
frontend/src/
├── routes/admin/materials/
│   └── +page.svelte                    # Main materials page
├── lib/api/admin.ts                    # Add API methods
└── lib/components/
    ├── MaterialTable.svelte            # Table component
    └── MaterialForm.svelte             # Upload/edit form
```

### Testing the Feature

1. **Login as admin**: http://localhost:3000/login
   - Email: admin@research.edu
   - Password: admin123

2. **Navigate to Materials**: Click "Materials" in sidebar

3. **Upload a test file**:
   - Click "Upload Material"
   - Fill in title and description
   - Select file type
   - Choose a file (PDF, video, or text)
   - Click "Upload"

4. **Verify in MinIO**:
   - Go to http://localhost:19001
   - Login with minioadmin/minioadmin
   - Browse `research-materials` bucket
   - Verify file was uploaded

5. **Test student view**:
   - Logout and login as student
   - Navigate to Materials
   - Verify uploaded material appears

---

## API Testing

### Get Auth Token

```bash
# Login as admin
curl -X POST http://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@research.edu","password":"admin123"}'

# Response: { "accessToken": "eyJ...", "refreshToken": "...", "user": {...} }
```

### Material Endpoints

```bash
# Set token
TOKEN="eyJ..."

# List materials
curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:5001/api/v1/admin/materials

# Upload material
curl -X POST http://localhost:5001/api/v1/admin/materials/upload \
  -H "Authorization: Bearer $TOKEN" \
  -F "title=Research Protocol" \
  -F "description=Study guidelines" \
  -F "type=Pdf" \
  -F "file=@document.pdf"

# Get material detail
curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:5001/api/v1/admin/materials/{id}

# Update material
curl -X PUT http://localhost:5001/api/v1/admin/materials/{id} \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"Updated Title","type":"Pdf","orderIndex":0,"isActive":true}'

# Delete material (soft delete)
curl -X DELETE http://localhost:5001/api/v1/admin/materials/{id} \
  -H "Authorization: Bearer $TOKEN"
```

---

## Troubleshooting

### Common Issues

**1. 500 Internal Error on frontend**
- Check browser console for specific error
- Verify import patterns use namespace syntax:
  ```typescript
  import * as Button from '$lib/components/ui/button';
  ```

**2. MinIO connection failed**
- Ensure MinIO container is running: `docker-compose ps`
- Check MinIO health: `curl http://localhost:19000/minio/health/live`
- Verify bucket exists in MinIO console

**3. File upload fails**
- Check file size (<1GB limit)
- Verify file extension is supported
- Check nginx `client_max_body_size` (currently 10M, may need increase for large files)

**4. 403 Forbidden on admin endpoints**
- Verify logged in as Admin role
- Check JWT token is valid and not expired
- Ensure Authorization header is set correctly

### Rebuild Docker

```bash
cd docker

# Stop and remove containers
docker-compose down

# Rebuild with no cache
docker-compose build --no-cache

# Start fresh
docker-compose up -d

# Check logs
docker-compose logs --tail=50
```

### Database Reset

```bash
# Remove postgres volume (WARNING: deletes all data)
docker-compose down -v

# Restart (will recreate database with seed data)
docker-compose up -d
```

---

## Related Documentation

- [Feature Specification](./spec.md)
- [Implementation Plan](./plan.md)
- [Research Decisions](./research.md)
- [Data Model](./data-model.md)
- [API Contracts](./contracts/admin-materials-api.yaml)
