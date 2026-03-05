#!/bin/bash
# Script to create FDA_UAT database
# Usage: ./create_uat_database.sh

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
NEW_DB_NAME="FDA_UAT"
POSTGRES_USER="${POSTGRES_USER:-dev_user}"
POSTGRES_PASSWORD="${POSTGRES_PASSWORD:-123456}"
POSTGRES_HOST="${POSTGRES_HOST:-fda_postgres_shared}"
POSTGRES_PORT="${POSTGRES_PORT:-5432}"

# Parse connection string from .env.uat if available
if [ -f .env.uat ]; then
    CONN_STR=$(grep "ConnectionStrings__PostgreSQLConnection" .env.uat | cut -d'=' -f2- | tr -d ' ')
    if [ ! -z "$CONN_STR" ]; then
        # Extract Host (could be IP or container name)
        if echo "$CONN_STR" | grep -q "Host="; then
            POSTGRES_HOST=$(echo "$CONN_STR" | sed -n 's/.*Host=\([^;]*\).*/\1/p')
        fi
        # Extract Port
        if echo "$CONN_STR" | grep -q "Port="; then
            POSTGRES_PORT=$(echo "$CONN_STR" | sed -n 's/.*Port=\([^;]*\).*/\1/p')
        fi
        # Extract User
        if echo "$CONN_STR" | grep -q "Username="; then
            POSTGRES_USER=$(echo "$CONN_STR" | sed -n 's/.*Username=\([^;]*\).*/\1/p')
        fi
    fi
fi

echo -e "${GREEN}🚀 Creating UAT Database Script${NC}"
echo "=========================================="
echo ""

# Load environment variables
if [ -f .env.uat ]; then
    export $(cat .env.uat | grep -v '^#' | xargs)
fi

# Override with .env.uat values if available
if [ ! -z "$POSTGRES_USER" ]; then
    POSTGRES_USER="$POSTGRES_USER"
fi

echo -e "${YELLOW}Configuration:${NC}"
echo "  Database: $NEW_DB_NAME"
echo "  User: $POSTGRES_USER"
echo "  Host: $POSTGRES_HOST"
echo ""

