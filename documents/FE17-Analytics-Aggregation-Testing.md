# FE-17 Analytics Aggregation - API Testing Guide

## 📋 Testing Principles

### ❌ ĐỪNG
- Expect API trả ngay analytics result
- Assert dữ liệu raw trong response

### ✅ PHẢI
- Test job được enqueue
- Test job chạy thành công
- Test analytics tables được ghi đúng
- Test API read chỉ đọc aggregated data

---

## 1. Test Group 1 – Trigger Aggregation APIs (Command APIs)

### 1.1 Trigger Frequency Aggregation (FeatG47)

**Endpoint:** `POST /api/v1/analytics/frequency/aggregate`

**Request:**
```json
{
  "bucketType": "day",
  "startDate": "2025-01-01T00:00:00Z",
  "endDate": "2025-01-31T23:59:59Z",
  "administrativeAreaIds": null
}
```

**Expected Response (202 Accepted):**
```json
{
  "success": true,
  "message": "Frequency aggregation job started",
  "statusCode": 202,
  "data": {
    "jobRunId": "550e8400-e29b-41d4-a716-446655440000",
    "jobType": "FREQUENCY_AGG",
    "status": "RUNNING",
    "startedAt": "2025-01-17T13:50:00Z"
  }
}
```

**Assertion Checklist:**
- ✅ HTTP status = 202 Accepted
- ✅ Response contains `jobRunId`
- ✅ Response contains `jobType = "FREQUENCY_AGG"`
- ✅ Response contains `status = "RUNNING"`
- ✅ Request does NOT block (returns immediately)
- ✅ Hangfire Dashboard shows new job
- ✅ `AnalyticsJobRuns` table has new record with `Status = "RUNNING"`

**Hangfire Dashboard Check:**
- Navigate to `/hangfire`
- Verify job appears in "Enqueued" or "Processing" queue
- Job method: `FrequencyAggregationBackgroundJob.ExecuteAsync`

---

### 1.2 Trigger Severity Aggregation (FeatG48)

**Endpoint:** `POST /api/v1/analytics/severity/aggregate`

**Request:**
```json
{
  "bucketType": "day",
  "startDate": "2025-01-01T00:00:00Z",
  "endDate": "2025-01-31T23:59:59Z",
  "administrativeAreaIds": ["550e8400-e29b-41d4-a716-446655440001"]
}
```

**Expected Response (202 Accepted):**
```json
{
  "success": true,
  "message": "Severity aggregation job started",
  "statusCode": 202,
  "data": {
    "jobRunId": "550e8400-e29b-41d4-a716-446655440002",
    "jobType": "SEVERITY_AGG",
    "status": "RUNNING",
    "startedAt": "2025-01-17T13:50:00Z"
  }
}
```

**Assertion Checklist:**
- ✅ HTTP status = 202 Accepted
- ✅ Response contains `jobRunId`
- ✅ Response contains `jobType = "SEVERITY_AGG"`
- ✅ Hangfire Dashboard shows new job
- ✅ `AnalyticsJobRuns` table has new record

---

### 1.3 Trigger Hotspot Aggregation (FeatG49)

**Endpoint:** `POST /api/v1/analytics/hotspots/aggregate`

**Request:**
```json
{
  "periodStart": "2025-01-01T00:00:00Z",
  "periodEnd": "2025-01-31T23:59:59Z",
  "topN": 20
}
```

**Expected Response (202 Accepted):**
```json
{
  "success": true,
  "message": "Hotspot aggregation job started",
  "statusCode": 202,
  "data": {
    "jobRunId": "550e8400-e29b-41d4-a716-446655440003",
    "jobType": "HOTSPOT_AGG",
    "status": "RUNNING",
    "startedAt": "2025-01-17T13:50:00Z"
  }
}
```

**Assertion Checklist:**
- ✅ HTTP status = 202 Accepted
- ✅ Response contains `jobRunId`
- ✅ Response contains `jobType = "HOTSPOT_AGG"`
- ✅ Hangfire Dashboard shows new job

---

### 1.4 Negative Test Cases

#### Case 1: Invalid Date Range
**Request:**
```json
{
  "bucketType": "day",
  "startDate": "2025-01-31T00:00:00Z",
  "endDate": "2025-01-01T00:00:00Z"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "Start date must be before end date",
  "statusCode": 400
}
```

#### Case 2: Missing Permission (Non-Admin User)
**Request:** Same as above, but with User role token (not Admin)

**Expected Response (403 Forbidden):**
```json
{
  "success": false,
  "message": "Forbidden",
  "statusCode": 403
}
```

#### Case 3: Invalid Bucket Type
**Request:**
```json
{
  "bucketType": "invalid",
  "startDate": "2025-01-01T00:00:00Z",
  "endDate": "2025-01-31T23:59:59Z"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "Bucket type must be one of: day, week, month, year",
  "statusCode": 400
}
```

#### Case 4: Duplicate Trigger (Idempotent)
**Request:** Same request sent twice

**Expected Response:**
- ✅ Both return 202 Accepted
- ✅ Two separate `AnalyticsJobRuns` records created
- ✅ Both jobs can run (idempotent at data level via upsert)

---

## 2. Test Group 2 – Background Job Execution (Integration Test)

### 2.1 Setup Requirements

**Test Environment:**
- PostgreSQL (Docker or test database)
- Hangfire with PostgreSQL storage
- Redis for cache testing (optional)

**Data Structure Overview:**

