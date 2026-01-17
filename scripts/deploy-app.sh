#!/bin/bash
# =============================================================================
# Research Platform - Application Deployment Script
# =============================================================================
# Run this AFTER server-setup.sh and Cloudflare tunnel authentication
# Run as deploy user: bash deploy-app.sh
# =============================================================================

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN} Research Platform - Deployment${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# =============================================================================
# Configuration - EDIT THESE VALUES
# =============================================================================
REPO_URL=""  # Will prompt if empty
DOMAIN=""    # Will prompt if empty
TUNNEL_ID="" # Will prompt if empty

# =============================================================================
# Prompt for configuration
# =============================================================================
if [ -z "$REPO_URL" ]; then
    read -p "Enter your Git repository URL: " REPO_URL
fi

if [ -z "$DOMAIN" ]; then
    read -p "Enter your domain (e.g., research.example.com): " DOMAIN
fi

if [ -z "$TUNNEL_ID" ]; then
    echo ""
    echo -e "${CYAN}Your Cloudflare tunnels:${NC}"
    cloudflared tunnel list 2>/dev/null || echo "No tunnels found. Create one first with: cloudflared tunnel create research-platform"
    echo ""
    read -p "Enter your Tunnel ID (from above): " TUNNEL_ID
fi

# =============================================================================
# Clone Repository
# =============================================================================
echo -e "${YELLOW}[1/5] Cloning repository...${NC}"

cd /home/deploy

if [ -d "app/.git" ]; then
    echo "Repository already exists, pulling latest..."
    cd app
    git pull
else
    rm -rf app
    git clone "$REPO_URL" app
    cd app
fi

echo -e "${GREEN}✓ Repository cloned${NC}"

# =============================================================================
# Create Environment File
# =============================================================================
echo -e "${YELLOW}[2/5] Creating environment file...${NC}"

# Generate secure passwords
JWT_SECRET=$(openssl rand -base64 32)
DB_PASSWORD=$(openssl rand -base64 24 | tr -d '/+=' | head -c 32)
MINIO_ROOT_PASSWORD=$(openssl rand -base64 24 | tr -d '/+=' | head -c 32)
MINIO_SECRET_KEY=$(openssl rand -base64 24 | tr -d '/+=' | head -c 32)

cat > docker/.env << EOF
# =============================================================================
# PRODUCTION ENVIRONMENT - Auto-generated $(date)
# =============================================================================

# Database Configuration
DB_USER=research
DB_PASSWORD=${DB_PASSWORD}
DB_NAME=research_platform

# MinIO (S3-Compatible Storage)
MINIO_ROOT_USER=minio_admin
MINIO_ROOT_PASSWORD=${MINIO_ROOT_PASSWORD}
MINIO_ACCESS_KEY=research_access
MINIO_SECRET_KEY=${MINIO_SECRET_KEY}

# JWT Configuration
JWT_SECRET=${JWT_SECRET}
JWT_ISSUER=ResearchPlatform
JWT_AUDIENCE=ResearchPlatform
JWT_ACCESS_EXPIRY=15
JWT_REFRESH_EXPIRY=7

# Application URLs
FRONTEND_URL=https://${DOMAIN}
API_URL=https://${DOMAIN}/api/v1

# Environment
ASPNETCORE_ENVIRONMENT=Production

# Rate Limiting
RATE_LIMIT_REQUESTS=50
RATE_LIMIT_WINDOW=1
EOF

echo -e "${GREEN}✓ Environment file created${NC}"

# Save credentials to a secure file
cat > /home/deploy/credentials.txt << EOF
=============================================================================
PRODUCTION CREDENTIALS - KEEP SECURE - Generated $(date)
=============================================================================

Database:
  User: research
  Password: ${DB_PASSWORD}
  Database: research_platform

MinIO:
  Root User: minio_admin
  Root Password: ${MINIO_ROOT_PASSWORD}
  Access Key: research_access
  Secret Key: ${MINIO_SECRET_KEY}

JWT Secret: ${JWT_SECRET}

Domain: https://${DOMAIN}
=============================================================================
EOF
chmod 600 /home/deploy/credentials.txt

echo -e "${CYAN}Credentials saved to /home/deploy/credentials.txt${NC}"

# =============================================================================
# Configure Cloudflare Tunnel
# =============================================================================
echo -e "${YELLOW}[3/5] Configuring Cloudflare Tunnel...${NC}"

sudo mkdir -p /etc/cloudflared

# Find credentials file
CREDS_FILE=$(find /root/.cloudflared -name "*.json" 2>/dev/null | head -1)
if [ -z "$CREDS_FILE" ]; then
    CREDS_FILE=$(find ~/.cloudflared -name "*.json" 2>/dev/null | head -1)
fi

if [ -z "$CREDS_FILE" ]; then
    echo -e "${RED}No credentials file found. Make sure you ran 'cloudflared tunnel login' and 'cloudflared tunnel create'${NC}"
    exit 1
fi

sudo tee /etc/cloudflared/config.yml > /dev/null << EOF
# Cloudflare Tunnel Configuration
tunnel: ${TUNNEL_ID}
credentials-file: ${CREDS_FILE}

ingress:
  # Main application
  - hostname: ${DOMAIN}
    service: http://localhost:80
    originRequest:
      noTLSVerify: true

  # Catch-all (required)
  - service: http_status:404
EOF

echo -e "${GREEN}✓ Tunnel configuration created${NC}"

# Route DNS
echo -e "${YELLOW}Routing DNS through tunnel...${NC}"
cloudflared tunnel route dns "$TUNNEL_ID" "$DOMAIN" 2>/dev/null || echo "DNS route may already exist"

# Install as service
echo -e "${YELLOW}Installing tunnel as system service...${NC}"
sudo cloudflared service install 2>/dev/null || echo "Service may already be installed"
sudo systemctl enable cloudflared
sudo systemctl restart cloudflared

echo -e "${GREEN}✓ Cloudflare Tunnel configured and running${NC}"

# =============================================================================
# Build and Deploy
# =============================================================================
echo -e "${YELLOW}[4/5] Building and deploying application...${NC}"

cd /home/deploy/app/docker

# Build and start
docker compose -f docker-compose.prod.yml up -d --build

echo -e "${GREEN}✓ Application deployed${NC}"

# =============================================================================
# Verify Deployment
# =============================================================================
echo -e "${YELLOW}[5/5] Verifying deployment...${NC}"

echo ""
echo "Waiting for services to start..."
sleep 10

echo ""
echo -e "${CYAN}Container Status:${NC}"
docker compose -f docker-compose.prod.yml ps

echo ""
echo -e "${CYAN}Cloudflare Tunnel Status:${NC}"
sudo systemctl status cloudflared --no-pager | head -10

# =============================================================================
# Complete
# =============================================================================
echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN} Deployment Complete!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "Your application is now available at:"
echo -e "${CYAN}  https://${DOMAIN}${NC}"
echo ""
echo -e "Important files:"
echo -e "  Credentials: ${YELLOW}/home/deploy/credentials.txt${NC}"
echo -e "  App directory: ${YELLOW}/home/deploy/app${NC}"
echo -e "  Tunnel config: ${YELLOW}/etc/cloudflared/config.yml${NC}"
echo ""
echo -e "Useful commands:"
echo -e "  View logs: ${YELLOW}cd ~/app/docker && docker compose -f docker-compose.prod.yml logs -f${NC}"
echo -e "  Restart: ${YELLOW}docker compose -f docker-compose.prod.yml restart${NC}"
echo -e "  Tunnel status: ${YELLOW}sudo systemctl status cloudflared${NC}"
echo ""
