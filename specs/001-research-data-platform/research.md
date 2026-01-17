# Research: Research Data Collection Platform

**Branch**: `001-research-data-platform` | **Date**: 2026-01-10

## Overview

Technology decisions and best practices research for building the research data collection platform.

---

## Technology Stack Decisions

### Decision 1: Frontend Framework - SvelteKit

**Decision**: Use SvelteKit with TypeScript for the frontend application

**Rationale**:
- Lightweight bundle size ideal for mobile-first design on potentially slow hospital networks
- Built-in routing matches the multi-page application structure (student, evaluator, admin sections)
- Server-side rendering (SSR) improves initial load performance
- Excellent TypeScript support for type safety
- Active ecosystem with shadcn-svelte for consistent UI components

**Alternatives Considered**:
| Alternative | Reason Rejected |
|-------------|-----------------|
| Next.js (React) | Larger bundle size, more complex for this scale |
| Vue/Nuxt | Less mature component library ecosystem |
| Plain HTML/JS | Insufficient for complex questionnaire state management |

---

### Decision 2: Backend Framework - .NET 9 Web API

**Decision**: Use ASP.NET Core 9 with Entity Framework Core for the backend API

**Rationale**:
- Excellent performance for API workloads
- Entity Framework Core provides robust ORM with PostgreSQL support
- Built-in dependency injection simplifies testing
- Strong typing reduces runtime errors
- JWT authentication built into the framework
- Well-documented patterns for role-based access control

**Alternatives Considered**:
| Alternative | Reason Rejected |
|-------------|-----------------|
| Node.js/Express | Less structured, TypeScript ORM options less mature |
| Python/FastAPI | Team expertise favors .NET |
| Go | Overkill for this scale; less rapid development |

---

### Decision 3: Database - PostgreSQL

**Decision**: Use PostgreSQL 16 as the primary relational database

**Rationale**:
- JSON column support for flexible questionnaire options storage
- Strong data integrity with foreign keys and constraints
- Excellent performance for the expected scale (200 users, thousands of answers)
- Open source with no licensing costs
- Mature tooling for backups and migrations

**Alternatives Considered**:
| Alternative | Reason Rejected |
|-------------|-----------------|
| SQL Server | Licensing costs for small deployment |
| MySQL | Less robust JSON support |
| MongoDB | Relational model better fits structured questionnaire data |

---

### Decision 4: File Storage - S3-Compatible Object Storage

**Decision**: Use S3-compatible storage (DigitalOcean Spaces or MinIO) for PDFs and videos

**Rationale**:
- Separates large binary files from database
- Cost-effective for video storage (up to 500MB per file)
- Pre-signed URLs enable secure direct downloads
- Easy backup and CDN integration
- MinIO provides local development parity

**Alternatives Considered**:
| Alternative | Reason Rejected |
|-------------|-----------------|
| Database BLOBs | Poor performance for large files, bloats backups |
| Local filesystem | Not scalable, complicates deployment |
| Direct cloud provider SDK | S3 API is universal standard |

---

### Decision 5: Authentication - JWT with Refresh Tokens

**Decision**: Implement JWT-based authentication with short-lived access tokens and refresh tokens

**Rationale**:
- Stateless authentication reduces server memory
- Access tokens (15 min) + refresh tokens (7 days) balance security and UX
- HttpOnly cookies for refresh tokens prevent XSS theft
- Standard pattern well-supported by .NET and SvelteKit
- Supports role claims for authorization

**Alternatives Considered**:
| Alternative | Reason Rejected |
|-------------|-----------------|
| Session cookies | Requires server-side session storage |
| OAuth2/OIDC | Overkill; no external identity provider needed |
| API keys | Not suitable for user authentication |

---

### Decision 6: UI Component Library - shadcn-svelte + TailwindCSS

**Decision**: Use shadcn-svelte components with TailwindCSS for styling

**Rationale**:
- Accessible components out of the box (WCAG compliance)
- Mobile-first responsive design built-in
- Consistent design system across admin/student/evaluator interfaces
- Copy-paste components allow customization without library lock-in
- TailwindCSS utility classes speed up development

**Alternatives Considered**:
| Alternative | Reason Rejected |
|-------------|-----------------|
| Bootstrap | Less modern appearance, larger bundle |
| Material UI | React-focused, not Svelte native |
| Custom CSS | Time-consuming, accessibility risk |

---

### Decision 7: Charts - Chart.js

**Decision**: Use Chart.js for dashboard analytics visualizations

**Rationale**:
- Lightweight (~60KB gzipped)
- Supports required chart types: bar (pre vs post scores), pie (completion rates), line (trends)
- Good Svelte integration via svelte-chartjs wrapper
- Responsive by default
- Sufficient for the dashboard's simple comparative visualizations