The analytics aggregation works with a hierarchical administrative area structure:
- **City** (level: `"city"`) - Top level, no parent
- **District** (level: `"district"`) - Parent is City
- **Ward** (level: `"ward"`) - Parent is District

**Data Flow:**
1. `AdministrativeAreas` → Defines geographic boundaries (city/district/ward)
2. `stations` → Belongs to an AdministrativeArea (ward level)
3. `SensorReadings` → Belongs to a Station, contains raw water level measurements
4. `FloodEvents` → Represents flood incidents in an AdministrativeArea
5. Analytics aggregation jobs process raw data and populate:
   - `FloodAnalyticsFrequency` → Event counts per area/time bucket
   - `FloodAnalyticsSeverity` → Severity metrics per area/time bucket
   - `FloodAnalyticsHotspots` → Hotspot rankings per area/period

**Important Notes:**
- `AdministrativeAreas` table does NOT have `CreatedAt`/`UpdatedAt` columns
- `stations` table requires `CreatedBy`, `UpdatedBy`, `CreatedAt`, `UpdatedAt`
- `SensorReadings` table name is `"SensorReadings"` (PascalCase, quoted in SQL)
- All table names use PascalCase (e.g., `"AdministrativeAreas"`, `"SensorReadings"`)

**Seed Data:**
```sql
-- 1. Create AdministrativeAreas (hierarchical: City → District → Ward)
-- Note: AdministrativeAreas table does NOT have CreatedAt/UpdatedAt columns
-- Structure: City (top level) → District (parent: city) → Ward (parent: district)

-- City (top level, no parent)
-- Geometry: Polygon covering Đà Nẵng city (approximate boundaries)
INSERT INTO "AdministrativeAreas" ("Id", "Name", "Level", "Code", "ParentId", "Geometry")
VALUES 
  ('550e8400-e29b-41d4-a716-446655440000', 'Đà Nẵng', 'city', 'DN', NULL, 
   '{"type":"Polygon","coordinates":[[[108.15,15.95],[108.25,15.95],[108.25,16.15],[108.15,16.15],[108.15,15.95]]]}');

-- District (parent: city)
-- Geometry: Polygon for Hải Châu district (central Đà Nẵng)
INSERT INTO "AdministrativeAreas" ("Id", "Name", "Level", "Code", "ParentId", "Geometry")
VALUES 
  ('550e8400-e29b-41d4-a716-446655440001', 'Hải Châu', 'district', 'HAI_CHAU', 
   '550e8400-e29b-41d4-a716-446655440000', 
   '{"type":"Polygon","coordinates":[[[108.20,16.04],[108.23,16.04],[108.23,16.07],[108.20,16.07],[108.20,16.04]]]}'),
  ('550e8400-e29b-41d4-a716-446655440002', 'Thanh Khê', 'district', 'THANH_KHE', 
   '550e8400-e29b-41d4-a716-446655440000', 
   '{"type":"Polygon","coordinates":[[[108.18,16.05],[108.21,16.05],[108.21,16.08],[108.18,16.08],[108.18,16.05]]]}');

-- Ward (parent: district)
-- Geometry: Polygons for wards (smaller areas within districts)
INSERT INTO "AdministrativeAreas" ("Id", "Name", "Level", "Code", "ParentId", "Geometry")
VALUES 
  ('550e8400-e29b-41d4-a716-446655440010', 'Phường Bình Hiên', 'ward', 'BINH_HIEN', 
   '550e8400-e29b-41d4-a716-446655440001', 
   '{"type":"Polygon","coordinates":[[[108.20,16.04],[108.215,16.04],[108.215,16.055],[108.20,16.055],[108.20,16.04]]]}'),
  ('550e8400-e29b-41d4-a716-446655440011', 'Phường Hải Châu I', 'ward', 'HAI_CHAU_1', 
   '550e8400-e29b-41d4-a716-446655440001', 
   '{"type":"Polygon","coordinates":[[[108.215,16.04],[108.23,16.04],[108.23,16.055],[108.215,16.055],[108.215,16.04]]]}'),
  ('550e8400-e29b-41d4-a716-446655440012', 'Phường Thanh Khê Tây', 'ward', 'THANH_KHE_TAY', 
   '550e8400-e29b-41d4-a716-446655440002', 
   '{"type":"Polygon","coordinates":[[[108.18,16.05],[108.195,16.05],[108.195,16.065],[108.18,16.065],[108.18,16.05]]]}');

-- 2. Create Stations with Thresholds
-- Note: stations table has CreatedBy, UpdatedBy, CreatedAt, UpdatedAt (all required)
-- Use a test user GUID for CreatedBy/UpdatedBy
INSERT INTO stations ("Id", "Code", "Name", "LocationDesc", "Latitude", "Longitude", "RoadName", 
                      "Direction", "Status", "AdministrativeAreaId", "ThresholdWarning", 
                      "ThresholdCritical", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt")
VALUES 
  ('550e8400-e29b-41d4-a716-446655440020', 'ST001', 'Trạm Bạch Đằng', 'Gần cầu Rồng, đường Bạch Đằng', 
   16.0544, 108.2225, 'Đường Bạch Đằng', 'upstream', 'active',
   '550e8400-e29b-41d4-a716-446655440010', 2.5, 3.5, 
   '00000000-0000-0000-0000-000000000001', NOW(), 
   '00000000-0000-0000-0000-000000000001', NOW()),
  ('550e8400-e29b-41d4-a716-446655440021', 'ST002', 'Trạm Trần Phú', 'Đường Trần Phú, quận Hải Châu', 
   16.0600, 108.2300, 'Đường Trần Phú', 'downstream', 'active',
   '550e8400-e29b-41d4-a716-446655440011', 2.0, 3.0, 
   '00000000-0000-0000-0000-000000000001', NOW(), 
   '00000000-0000-0000-0000-000000000001', NOW());

-- 3. Create SensorReadings (with exceedances)
-- Note: SensorReadings table name is "SensorReadings" (PascalCase)
-- Required fields: Id, StationId, Value, Distance, SensorHeight, Unit, Status, MeasuredAt, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt
INSERT INTO "SensorReadings" ("Id", "StationId", "Value", "Distance", "SensorHeight", "Unit", 
                              "Status", "MeasuredAt", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt")
VALUES 
  -- Readings that exceed threshold_warning (2.5) for Trạm Bạch Đằng
  ('550e8400-e29b-41d4-a716-446655440030', '550e8400-e29b-41d4-a716-446655440020', 3.0, 0.0, 0.0, 'cm', 0, 
   '2025-01-15 10:00:00+00', '00000000-0000-0000-0000-000000000001', NOW(), 
   '00000000-0000-0000-0000-000000000001', NOW()),
  ('550e8400-e29b-41d4-a716-446655440031', '550e8400-e29b-41d4-a716-446655440020', 3.2, 0.0, 0.0, 'cm', 0, 
   '2025-01-15 11:00:00+00', '00000000-0000-0000-0000-000000000001', NOW(), 
   '00000000-0000-0000-0000-000000000001', NOW()),
  -- Reading below threshold
  ('550e8400-e29b-41d4-a716-446655440032', '550e8400-e29b-41d4-a716-446655440020', 2.0, 0.0, 0.0, 'cm', 0, 
   '2025-01-15 12:00:00+00', '00000000-0000-0000-0000-000000000001', NOW(), 
   '00000000-0000-0000-0000-000000000001', NOW()),
  -- Critical level reading (exceeds threshold_critical 3.5)
  ('550e8400-e29b-41d4-a716-446655440033', '550e8400-e29b-41d4-a716-446655440020', 4.0, 0.0, 0.0, 'cm', 0, 
   '2025-01-15 13:00:00+00', '00000000-0000-0000-0000-000000000001', NOW(), 
   '00000000-0000-0000-0000-000000000001', NOW());
```

