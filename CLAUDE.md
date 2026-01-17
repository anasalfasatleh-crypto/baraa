# baraa_project Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-01-10

## Active Technologies
- TypeScript 5.9.3 (Frontend), C# .NET 9.0 (Backend) (002-admin-materials)
- PostgreSQL (metadata), MinIO (file content) - both already configured (002-admin-materials)
- TypeScript 5.9.3 (Frontend - SvelteKit), C# .NET 9.0 (Backend) + SvelteKit 2.x, ASP.NET Core 9, Entity Framework Core, JWT Authentication (003-participant-registration)
- PostgreSQL (existing ApplicationDbContext) (003-participant-registration)

- TypeScript 5.x (Frontend), C# .NET 9 (Backend) (001-research-data-platform)

## Project Structure

```text
backend/
frontend/
tests/
```

## Commands

npm test; npm run lint

## Code Style

TypeScript 5.x (Frontend), C# .NET 9 (Backend): Follow standard conventions

## Recent Changes
- 003-participant-registration: Added TypeScript 5.9.3 (Frontend - SvelteKit), C# .NET 9.0 (Backend) + SvelteKit 2.x, ASP.NET Core 9, Entity Framework Core, JWT Authentication
- 002-admin-materials: Added TypeScript 5.9.3 (Frontend), C# .NET 9.0 (Backend)

- 001-research-data-platform: Added TypeScript 5.x (Frontend), C# .NET 9 (Backend)

<!-- MANUAL ADDITIONS START -->

## Participant Registration System (003-participant-registration)

### Overview
Separate participant registration and authentication system with auto-generated participant codes.

### Key Features
- **Self-registration**: Participants register with username/email, password, optional phone
- **Auto-generated codes**: Sequential pattern A1→A99→B1→B99→...→Z99→AA1
- **Separate login**: Dedicated participant login at `/participant/login`
- **Admin password reset**: Admins can reset participant passwords, generating temp password

### API Endpoints
- `POST /api/v1/participants/register` - Register new participant
- `POST /api/v1/participants/login` - Participant login
- `POST /api/v1/participants/refresh` - Refresh access token
- `GET /api/v1/participants/me` - Get participant profile
- `POST /api/v1/participants/me/change-password` - Change password
- `POST /api/v1/participants/logout` - Logout

### Admin Endpoints
- `GET /api/v1/admin/participants` - List participants (paginated)
- `GET /api/v1/admin/participants/search?q=` - Search participants
- `GET /api/v1/admin/participants/{id}` - Get participant details
- `POST /api/v1/admin/participants/{id}/reset-password` - Reset password

### Frontend Routes
- `/participant/register` - Registration page
- `/participant/login` - Login page
- `/participant` - Dashboard (shows participant code)
- `/participant/change-password` - Change password page
- `/admin/participants` - Admin participant management

### Security
- 5 failed login attempts = 1 minute lockout
- 24-hour session duration (or browser close)
- Password must change after admin reset
- BCrypt password hashing

<!-- MANUAL ADDITIONS END -->
