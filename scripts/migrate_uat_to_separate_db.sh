#!/bin/bash
# Script to migrate UAT from shared database to separate database
# Usage: ./migrate_uat_to_separate_db.sh

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
NEW_DB_NAME="FDA_UAT"  # Use uppercase to match PostgreSQL convention
OLD_SCHEMA="uat_schema"
NEW_SCHEMA="public"  # Use public schema in separate database
HANGFIRE_SCHEMA="hangfire"  # Use standard hangfire schema name
BACKUP_DIR="$HOME/backup"

echo -e "${GREEN}🚀 UAT Database Migration Script${NC}"
echo "=========================================="
echo ""

# Load environment variables
if [ -f .env ]; then
    export $(cat .env | grep -v '^#' | xargs)
fi

POSTGRES_USER="${POSTGRES_USER:-postgres}"
POSTGRES_DB="${POSTGRES_DB:-fda_db}"

# Step 1: Backup
echo -e "${YELLOW}Step 1: Creating backup...${NC}"
if [ -f "./scripts/backup_uat_schema.sh" ]; then
    ./scripts/backup_uat_schema.sh
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ Backup completed${NC}"
    else
        echo -e "${RED}❌ Backup failed! Aborting migration.${NC}"
        exit 1
    fi
else
    echo -e "${YELLOW}⚠️  Backup script not found. Creating manual backup...${NC}"
    mkdir -p "$BACKUP_DIR"
    DATE=$(date +%Y%m%d_%H%M%S)
    BACKUP_FILE="$BACKUP_DIR/uat_schema_$DATE.sql"
    
    docker exec fda_postgres_shared pg_dump \
        -U "$POSTGRES_USER" \
        -d "$POSTGRES_DB" \
        -n "$OLD_SCHEMA" \
        --clean \
        --if-exists \
        > "$BACKUP_FILE"
    
    if [ $? -eq 0 ]; then
        gzip -f "$BACKUP_FILE"
        echo -e "${GREEN}✅ Backup completed: ${BACKUP_FILE}.gz${NC}"
    else
        echo -e "${RED}❌ Backup failed! Aborting migration.${NC}"
        exit 1
    fi
fi

echo ""

# Step 2: Check if postgres container is running
echo -e "${YELLOW}Step 2: Checking PostgreSQL container...${NC}"
if ! docker ps | grep -q fda_postgres_shared; then
    echo -e "${RED}❌ Error: fda_postgres_shared container is not running!${NC}"
    exit 1
fi
echo -e "${GREEN}✅ PostgreSQL container is running${NC}"
echo ""

# Step 3: Create new database
echo -e "${YELLOW}Step 3: Creating new database '$NEW_DB_NAME'...${NC}"
docker exec fda_postgres_shared psql -U "$POSTGRES_USER" -d postgres -c "
    SELECT 1 FROM pg_database WHERE datname = '$NEW_DB_NAME'
" | grep -q 1 && {
    echo -e "${YELLOW}⚠️  Database '$NEW_DB_NAME' already exists.${NC}"
    read -p "Do you want to drop and recreate it? (yes/no): " confirm
    if [ "$confirm" = "yes" ]; then
        echo "Dropping existing database..."
        docker exec fda_postgres_shared psql -U "$POSTGRES_USER" -d postgres -c "
            DROP DATABASE IF EXISTS $NEW_DB_NAME;
        "
        echo "Creating new database..."
        docker exec fda_postgres_shared psql -U "$POSTGRES_USER" -d postgres -c "
            CREATE DATABASE $NEW_DB_NAME
                WITH OWNER = $POSTGRES_USER
                ENCODING = 'UTF8'
                LC_COLLATE = 'en_US.utf8'
                LC_CTYPE = 'en_US.utf8'
                TEMPLATE = template0;
        "
        echo -e "${GREEN}✅ Database created${NC}"
    else
        echo -e "${YELLOW}⚠️  Using existing database${NC}"
    fi
} || {
    docker exec fda_postgres_shared psql -U "$POSTGRES_USER" -d postgres -c "
        CREATE DATABASE $NEW_DB_NAME
            WITH OWNER = $POSTGRES_USER
            ENCODING = 'UTF8'
            LC_COLLATE = 'en_US.utf8'
            LC_CTYPE = 'en_US.utf8'
            TEMPLATE = template0;
    "
    echo -e "${GREEN}✅ Database created${NC}"
}
echo ""

# Step 4: Export and import data
echo -e "${YELLOW}Step 4: Migrating data...${NC}"