### 2.2 Test Flow: Severity Aggregation

**Step 1: Trigger Aggregation**
```bash
POST /api/v1/analytics/severity/aggregate
{
  "bucketType": "day",
  "startDate": "2025-01-15T00:00:00Z",
  "endDate": "2025-01-15T23:59:59Z",
  "administrativeAreaIds": ["550e8400-e29b-41d4-a716-446655440010"]
}
```

**Step 2: Wait for Job Completion**
- Poll Hangfire dashboard or use `GetJobStatus` API
- Wait until job status = "SUCCESS" or "FAILED"

**Step 3: Verify AnalyticsJobRun Table**
```sql
SELECT 
  "Id",
  "JobId",
  "Status",
  "StartedAt",
  "FinishedAt",
  "ExecutionTimeMs",
  "RecordsProcessed",
  "RecordsCreated",
  "ErrorMessage"
FROM "AnalyticsJobRuns"
WHERE "Id" = '<jobRunId>';
```

**Expected Results:**
| Field | Expected Value |
|-------|----------------|
| `Status` | `"SUCCESS"` |
| `StartedAt` | NOT NULL |
| `FinishedAt` | NOT NULL |
| `ExecutionTimeMs` | > 0 |
| `RecordsProcessed` | > 0 |
| `RecordsCreated` | > 0 |
| `ErrorMessage` | NULL |

**Step 4: Verify FloodAnalyticsSeverity Table**
```sql
SELECT 
  "Id",
  "AdministrativeAreaId",
  "TimeBucket",
  "BucketType",
  "MaxLevel",
  "AvgLevel",
  "MinLevel",
  "DurationHours",
  "ReadingCount",
  "CalculatedAt"
FROM "FloodAnalyticsSeverity"
WHERE "AdministrativeAreaId" = '550e8400-e29b-41d4-a716-446655440010'
  AND "TimeBucket" >= '2025-01-15 00:00:00'
  AND "TimeBucket" < '2025-01-16 00:00:00';
```

**Expected Results:**
| Field | Expected Value |
|-------|----------------|
| `AdministrativeAreaId` | `550e8400-e29b-41d4-a716-446655440010` (Phường Bình Hiên) |
| `TimeBucket` | `2025-01-15 00:00:00` |
| `BucketType` | `"day"` |
| `MaxLevel` | `4.0` (max of readings: 3.0, 3.2, 2.0, 4.0) |
| `AvgLevel` | `3.05` (average of readings: (3.0 + 3.2 + 2.0 + 4.0) / 4) |
| `MinLevel` | `2.0` (min of readings) |
| `DurationHours` | `3` (hours with value >= threshold_warning: 10:00, 11:00, 13:00) |
| `ReadingCount` | `4` (total readings) |
| `CalculatedAt` | NOT NULL |

**🔑 Important Assertions:**
- ✅ Data comes from aggregated table, NOT raw `sensor_readings`
- ✅ Severity calculated using `Value + Station.ThresholdWarning/ThresholdCritical`
- ✅ No duplicate rows (upsert works correctly)

---

