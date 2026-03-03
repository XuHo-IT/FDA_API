#!/bin/bash
# Restore UAT Schema Script
# Usage: ./restore_uat_schema.sh <backup_file.sql.gz>

set -e

if [ -z "$1" ]; then
    echo "❌ Error: Backup file is required!"
    echo "Usage: $0 <backup_file.sql.gz>"
    echo ""
    echo "Available backups:"
    ls -lh ~/backup/uat_schema_*.sql.gz 2>/dev/null || echo "No backups found"
    exit 1
fi

BACKUP_FILE="$1"

# Check if backup file exists
if [ ! -f "$BACKUP_FILE" ]; then
    echo "❌ Error: Backup file not found: $BACKUP_FILE"
    exit 1
fi

# Load environment variables from .env if exists
if [ -f .env ]; then
    export $(cat .env | grep -v '^#' | xargs)
fi

echo "🔄 Starting UAT schema restore..."
echo "📅 Date: $(date)"
echo "📁 Backup file: $BACKUP_FILE"

# Check if postgres_dev container is running
if ! docker ps | grep -q postgres_dev; then
    echo "❌ Error: postgres_dev container is not running!"
    exit 1
fi

# Confirm restore
read -p "⚠️ WARNING: This will REPLACE the current uat_schema. Continue? (yes/no): " confirm
if [ "$confirm" != "yes" ]; then
    echo "❌ Restore cancelled."
    exit 1
fi

# Decompress if needed
if [[ "$BACKUP_FILE" == *.gz ]]; then
    echo "📦 Decompressing backup..."
    TEMP_FILE="${BACKUP_FILE%.gz}"
    gunzip -c "$BACKUP_FILE" > "$TEMP_FILE"
    BACKUP_FILE="$TEMP_FILE"
    CLEANUP_TEMP=true
else
    CLEANUP_TEMP=false
fi

# Restore schema
echo "💾 Restoring uat_schema..."
docker exec -i postgres_dev psql \
    -U "${POSTGRES_USER:-postgres}" \
    -d "${POSTGRES_DB:-fda_db}" \
    < "$BACKUP_FILE"

if [ $? -eq 0 ]; then
    echo "✅ Restore completed successfully!"
else
    echo "❌ Restore failed!"
    exit 1
fi

# Clean up temp file
if [ "$CLEANUP_TEMP" = true ]; then
    rm -f "$TEMP_FILE"
fi

echo "✅ Restore process completed!"

