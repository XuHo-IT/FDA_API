# UAT Deployment Scripts

Scripts hỗ trợ triển khai và quản lý môi trường UAT.

## 📋 Scripts

### 1. `setup_uat_environment.sh`
**Mục đích**: Setup môi trường UAT lần đầu (chạy 1 lần duy nhất)

**Usage**:
```bash
cd ~/FDA_API
chmod +x scripts/setup_uat_environment.sh
./scripts/setup_uat_environment.sh
```

**Chức năng**:
- Tạo Docker network `fda_api_default`
- Tạo thư mục backup
- Kiểm tra DEV containers đang chạy
- Verify network connectivity

---

### 2. `backup_uat_schema.sh`
**Mục đích**: Backup schema `uat_schema` từ database

**Usage**:
```bash
cd ~/FDA_API
./scripts/backup_uat_schema.sh
```

**Output**: `~/backup/uat_schema_YYYYMMDD_HHMMSS.sql.gz`

**Tự động**:
- Compress backup (gzip)
- Xóa backup cũ hơn 7 ngày

**Lưu ý**: Chạy script này **TRƯỚC** mỗi migration lớn hoặc deployment quan trọng.

---

### 3. `backup_dev_schema.sh`
**Mục đích**: Backup schema `public` (DEV) từ database

**Usage**:
```bash
cd ~/FDA_API
./scripts/backup_dev_schema.sh
```

**Output**: `~/backup/dev_schema_YYYYMMDD_HHMMSS.sql.gz`

---

### 4. `restore_uat_schema.sh`
**Mục đích**: Restore schema `uat_schema` từ backup file

**Usage**:
```bash
# List available backups
ls -lh ~/backup/uat_schema_*.sql.gz

# Restore
cd ~/FDA_API
./scripts/restore_uat_schema.sh ~/backup/uat_schema_20260129_120000.sql.gz
```

**⚠️ WARNING**: Script sẽ **REPLACE** toàn bộ `uat_schema` hiện tại. Đảm bảo đã backup trước khi restore.

---

## 🔄 Workflow Khuyến Nghị

### Trước khi deploy UAT:

```bash
# 1. Backup UAT schema
./scripts/backup_uat_schema.sh

# 2. Deploy (CI/CD sẽ tự động hoặc manual)
docker compose -p fda_uat -f docker-compose.uat.yml pull
docker compose -p fda_uat -f docker-compose.uat.yml up -d --force-recreate

# 3. Verify
docker compose -p fda_uat -f docker-compose.uat.yml logs -f
```

### Nếu deployment fail:

```bash
# 1. Stop container (chỉ khi thực sự cần)
docker compose -p fda_uat -f docker-compose.uat.yml down

# 2. Restore database (nếu cần)
./scripts/restore_uat_schema.sh ~/backup/uat_schema_<latest>.sql.gz

# 3. Rollback image (nếu cần)
docker pull xuhoit/fda_api-fdaapi:uat-<old-commit-sha>
docker tag xuhoit/fda_api-fdaapi:uat-<old-commit-sha> xuhoit/fda_api-fdaapi:uat
docker compose -p fda_uat -f docker-compose.uat.yml up -d --force-recreate
```

---

## 📅 Cron Job (Tùy chọn)

Backup tự động hàng ngày:

```bash
# Edit crontab
crontab -e

# Thêm dòng sau (backup UAT schema mỗi ngày lúc 2 AM)
0 2 * * * cd /home/dev/FDA_API && ./scripts/backup_uat_schema.sh >> ~/backup/backup.log 2>&1
```

---

## 🔍 Troubleshooting

### Script không chạy được:

```bash
# Check permissions
chmod +x scripts/*.sh

# Check .env file exists
ls -la .env

# Check postgres_dev container
docker ps | grep postgres_dev
```

### Backup file quá lớn:

```bash
# Check backup size
du -h ~/backup/*.sql.gz

# Manual cleanup (giữ lại 5 file mới nhất)
cd ~/backup
ls -t uat_schema_*.sql.gz | tail -n +6 | xargs rm -f
```

---

## 📝 Notes

- Tất cả scripts yêu cầu file `.env` trong thư mục project
- Scripts tự động load environment variables từ `.env`
- Backup được lưu tại `~/backup/`
- Retention: 7 ngày (có thể chỉnh trong script)