### 2.3 Test Flow: Frequency Aggregation

**Step 1: Create FloodEvents (for frequency calculation)**
```sql
-- Note: FloodEvents table has CreatedAt (required)
INSERT INTO "FloodEvents" ("Id", "AdministrativeAreaId", "StartTime", "EndTime", "PeakLevel", "DurationHours", "CreatedAt")
VALUES 
  ('550e8400-e29b-41d4-a716-446655440040', '550e8400-e29b-41d4-a716-446655440010', 
   '2025-01-15 10:00:00+00', '2025-01-15 12:00:00+00', 3.2, 2, NOW());
```

**Step 2: Trigger Frequency Aggregation**
```bash
POST /api/v1/analytics/frequency/aggregate
{
    "bucketType": "day",
    "startDate": "2025-01-15T00:00:00Z",
    "endDate": "2025-01-15T23:59:59Z"
}
```

**Step 3: Verify FloodAnalyticsFrequency Table**
```sql
SELECT 
  "AdministrativeAreaId",
  "TimeBucket",
  "BucketType",
  "EventCount",
  "ExceedCount",
  "CalculatedAt"
FROM "FloodAnalyticsFrequency"
WHERE "AdministrativeAreaId" = '550e8400-e29b-41d4-a716-446655440010'
  AND "TimeBucket" = '2025-01-15 00:00:00';
```

**Expected Results:**
| Field | Expected Value |
|-------|----------------|
| `AdministrativeAreaId` | `550e8400-e29b-41d4-a716-446655440010` (Phường Bình Hiên) |
| `EventCount` | `1` (count of FloodEvents) |
| `ExceedCount` | `3` (count of readings >= threshold_warning: 3.0, 3.2, 4.0) |
| `TimeBucket` | `2025-01-15 00:00:00` |

---

### 2.4 Test Flow: Hotspot Aggregation

**Prerequisites:**
- Frequency and Severity data must exist first

**Step 1: Trigger Hotspot Aggregation**
```bash
POST /api/v1/analytics/hotspots/aggregate
{
  "periodStart": "2025-01-01T00:00:00Z",
  "periodEnd": "2025-01-31T23:59:59Z",
  "topN": 10
}
```

**Step 2: Verify FloodAnalyticsHotspots Table**
```sql
SELECT 
  "AdministrativeAreaId",
  "Score",
  "Rank",
  "PeriodStart",
  "PeriodEnd",
  "CalculatedAt"
FROM "FloodAnalyticsHotspots"
WHERE "PeriodStart" = '2025-01-01 00:00:00'
  AND "PeriodEnd" = '2025-01-31 23:59:59'
ORDER BY "Score" DESC;
```

**Expected Results:**
- ✅ Records sorted by `Score` DESC
- ✅ `Rank` assigned correctly (1, 2, 3, ...)
- ✅ Only top N records (if TopN specified)
- ✅ Score calculated: `(frequency * 0.4) + (severity * 0.35) + (duration * 0.25)`

---

## 3. Test Group 3 – Read APIs (Query APIs)

### 3.1 Get Frequency Analytics (FeatG51)

**Endpoint:** `GET /api/v1/analytics/frequency?administrativeAreaId={areaId}&startDate={start}&endDate={end}&bucketType={type}`

**Request:**
```
GET /api/v1/analytics/frequency?administrativeAreaId=550e8400-e29b-41d4-a716-446655440010&startDate=2025-01-15T00:00:00Z&endDate=2025-01-15T23:59:59Z&bucketType=day
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Frequency analytics retrieved successfully",
  "statusCode": 200,
  "data": {
    "administrativeAreaId": "550e8400-e29b-41d4-a716-446655440010",
    "administrativeAreaName": "Phường Bình Hiên",
    "bucketType": "day",
    "dataPoints": [
      {
        "timeBucket": "2025-01-15T00:00:00Z",
        "eventCount": 1,
        "exceedCount": 2,
        "calculatedAt": "2025-01-17T13:50:00Z"
      }
    ]
  }
}
```

**Assertion Checklist:**
- ✅ HTTP status = 200 OK
- ✅ Data comes from `FloodAnalyticsFrequency` table
- ✅ NO join with raw `sensor_readings` or `flood_events`
- ✅ Response contains `administrativeAreaName`
- ✅ `dataPoints` array contains aggregated data

**Cache Test:**
1. **First Call:** Cache miss
   - Response message: `"Frequency analytics retrieved successfully"`
   - Check Redis: Key exists with TTL = 3600 seconds

2. **Second Call (within 1 hour):** Cache hit
   - Response message: `"Frequency analytics retrieved successfully (cached)"`
   - Response time should be faster

3. **Modify Raw Data (should NOT affect cache):**
   ```sql
   INSERT INTO "SensorReadings" ("Id", "StationId", "Value", "Distance", "SensorHeight", "Unit", 
                                 "Status", "MeasuredAt", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt")
   VALUES ('550e8400-e29b-41d4-a716-446655440034', '550e8400-e29b-41d4-a716-446655440020', 4.0, 0.0, 0.0, 'cm', 0, 
          '2025-01-15 14:00:00+00', '00000000-0000-0000-0000-000000000001', NOW(), 
          '00000000-0000-0000-0000-000000000001', NOW());
   ```
   - Call API again → Should still return cached data (old aggregated data)
   - Cache does NOT reflect new raw data until aggregation runs again

---

### 3.2 Get Severity Analytics (FeatG52)

