#!/bin/bash
# =============================================================================
# Research Platform - DigitalOcean Server Setup Script
# =============================================================================
# This script prepares a fresh Ubuntu 24.04 droplet for the Research Platform
# Run as root: bash server-setup.sh
# =============================================================================

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN} Research Platform - Server Setup${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Check if running as root
if [ "$EUID" -ne 0 ]; then
    echo -e "${RED}Please run as root (sudo bash server-setup.sh)${NC}"
    exit 1
fi

# =============================================================================
# STEP 1: Update System
# =============================================================================
echo -e "${YELLOW}[1/7] Updating system packages...${NC}"
apt update && apt upgrade -y
echo -e "${GREEN}✓ System updated${NC}"

# =============================================================================
# STEP 2: Install Essential Tools
# =============================================================================
echo -e "${YELLOW}[2/7] Installing essential tools...${NC}"
apt install -y curl wget git nano htop ufw
echo -e "${GREEN}✓ Essential tools installed${NC}"

# =============================================================================
# STEP 3: Create Deploy User
# =============================================================================
echo -e "${YELLOW}[3/7] Creating deploy user...${NC}"

if id "deploy" &>/dev/null; then
    echo "User 'deploy' already exists"
else
    adduser --disabled-password --gecos "" deploy
    echo "deploy:$(openssl rand -base64 12)" | chpasswd
    usermod -aG sudo deploy

    # Allow passwordless sudo
    echo "deploy ALL=(ALL) NOPASSWD:ALL" > /etc/sudoers.d/deploy

    # Copy SSH keys
    mkdir -p /home/deploy/.ssh
    if [ -f /root/.ssh/authorized_keys ]; then
        cp /root/.ssh/authorized_keys /home/deploy/.ssh/
    fi
    chown -R deploy:deploy /home/deploy/.ssh
    chmod 700 /home/deploy/.ssh
    chmod 600 /home/deploy/.ssh/authorized_keys 2>/dev/null || true
fi
echo -e "${GREEN}✓ Deploy user created${NC}"

# =============================================================================
# STEP 4: Configure Firewall
# =============================================================================
echo -e "${YELLOW}[4/7] Configuring firewall...${NC}"
ufw allow OpenSSH
ufw --force enable
echo -e "${GREEN}✓ Firewall configured (SSH only)${NC}"

# =============================================================================
# STEP 5: Set Timezone
# =============================================================================
echo -e "${YELLOW}[5/7] Setting timezone to UTC...${NC}"
timedatectl set-timezone UTC
echo -e "${GREEN}✓ Timezone set to UTC${NC}"

# =============================================================================
# STEP 6: Install Docker & Docker Compose
# =============================================================================
echo -e "${YELLOW}[6/7] Installing Docker...${NC}"

if command -v docker &> /dev/null; then
    echo "Docker already installed"
else
    curl -fsSL https://get.docker.com | sh
fi

# Install Docker Compose plugin
apt install -y docker-compose-plugin

# Add deploy user to docker group
usermod -aG docker deploy

echo -e "${GREEN}✓ Docker installed${NC}"
docker --version
docker compose version

# =============================================================================
# STEP 7: Install Cloudflare Tunnel (cloudflared)
# =============================================================================
echo -e "${YELLOW}[7/7] Installing Cloudflare Tunnel...${NC}"

if command -v cloudflared &> /dev/null; then
    echo "cloudflared already installed"
else
    curl -L --output /tmp/cloudflared.deb https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64.deb
    dpkg -i /tmp/cloudflared.deb
    rm /tmp/cloudflared.deb
fi

echo -e "${GREEN}✓ Cloudflare Tunnel installed${NC}"
cloudflared --version

# =============================================================================
# STEP 8: Create Swap Space (for 2GB RAM droplet)
# =============================================================================
echo -e "${YELLOW}[Bonus] Creating swap space...${NC}"

if [ -f /swapfile ]; then
    echo "Swap already exists"
else
    fallocate -l 2G /swapfile
    chmod 600 /swapfile
    mkswap /swapfile
    swapon /swapfile
    echo '/swapfile none swap sw 0 0' >> /etc/fstab
fi

echo -e "${GREEN}✓ Swap space configured${NC}"
free -h

# =============================================================================
# STEP 9: Create app directory
# =============================================================================
echo -e "${YELLOW}Creating application directory...${NC}"
mkdir -p /home/deploy/app
chown -R deploy:deploy /home/deploy/app
echo -e "${GREEN}✓ App directory created${NC}"

# =============================================================================
# COMPLETE
# =============================================================================
echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN} Server Setup Complete!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "Next steps:"
echo -e "1. ${YELLOW}Authenticate Cloudflare Tunnel:${NC}"
echo -e "   cloudflared tunnel login"
echo ""
echo -e "2. ${YELLOW}Create tunnel:${NC}"
echo -e "   cloudflared tunnel create research-platform"
echo ""
echo -e "3. ${YELLOW}Clone your application:${NC}"
echo -e "   su - deploy"
echo -e "   git clone <your-repo-url> ~/app"
echo ""
echo -e "4. ${YELLOW}Configure environment:${NC}"
echo -e "   cd ~/app/docker"
echo -e "   cp .env.example .env"
echo -e "   nano .env"
echo ""
echo -e "5. ${YELLOW}Configure Cloudflare tunnel:${NC}"
echo -e "   sudo nano /etc/cloudflared/config.yml"
echo ""
echo -e "6. ${YELLOW}Deploy application:${NC}"
echo -e "   docker compose -f docker-compose.prod.yml up -d --build"
echo ""
echo -e "${GREEN}Server IP: $(curl -s ifconfig.me)${NC}"
echo ""
