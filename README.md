# Research Data Collection Platform

ICU Delirium Psychoeducational Study - A full-stack web application for managing research data collection, student assessments, educational materials, and evaluator scoring.

## Tech Stack

### Backend
- **.NET 9** - Web API with ASP.NET Core
- **Entity Framework Core 9** - ORM with PostgreSQL provider
- **PostgreSQL 16** - Primary database
- **MinIO** - S3-compatible object storage for educational materials
- **JWT Authentication** - Secure token-based auth with refresh tokens
- **BCrypt** - Password hashing
- **Swagger/OpenAPI** - API documentation

### Frontend
- **SvelteKit** - Full-stack web framework
- **TypeScript** - Type-safe JavaScript
- **TailwindCSS v4** - Utility-first CSS (CSS-first configuration)
- **Chart.js** - Data visualization
- **Svelte 5 Runes** - Modern reactive programming

### DevOps
- **Docker Compose** - Multi-container orchestration
- **Nginx** - Reverse proxy and load balancer
- **Git** - Version control

## Project Structure

```
baraa_project/
├── backend/
│   ├── src/
│   │   └── ResearchPlatform.Api/
│   │       ├── Auth/              # JWT authentication & authorization
│   │       ├── Controllers/       # API endpoints
│   │       ├── Data/              # EF Core DbContext & migrations
│   │       ├── DTOs/              # Data transfer objects
│   │       ├── Middleware/        # Global exception handling
│   │       ├── Models/            # Entity models & enums
│   │       ├── Services/          # Business logic & audit logging
│   │       └── Validators/        # FluentValidation validators
│   └── ResearchPlatform.sln
├── frontend/
│   └── src/
│       ├── lib/
│       │   ├── api/               # API client & auth functions
│       │   ├── components/        # Reusable Svelte components
│       │   └── stores/            # Svelte stores (auth state)
│       └── routes/                # SvelteKit file-based routing
│           ├── login/             # Login page
│           ├── student/           # Student dashboard
│           ├── evaluator/         # Evaluator dashboard
│           └── admin/             # Admin dashboard
├── docker/
│   ├── docker-compose.yml         # Service orchestration
│   ├── Dockerfile.backend         # Backend container
│   ├── Dockerfile.frontend        # Frontend container
│   └── nginx.conf                 # Reverse proxy config
└── specs/
    └── 001-research-data-platform/
        ├── spec.md                # Feature specification
        ├── plan.md                # Implementation plan
        ├── tasks.md               # Task breakdown (166 tasks)
        └── data-model.md          # Database schema
```

## Features Implemented - ALL PHASES COMPLETE ✅

### ✅ Phase 1-2: Setup & Authentication
- .NET 9 backend with Entity Framework Core 9
- SvelteKit frontend with TailwindCSS v4 and Svelte 5 Runes
- JWT authentication with refresh tokens
- Role-based authorization (Admin, Evaluator, Student)
- PostgreSQL database with complete schema
- MinIO object storage integration

### ✅ Phase 3: Student Pre-Test Assessment
- Multi-step questionnaire system
- Support for multiple question types (Likert, True/False, Multiple Choice, Dropdown, Text)
- Auto-save functionality
- Progress tracking
- Completion validation

### ✅ Phase 4: Educational Materials
- File upload and download
- Material access tracking
- MinIO S3-compatible storage
- Material categorization
- Access control by student

### ✅ Phase 5: Student Post-Test
- Post-test questionnaire completion
- Completion status tracking
- Same multi-question type support as pre-test

### ✅ Phase 6: Evaluator Scoring
- Manual scoring interface for evaluators
- Batch management (pre/post test pairs)
- Student assignment system
- Score calculation and aggregation
- Completion tracking

### ✅ Phase 7: Administrator Management
- User CRUD (Create, Read, Update, Delete)
- CSV import for bulk user creation
- Post-test batch creation
- Evaluator assignment management
- Dashboard with statistics
- Comprehensive data views

### ✅ Phase 8: Data Export
- Excel export with 418+ columns
- CSV export option
- Complete research data aggregation
- SPSS-ready format
- Demographics, answers, scores, and timing data

### ✅ Phase 9: Questionnaire Builder
- Dynamic questionnaire creation and editing
- Multi-step question management
- Real-time question reordering
- Question type customization
- Options management for multiple choice
- Likert scale configuration

### ✅ Phase 10: Polish & Security
- FluentValidation for all inputs
- Rate limiting middleware (100 req/min)
- Request logging and monitoring
- Database auto-seeding for development
- Swagger/OpenAPI documentation
- Comprehensive security review
- Production deployment guides

## Quick Start

### 🚀 Fastest Way: Docker Compose (Recommended)

```bash
# Clone repository
git clone <repository-url>
cd baraa_project

# Start all services
cd docker
docker-compose up -d

# View logs
docker-compose logs -f
```

**Access the application:**
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5001/api/v1
- **Swagger UI**: http://localhost:5001/swagger
- **MinIO Console**: http://localhost:19001 (minioadmin/minioadmin)
- **PostgreSQL**: localhost:54320