**Endpoint:** `GET /api/v1/analytics/severity?administrativeAreaId={areaId}&startDate={start}&endDate={end}&bucketType={type}`

**Request:**
```
GET /api/v1/analytics/severity?administrativeAreaId=550e8400-e29b-41d4-a716-446655440010&startDate=2025-01-15T00:00:00Z&endDate=2025-01-15T23:59:59Z&bucketType=day
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Severity analytics retrieved successfully",
  "statusCode": 200,
  "data": {
    "administrativeAreaId": "550e8400-e29b-41d4-a716-446655440010",
    "administrativeAreaName": "Phường Bình Hiên",
    "bucketType": "day",
    "dataPoints": [
      {
        "timeBucket": "2025-01-15T00:00:00Z",
        "maxLevel": 3.2,
        "avgLevel": 2.73,
        "minLevel": 2.0,
        "durationHours": 2,
        "readingCount": 3,
        "calculatedAt": "2025-01-17T13:50:00Z"
      }
    ]
  }
}
```

**Assertion Checklist:**
- ✅ HTTP status = 200 OK
- ✅ Data comes from `FloodAnalyticsSeverity` table
- ✅ NO join with raw `sensor_readings`
- ✅ `maxLevel`, `avgLevel`, `minLevel` are correct
- ✅ `durationHours` = hours with severity >= WARNING

---

### 3.3 Get Hotspot Rankings (FeatG53)

**Endpoint:** `GET /api/v1/analytics/hotspots?periodStart={start}&periodEnd={end}&topN={n}&areaLevel={level}`

**Request:**
```
GET /api/v1/analytics/hotspots?periodStart=2025-01-01T00:00:00Z&periodEnd=2025-01-31T23:59:59Z&topN=10&areaLevel=district
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Hotspot rankings retrieved successfully",
  "statusCode": 200,
  "data": {
    "periodStart": "2025-01-01T00:00:00Z",
    "periodEnd": "2025-01-31T23:59:59Z",
    "areaLevel": "district",
    "hotspots": [
      {
        "administrativeAreaId": "550e8400-e29b-41d4-a716-446655440001",
        "administrativeAreaName": "Hải Châu",
        "score": 85.5,
        "rank": 1,
        "calculatedAt": "2025-01-17T13:50:00Z"
      },
      {
        "administrativeAreaId": "550e8400-e29b-41d4-a716-446655440002",
        "administrativeAreaName": "Thanh Khê",
        "score": 72.3,
        "rank": 2,
        "calculatedAt": "2025-01-17T13:50:00Z"
      }
    ]
  }
}
```

**Assertion Checklist:**
- ✅ HTTP status = 200 OK
- ✅ Data comes from `FloodAnalyticsHotspots` table
- ✅ Hotspots sorted by `Score` DESC
- ✅ `Rank` assigned correctly (1, 2, 3, ...)
- ✅ Only top N records returned
- ✅ Cache TTL = 6 hours

---

### 3.4 Get Job Status (FeatG50)

**Endpoint:** `GET /api/v1/analytics/jobs/{jobRunId}`

**Request:**
```
GET /api/v1/analytics/jobs/550e8400-e29b-41d4-a716-446655440002
```

**Expected Response - Running (200 OK):**
```json
{
  "success": true,
  "message": "Job status retrieved successfully",
  "statusCode": 200,
  "data": {
    "jobRunId": "550e8400-e29b-41d4-a716-446655440002",
    "jobType": "SEVERITY_AGG",
    "status": "RUNNING",
    "startedAt": "2025-01-17T13:50:00Z",
    "finishedAt": null,
    "executionTimeMs": null,
    "recordsProcessed": 0,
    "recordsCreated": 0,
    "errorMessage": null
  }
}
```

**Expected Response - Success (200 OK):**
```json
{
  "success": true,
  "message": "Job status retrieved successfully",
  "statusCode": 200,
  "data": {
    "jobRunId": "550e8400-e29b-41d4-a716-446655440002",
    "jobType": "SEVERITY_AGG",
    "status": "SUCCESS",
    "startedAt": "2025-01-17T13:50:00Z",
    "finishedAt": "2025-01-17T13:50:15Z",
    "executionTimeMs": 15000,
    "recordsProcessed": 10,
    "recordsCreated": 5,
    "errorMessage": null
  }
}
```

**Expected Response - Failed (200 OK):**
```json
{
  "success": true,
  "message": "Job status retrieved successfully",
  "statusCode": 200,
  "data": {
    "jobRunId": "550e8400-e29b-41d4-a716-446655440002",
    "jobType": "SEVERITY_AGG",
    "status": "FAILED",
    "startedAt": "2025-01-17T13:50:00Z",
    "finishedAt": "2025-01-17T13:50:05Z",
    "executionTimeMs": 5000,
    "recordsProcessed": 3,
    "recordsCreated": 0,
    "errorMessage": "Database connection timeout"
  }
}
```

**Expected Response - Not Found (404):**
```json
{
  "success": false,
  "message": "Job run not found",
  "statusCode": 404
}
```

---

## 4. Test Group 4 – Failure & Retry

### 4.1 Force Job Failure

**Test Scenario:** Simulate database error during aggregation

**Setup:**
1. Temporarily break database connection in repository
2. Trigger aggregation job
3. Monitor job failure

**Expected Behavior:**
- ✅ Job status = "FAILED"
- ✅ `ErrorMessage` contains error details
- ✅ `FinishedAt` is set
- ✅ `ExecutionTimeMs` is recorded
- ✅ Hangfire shows job in "Failed" state
- ✅ No partial data written to analytics tables

