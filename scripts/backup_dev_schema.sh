#!/bin/bash
# Backup DEV Schema Script
# Usage: ./backup_dev_schema.sh

set -e

# Configuration
BACKUP_DIR="$HOME/backup"
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/dev_schema_$DATE.sql"
RETENTION_DAYS=7  # Giữ backup trong 7 ngày

# Load environment variables from .env if exists
if [ -f .env ]; then
    export $(cat .env | grep -v '^#' | xargs)
fi

# Create backup directory if not exists
mkdir -p "$BACKUP_DIR"

echo "🔄 Starting DEV schema backup..."
echo "📅 Date: $(date)"
echo "📁 Backup file: $BACKUP_FILE"

# Check if postgres_dev container is running
if ! docker ps | grep -q postgres_dev; then
    echo "❌ Error: postgres_dev container is not running!"
    exit 1
fi

# Backup DEV schema (public schema)
echo "💾 Backing up public schema..."
docker exec postgres_dev pg_dump \
    -U "${POSTGRES_USER:-postgres}" \
    -d "${POSTGRES_DB:-fda_db}" \
    -n public \
    --clean \
    --if-exists \
    > "$BACKUP_FILE"

# Check if backup was successful
if [ $? -eq 0 ] && [ -f "$BACKUP_FILE" ] && [ -s "$BACKUP_FILE" ]; then
    # Compress backup
    echo "🗜️ Compressing backup..."
    gzip -f "$BACKUP_FILE"
    BACKUP_FILE="${BACKUP_FILE}.gz"
    
    echo "✅ Backup completed successfully!"
    echo "📦 File: $BACKUP_FILE"
    echo "📊 Size: $(du -h "$BACKUP_FILE" | cut -f1)"
    
    # Clean up old backups (older than RETENTION_DAYS)
    echo "🧹 Cleaning up old backups (older than $RETENTION_DAYS days)..."
    find "$BACKUP_DIR" -name "dev_schema_*.sql.gz" -type f -mtime +$RETENTION_DAYS -delete
    
    echo "✅ Backup process completed!"
    exit 0
else
    echo "❌ Backup failed!"
    rm -f "$BACKUP_FILE"
    exit 1
fi

