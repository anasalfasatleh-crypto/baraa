# Quickstart Guide: Research Data Collection Platform

**Branch**: `001-research-data-platform` | **Date**: 2026-01-10

## Prerequisites

- **Node.js** 20+ (for frontend)
- **.NET SDK** 9.0+ (for backend)
- **Docker** & **Docker Compose** (for local services)
- **PostgreSQL** client (optional, for direct DB access)

## Quick Start (Docker Compose)

The fastest way to run the entire stack locally:

```bash
# Clone and enter repository
cd baraa_project

# Start all services (PostgreSQL, MinIO, Backend, Frontend)
docker-compose up -d

# View logs
docker-compose logs -f

# Access the application
# Frontend: http://localhost:5173
# Backend API: http://localhost:5000/api/v1
# MinIO Console: http://localhost:9001 (admin/admin123)
```

## Manual Setup

### 1. Database Setup

```bash
# Start PostgreSQL via Docker
docker run -d \
  --name research-postgres \
  -e POSTGRES_USER=research \
  -e POSTGRES_PASSWORD=research_dev \
  -e POSTGRES_DB=research_platform \
  -p 5432:5432 \
  postgres:16

# Verify connection
psql -h localhost -U research -d research_platform
```

### 2. Object Storage Setup (MinIO)

```bash
# Start MinIO for local S3-compatible storage
docker run -d \
  --name research-minio \
  -e MINIO_ROOT_USER=minioadmin \
  -e MINIO_ROOT_PASSWORD=minioadmin \
  -p 9000:9000 \
  -p 9001:9001 \
  minio/minio server /data --console-address ":9001"

# Create bucket via MinIO Console at http://localhost:9001
# Or via CLI:
docker exec research-minio mc alias set local http://localhost:9000 minioadmin minioadmin
docker exec research-minio mc mb local/research-materials
```

### 3. Backend Setup

```bash
cd backend

# Restore packages
dotnet restore

# Update appsettings.Development.json with connection strings:
# - PostgreSQL: Host=localhost;Database=research_platform;Username=research;Password=research_dev
# - MinIO: Endpoint=localhost:9000;AccessKey=minioadmin;SecretKey=minioadmin;Bucket=research-materials

# Run database migrations
dotnet ef database update --project src/ResearchPlatform.Api

# Start backend
dotnet run --project src/ResearchPlatform.Api

# API available at http://localhost:5000
# Swagger UI at http://localhost:5000/swagger
```

### 4. Frontend Setup

```bash
cd frontend

# Install dependencies
npm install

# Configure API endpoint in .env:
echo "PUBLIC_API_URL=http://localhost:5000/api/v1" > .env

# Start development server
npm run dev

# Frontend available at http://localhost:5173
```

## Environment Variables

### Backend (`appsettings.Development.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=research_platform;Username=research;Password=research_dev"
  },
  "Jwt": {
    "Secret": "your-256-bit-secret-key-for-development-only",
    "Issuer": "ResearchPlatform",
    "Audience": "ResearchPlatform",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "Storage": {
    "Endpoint": "localhost:9000",
    "AccessKey": "minioadmin",
    "SecretKey": "minioadmin",
    "BucketName": "research-materials",
    "UseSSL": false
  }
}
```

### Frontend (`.env`)

```bash
PUBLIC_API_URL=http://localhost:5000/api/v1
```

## Seed Data

The database is automatically seeded with demo data when running in Development mode on first startup.

**Demo Account Credentials**:
- **Admin**: `admin@research.edu` / `admin123`
- **Evaluator 1**: `evaluator1@research.edu` / `eval123` (Dr. Sarah Johnson)
- **Evaluator 2**: `evaluator2@research.edu` / `eval123` (Dr. Michael Chen)
- **Student 1**: `student1@nursing.edu` / `student123` (Emily Rodriguez)
- **Student 2**: `student2@nursing.edu` / `student123` (James Wilson)

The seed data also includes:
- A pre-test questionnaire with 4 sample questions
- An empty post-test questionnaire (inactive by default)
- Evaluator assignments (Evaluator 1 → Student 1, Evaluator 2 → Student 2)

## Running Tests

### Backend Tests

```bash
cd backend

# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test category
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
```

### Frontend Tests

```bash
cd frontend

# Run unit tests
npm run test

# Run with coverage
npm run test:coverage

# Run E2E tests (requires backend running)
npm run test:e2e
```

## Common Development Tasks

### Add New Database Migration

```bash
cd backend
dotnet ef migrations add <MigrationName> --project src/ResearchPlatform.Api
dotnet ef database update --project src/ResearchPlatform.Api
```

### Generate API Client (Frontend)

```bash
cd frontend
# After updating OpenAPI spec
npx openapi-typescript ../specs/001-research-data-platform/contracts/openapi.yaml -o src/lib/api/types.ts
```

### Reset Database

```bash
# Drop and recreate
docker exec research-postgres psql -U research -c "DROP DATABASE research_platform;"
docker exec research-postgres psql -U research -c "CREATE DATABASE research_platform;"

# Re-run migrations
cd backend
dotnet ef database update --project src/ResearchPlatform.Api
```

## Debugging

### Backend Logs

```bash
# Docker logs
docker-compose logs -f backend

# Or in development
cd backend && dotnet run --project src/ResearchPlatform.Api
# Logs output to console
```

### Database Queries

```bash
# Connect to PostgreSQL
docker exec -it research-postgres psql -U research -d research_platform

# View recent answers
SELECT * FROM answers ORDER BY saved_at DESC LIMIT 10;

# Check user status
SELECT id, email, role, status FROM users;
```

### API Testing

```bash
# Login and get token
TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@research.edu","password":"admin123"}' \
  | jq -r '.accessToken')

# Make authenticated request
curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:5000/api/v1/admin/dashboard
```

## Deployment

See `docker/` directory for production deployment configuration:

```bash
# Build production images
docker build -t research-backend -f docker/Dockerfile.backend backend/
docker build -t research-frontend -f docker/Dockerfile.frontend frontend/

# Deploy with docker-compose
docker-compose -f docker/docker-compose.prod.yml up -d
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Port 5432 in use | Stop local PostgreSQL or change port in docker-compose |
| CORS errors | Ensure frontend URL in backend CORS config |
| JWT expired | Refresh token endpoint: POST /api/v1/auth/refresh |
| File upload fails | Check MinIO bucket exists and permissions |
| Migrations fail | Ensure PostgreSQL is running and connection string correct |

## Related Documentation

- [spec.md](./spec.md) - Feature specification
- [research.md](./research.md) - Technology decisions
- [data-model.md](./data-model.md) - Database schema
- [contracts/openapi.yaml](./contracts/openapi.yaml) - API specification