**Verify:**
```sql
SELECT "Status", "ErrorMessage", "FinishedAt"
FROM "AnalyticsJobRuns"
WHERE "Id" = '<jobRunId>';
```

**Expected:**
- `Status` = `"FAILED"`
- `ErrorMessage` = NOT NULL
- `FinishedAt` = NOT NULL

---

### 4.2 Retry Success (Idempotent)

**Test Scenario:** Fix error and retry same aggregation

**Steps:**
1. Fix database connection
2. Trigger same aggregation again (same date range, same area)
3. Verify no duplicate rows

**Expected Behavior:**
- ✅ New `AnalyticsJobRun` record created
- ✅ Job runs successfully
- ✅ Analytics tables updated via UPSERT (no duplicates)
- ✅ Previous failed run remains in history

**Verify Idempotency:**
```sql
-- Should return 1 row (not 2) - upsert prevents duplicates
SELECT COUNT(*) 
FROM "FloodAnalyticsSeverity"
WHERE "AdministrativeAreaId" = '550e8400-e29b-41d4-a716-446655440010'
  AND "TimeBucket" = '2025-01-15 00:00:00'
  AND "BucketType" = 'day';
```

**Expected:** COUNT = 1 (upsert prevents duplicates)

---

## 5. API Test Scripts (curl / Postman)

### 5.1 Trigger Frequency Aggregation

```bash
curl -X POST \
  http://localhost:5000/api/v1/analytics/frequency/aggregate \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <ADMIN_TOKEN>" \
  -d '{
    "bucketType": "day",
    "startDate": "2025-01-01T00:00:00Z",
    "endDate": "2025-01-31T23:59:59Z",
    "administrativeAreaIds": null
  }'
```

**Save `jobRunId` from response for next steps**

---

### 5.2 Check Job Status

```bash
curl -X GET \
  "http://localhost:5000/api/v1/analytics/jobs/550e8400-e29b-41d4-a716-446655440002" \
  -H "Authorization: Bearer <USER_TOKEN>"
```

**Poll until `status = "SUCCESS"` or `"FAILED"`**

---

### 5.3 Get Frequency Analytics

```bash
curl -X GET \
  "http://localhost:5000/api/v1/analytics/frequency?administrativeAreaId=550e8400-e29b-41d4-a716-446655440010&startDate=2025-01-01T00:00:00Z&endDate=2025-01-31T23:59:59Z&bucketType=day" \
  -H "Authorization: Bearer <USER_TOKEN>"
```

---

### 5.4 Get Severity Analytics

```bash
curl -X GET \
  "http://localhost:5000/api/v1/analytics/severity?administrativeAreaId=550e8400-e29b-41d4-a716-446655440010&startDate=2025-01-01T00:00:00Z&endDate=2025-01-31T23:59:59Z&bucketType=day" \
  -H "Authorization: Bearer <USER_TOKEN>"
```

---

### 5.5 Get Hotspot Rankings

```bash
curl -X GET \
  "http://localhost:5000/api/v1/analytics/hotspots?periodStart=2025-01-01T00:00:00Z&periodEnd=2025-01-31T23:59:59Z&topN=20&areaLevel=district" \
  -H "Authorization: Bearer <USER_TOKEN>"
```

---

## 6. Test Matrix

| Test Layer | Tool | Purpose |
|------------|------|---------|
| **API Tests** | Postman / REST Client / curl | Test HTTP endpoints, request/response |
| **Integration Tests** | xUnit + WebApplicationFactory | Test full flow: API → Hangfire → DB |
| **Job Tests** | Hangfire Test Server | Test background job execution |
| **Database Tests** | PostgreSQL Docker | Test data persistence, upsert logic |
| **Cache Tests** | Redis Test Instance | Test caching behavior |

---

## 7. Test Order (Critical)

### ✅ Correct Order:
1. **Trigger API** → Get `jobRunId`
2. **Verify Hangfire Job** → Check job appears in dashboard
3. **Wait for Completion** → Poll `GetJobStatus` API
4. **Verify Analytics DB** → Query analytics tables directly
5. **Verify Read API** → Test query endpoints return aggregated data
6. **Verify Cache** → Test cache hit/miss behavior

### ❌ Wrong Order (Don't Do This):
- ❌ Call read API immediately after trigger (data not ready yet)
- ❌ Assert raw data in aggregation response
- ❌ Skip job status verification

---

## 8. Checklist "DONE chuẩn FE-17"

### Trigger APIs
- [ ] All trigger APIs return 202 Accepted
- [ ] Response contains `jobRunId`
- [ ] Response contains correct `jobType`
- [ ] Request does NOT block (returns immediately)
- [ ] Hangfire dashboard shows new job

### Background Jobs
- [ ] Hangfire job appears in queue
- [ ] Job executes successfully
- [ ] Job status updates correctly (RUNNING → SUCCESS/FAILED)
- [ ] Job retries on failure (if configured)
- [ ] Execution time is recorded

### Analytics Tables
- [ ] `AnalyticsJobRuns` table populated correctly
- [ ] `FloodAnalyticsFrequency` table populated correctly
- [ ] `FloodAnalyticsSeverity` table populated correctly
- [ ] `FloodAnalyticsHotspots` table populated correctly
- [ ] No duplicate rows after retry (upsert works)
- [ ] Data calculated correctly (max, avg, min, counts)