# Export data
echo "Exporting data from $OLD_SCHEMA..."
EXPORT_FILE="/tmp/uat_schema_export_$(date +%Y%m%d_%H%M%S).sql"
docker exec fda_postgres_shared pg_dump \
    -U "$POSTGRES_USER" \
    -d "$POSTGRES_DB" \
    -n "$OLD_SCHEMA" \
    --clean \
    --if-exists \
    > "$EXPORT_FILE"

if [ $? -ne 0 ] || [ ! -s "$EXPORT_FILE" ]; then
    echo -e "${RED}❌ Export failed!${NC}"
    rm -f "$EXPORT_FILE"
    exit 1
fi

# Modify schema name if needed
if [ "$NEW_SCHEMA" != "$OLD_SCHEMA" ]; then
    echo "Changing schema name from $OLD_SCHEMA to $NEW_SCHEMA..."
    sed "s/$OLD_SCHEMA/$NEW_SCHEMA/g" "$EXPORT_FILE" > "${EXPORT_FILE}.modified"
    mv "${EXPORT_FILE}.modified" "$EXPORT_FILE"
fi

# Import data
echo "Importing data to new database..."
docker exec -i fda_postgres_shared psql \
    -U "$POSTGRES_USER" \
    -d "$NEW_DB_NAME" \
    < "$EXPORT_FILE"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Data migrated successfully${NC}"
    rm -f "$EXPORT_FILE"
else
    echo -e "${RED}❌ Import failed!${NC}"
    echo "Export file kept at: $EXPORT_FILE"
    exit 1
fi
echo ""

# Step 5: Create Hangfire schema
echo -e "${YELLOW}Step 5: Creating Hangfire schema...${NC}"
docker exec fda_postgres_shared psql -U "$POSTGRES_USER" -d "$NEW_DB_NAME" -c "
    CREATE SCHEMA IF NOT EXISTS $HANGFIRE_SCHEMA;
    GRANT ALL ON SCHEMA $HANGFIRE_SCHEMA TO $POSTGRES_USER;
" > /dev/null

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Hangfire schema created${NC}"
else
    echo -e "${RED}❌ Failed to create Hangfire schema${NC}"
    exit 1
fi
echo ""

# Step 6: Verify migration
echo -e "${YELLOW}Step 6: Verifying migration...${NC}"
TABLE_COUNT=$(docker exec fda_postgres_shared psql -U "$POSTGRES_USER" -d "$NEW_DB_NAME" -t -c "
    SELECT COUNT(*) FROM information_schema.tables 
    WHERE table_schema = '$NEW_SCHEMA';
" | tr -d ' ')

if [ "$TABLE_COUNT" -gt 0 ]; then
    echo -e "${GREEN}✅ Found $TABLE_COUNT tables in $NEW_SCHEMA schema${NC}"
else
    echo -e "${RED}❌ No tables found! Migration may have failed.${NC}"
    exit 1
fi

# Check some important tables
echo "Checking important tables..."
docker exec fda_postgres_shared psql -U "$POSTGRES_USER" -d "$NEW_DB_NAME" -t -c "
    SELECT 
        CASE WHEN EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = '$NEW_SCHEMA' AND table_name = 'Users') 
             THEN '✅ Users table exists'
             ELSE '❌ Users table missing'
        END,
        CASE WHEN EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = '$NEW_SCHEMA' AND table_name = 'Areas') 
             THEN '✅ Areas table exists'
             ELSE '❌ Areas table missing'
        END,
        CASE WHEN EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = '$NEW_SCHEMA' AND table_name = 'PredictionLogs') 
             THEN '✅ PredictionLogs table exists'
             ELSE '❌ PredictionLogs table missing'
        END;
" | grep -v "^$"

echo ""

# Summary
echo -e "${GREEN}=========================================="
echo "✅ Migration completed successfully!"
echo "==========================================${NC}"
echo ""
echo "Summary:"
echo "  - New Database: $NEW_DB_NAME"
echo "  - Schema: $NEW_SCHEMA"
echo "  - Hangfire Schema: $HANGFIRE_SCHEMA"
echo "  - Tables migrated: $TABLE_COUNT"
echo ""
echo "Next steps:"
echo "  1. Update docker-compose.uat.yml:"
echo "     - Change Database to: $NEW_DB_NAME"
echo "     - Update SearchPath if needed: $NEW_SCHEMA"
echo ""
echo "  2. Commit and push code changes"
echo ""
echo "  3. Deploy UAT:"
echo "     docker compose -p fda_uat -f docker-compose.uat.yml up -d --force-recreate"
echo ""
echo "  4. Verify:"
echo "     - Check application logs"
echo "     - Check Hangfire dashboard"
echo "     - Verify recurring jobs"
echo ""

