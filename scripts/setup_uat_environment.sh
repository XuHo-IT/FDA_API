#!/bin/bash
# Setup UAT Environment Script
# Chạy script này 1 lần duy nhất trên VPS để setup môi trường UAT

set -e

echo "🚀 Setting up UAT environment..."

# 1. Create network if not exists
echo "📡 Creating Docker network..."
docker network create fda_api_default || echo "Network already exists"

# 2. Create backup directory
echo "📁 Creating backup directory..."
mkdir -p ~/backup

# 3. Make backup scripts executable
echo "🔧 Setting up backup scripts..."
chmod +x ~/backup/*.sh 2>/dev/null || true

# 4. Check if DEV containers are running
echo "🔍 Checking DEV containers..."
if ! docker ps | grep -q postgres_dev; then
    echo "⚠️ Warning: postgres_dev container is not running!"
    echo "   Please start DEV environment first:"
    echo "   docker compose -p fda_dev -f docker-compose.dev.yml up -d"
    exit 1
fi

# 5. Verify network connectivity
echo "🔗 Verifying network connectivity..."
if docker network inspect fda_api_default | grep -q postgres_dev; then
    echo "✅ Network connectivity OK"
else
    echo "⚠️ Warning: postgres_dev might not be in fda_api_default network"
    echo "   This is OK if DEV uses default network"
fi

# 6. Check UAT container
echo "🔍 Checking UAT container status..."
if docker ps -a | grep -q fdaapi_uat; then
    echo "ℹ️ UAT container exists"
    if docker ps | grep -q fdaapi_uat; then
        echo "✅ UAT container is running"
    else
        echo "⚠️ UAT container exists but is not running"
    fi
else
    echo "ℹ️ UAT container does not exist yet"
fi

echo ""
echo "✅ UAT environment setup completed!"
echo ""
echo "📋 Next steps:"
echo "   1. Ensure .env file exists with correct database credentials"
echo "   2. Run: docker compose -p fda_uat -f docker-compose.uat.yml up -d"
echo "   3. Check logs: docker compose -p fda_uat -f docker-compose.uat.yml logs -f"
echo "   4. Test backup: ~/backup/backup_uat_schema.sh"