# Check if postgres is accessible (container or remote)
echo -e "${YELLOW}Step 1: Checking PostgreSQL connection...${NC}"
if [[ "$POSTGRES_HOST" =~ ^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
    # It's an IP address - remote connection
    echo "   Detected remote PostgreSQL server: $POSTGRES_HOST:$POSTGRES_PORT"
    USE_DOCKER_EXEC=false
else
    # It's a container name - local docker
    if ! docker ps | grep -q "$POSTGRES_HOST"; then
        echo -e "${RED}❌ Error: $POSTGRES_HOST container is not running!${NC}"
        echo "   Please start the PostgreSQL container first"
        exit 1
    fi
    echo "   Detected local PostgreSQL container: $POSTGRES_HOST"
    USE_DOCKER_EXEC=true
fi
echo -e "${GREEN}✅ PostgreSQL connection OK${NC}"
echo ""

# Check if database already exists
echo -e "${YELLOW}Step 2: Checking if database exists...${NC}"
if [ "$USE_DOCKER_EXEC" = true ]; then
    DB_EXISTS=$(docker exec "$POSTGRES_HOST" psql -U "$POSTGRES_USER" -d postgres -t -c "SELECT 1 FROM pg_database WHERE datname = '$NEW_DB_NAME';" 2>/dev/null | tr -d ' ' || echo "")
else
    # Remote connection - need PGPASSWORD
    export PGPASSWORD="$POSTGRES_PASSWORD"
    DB_EXISTS=$(psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d postgres -t -c "SELECT 1 FROM pg_database WHERE datname = '$NEW_DB_NAME';" 2>/dev/null | tr -d ' ' || echo "")
fi

if [ "$DB_EXISTS" = "1" ]; then
    echo -e "${YELLOW}⚠️  Database '$NEW_DB_NAME' already exists.${NC}"
    read -p "Do you want to drop and recreate it? (yes/no): " confirm
    if [ "$confirm" = "yes" ]; then
        echo "Dropping existing database..."
        if [ "$USE_DOCKER_EXEC" = true ]; then
            docker exec "$POSTGRES_HOST" psql -U "$POSTGRES_USER" -d postgres -c "DROP DATABASE IF EXISTS \"$NEW_DB_NAME\";" 2>/dev/null || true
        else
            export PGPASSWORD="$POSTGRES_PASSWORD"
            psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d postgres -c "DROP DATABASE IF EXISTS \"$NEW_DB_NAME\";" 2>/dev/null || true
        fi
        echo "Creating new database..."
        if [ "$USE_DOCKER_EXEC" = true ]; then
            docker exec "$POSTGRES_HOST" psql -U "$POSTGRES_USER" -d postgres -c "
                CREATE DATABASE \"$NEW_DB_NAME\"
                    WITH OWNER = $POSTGRES_USER
                    ENCODING = 'UTF8'
                    LC_COLLATE = 'en_US.utf8'
                    LC_CTYPE = 'en_US.utf8'
                    TEMPLATE = template0;
            "
        else
            export PGPASSWORD="$POSTGRES_PASSWORD"
            psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d postgres -c "
                CREATE DATABASE \"$NEW_DB_NAME\"
                    WITH OWNER = $POSTGRES_USER
                    ENCODING = 'UTF8'
                    LC_COLLATE = 'en_US.utf8'
                    LC_CTYPE = 'en_US.utf8'
                    TEMPLATE = template0;
            "
        fi
        echo -e "${GREEN}✅ Database recreated${NC}"
    else
        echo -e "${YELLOW}⚠️  Using existing database${NC}"
    fi
else
    echo "Creating new database..."
    if [ "$USE_DOCKER_EXEC" = true ]; then
        docker exec "$POSTGRES_HOST" psql -U "$POSTGRES_USER" -d postgres -c "
            CREATE DATABASE \"$NEW_DB_NAME\"
                WITH OWNER = $POSTGRES_USER
                ENCODING = 'UTF8'
                LC_COLLATE = 'en_US.utf8'
                LC_CTYPE = 'en_US.utf8'
                TEMPLATE = template0;
        "
    else
        export PGPASSWORD="$POSTGRES_PASSWORD"
        psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d postgres -c "
            CREATE DATABASE \"$NEW_DB_NAME\"
                WITH OWNER = $POSTGRES_USER
                ENCODING = 'UTF8'
                LC_COLLATE = 'en_US.utf8'
                LC_CTYPE = 'en_US.utf8'
                TEMPLATE = template0;
        "
    fi
    echo -e "${GREEN}✅ Database created${NC}"
fi
echo ""

# Create hangfire schema
echo -e "${YELLOW}Step 3: Creating Hangfire schema...${NC}"
if [ "$USE_DOCKER_EXEC" = true ]; then
    docker exec "$POSTGRES_HOST" psql -U "$POSTGRES_USER" -d "$NEW_DB_NAME" -c "
        CREATE SCHEMA IF NOT EXISTS hangfire;
        GRANT ALL ON SCHEMA hangfire TO $POSTGRES_USER;
    " > /dev/null 2>&1
else
    export PGPASSWORD="$POSTGRES_PASSWORD"
    psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d "$NEW_DB_NAME" -c "
        CREATE SCHEMA IF NOT EXISTS hangfire;
        GRANT ALL ON SCHEMA hangfire TO $POSTGRES_USER;
    " > /dev/null 2>&1
fi

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Hangfire schema created${NC}"
else
    echo -e "${RED}❌ Failed to create Hangfire schema${NC}"
    exit 1
fi
echo ""

# Verify
echo -e "${YELLOW}Step 4: Verifying database...${NC}"
if [ "$USE_DOCKER_EXEC" = true ]; then
    SCHEMA_COUNT=$(docker exec "$POSTGRES_HOST" psql -U "$POSTGRES_USER" -d "$NEW_DB_NAME" -t -c "SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name IN ('public', 'hangfire');" 2>/dev/null | tr -d ' ')
else
    export PGPASSWORD="$POSTGRES_PASSWORD"
    SCHEMA_COUNT=$(psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d "$NEW_DB_NAME" -t -c "SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name IN ('public', 'hangfire');" 2>/dev/null | tr -d ' ')
fi

if [ "$SCHEMA_COUNT" -ge 2 ]; then
    echo -e "${GREEN}✅ Database verified: Found $SCHEMA_COUNT schemas (public, hangfire)${NC}"
else
    echo -e "${YELLOW}⚠️  Database created but schema count is unexpected: $SCHEMA_COUNT${NC}"
fi

echo ""
echo -e "${GREEN}=========================================="
echo "✅ Database creation completed!"
echo "==========================================${NC}"
echo ""
echo "Summary:"
echo "  - Database: $NEW_DB_NAME"
echo "  - Schemas: public (default), hangfire"
echo ""
echo "Next steps:"
echo "  1. Run migrations to create tables:"
echo "     docker compose -p fda_uat -f docker-compose.uat.yml up -d"
echo ""
echo "  2. Or manually run migrations:"
echo "     docker exec fdaapi_uat dotnet ef database update"
echo ""
echo "  3. Verify tables:"
echo "     docker exec -it $POSTGRES_HOST psql -U $POSTGRES_USER -d \"$NEW_DB_NAME\" -c \"\\dt public.*\""
echo ""