### Read APIs
- [ ] Read APIs return 200 OK
- [ ] Data comes from analytics tables (NOT raw tables)
- [ ] Response contains correct aggregated data
- [ ] Cache works correctly (hit/miss)
- [ ] Cache TTL is correct (1 hour for frequency/severity, 6 hours for hotspots)

### Error Handling
- [ ] Job failure is recorded correctly
- [ ] Error message is stored in `AnalyticsJobRuns`
- [ ] Failed jobs can be retried
- [ ] No partial data on failure

### Security
- [ ] Trigger APIs require Admin role
- [ ] Read APIs require User role
- [ ] Unauthorized requests return 403

---

## 9. Sample Test Data

### Administrative Areas
```sql
-- Note: AdministrativeAreas table does NOT have CreatedAt/UpdatedAt columns
-- Hierarchical structure: City → District → Ward

-- City (top level, no parent)
-- Geometry: Polygon covering Đà Nẵng city (approximate boundaries)
INSERT INTO "AdministrativeAreas" ("Id", "Name", "Level", "Code", "ParentId", "Geometry")
VALUES 
  ('550e8400-e29b-41d4-a716-446655440000', 'Đà Nẵng', 'city', 'DN', NULL, 
   '{"type":"Polygon","coordinates":[[[108.15,15.95],[108.25,15.95],[108.25,16.15],[108.15,16.15],[108.15,15.95]]]}');

-- Districts (parent: city)
-- Geometry: Polygons for districts (Hải Châu and Thanh Khê)
INSERT INTO "AdministrativeAreas" ("Id", "Name", "Level", "Code", "ParentId", "Geometry")
VALUES 
  ('550e8400-e29b-41d4-a716-446655440001', 'Hải Châu', 'district', 'HAI_CHAU', 
   '550e8400-e29b-41d4-a716-446655440000', 
   '{"type":"Polygon","coordinates":[[[108.20,16.04],[108.23,16.04],[108.23,16.07],[108.20,16.07],[108.20,16.04]]]}'),
  ('550e8400-e29b-41d4-a716-446655440002', 'Thanh Khê', 'district', 'THANH_KHE', 
   '550e8400-e29b-41d4-a716-446655440000', 
   '{"type":"Polygon","coordinates":[[[108.18,16.05],[108.21,16.05],[108.21,16.08],[108.18,16.08],[108.18,16.05]]]}');

-- Wards (parent: district)
-- Geometry: Polygons for wards (smaller areas within districts)
INSERT INTO "AdministrativeAreas" ("Id", "Name", "Level", "Code", "ParentId", "Geometry")
VALUES 
  ('550e8400-e29b-41d4-a716-446655440010', 'Phường Bình Hiên', 'ward', 'BINH_HIEN', 
   '550e8400-e29b-41d4-a716-446655440001', 
   '{"type":"Polygon","coordinates":[[[108.20,16.04],[108.215,16.04],[108.215,16.055],[108.20,16.055],[108.20,16.04]]]}'),
  ('550e8400-e29b-41d4-a716-446655440011', 'Phường Hải Châu I', 'ward', 'HAI_CHAU_1', 
   '550e8400-e29b-41d4-a716-446655440001', 
   '{"type":"Polygon","coordinates":[[[108.215,16.04],[108.23,16.04],[108.23,16.055],[108.215,16.055],[108.215,16.04]]]}'),
  ('550e8400-e29b-41d4-a716-446655440012', 'Phường Thanh Khê Tây', 'ward', 'THANH_KHE_TAY', 
   '550e8400-e29b-41d4-a716-446655440002', 
   '{"type":"Polygon","coordinates":[[[108.18,16.05],[108.195,16.05],[108.195,16.065],[108.18,16.065],[108.18,16.05]]]}');
```

### Stations with Thresholds
```sql
-- Note: stations table requires CreatedBy, UpdatedBy, CreatedAt, UpdatedAt
-- Use test user GUID for CreatedBy/UpdatedBy
INSERT INTO stations ("Id", "Code", "Name", "LocationDesc", "Latitude", "Longitude", "RoadName", 
                      "Direction", "Status", "AdministrativeAreaId", "ThresholdWarning", 
                      "ThresholdCritical", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt")
VALUES 
  ('550e8400-e29b-41d4-a716-446655440020', 'ST001', 'Trạm Bạch Đằng', 'Gần cầu Rồng, đường Bạch Đằng', 
   16.0544, 108.2225, 'Đường Bạch Đằng', 'upstream', 'active',
   '550e8400-e29b-41d4-a716-446655440010', 2.5, 3.5, 
   '00000000-0000-0000-0000-000000000001', NOW(), 
   '00000000-0000-0000-0000-000000000001', NOW()),
  ('550e8400-e29b-41d4-a716-446655440021', 'ST002', 'Trạm Trần Phú', 'Đường Trần Phú, quận Hải Châu', 
   16.0600, 108.2300, 'Đường Trần Phú', 'downstream', 'active',
   '550e8400-e29b-41d4-a716-446655440011', 2.0, 3.0, 
   '00000000-0000-0000-0000-000000000001', NOW(), 
   '00000000-0000-0000-0000-000000000001', NOW());
```