**Demo Credentials** (auto-seeded in development):
- **Admin**: `admin@research.edu` / `admin123`
- **Evaluator**: `evaluator1@research.edu` / `eval123`
- **Student**: `student1@nursing.edu` / `student123`

### 📖 Detailed Guides

- **[Quickstart Guide](specs/001-research-data-platform/quickstart.md)** - Local development setup
- **[Deployment Guide](DEPLOYMENT.md)** - Production deployment
- **[Security Documentation](SECURITY.md)** - Security review and compliance

### Manual Setup (Alternative)

See the [Quickstart Guide](specs/001-research-data-platform/quickstart.md) for detailed manual setup instructions including:
- PostgreSQL database setup
- MinIO object storage configuration
- Backend and frontend development servers
- Environment variable configuration

## API Documentation

Full API documentation available at `/swagger` when running in development mode.

### Key Endpoint Groups

**Authentication** (`/api/v1/auth`)
- Login, logout, refresh token, change password

**Student** (`/api/v1/student`)
- Questionnaire access, answer submission, material downloads, progress tracking

**Evaluator** (`/api/v1/evaluator`)
- Assigned student viewing, batch management, scoring submission

**Admin** (`/api/v1/admin`)
- User CRUD, CSV import, batch creation, evaluator assignments
- Dashboard statistics, data export (Excel/CSV)
- Questionnaire builder (create, edit, delete questionnaires)

**Health Check**
- **GET** `/health` - Application health status

## User Roles

1. **Admin** - Full system access, user management, data export
2. **Evaluator** - Score student responses, view assigned students
3. **Student** - Complete assessments, access materials

## Security Features

### Authentication & Authorization
- JWT-based authentication with 15-minute access tokens
- 7-day refresh token expiration (configurable)
- BCrypt password hashing with automatic salt
- Role-based access control (RBAC) with 4 authorization policies
- All protected endpoints secured with `[Authorize]` attributes

### Input Validation & Protection
- FluentValidation on all request DTOs
- SQL injection protection via EF Core parameterized queries
- Email format and length validation
- Password minimum 8 characters

### Rate Limiting & Monitoring
- Rate limiting: 100 requests/minute per user/IP (configurable)
- Request logging with unique request IDs
- Performance timing tracking
- Centralized exception handling

### Network Security
- CORS with restricted origins (no wildcard allowed)
- HTTPS enforcement in production
- Security headers (X-Frame-Options, CSP, etc.)
- Nginx reverse proxy with rate limiting

### Data Security
- File upload security via MinIO S3-compatible storage
- Access control on all materials
- Audit logging for sensitive operations
- Database encryption at rest (PostgreSQL configuration)

See [SECURITY.md](SECURITY.md) for complete security documentation and production hardening checklist.

## Development Status

**Project Status:** ✅ **COMPLETE** - All 10 phases implemented and tested

- ✅ Phase 1: Setup & Infrastructure (11 tasks)
- ✅ Phase 2: Authentication & Authorization (26 tasks)
- ✅ Phase 3: Student Pre-Test Assessment (34 tasks)
- ✅ Phase 4: Educational Materials (16 tasks)
- ✅ Phase 5: Student Post-Test (9 tasks)
- ✅ Phase 6: Evaluator Scoring (16 tasks)
- ✅ Phase 7: Administrator Management (27 tasks)
- ✅ Phase 8: Data Export (6 tasks)
- ✅ Phase 9: Questionnaire Builder (12 tasks)
- ✅ Phase 10: Polish & Cross-Cutting Concerns (8 tasks)

**Total:** 166/166 tasks completed (100%)

## Production Readiness

### ✅ Completed
- Full-stack application with all features
- Comprehensive security implementation
- Database auto-seeding for development
- Docker deployment configuration
- Nginx reverse proxy setup
- Documentation (Quickstart, Deployment, Security)

### ⚠️ Before Production Deployment
See [DEPLOYMENT.md](DEPLOYMENT.md) for the complete production deployment checklist including:
- Change JWT secret to 256-bit cryptographically secure key
- Update database credentials
- Configure MinIO/S3 production credentials
- Set production CORS origins
- Enable HTTPS and security headers
- Configure SSL certificates
- Set up monitoring and logging
- Review compliance requirements

## Commands

### Backend
```bash
# Build
dotnet build

# Run tests
dotnet test

# Create migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Run API
dotnet run
```

### Frontend
```bash
# Install dependencies
npm install

# Development server
npm run dev

# Type check
npm run check

# Build for production
npm run build

# Preview production build
npm run preview
```

## Configuration

### Backend Environment Variables
- `ConnectionStrings__DefaultConnection` - PostgreSQL connection
- `Jwt__Secret` - JWT signing key (256-bit)
- `Storage__Endpoint` - MinIO endpoint
- `Storage__AccessKey` - MinIO access key
- `Storage__SecretKey` - MinIO secret key

### Frontend Environment Variables
- `PUBLIC_API_URL` - Backend API URL

## Contributing

See [tasks.md](specs/001-research-data-platform/tasks.md) for the complete task breakdown and implementation plan.

## License

Proprietary - Research Project