**Alternatives Considered**:
| Alternative | Reason Rejected |
|-------------|-----------------|
| D3.js | Overkill complexity for basic charts |
| ApexCharts | Larger bundle size |
| Recharts | React-specific |

---

### Decision 8: Testing Strategy

**Decision**: Multi-layer testing approach

**Frontend Testing**:
- Unit tests: Vitest for component logic and stores
- E2E tests: Playwright for critical user flows (questionnaire completion, submission)

**Backend Testing**:
- Unit tests: xUnit with FluentAssertions for service layer
- Integration tests: TestContainers for database tests
- Contract tests: Verify API responses match OpenAPI spec

**Rationale**:
- Unit tests catch logic errors early
- Integration tests ensure database operations work correctly
- E2E tests verify complete user journeys
- Contract tests prevent API drift

---

### Decision 9: Deployment - Docker + Docker Compose

**Decision**: Containerized deployment with Docker Compose on DigitalOcean Droplet

**Rationale**:
- Reproducible builds across development and production
- Single-server deployment fits budget ($12/month droplet)
- Nginx reverse proxy handles HTTPS termination
- Easy rollback via container versioning
- PostgreSQL and MinIO can run as containers for dev

**Alternatives Considered**:
| Alternative | Reason Rejected |
|-------------|-----------------|
| Kubernetes | Overkill for single-server deployment |
| Platform-as-a-Service | Higher cost, less control |
| Bare metal | Harder to reproduce, manage dependencies |

---

## Best Practices Applied

### Questionnaire Auto-Save Pattern

**Pattern**: Debounced save on step navigation + periodic background save

**Implementation**:
1. On step change: Immediately save current step answers to API
2. Background: Every 30 seconds, save any dirty answers
3. On browser close/refresh: `beforeunload` triggers final save attempt
4. Server: Idempotent save endpoint accepts partial answer sets

**Why**: Prevents data loss from session timeout, network issues, or accidental navigation.

---

### Immutable Submission Pattern

**Pattern**: Soft-lock via status field with audit trigger

**Implementation**:
1. Answer records have `submitted_at` nullable timestamp
2. On submission: Set `submitted_at`, calculate scores in transaction
3. Database trigger prevents UPDATE on rows where `submitted_at IS NOT NULL`
4. Admin cannot modify; only audit log records any access

**Why**: Ensures research data integrity; prevents accidental or intentional tampering.

---

### Role-Based Access Control Pattern

**Pattern**: Claims-based authorization with policy enforcement

**Implementation**:
1. JWT contains role claim: `admin`, `evaluator`, `student`
2. API endpoints decorated with `[Authorize(Policy = "AdminOnly")]` etc.
3. Frontend routes check role before rendering
4. Evaluator endpoints filter by assigned students at query level

**Why**: Defense in depth; both frontend and backend enforce access rules.

---

### Scoring Calculation Pattern

**Pattern**: Separate raw answers from calculated scores

**Implementation**:
1. Store raw answer values in `Answers` table
2. On submission: Calculate scores, store in `Scores` table
3. Combined score calculated on-demand or when evaluator finalizes
4. Export includes both raw and calculated data

**Why**: Allows recalculation if scoring formula changes; preserves audit trail.

---

### Export Data Pattern

**Pattern**: Server-side streaming for large exports

**Implementation**:
1. Admin requests export via API
2. Server queries data in batches
3. Stream CSV/Excel rows to response (no full dataset in memory)
4. Include SPSS-compatible column headers and formats

**Why**: Handles large datasets without memory exhaustion on 2GB server.

---

## Security Considerations

| Concern | Mitigation |
|---------|------------|
| SQL Injection | EF Core parameterized queries |
| XSS | Svelte auto-escapes, CSP headers |
| CSRF | SameSite cookies, CORS restrictions |
| File Upload | Type validation, size limits, virus scan optional |
| Password Storage | bcrypt hashing with salt |
| Session Hijacking | HttpOnly cookies, short token expiry |
| Audit Trail | Immutable log table with timestamps |

---

## Performance Considerations

| Concern | Mitigation |
|---------|------------|
| Slow questionnaire load | Lazy load questions by step |
| Large video files | Pre-signed S3 URLs, direct browser download |
| Dashboard query speed | Materialized views or cached aggregates |
| 100 concurrent users | Connection pooling, async endpoints |
| Export performance | Streaming response, background job if >10s |

---

## Resolved Clarifications

All technical unknowns from the specification have been resolved:

1. **Authentication method**: JWT with refresh tokens (Decision 5)
2. **File storage approach**: S3-compatible object storage (Decision 4)
3. **Score calculation timing**: On submission + on-demand for combined (Scoring Pattern)
4. **Export format compatibility**: SPSS-compatible CSV with proper headers (Export Pattern)
5. **Auto-save mechanism**: Debounced + periodic + beforeunload (Auto-Save Pattern)
