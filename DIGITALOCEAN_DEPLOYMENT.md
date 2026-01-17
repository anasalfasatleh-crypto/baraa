# DigitalOcean Deployment Guide with Cloudflare Tunnel

This guide walks you through deploying the Research Platform on a fresh DigitalOcean droplet using Cloudflare Tunnel for secure, zero-trust access.

## Architecture Overview

```
Internet → Cloudflare Edge (SSL/DDoS) → Encrypted Tunnel → Droplet → Nginx → Services
                                                              ↓
                                              ┌───────────────┼───────────────┐
                                              ↓               ↓               ↓
                                          Frontend        Backend        PostgreSQL
                                         (SvelteKit)    (ASP.NET 9)        MinIO
```

**Security Benefits:**
- No open inbound ports (except SSH for management)
- Outbound-only encrypted connection
- Free SSL/TLS - no certificate management required
- Built-in DDoS protection
- Zero-trust security model

---

## Table of Contents

1. [Prerequisites](#1-prerequisites)
2. [Create DigitalOcean Droplet](#2-create-digitalocean-droplet)
3. [Initial Server Setup](#3-initial-server-setup)
4. [Install Docker & Docker Compose](#4-install-docker--docker-compose)
5. [Install Cloudflare Tunnel](#5-install-cloudflare-tunnel-cloudflared)
6. [Clone & Configure Application](#6-clone--configure-application)
7. [Configure Cloudflare Tunnel](#7-configure-cloudflare-tunnel)
8. [Deploy Application](#8-deploy-application)
9. [Verify Deployment](#9-verify-deployment)
10. [Post-Deployment Configuration](#10-post-deployment-configuration)
11. [Maintenance Commands](#11-maintenance-commands)
12. [Troubleshooting](#12-troubleshooting)

---

## 1. Prerequisites

Before starting, ensure you have:

- [ ] **DigitalOcean account** with payment method configured
  - Sign up at: https://cloud.digitalocean.com/registrations/new

- [ ] **Cloudflare account** (free tier works)
  - Sign up at: https://dash.cloudflare.com/sign-up

- [ ] **Domain name** added to Cloudflare with DNS managed by Cloudflare
  - Add your domain at: https://dash.cloudflare.com/ → "Add a Site"
  - Update your domain registrar's nameservers to Cloudflare's

- [ ] **SSH key pair** generated on your local machine
  ```bash
  # Generate SSH key if you don't have one
  ssh-keygen -t ed25519 -C "your_email@example.com"

  # View your public key (you'll need this for DigitalOcean)
  cat ~/.ssh/id_ed25519.pub
  ```

- [ ] **Git repository URL** for this project

---

## 2. Create DigitalOcean Droplet

### Step 2.1: Log into DigitalOcean
Go to https://cloud.digitalocean.com and log in.

### Step 2.2: Create a New Droplet
1. Click **"Create"** → **"Droplets"**

### Step 2.3: Choose an Image
- Select **Ubuntu 22.04 (LTS) x64**

### Step 2.4: Choose Droplet Size
- Select **Basic** plan
- Choose **Regular (SSD)**
- Select **$12/mo** (2 GB RAM / 1 vCPU / 50 GB SSD)

  > **Note:** This is the minimum recommended. For production with many users, consider $24/mo (4GB RAM / 2 vCPU).

### Step 2.5: Choose Datacenter Region
- Select the region closest to your users
- Examples: `NYC1`, `LON1`, `FRA1`, `SGP1`

### Step 2.6: Authentication
1. Select **SSH keys**
2. Click **"New SSH Key"**
3. Paste your public key (from `cat ~/.ssh/id_ed25519.pub`)
4. Give it a name and click **"Add SSH Key"**

### Step 2.7: Finalize and Create
1. Set hostname: `research-platform` (or your preferred name)
2. Click **"Create Droplet"**
3. **Save the IP address** shown after creation (e.g., `164.90.xxx.xxx`)

---

## 3. Initial Server Setup

### Step 3.1: Connect to Your Droplet

```bash
ssh root@YOUR_DROPLET_IP
```

### Step 3.2: Update System Packages

```bash
apt update && apt upgrade -y
```

### Step 3.3: Create a Deploy User

```bash
# Create user
adduser deploy
# Follow prompts to set password

# Add to sudo group
usermod -aG sudo deploy

# Allow passwordless sudo (optional, for convenience)
echo "deploy ALL=(ALL) NOPASSWD:ALL" >> /etc/sudoers.d/deploy
```

### Step 3.4: Set Up SSH for Deploy User

```bash
# Copy SSH keys to deploy user
mkdir -p /home/deploy/.ssh
cp ~/.ssh/authorized_keys /home/deploy/.ssh/
chown -R deploy:deploy /home/deploy/.ssh
chmod 700 /home/deploy/.ssh
chmod 600 /home/deploy/.ssh/authorized_keys
```

### Step 3.5: Set Timezone

```bash
timedatectl set-timezone UTC
```

### Step 3.6: Configure Firewall (UFW)

```bash
# Allow SSH
ufw allow OpenSSH

# Enable firewall
ufw --force enable

# Check status
ufw status
```

> **Important:** We do NOT open ports 80 or 443 because Cloudflare Tunnel creates an outbound connection - no inbound ports needed!

### Step 3.7: Install Essential Tools

```bash
apt install -y curl wget git nano htop
```

---

## 4. Install Docker & Docker Compose

### Step 4.1: Install Docker Engine

```bash
# Install Docker using the official script
curl -fsSL https://get.docker.com | sh

# Verify installation
docker --version
```

### Step 4.2: Install Docker Compose Plugin

```bash
apt install -y docker-compose-plugin

# Verify installation
docker compose version
```

### Step 4.3: Add Deploy User to Docker Group

```bash
usermod -aG docker deploy

# Apply changes (or log out and back in)
newgrp docker
```

### Step 4.4: Verify Docker Works

```bash
# Test as deploy user
su - deploy -c "docker run hello-world"
```

---

## 5. Install Cloudflare Tunnel (cloudflared)

### Step 5.1: Download and Install cloudflared

```bash
# Download the latest .deb package
curl -L --output cloudflared.deb https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64.deb

# Install it
dpkg -i cloudflared.deb

# Verify installation
cloudflared --version

# Clean up
rm cloudflared.deb
```

### Step 5.2: Authenticate with Cloudflare

```bash
cloudflared tunnel login
```

This will display a URL. Open it in your browser, select your domain, and authorize the connection.

After authorization, a certificate will be saved to `~/.cloudflared/cert.pem`.

### Step 5.3: Create a Tunnel

```bash
cloudflared tunnel create research-platform
```

**Important:** Save the output! It will show:
- Tunnel ID (UUID format like `a1b2c3d4-e5f6-7890-abcd-ef1234567890`)
- Credentials file path (like `/root/.cloudflared/a1b2c3d4-....json`)

---

## 6. Clone & Configure Application

### Step 6.1: Switch to Deploy User

```bash
su - deploy
```

### Step 6.2: Clone the Repository

```bash
git clone YOUR_REPOSITORY_URL ~/app
cd ~/app
```

### Step 6.3: Create Production Environment File

```bash
# Copy the example file
cp docker/.env.example docker/.env

# Edit the file
nano docker/.env
```

### Step 6.4: Configure Environment Variables

Replace the contents with your production values:

```env
# ===========================================
# PRODUCTION ENVIRONMENT CONFIGURATION
# ===========================================

# Database Configuration
DB_USER=research
DB_PASSWORD=CHANGE_ME_USE_STRONG_32_CHAR_PASSWORD
DB_NAME=research_platform

# MinIO (S3-Compatible Storage)
MINIO_ROOT_USER=minio_admin
MINIO_ROOT_PASSWORD=CHANGE_ME_USE_STRONG_32_CHAR_PASSWORD
MINIO_ACCESS_KEY=research_access
MINIO_SECRET_KEY=CHANGE_ME_USE_STRONG_32_CHAR_PASSWORD

# JWT Configuration
# Generate with: openssl rand -base64 32
JWT_SECRET=CHANGE_ME_GENERATE_WITH_OPENSSL_RAND_BASE64_32
JWT_ISSUER=ResearchPlatform
JWT_AUDIENCE=ResearchPlatform
JWT_ACCESS_EXPIRY=15
JWT_REFRESH_EXPIRY=7

# Application URLs (replace with your domain)
FRONTEND_URL=https://research.yourdomain.com
API_URL=https://research.yourdomain.com/api/v1

# Environment
ASPNETCORE_ENVIRONMENT=Production

# Rate Limiting
RATE_LIMIT_REQUESTS=50
RATE_LIMIT_WINDOW=1
```

### Step 6.5: Generate Secure Passwords

Run these commands to generate secure values:

```bash
# Generate JWT Secret (256-bit)
echo "JWT_SECRET: $(openssl rand -base64 32)"

# Generate Database Password
echo "DB_PASSWORD: $(openssl rand -base64 24 | tr -d '/+=' | head -c 32)"

# Generate MinIO Passwords
echo "MINIO_ROOT_PASSWORD: $(openssl rand -base64 24 | tr -d '/+=' | head -c 32)"
echo "MINIO_SECRET_KEY: $(openssl rand -base64 24 | tr -d '/+=' | head -c 32)"
```

Copy these values into your `.env` file.

---

## 7. Configure Cloudflare Tunnel

### Step 7.1: Create Tunnel Configuration Directory

```bash
# Switch back to root for cloudflared config
exit  # Back to root

mkdir -p /etc/cloudflared
```

### Step 7.2: Create Tunnel Configuration File

```bash
nano /etc/cloudflared/config.yml
```

Add the following content (replace placeholders):

```yaml
# Cloudflare Tunnel Configuration
tunnel: YOUR_TUNNEL_ID
credentials-file: /root/.cloudflared/YOUR_TUNNEL_ID.json

# Ingress rules - route traffic to services
ingress:
  # Main application (Frontend + API via Nginx)
  - hostname: research.yourdomain.com
    service: http://localhost:80
    originRequest:
      noTLSVerify: true

  # Optional: MinIO Console (for admin access)
  - hostname: minio.yourdomain.com
    service: http://localhost:9001
    originRequest:
      noTLSVerify: true

  # Catch-all rule (required)
  - service: http_status:404
```

> **Replace:**
> - `YOUR_TUNNEL_ID` with your actual tunnel ID
> - `yourdomain.com` with your actual domain

### Step 7.3: Route DNS Through Tunnel

```bash
# Create DNS record for main app
cloudflared tunnel route dns research-platform research.yourdomain.com

# Optional: Create DNS record for MinIO console
cloudflared tunnel route dns research-platform minio.yourdomain.com
```

### Step 7.4: Install as System Service

```bash
cloudflared service install

# Start the service
systemctl start cloudflared

# Enable on boot
systemctl enable cloudflared

# Check status
systemctl status cloudflared
```

### Step 7.5: Verify Tunnel is Running

```bash
cloudflared tunnel info research-platform
```

You should see your tunnel status as "healthy".

---

## 8. Deploy Application

### Step 8.1: Switch to Deploy User and Navigate to App

```bash
su - deploy
cd ~/app/docker
```

### Step 8.2: Build and Start All Services

```bash
docker compose -f docker-compose.prod.yml up -d --build
```

This will:
- Build the frontend and backend Docker images
- Start PostgreSQL and MinIO
- Start Nginx as reverse proxy
- Connect all services on an internal network

### Step 8.3: Monitor Build Progress

```bash
# Follow the logs
docker compose -f docker-compose.prod.yml logs -f
```

Press `Ctrl+C` to exit log view.

### Step 8.4: Check Container Status

```bash
docker compose -f docker-compose.prod.yml ps
```

All services should show "Up" status:
- `frontend` - Up
- `backend` - Up (healthy)
- `postgres` - Up (healthy)
- `minio` - Up (healthy)
- `nginx` - Up

### Step 8.5: Run Database Migrations (if needed)

```bash
docker compose -f docker-compose.prod.yml exec backend dotnet ef database update
```

---

## 9. Verify Deployment

### Verification Checklist

- [ ] **All containers running:**
  ```bash
  docker compose -f docker-compose.prod.yml ps
  ```

- [ ] **Cloudflare tunnel active:**
  ```bash
  sudo systemctl status cloudflared
  ```

- [ ] **Frontend accessible:**
  Open `https://research.yourdomain.com` in your browser

- [ ] **API health check:**
  ```bash
  curl https://research.yourdomain.com/api/v1/health
  ```
  Should return `{"status":"healthy"}`

- [ ] **Test participant registration:**
  Go to `https://research.yourdomain.com/participant/register`

- [ ] **Test login:**
  Go to `https://research.yourdomain.com/participant/login`

- [ ] **MinIO console (if configured):**
  Open `https://minio.yourdomain.com`

---

## 10. Post-Deployment Configuration

### 10.1: Set Up Database Backups

Create a backup script:

```bash
sudo nano /usr/local/bin/backup-db.sh
```

Add the following:

```bash
#!/bin/bash
BACKUP_DIR="/home/deploy/backups"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
mkdir -p $BACKUP_DIR

# Create backup
cd /home/deploy/app/docker
docker compose -f docker-compose.prod.yml exec -T postgres pg_dump -U research research_platform > $BACKUP_DIR/db_backup_$TIMESTAMP.sql

# Compress
gzip $BACKUP_DIR/db_backup_$TIMESTAMP.sql

# Keep only last 7 days of backups
find $BACKUP_DIR -name "db_backup_*.sql.gz" -mtime +7 -delete

echo "Backup completed: db_backup_$TIMESTAMP.sql.gz"
```

Make it executable and schedule:

```bash
chmod +x /usr/local/bin/backup-db.sh

# Add to crontab (daily at 2 AM)
(crontab -l 2>/dev/null; echo "0 2 * * * /usr/local/bin/backup-db.sh >> /var/log/db-backup.log 2>&1") | crontab -
```

### 10.2: Configure Log Rotation

```bash
sudo nano /etc/logrotate.d/docker-containers
```

Add:

```
/var/lib/docker/containers/*/*.log {
    rotate 7
    daily
    compress
    missingok
    delaycompress
    copytruncate
}
```

### 10.3: Set Up Swap Space (Recommended for 2GB RAM)

```bash
# Create 2GB swap file
fallocate -l 2G /swapfile
chmod 600 /swapfile
mkswap /swapfile
swapon /swapfile

# Make permanent
echo '/swapfile none swap sw 0 0' >> /etc/fstab
```

### 10.4: Optional - Install Monitoring (Netdata)

```bash
# Install Netdata for real-time monitoring
bash <(curl -Ss https://get.netdata.cloud/kickstart.sh) --stable-channel

# Access at http://YOUR_IP:19999 (or add to Cloudflare tunnel)
```

---

## 11. Maintenance Commands

### View Logs

```bash
cd ~/app/docker

# All services
docker compose -f docker-compose.prod.yml logs -f

# Specific service
docker compose -f docker-compose.prod.yml logs -f backend
docker compose -f docker-compose.prod.yml logs -f frontend
docker compose -f docker-compose.prod.yml logs -f nginx
```

### Restart Services

```bash
# Restart all
docker compose -f docker-compose.prod.yml restart

# Restart specific service
docker compose -f docker-compose.prod.yml restart backend
```

### Stop Services

```bash
docker compose -f docker-compose.prod.yml down
```

### Update Application

```bash
cd ~/app

# Pull latest changes
git pull

# Rebuild and restart
cd docker
docker compose -f docker-compose.prod.yml up -d --build
```

### Database Operations

```bash
# Manual backup
docker compose -f docker-compose.prod.yml exec postgres pg_dump -U research research_platform > backup.sql

# Restore from backup
docker compose -f docker-compose.prod.yml exec -T postgres psql -U research research_platform < backup.sql

# Access database shell
docker compose -f docker-compose.prod.yml exec postgres psql -U research research_platform
```

### Cloudflare Tunnel Commands

```bash
# Check tunnel status
sudo systemctl status cloudflared

# Restart tunnel
sudo systemctl restart cloudflared

# View tunnel logs
sudo journalctl -u cloudflared -f

# List tunnels
cloudflared tunnel list

# Get tunnel info
cloudflared tunnel info research-platform
```

### Clean Up Docker Resources

```bash
# Remove unused images
docker image prune -a

# Remove unused volumes (careful - may delete data!)
docker volume prune

# Full cleanup
docker system prune -a
```

---

## 12. Troubleshooting

### Container Won't Start

```bash
# Check logs for the specific container
docker compose -f docker-compose.prod.yml logs backend

# Check container status
docker compose -f docker-compose.prod.yml ps

# Restart the container
docker compose -f docker-compose.prod.yml restart backend
```

**Common issues:**
- Missing environment variables in `.env` file
- Port conflicts
- Insufficient memory

### Database Connection Errors

```bash
# Check if PostgreSQL is running
docker compose -f docker-compose.prod.yml ps postgres

# Check PostgreSQL logs
docker compose -f docker-compose.prod.yml logs postgres

# Verify database credentials in .env match
```

**Common issues:**
- Wrong password in `.env`
- Database not initialized yet
- Container network issues

### Cloudflare Tunnel Not Connecting

```bash
# Check tunnel service status
sudo systemctl status cloudflared

# View detailed logs
sudo journalctl -u cloudflared -n 100

# Verify config file
cat /etc/cloudflared/config.yml

# Test configuration
cloudflared tunnel --config /etc/cloudflared/config.yml run
```

**Common issues:**
- Wrong tunnel ID in config
- Credentials file path incorrect
- DNS records not created

### 502 Bad Gateway Errors

This usually means Nginx can't reach the backend/frontend.

```bash
# Check if all containers are running
docker compose -f docker-compose.prod.yml ps

# Check Nginx logs
docker compose -f docker-compose.prod.yml logs nginx

# Verify internal network connectivity
docker compose -f docker-compose.prod.yml exec nginx ping backend
```

### Permission Issues

```bash
# Fix ownership of app directory
sudo chown -R deploy:deploy /home/deploy/app

# Fix docker socket permissions
sudo chmod 666 /var/run/docker.sock
```

### Memory Issues (Container Killed)

```bash
# Check memory usage
free -h
docker stats

# Add swap space if not already done
fallocate -l 2G /swapfile
chmod 600 /swapfile
mkswap /swapfile
swapon /swapfile
```

### Slow Performance

```bash
# Check CPU and memory
htop

# Check disk space
df -h

# Check Docker resource usage
docker stats
```

---

## Quick Reference

| Task | Command |
|------|---------|
| Start all services | `docker compose -f docker-compose.prod.yml up -d` |
| Stop all services | `docker compose -f docker-compose.prod.yml down` |
| View logs | `docker compose -f docker-compose.prod.yml logs -f` |
| Restart services | `docker compose -f docker-compose.prod.yml restart` |
| Update application | `git pull && docker compose -f docker-compose.prod.yml up -d --build` |
| Check tunnel status | `sudo systemctl status cloudflared` |
| Backup database | `/usr/local/bin/backup-db.sh` |
| SSH into container | `docker compose -f docker-compose.prod.yml exec backend bash` |

---

## Security Checklist

- [ ] Strong, unique passwords for all services
- [ ] JWT secret is 256-bit random value
- [ ] SSH key authentication only (password auth disabled)
- [ ] UFW firewall enabled with only SSH allowed
- [ ] Regular database backups configured
- [ ] Log rotation configured
- [ ] Cloudflare security settings enabled (DDoS protection, WAF)

---

## Support

If you encounter issues not covered in this guide:

1. Check the application logs: `docker compose logs`
2. Check the Cloudflare dashboard for tunnel status
3. Review the [Cloudflare Tunnel documentation](https://developers.cloudflare.com/cloudflare-one/connections/connect-networks/)
4. Check [DigitalOcean community tutorials](https://www.digitalocean.com/community/tutorials)