### Sensor Readings (with Exceedances)
```sql
-- Note: SensorReadings table name is "SensorReadings" (PascalCase)
-- Required fields: Id, StationId, Value, Distance, SensorHeight, Unit, Status, MeasuredAt, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt
-- Readings that exceed threshold_warning (2.5) for Trạm Bạch Đằng
INSERT INTO "SensorReadings" ("Id", "StationId", "Value", "Distance", "SensorHeight", "Unit", 
                              "Status", "MeasuredAt", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt")
VALUES 
  ('550e8400-e29b-41d4-a716-446655440030', '550e8400-e29b-41d4-a716-446655440020', 3.0, 0.0, 0.0, 'cm', 0, 
   '2025-01-15 10:00:00+00', '00000000-0000-0000-0000-000000000001', NOW(), 
   '00000000-0000-0000-0000-000000000001', NOW()),
  ('550e8400-e29b-41d4-a716-446655440031', '550e8400-e29b-41d4-a716-446655440020', 3.2, 0.0, 0.0, 'cm', 0, 
   '2025-01-15 11:00:00+00', '00000000-0000-0000-0000-000000000001', NOW(), 
   '00000000-0000-0000-0000-000000000001', NOW()),
  -- Reading below threshold
  ('550e8400-e29b-41d4-a716-446655440032', '550e8400-e29b-41d4-a716-446655440020', 2.0, 0.0, 0.0, 'cm', 0, 
   '2025-01-15 12:00:00+00', '00000000-0000-0000-0000-000000000001', NOW(), 
   '00000000-0000-0000-0000-000000000001', NOW()),
  -- Critical level reading (exceeds threshold_critical 3.5)
  ('550e8400-e29b-41d4-a716-446655440033', '550e8400-e29b-41d4-a716-446655440020', 4.0, 0.0, 0.0, 'cm', 0, 
   '2025-01-15 13:00:00+00', '00000000-0000-0000-0000-000000000001', NOW(), 
   '00000000-0000-0000-0000-000000000001', NOW());
```

### Flood Events
```sql
-- Note: FloodEvents table has CreatedAt (required)
INSERT INTO "FloodEvents" ("Id", "AdministrativeAreaId", "StartTime", "EndTime", "PeakLevel", "DurationHours", "CreatedAt")
VALUES 
  ('550e8400-e29b-41d4-a716-446655440040', '550e8400-e29b-41d4-a716-446655440010', 
   '2025-01-15 10:00:00+00', '2025-01-15 12:00:00+00', 3.2, 2, NOW());
```

---

## 10. Common Test Scenarios

### Scenario 1: Full Aggregation Flow
1. Seed data (AdministrativeAreas, Stations, SensorReadings)
2. Trigger Frequency Aggregation → Get `jobRunId1`
3. Trigger Severity Aggregation → Get `jobRunId2`
4. Wait for both jobs to complete
5. Verify analytics tables populated
6. Trigger Hotspot Aggregation → Get `jobRunId3`
7. Wait for hotspot job to complete
8. Query all analytics endpoints
9. Verify cache behavior

### Scenario 2: Multiple Time Buckets
1. Create readings across multiple days (Jan 1-5)
2. Trigger aggregation with `bucketType = "day"`
3. Verify 5 records created (one per day)
4. Trigger aggregation with `bucketType = "week"`
5. Verify 1 record created (one per week)

### Scenario 3: Empty Data
1. Trigger aggregation for area with no readings
2. Verify job completes successfully
3. Verify analytics table has record with `EventCount = 0`, `ExceedCount = 0`

### Scenario 4: Large Dataset
1. Create 1000+ sensor readings
2. Trigger aggregation
3. Verify job completes within reasonable time (< 5 minutes)
4. Verify all records processed correctly

---

## 11. Performance Benchmarks

| Metric | Expected Value |
|--------|----------------|
| Trigger API Response Time | < 100ms |
| Job Execution (1000 readings) | < 30 seconds |
| Read API Response Time (cached) | < 50ms |
| Read API Response Time (uncached) | < 500ms |

---

## 12. Troubleshooting

### Issue: Job Stuck in RUNNING
**Check:**
- Hangfire dashboard shows job status
- Database connection is active
- No exceptions in application logs

**Solution:**
- Check Hangfire logs
- Verify database connectivity
- Restart Hangfire server if needed

### Issue: No Data in Analytics Tables
**Check:**
- Job status = SUCCESS?
- `RecordsCreated` > 0?
- Date range matches seed data?

**Solution:**
- Verify seed data exists
- Check date range in request
- Verify administrative area IDs are correct

### Issue: Cache Not Working
**Check:**
- Redis connection string configured?
- Cache key format correct?
- TTL set correctly?

**Solution:**
- Verify Redis is running
- Check cache key in Redis: `analytics:frequency:{areaId}:{bucketType}:{startDate}:{endDate}`
- Verify TTL = 3600 seconds

---

## 13. Postman Collection Structure

```
FE-17 Analytics Aggregation
├── Trigger Aggregation
│   ├── Frequency Aggregation
│   ├── Severity Aggregation
│   └── Hotspot Aggregation
├── Job Management
│   └── Get Job Status
└── Query Analytics
    ├── Get Frequency Analytics
    ├── Get Severity Analytics
    └── Get Hotspot Rankings
```

---

## 14. Notes

- **FE-18 (Hotspots Map) is NOT implemented yet**
- FeatG49 (Hotspot Aggregation) and FeatG53 (Get Hotspot Rankings) are prepared for future integration
- All table names use PascalCase (e.g., `FloodAnalyticsFrequency`, not `flood_analytics_frequency`)
- Hangfire dashboard available at `/hangfire` (requires authentication)

