# Production Deployment Guide

This guide covers deploying the Research Data Collection Platform to production using Docker Compose.

## Prerequisites

- Docker 24+ and Docker Compose v2+
- Domain name with DNS configured
- SSL certificates (Let's Encrypt recommended)
- 4GB+ RAM, 2+ CPU cores
- 50GB+ disk space

## Pre-Deployment Checklist

### 1. Security Configuration

⚠️ **CRITICAL**: Complete all items before deploying to production.

- [ ] Generate strong JWT secret: `openssl rand -base64 32`
- [ ] Create strong database password (16+ characters)
- [ ] Create strong MinIO credentials
- [ ] Review and update CORS allowed origins
- [ ] Configure SSL certificates
- [ ] Review rate limiting settings
- [ ] Disable database auto-seeding (production)
- [ ] Review [SECURITY.md](SECURITY.md) recommendations

### 2. Infrastructure Setup

- [ ] Configure firewall rules (ports 80, 443 only)
- [ ] Set up database backups
- [ ] Configure monitoring and alerting
- [ ] Set up log aggregation
- [ ] Plan disaster recovery strategy

### 3. Environment Configuration

- [ ] Copy `.env.example` to `.env`
- [ ] Update all `CHANGE_ME` values
- [ ] Configure production URLs
- [ ] Verify SSL certificate paths

## Quick Start Production Deployment

### Step 1: Clone Repository

```bash
git clone https://github.com/yourorg/research-platform.git
cd research-platform
```

### Step 2: Configure Environment

```bash
cd docker
cp .env.example .env
nano .env  # Edit with your production values
```

**Critical values to update in `.env`**:
```bash
DB_PASSWORD=your_secure_database_password_here
MINIO_ROOT_PASSWORD=your_secure_minio_password_here
JWT_SECRET=$(openssl rand -base64 32)
FRONTEND_URL=https://research.yourorg.edu
API_URL=https://api.research.yourorg.edu/api/v1
```

### Step 3: SSL Certificate Setup

#### Option A: Let's Encrypt (Recommended)

```bash
# Install certbot
sudo apt-get update
sudo apt-get install certbot

# Create directory for certificates
mkdir -p ssl

# Obtain certificate
sudo certbot certonly --standalone \
  -d research.yourorg.edu \
  -d api.research.yourorg.edu

# Copy certificates
sudo cp /etc/letsencrypt/live/research.yourorg.edu/fullchain.pem ssl/cert.pem
sudo cp /etc/letsencrypt/live/research.yourorg.edu/privkey.pem ssl/key.pem
sudo chmod 644 ssl/*.pem
```

#### Option B: Self-Signed (Development/Testing Only)

```bash
mkdir -p ssl
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout ssl/key.pem \
  -out ssl/cert.pem \
  -subj "/CN=research.yourorg.edu"
```

### Step 4: Update Nginx Configuration

```bash
# Use production nginx config
cp nginx.prod.conf nginx.conf

# Update server_name in nginx.conf
sed -i 's/server_name _;/server_name research.yourorg.edu api.research.yourorg.edu;/' nginx.conf
```

### Step 5: Build and Deploy

```bash
# Build images
docker-compose -f docker-compose.prod.yml build

# Start services
docker-compose -f docker-compose.prod.yml up -d

# View logs
docker-compose -f docker-compose.prod.yml logs -f
```

### Step 6: Initialize Database

```bash
# Run migrations
docker exec research-backend-prod dotnet ef database update

# Verify backend is healthy
curl https://api.research.yourorg.edu/health
```

### Step 7: Create Admin User

```bash
# Connect to database
docker exec -it research-postgres-prod psql -U research -d research_platform

# Create admin user (replace values)
INSERT INTO users (email, password_hash, name, role, status, created_at, updated_at)
VALUES (
  'admin@yourorg.edu',
  '$2a$11$...',  -- Generate with BCrypt
  'Administrator',
  'Admin',
  'Active',
  NOW(),
  NOW()
);

# Exit psql
\q
```

Or use the API (temporarily enable in production):

```bash
curl -X POST https://api.research.yourorg.edu/api/v1/admin/users \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@yourorg.edu",
    "password": "SecurePassword123!",
    "name": "Administrator",
    "role": "Admin"
  }'
```

## Post-Deployment Verification

### Health Checks

```bash
# Nginx health
curl https://research.yourorg.edu/health

# Backend health
curl https://api.research.yourorg.edu/health

# Database connection
docker exec research-backend-prod dotnet ef database update --dry-run

# MinIO health
docker exec research-minio-prod mc admin info local
```

### Service Status

```bash
# Check all services are running
docker-compose -f docker-compose.prod.yml ps

# Expected output:
# NAME                      STATUS    PORTS
# research-backend-prod     Up        5000/tcp
# research-frontend-prod    Up        5173/tcp
# research-postgres-prod    Up        5432/tcp
# research-minio-prod       Up        9000-9001/tcp
# research-nginx-prod       Up        0.0.0.0:80->80/tcp, 0.0.0.0:443->443/tcp
```

### Functional Testing

```bash
# Test login endpoint
TOKEN=$(curl -s -X POST https://api.research.yourorg.edu/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@yourorg.edu","password":"YourPassword"}' \
  | jq -r '.accessToken')

echo "Token: $TOKEN"

# Test authenticated endpoint
curl -H "Authorization: Bearer $TOKEN" \
  https://api.research.yourorg.edu/api/v1/admin/dashboard
```

## Monitoring and Maintenance

### Log Management

```bash
# View all logs
docker-compose -f docker-compose.prod.yml logs -f

# View specific service
docker-compose -f docker-compose.prod.yml logs -f backend

# View last 100 lines
docker-compose -f docker-compose.prod.yml logs --tail=100 backend

# Export logs
docker-compose -f docker-compose.prod.yml logs --no-color > application.log
```

### Database Backups

#### Automated Backup Script

Create `backup-db.sh`:

```bash
#!/bin/bash
BACKUP_DIR="/backups/postgres"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/research_platform_$TIMESTAMP.sql"

mkdir -p $BACKUP_DIR

docker exec research-postgres-prod pg_dump -U research research_platform > $BACKUP_FILE

# Compress
gzip $BACKUP_FILE

# Keep only last 30 days
find $BACKUP_DIR -name "*.sql.gz" -mtime +30 -delete

echo "Backup completed: ${BACKUP_FILE}.gz"
```

Schedule with cron:

```bash
# Run daily at 2 AM
0 2 * * * /path/to/backup-db.sh >> /var/log/db-backup.log 2>&1
```

#### Manual Backup

```bash
# Backup
docker exec research-postgres-prod pg_dump -U research research_platform > backup.sql

# Restore
cat backup.sql | docker exec -i research-postgres-prod psql -U research -d research_platform
```

### Updates and Rollbacks

#### Update Application

```bash
# Pull latest code
git pull origin main

# Rebuild images
docker-compose -f docker-compose.prod.yml build

# Rolling update (zero downtime)
docker-compose -f docker-compose.prod.yml up -d --no-deps --build backend
docker-compose -f docker-compose.prod.yml up -d --no-deps --build frontend

# Run migrations
docker exec research-backend-prod dotnet ef database update
```

#### Rollback

```bash
# Checkout previous version
git checkout <previous-commit-hash>

# Rebuild and restart
docker-compose -f docker-compose.prod.yml build
docker-compose -f docker-compose.prod.yml up -d

# Rollback database migration
docker exec research-backend-prod dotnet ef database update <PreviousMigrationName>
```

### SSL Certificate Renewal

```bash
# Renew Let's Encrypt certificate
sudo certbot renew

# Copy renewed certificates
sudo cp /etc/letsencrypt/live/research.yourorg.edu/fullchain.pem ssl/cert.pem
sudo cp /etc/letsencrypt/live/research.yourorg.edu/privkey.pem ssl/key.pem

# Reload nginx
docker-compose -f docker-compose.prod.yml restart nginx
```

Automate with cron:

```bash
0 0 1 * * certbot renew --quiet && cp /etc/letsencrypt/live/research.yourorg.edu/*.pem /path/to/ssl/ && docker-compose -f /path/to/docker-compose.prod.yml restart nginx
```

## Performance Tuning

### Database Optimization

Edit PostgreSQL configuration:

```bash
# Create custom postgres.conf
cat > postgres-custom.conf <<EOF
max_connections = 100
shared_buffers = 256MB
effective_cache_size = 1GB
maintenance_work_mem = 64MB
checkpoint_completion_target = 0.9
wal_buffers = 16MB
default_statistics_target = 100
random_page_cost = 1.1
effective_io_concurrency = 200
work_mem = 6553kB
min_wal_size = 1GB
max_wal_size = 4GB
EOF

# Mount in docker-compose
# volumes:
#   - ./postgres-custom.conf:/etc/postgresql/postgresql.conf
```

### Nginx Tuning

Adjust worker processes based on CPU cores:

```nginx
# In nginx.conf
worker_processes auto;
worker_rlimit_nofile 65535;

events {
    worker_connections 4096;
    use epoll;
    multi_accept on;
}
```

### Resource Limits

Add to `docker-compose.prod.yml`:

```yaml
services:
  backend:
    deploy:
      resources:
        limits:
          cpus: '2.0'
          memory: 2G
        reservations:
          cpus: '1.0'
          memory: 1G
```

## Scaling

### Horizontal Scaling

For high availability, deploy multiple backend instances:

```yaml
services:
  backend:
    deploy:
      replicas: 3
```

Update nginx upstream:

```nginx
upstream backend {
    least_conn;
    server backend-1:5000;
    server backend-2:5000;
    server backend-3:5000;
}
```

### Database Read Replicas

For read-heavy workloads, configure PostgreSQL replicas and update connection strings to use read replicas for queries.

## Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| 502 Bad Gateway | Check backend logs: `docker logs research-backend-prod` |
| Database connection failed | Verify postgres is healthy: `docker ps` |
| SSL certificate errors | Check cert paths and permissions |
| Rate limit 429 errors | Adjust rate limits in `.env` |
| Out of memory | Increase Docker memory limits |

### Debug Mode

Temporarily enable debug logging:

```bash
docker exec research-backend-prod \
  dotnet ResearchPlatform.Api.dll --environment Development
```

### Container Shell Access

```bash
# Backend
docker exec -it research-backend-prod /bin/bash

# Database
docker exec -it research-postgres-prod psql -U research -d research_platform

# MinIO
docker exec -it research-minio-prod sh
```

## Security Hardening

### Firewall Configuration

```bash
# Allow only HTTP/HTTPS
sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow 22/tcp  # SSH
sudo ufw allow 80/tcp  # HTTP
sudo ufw allow 443/tcp # HTTPS
sudo ufw enable
```

### Docker Security

```bash
# Run Docker daemon in rootless mode
# Enable user namespace remapping
# Use security scanning

docker scan research-backend:latest
```

### Regular Updates

```bash
# Update base images monthly
docker pull postgres:16-alpine
docker pull minio/minio:latest
docker pull nginx:alpine
docker pull mcr.microsoft.com/dotnet/aspnet:9.0
docker pull node:20-alpine

# Rebuild
docker-compose -f docker-compose.prod.yml build --no-cache
docker-compose -f docker-compose.prod.yml up -d
```

## Compliance

### HIPAA Compliance (if applicable)

- [ ] Enable database encryption at rest
- [ ] Configure audit logging for all data access
- [ ] Implement data retention policies
- [ ] Set up automated backup verification
- [ ] Document security controls

### GDPR Compliance

- [ ] Implement data export functionality
- [ ] Add data deletion workflows
- [ ] Configure data retention policies
- [ ] Document data processing activities
- [ ] Add privacy policy acceptance

## Support

For production issues:

- **Emergency**: [emergency@yourorg.edu]
- **Support**: [support@yourorg.edu]
- **Security**: [security@yourorg.edu]

## Additional Resources

- [Security Documentation](SECURITY.md)
- [Quickstart Guide](specs/001-research-data-platform/quickstart.md)
- [API Documentation](http://localhost:5000/swagger) (development only)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Let's Encrypt Documentation](https://letsencrypt.org/docs/)
