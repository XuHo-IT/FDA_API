# FE-17 – Analytics Aggregation

> **Feature Name**: Analytics Aggregation (Frequency, Severity, Hotspots)
> **Created**: 2026-01-17
> **Status**: 🟡 Planning
> **Backend Features**: FeatG47 (Frequency Aggregation), FeatG48 (Severity Aggregation), FeatG49 (Hotspot Aggregation), FeatG50 (Get Job Status), FeatG51 (Get Frequency Analytics), FeatG52 (Get Severity Analytics), FeatG53 (Get Hotspot Rankings), FeatG57-66 (Additional Analytics Features)
> **Priority**: High
> **Related**: FE-16 (View Flood History & Trends), FE-18 (Hotspots Map - ⚠️ Not yet implemented)
> 
> **⚠️ Note**: FE-18 (Hotspots Map) is not yet implemented. FeatG49 (Hotspot Aggregation) and FeatG53 (Get Hotspot Rankings) are prepared for future FE-18 integration but can be used independently for reporting and analysis purposes.

### Feature Numbering Reference
> **Feature Numbers Used**: FeatG47-53, FeatG57-66
> - **FeatG47**: Frequency Aggregation Job
> - **FeatG48**: Severity Aggregation Job  
> - **FeatG49**: Hotspot Aggregation Job (⚠️ Prepared for FE-18)
> - **FeatG50**: Get Aggregation Job Status
> - **FeatG51**: Get Frequency Analytics (for FE-16)
> - **FeatG52**: Get Severity Analytics (for FE-16)
> - **FeatG53**: Get Hotspot Rankings (⚠️ Prepared for FE-18)
> - **FeatG57**: Create Administrative Area
> - **FeatG58**: List Administrative Areas
> - **FeatG59**: Get Administrative Area
> - **FeatG60**: Update Administrative Area
> - **FeatG61**: Delete Administrative Area
> - **FeatG62**: Create Flood Event
> - **FeatG63**: List Flood Events
> - **FeatG64**: Get Flood Event
> - **FeatG65**: Update Flood Event
> - **FeatG66**: Delete Flood Event
> 
> **Note**: Feature numbers FeatG47-53 and FeatG57-66 are assigned to FE-17. Features 57-61 (AdministrativeArea management) and 62-66 (FloodEvent management) support the analytics aggregation system by providing data management capabilities. Previous features: FeatG7 (Auth), FeatG28-30 (FE-07), FeatG32-37 (Area Management), FeatG39-41 (FE-16), FeatG42-46 (Alerts & Flood History).

---

## 📋 TABLE OF CONTENTS

1. [Executive Summary](#executive-summary)
2. [Feature Analysis](#feature-analysis)
3. [Domain Model](#domain-model)
4. [API Specifications](#api-specifications)
5. [Business Workflow](#business-workflow)
6. [Implementation Plan](#implementation-plan)
7. [Database Schema Requirements](#database-schema-requirements)
8. [Testing Strategy](#testing-strategy)
9. [Performance Considerations](#performance-considerations)

---

## 📊 EXECUTIVE SUMMARY

### Feature Overview

**Problem**: The system needs to aggregate raw flood sensor data into analytics metrics (frequency, severity, hotspots) to enable:
- Fast queries for FE-16 (History & Trends) without scanning millions of raw readings
- Efficient hotspot identification for FE-18 (Map visualization - ⚠️ Not yet implemented, but endpoints prepared)
- Historical analysis and reporting for authorities
- Data-driven decision making for flood management

**Solution**: Implement a batch aggregation system that:
- Runs scheduled jobs to compute analytics metrics
- Stores pre-aggregated results in dedicated analytics tables
- Provides cache layer for fast retrieval
- Supports manual re-aggregation for data corrections

### Backend Features to Implement

| Feature | Endpoint | Type | Description | Status |
|---------|----------|------|-------------|--------|
| **FeatG47** | `POST /api/v1/analytics/aggregate-frequency` | Command | Trigger frequency aggregation job | 🟡 New |
| **FeatG48** | `POST /api/v1/analytics/aggregate-severity` | Command | Trigger severity aggregation job | 🟡 New |
| **FeatG49** | `POST /api/v1/analytics/aggregate-hotspots` | Command | Trigger hotspot aggregation job | 🟡 New |
| **FeatG50** | `GET /api/v1/analytics/jobs/{jobId}/status` | Query | Get aggregation job status | 🟡 New |
| **FeatG51** | `GET /api/v1/analytics/frequency` | Query | Get frequency analytics (consumed by FE-16) | 🟡 New |
| **FeatG52** | `GET /api/v1/analytics/severity` | Query | Get severity analytics (consumed by FE-16) | 🟡 New |
| **FeatG53** | `GET /api/v1/analytics/hotspots` | Query | Get hotspot rankings (⚠️ Prepared for FE-18, not yet implemented) | 🟡 New |
| **FeatG57** | `POST /api/v1/admin/administrative-areas` | Command | Create administrative area | 🟡 New |
| **FeatG58** | `GET /api/v1/admin/administrative-areas` | Query | List administrative areas | 🟡 New |
| **FeatG59** | `GET /api/v1/admin/administrative-areas/{id}` | Query | Get administrative area | 🟡 New |
| **FeatG60** | `PUT /api/v1/admin/administrative-areas/{id}` | Command | Update administrative area | 🟡 New |
| **FeatG61** | `DELETE /api/v1/admin/administrative-areas/{id}` | Command | Delete administrative area | 🟡 New |
| **FeatG62** | `POST /api/v1/admin/flood-events` | Command | Create flood event | 🟡 New |
| **FeatG63** | `GET /api/v1/admin/flood-events` | Query | List flood events | 🟡 New |
| **FeatG64** | `GET /api/v1/admin/flood-events/{id}` | Query | Get flood event | 🟡 New |
| **FeatG65** | `PUT /api/v1/admin/flood-events/{id}` | Command | Update flood event | 🟡 New |
| **FeatG66** | `DELETE /api/v1/admin/flood-events/{id}` | Command | Delete flood event | 🟡 New |

### Key Requirements

1. **Batch Aggregation**: Scheduled jobs (daily/weekly/monthly) to compute metrics
2. **Idempotent Jobs**: Re-running jobs should not double-count data
3. **Time Buckets**: Support day/week/month/year granularity
4. **Area-Based Aggregation**: Group by administrative areas (ward/district)
5. **Cache Layer**: Redis cache for frequently accessed analytics
6. **Job Management**: Track job runs, status, errors for debugging

---

## 🔍 FEATURE ANALYSIS

### Key Requirements

#### 1. Analytics Metrics

| Metric | Definition | Input Data | Output Table |
|--------|------------|------------|--------------|
| **Frequency** | Number of flood events / threshold exceedances | `flood_events`, `flood_measurements` | `flood_analytics_frequency` |
| **Severity** | Max/avg water level, duration above threshold | `flood_measurements` | `flood_analytics_severity` |
| **Hotspots** | Ranking of areas by flood risk score | Frequency + Severity | `flood_analytics_hotspots` |

#### 2. Time Buckets

| Bucket Type | Use Case | Retention |
|-------------|----------|-----------|
| **Day** | Daily trends, 30-day charts | 1 year |
| **Week** | Weekly patterns, seasonal analysis | 2 years |
| **Month** | Monthly reports, long-term trends | 5 years |
| **Year** | Annual summaries, historical analysis | Forever |

#### 3. Business Rules

| Rule | Description | Status |
|------|-------------|--------|
| **Idempotent Aggregation** | Re-running job for same bucket replaces data, doesn't append | 🟡 New |
| **Missing Data Handling** | Flag incomplete buckets, don't block entire job | 🟡 New |
| **Late Data** | Support backfilling historical buckets | 🟡 New |
| **Cache Invalidation** | Invalidate cache when re-aggregating | 🟡 New |
| **Job Scheduling** | Daily at 2 AM, Weekly on Monday, Monthly on 1st | 🟡 New |

---

## 🗄️ DOMAIN MODEL

### Input Tables (Raw & Domain Data)

#### 1. `flood_measurements` / `sensor_readings`

Raw timeseries water level data.

```sql
-- Already exists
CREATE TABLE sensor_readings (
    id              UUID PRIMARY KEY,
    station_id      UUID NOT NULL REFERENCES stations(id),
    value           DOUBLE PRECISION NOT NULL,  -- Water level
    measured_at     TIMESTAMPTZ NOT NULL,
    severity_level  INT,                        -- 0=safe, 1=caution, 2=warning, 3=critical
    created_at      TIMESTAMPTZ NOT NULL
);
```

#### 2. `flood_events`

Represents a flood event (derived from raw data or ingested).

```sql
-- May need to be created
CREATE TABLE flood_events (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    area_id         UUID NOT NULL REFERENCES areas(id),
    start_time      TIMESTAMPTZ NOT NULL,
    end_time        TIMESTAMPTZ NOT NULL,
    peak_level      NUMERIC(14,4),
    duration_hours  INT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

#### 3. `stations`

Station metadata for spatial grouping.

```sql
-- Already exists
CREATE TABLE stations (
    id              UUID PRIMARY KEY,
    area_id         UUID REFERENCES areas(id),
    code            VARCHAR(50),
    name            VARCHAR(255),
    latitude        NUMERIC(10,6),
    longitude       NUMERIC(10,6),
    status          VARCHAR(20)
);
```

#### 4. `areas` / `administrative_units`

Administrative boundaries for area-based aggregation.

```sql
-- Already exists (or similar)
CREATE TABLE areas (
    id              UUID PRIMARY KEY,
    name            VARCHAR(255),
    level           VARCHAR(20),  -- 'ward', 'district', 'city'
    geometry        GEOMETRY(POLYGON),  -- Optional: PostGIS
    parent_id       UUID REFERENCES areas(id)
);
```

### Analytics Output Tables (Core of FE-17)

#### 5. `flood_analytics_frequency`

Stores frequency metrics (event count, exceed count).

```sql
CREATE TABLE flood_analytics_frequency (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    area_id         UUID NOT NULL REFERENCES areas(id),
    time_bucket     TIMESTAMPTZ NOT NULL,  -- Truncated to bucket start
    bucket_type     VARCHAR(20) NOT NULL,  -- 'day', 'week', 'month', 'year'
    event_count     INT DEFAULT 0,         -- Number of flood events
    exceed_count    INT DEFAULT 0,         -- Number of threshold exceedances
    calculated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_frequency_area_bucket UNIQUE (area_id, time_bucket, bucket_type)
);

CREATE INDEX ix_frequency_area_bucket ON flood_analytics_frequency(area_id, time_bucket);
CREATE INDEX ix_frequency_bucket_type ON flood_analytics_frequency(bucket_type, time_bucket);
```

#### 6. `flood_analytics_severity`

Stores severity metrics (max/avg level, duration).

```sql
CREATE TABLE flood_analytics_severity (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    area_id         UUID NOT NULL REFERENCES areas(id),
    time_bucket     TIMESTAMPTZ NOT NULL,
    bucket_type     VARCHAR(20) NOT NULL,
    max_level       NUMERIC(14,4),
    avg_level       NUMERIC(14,4),
    min_level       NUMERIC(14,4),
    duration_hours  INT DEFAULT 0,         -- Hours above threshold
    reading_count   INT DEFAULT 0,         -- Number of readings in bucket
    calculated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_severity_area_bucket UNIQUE (area_id, time_bucket, bucket_type)
);

CREATE INDEX ix_severity_area_bucket ON flood_analytics_severity(area_id, time_bucket);
CREATE INDEX ix_severity_bucket_type ON flood_analytics_severity(bucket_type, time_bucket);
```

#### 7. `flood_analytics_hotspots`

Stores hotspot rankings.

```sql
CREATE TABLE flood_analytics_hotspots (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    area_id         UUID NOT NULL REFERENCES areas(id),
    score           NUMERIC(14,4) NOT NULL,  -- Calculated risk score
    rank            INT,                       -- Ranking (1 = highest risk)
    period_start    TIMESTAMPTZ NOT NULL,     -- Period start
    period_end      TIMESTAMPTZ NOT NULL,     -- Period end
    calculated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_hotspot_area_period UNIQUE (area_id, period_start, period_end)
);

CREATE INDEX ix_hotspot_score ON flood_analytics_hotspots(score DESC, calculated_at DESC);
CREATE INDEX ix_hotspot_area ON flood_analytics_hotspots(area_id);
```

### Aggregation & Job Management Tables

#### 8. `analytics_jobs`

Manages aggregation job definitions.

```sql
CREATE TABLE analytics_jobs (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    job_type        VARCHAR(50) NOT NULL,  -- 'FREQUENCY_AGG', 'SEVERITY_AGG', 'HOTSPOT_AGG'
    schedule        VARCHAR(100),            -- Cron expression or schedule description
    is_active       BOOLEAN DEFAULT true,
    last_run_at     TIMESTAMPTZ,
    next_run_at     TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_job_type UNIQUE (job_type)
);

CREATE INDEX ix_jobs_type ON analytics_jobs(job_type);
CREATE INDEX ix_jobs_active ON analytics_jobs(is_active, next_run_at);
```

#### 9. `analytics_job_runs`

Logs each job execution.

```sql
CREATE TABLE analytics_job_runs (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    job_id          UUID NOT NULL REFERENCES analytics_jobs(id),
    started_at      TIMESTAMPTZ NOT NULL,
    finished_at     TIMESTAMPTZ,
    status          VARCHAR(20) NOT NULL,  -- 'RUNNING', 'SUCCESS', 'FAILED', 'CANCELLED'
    error_message   TEXT,
    records_processed INT,
    records_created  INT,
    execution_time_ms INT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_job_runs_job ON analytics_job_runs(job_id, started_at DESC);
CREATE INDEX ix_job_runs_status ON analytics_job_runs(status, started_at DESC);
```

### ERD Relationship Summary

```
areas
 ├── stations
 │    └── sensor_readings
 ├── flood_events
 ├── flood_analytics_frequency
 ├── flood_analytics_severity
 └── flood_analytics_hotspots

analytics_jobs
 └── analytics_job_runs
```

---

## 🔌 API SPECIFICATIONS

### FeatG47: Trigger Frequency Aggregation

**Endpoint**: `POST /api/v1/analytics/aggregate-frequency`
**Authorization**: `Policies("Admin")` - Admin only
**Pattern**: MediatR + Background Job

#### Request Body

```json
{
  "bucketType": "day",
  "startDate": "2026-01-01T00:00:00Z",
  "endDate": "2026-01-31T23:59:59Z",
  "areaIds": ["550e8400-e29b-41d4-a716-446655440000"]  // Optional: specific areas
}
```

#### Success Response (202 Accepted)

```json
{
  "success": true,
  "message": "Frequency aggregation job started",
  "data": {
    "jobRunId": "660e8400-e29b-41d4-a716-446655440000",
    "jobType": "FREQUENCY_AGG",
    "status": "RUNNING",
    "startedAt": "2026-01-17T10:00:00Z"
  }
}
```

---

### FeatG48: Trigger Severity Aggregation

**Endpoint**: `POST /api/v1/analytics/aggregate-severity`
**Authorization**: `Policies("Admin")`
**Pattern**: MediatR + Background Job

#### Request Body

```json
{
  "bucketType": "day",
  "startDate": "2026-01-01T00:00:00Z",
  "endDate": "2026-01-31T23:59:59Z",
  "areaIds": null  // null = all areas
}
```

#### Success Response (202 Accepted)

```json
{
  "success": true,
  "message": "Severity aggregation job started",
  "data": {
    "jobRunId": "660e8400-e29b-41d4-a716-446655440001",
    "jobType": "SEVERITY_AGG",
    "status": "RUNNING",
    "startedAt": "2026-01-17T10:00:00Z"
  }
}
```

---

### FeatG49: Trigger Hotspot Aggregation

**Endpoint**: `POST /api/v1/analytics/aggregate-hotspots`
**Authorization**: `Policies("Admin")`
**Pattern**: MediatR + Background Job

#### Request Body

```json
{
  "periodStart": "2026-01-01T00:00:00Z",
  "periodEnd": "2026-01-31T23:59:59Z",
  "topN": 50  // Optional: top N hotspots (default: all)
}
```

#### Success Response (202 Accepted)

```json
{
  "success": true,
  "message": "Hotspot aggregation job started",
  "data": {
    "jobRunId": "660e8400-e29b-41d4-a716-446655440002",
    "jobType": "HOTSPOT_AGG",
    "status": "RUNNING",
    "startedAt": "2026-01-17T10:00:00Z"
  }
}
```

---

### FeatG50: Get Aggregation Job Status

**Endpoint**: `GET /api/v1/analytics/jobs/{jobRunId}/status`
**Authorization**: `Policies("Admin", "Authority")`
**Pattern**: MediatR

#### Success Response (200 OK)

```json
{
  "success": true,
  "data": {
    "jobRunId": "660e8400-e29b-41d4-a716-446655440000",
    "jobType": "FREQUENCY_AGG",
    "status": "SUCCESS",
    "startedAt": "2026-01-17T10:00:00Z",
    "finishedAt": "2026-01-17T10:05:23Z",
    "executionTimeMs": 323000,
    "recordsProcessed": 15000,
    "recordsCreated": 1200,
    "errorMessage": null
  }
}
```

---

### FeatG51: Get Frequency Analytics

**Endpoint**: `GET /api/v1/analytics/frequency`
**Authorization**: `Policies("User")` - Authenticated users
**Pattern**: MediatR + Cache

#### Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `areaId` | UUID | No | Filter by area |
| `startDate` | DateTime | No | Start of period |
| `endDate` | DateTime | No | End of period |
| `bucketType` | string | No | day/week/month/year (default: day) |

#### Success Response (200 OK)

```json
{
  "success": true,
  "data": {
    "areaId": "550e8400-e29b-41d4-a716-446655440000",
    "areaName": "District 1",
    "bucketType": "day",
    "dataPoints": [
      {
        "timeBucket": "2026-01-15T00:00:00Z",
        "eventCount": 3,
        "exceedCount": 12,
        "calculatedAt": "2026-01-16T02:00:00Z"
      },
      {
        "timeBucket": "2026-01-16T00:00:00Z",
        "eventCount": 2,
        "exceedCount": 8,
        "calculatedAt": "2026-01-17T02:00:00Z"
      }
    ]
  }
}
```

---

### FeatG52: Get Severity Analytics

**Endpoint**: `GET /api/v1/analytics/severity`
**Authorization**: `Policies("User")`
**Pattern**: MediatR + Cache

#### Query Parameters

Same as FeatG46.

#### Success Response (200 OK)

```json
{
  "success": true,
  "data": {
    "areaId": "550e8400-e29b-41d4-a716-446655440000",
    "areaName": "District 1",
    "bucketType": "day",
    "dataPoints": [
      {
        "timeBucket": "2026-01-15T00:00:00Z",
        "maxLevel": 3.2,
        "avgLevel": 1.8,
        "minLevel": 0.5,
        "durationHours": 6,
        "readingCount": 288,
        "calculatedAt": "2026-01-16T02:00:00Z"
      }
    ]
  }
}
```

---

### FeatG53: Get Hotspot Rankings

**Endpoint**: `GET /api/v1/analytics/hotspots`
**Authorization**: `Policies("User")`
**Pattern**: MediatR + Cache

**⚠️ Note**: This endpoint is prepared for FE-18 (Hotspots Map) which is not yet implemented. The endpoint can be used independently for reporting, analysis, or future FE-18 integration.

#### Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `periodStart` | DateTime | No | Period start (default: last 30 days) |
| `periodEnd` | DateTime | No | Period end (default: now) |
| `topN` | int | No | Top N hotspots (default: 20) |
| `areaLevel` | string | No | ward/district (default: district) |

#### Success Response (200 OK)

```json
{
  "success": true,
  "data": {
    "periodStart": "2026-01-01T00:00:00Z",
    "periodEnd": "2026-01-31T23:59:59Z",
    "areaLevel": "district",
    "hotspots": [
      {
        "areaId": "550e8400-e29b-41d4-a716-446655440000",
        "areaName": "District 1",
        "score": 85.5,
        "rank": 1,
        "frequencyScore": 40.0,
        "severityScore": 35.5,
        "durationScore": 10.0,
        "calculatedAt": "2026-02-01T02:00:00Z"
      },
      {
        "areaId": "550e8400-e29b-41d4-a716-446655440001",
        "areaName": "District 2",
        "score": 72.3,
        "rank": 2,
        "frequencyScore": 35.0,
        "severityScore": 28.3,
        "durationScore": 9.0,
        "calculatedAt": "2026-02-01T02:00:00Z"
      }
    ]
  }
}
```

---

### FeatG57-66: Data Management Features

**Note**: Features 57-66 provide administrative capabilities to manage the data required for analytics aggregation. These are standard CRUD operations for AdministrativeArea and FloodEvent entities.

**Administrative Area Management (FeatG57-61):**
- Used to define geographic boundaries (city/district/ward) for area-based aggregation
- Stations are linked to administrative areas (ward level)
- Aggregation jobs group data by `AdministrativeAreaId`
- All endpoints require Admin role: `/api/v1/admin/administrative-areas`

**Flood Event Management (FeatG62-66):**
- Used to record flood incidents that are counted in frequency aggregation
- Each flood event must be associated with an `AdministrativeAreaId`
- Frequency aggregation (FeatG47) counts flood events per time bucket
- When flood events are created/updated/deleted, re-aggregation may be needed
- All endpoints require Admin role: `/api/v1/admin/flood-events`

**Integration with Analytics:**
- These features support the analytics aggregation system by providing data management capabilities
- Changes to administrative areas or flood events may require re-running aggregation jobs (FeatG47-49)
- See [Workflow B – Data Management](#workflow-b--data-management-featg57-66) for detailed flow diagrams

---

## 🔄 BUSINESS WORKFLOW

### Workflow A – Scheduled Aggregation (Primary Flow)

```
[Scheduler (Cron/Quartz)]
   ↓ (daily at 2 AM / weekly on Monday / monthly on 1st)
[Analytics Aggregation Service]
   ↓
[Load raw data from flood_measurements / flood_events]
   ↓
[Compute metrics (frequency, severity, hotspots)]
   ↓
[Persist to analytics tables (REPLACE per bucket - idempotent)]
   ↓
[Update cache (Redis)]
   ↓
[Log job run status]
   ↓
[Mark job success]
```

### Detailed Steps

#### Step 1 – Trigger Aggregation Job

**Triggered by:**
- Cron schedule (fixed schedule)
- Manual trigger from Admin API
- Backfill request (historical data)

**Job Types:**
- `FREQUENCY_AGG`: Count events and exceedances
- `SEVERITY_AGG`: Calculate max/avg/duration
- `HOTSPOT_AGG`: Calculate risk scores and rankings

#### Step 2 – Load Input Data

**Data Sources:**

| Source | Purpose |
|--------|---------|
| `flood_measurements` / `sensor_readings` | Severity, threshold crossing |
| `flood_events` | Frequency by event |
| `stations` | Spatial grouping |
| `areas` | Polygon aggregation |

#### Step 3 – Calculate Metrics

**Frequency Calculation:**
```csharp
// Count flood events in time bucket
var eventCount = floodEvents
    .Where(e => e.StartTime >= bucketStart && e.StartTime < bucketEnd)
    .Count();

// Count threshold exceedances
var exceedCount = sensorReadings
    .Where(r => r.MeasuredAt >= bucketStart && r.MeasuredAt < bucketEnd)
    .Where(r => r.SeverityLevel >= 2)  // Warning or Critical
    .Count();
```

**Severity Calculation:**
```csharp
var maxLevel = sensorReadings
    .Where(r => r.MeasuredAt >= bucketStart && r.MeasuredAt < bucketEnd)
    .Max(r => r.Value);

var avgLevel = sensorReadings
    .Where(r => r.MeasuredAt >= bucketStart && r.MeasuredAt < bucketEnd)
    .Average(r => r.Value);

var durationHours = sensorReadings
    .Where(r => r.MeasuredAt >= bucketStart && r.MeasuredAt < bucketEnd)
    .Where(r => r.SeverityLevel >= 2)
    .GroupBy(r => r.MeasuredAt.Date)
    .Count();
```

**Hotspot Score Calculation:**
```csharp
// Formula: score = (frequency * W1) + (severity * W2) + (duration * W3)
var frequencyScore = (eventCount / maxEventCount) * 100 * 0.4;  // 40% weight
var severityScore = (maxLevel / maxWaterLevel) * 100 * 0.35;      // 35% weight
var durationScore = (durationHours / maxDurationHours) * 100 * 0.25; // 25% weight

var totalScore = frequencyScore + severityScore + durationScore;
```

#### Step 4 – Persist Aggregated Results

**Write to:**
- `flood_analytics_frequency`
- `flood_analytics_severity`
- `flood_analytics_hotspots`

**Principle:**
- **Idempotent**: Replace per `(area_id, time_bucket, bucket_type)` - no double counting
- **Upsert logic**: Use `ON CONFLICT ... DO UPDATE` in PostgreSQL

#### Step 5 – Cache Analytics Output

**Cache Keys:**
```
analytics:frequency:{areaId}:{bucketType}:{date}
analytics:severity:{areaId}:{bucketType}:{date}
analytics:hotspots:{periodStart}:{periodEnd}:{topN}
```

**TTL:**
- Frequency/Severity: 1 hour (refreshed by daily job)
- Hotspots: 6 hours (refreshed by weekly job)

#### Step 6 – Job Logging

**Write to:**
- `analytics_job_runs`

**Status Values:**
- `RUNNING`: Job in progress
- `SUCCESS`: Completed successfully
- `FAILED`: Error occurred
- `CANCELLED`: Manually cancelled

#### Step 7 – Downstream Consumption

**FE-16 (History & Trends):**
- Reads from `flood_analytics_frequency` and `flood_analytics_severity`
- No direct query to raw `sensor_readings` for aggregated views

**FE-18 (Hotspots Map) - ⚠️ Not yet implemented:**
- Will read from `flood_analytics_hotspots` when implemented
- Fast query for top N hotspots
- **Current Status**: FeatG49 and FeatG53 provide hotspot data that can be consumed by FE-18 when it is implemented. The hotspot aggregation and query endpoints are ready for integration.

### Workflow B – Data Management (FeatG57-66)

Features 57-66 provide administrative capabilities to manage the data required for analytics aggregation:

#### Administrative Area Management (FeatG57-61)

**Purpose**: Manage hierarchical administrative areas (city/district/ward) used for area-based aggregation.

**Flow:**
```
[Admin] → Create/Update/Delete AdministrativeArea (FeatG57, FeatG60, FeatG61)
   ↓
[AdministrativeAreas Table]
   ↓
[Used by Aggregation Jobs] → Group sensor readings and flood events by area
   ↓
[Analytics Tables] → Store aggregated metrics per area
```

**Endpoints:**
- **FeatG57**: `POST /api/v1/admin/administrative-areas` - Create new administrative area
- **FeatG58**: `GET /api/v1/admin/administrative-areas` - List all administrative areas
- **FeatG59**: `GET /api/v1/admin/administrative-areas/{id}` - Get specific administrative area
- **FeatG60**: `PUT /api/v1/admin/administrative-areas/{id}` - Update administrative area
- **FeatG61**: `DELETE /api/v1/admin/administrative-areas/{id}` - Delete administrative area

**Integration with Analytics:**
- Administrative areas define the geographic boundaries for aggregation
- Stations belong to administrative areas (ward level)
- Aggregation jobs group data by `AdministrativeAreaId`
- Analytics queries filter by `administrativeAreaId` parameter

#### Flood Event Management (FeatG62-66)

**Purpose**: Manage flood event records that are used for frequency aggregation calculations.

**Flow:**
```
[Admin] → Create/Update/Delete FloodEvent (FeatG62, FeatG65, FeatG66)
   ↓
[FloodEvents Table]
   ↓
[Frequency Aggregation Job (FeatG47)] → Count flood events per time bucket
   ↓
[FloodAnalyticsFrequency Table] → Store event counts
```

**Endpoints:**
- **FeatG62**: `POST /api/v1/admin/flood-events` - Create new flood event
- **FeatG63**: `GET /api/v1/admin/flood-events` - List flood events (with filtering)
- **FeatG64**: `GET /api/v1/admin/flood-events/{id}` - Get specific flood event
- **FeatG65**: `PUT /api/v1/admin/flood-events/{id}` - Update flood event
- **FeatG66**: `DELETE /api/v1/admin/flood-events/{id}` - Delete flood event

**Integration with Analytics:**
- Flood events are counted in frequency aggregation (`event_count` field)
- Each flood event must be associated with an `AdministrativeAreaId`
- When flood events are created/updated/deleted, re-aggregation may be needed
- Frequency aggregation queries `FloodEvents` table to count events per time bucket

### Workflow C – Complete Analytics Flow (Including Data Management)

**End-to-End Flow:**
```
1. [Admin] Create Administrative Areas (FeatG57)
   ↓
2. [Admin] Create Stations (linked to Administrative Areas)
   ↓
3. [System] Sensor readings collected → stored in SensorReadings table
   ↓
4. [Admin] Create Flood Events (FeatG62) → stored in FloodEvents table
   ↓
5. [Scheduler/Admin] Trigger Aggregation Jobs (FeatG47-49)
   ↓
6. [Background Job] Aggregate data by AdministrativeArea and time bucket
   ↓
7. [Analytics Tables] Store aggregated results
   ↓
8. [Users] Query Analytics (FeatG51-53) → Fast queries from aggregated tables
   ↓
9. [Admin] Manage data as needed (FeatG57-66)
   - Update/Delete Administrative Areas (FeatG60-61)
   - Update/Delete Flood Events (FeatG65-66)
   - Re-trigger aggregation if data changes
```

---

## 🚀 IMPLEMENTATION PLAN

### Phase 1: Domain Layer

- [ ] Create `FloodAnalyticsFrequency` entity
- [ ] Create `FloodAnalyticsSeverity` entity
- [ ] Create `FloodAnalyticsHotspot` entity
- [ ] Create `AnalyticsJob` entity
- [ ] Create `AnalyticsJobRun` entity
- [ ] Create `FloodEvent` entity (if not exists)
- [ ] Create repository interfaces:
  - `IFloodAnalyticsFrequencyRepository`
  - `IFloodAnalyticsSeverityRepository`
  - `IFloodAnalyticsHotspotRepository`
  - `IAnalyticsJobRepository`
  - `IAnalyticsJobRunRepository`
- [ ] Update `AppDbContext` with new DbSets
- [ ] Create EF Core configurations

### Phase 2: Infrastructure Layer

- [ ] Implement PostgreSQL repositories
- [ ] Create database migrations for new tables
- [ ] Add indexes for performance
- [ ] Implement Redis cache service (optional)

### Phase 3: Application Layer - Aggregation Handlers

- [ ] Create `FDAAPI.App.FeatG42_FrequencyAggregation`
  - `FrequencyAggregationRequest.cs`
  - `FrequencyAggregationHandler.cs`
  - `FrequencyAggregationService.cs` (business logic)
- [ ] Create `FDAAPI.App.FeatG43_SeverityAggregation`
  - `SeverityAggregationRequest.cs`
  - `SeverityAggregationHandler.cs`
  - `SeverityAggregationService.cs`
- [ ] Create `FDAAPI.App.FeatG44_HotspotAggregation` (⚠️ Prepared for FE-18, not yet implemented)
  - `HotspotAggregationRequest.cs`
  - `HotspotAggregationHandler.cs`
  - `HotspotAggregationService.cs`

### Phase 4: Application Layer - Query Handlers

- [ ] Create `FDAAPI.App.FeatG46_GetFrequencyAnalytics`
- [ ] Create `FDAAPI.App.FeatG47_GetSeverityAnalytics`
- [ ] Create `FDAAPI.App.FeatG48_GetHotspotRankings` (⚠️ Prepared for FE-18, not yet implemented)
- [ ] Create `FDAAPI.App.FeatG45_GetJobStatus`

### Phase 5: Background Jobs (Scheduled Aggregation)

- [ ] Create `DailyAggregationJob.cs` (Quartz)
- [ ] Create `WeeklyAggregationJob.cs` (Quartz)
- [ ] Create `MonthlyAggregationJob.cs` (Quartz)
- [ ] Register jobs in `ServiceExtensions`
- [ ] Configure job scheduling

### Phase 6: Presentation Layer (Endpoints)

- [ ] Create `Endpoints/Feat42_FrequencyAggregation/FrequencyAggregationEndpoint.cs`
- [ ] Create `Endpoints/Feat43_SeverityAggregation/SeverityAggregationEndpoint.cs`
- [ ] Create `Endpoints/Feat44_HotspotAggregation/HotspotAggregationEndpoint.cs`
- [ ] Create `Endpoints/Feat45_GetJobStatus/GetJobStatusEndpoint.cs`
- [ ] Create `Endpoints/Feat46_GetFrequencyAnalytics/GetFrequencyAnalyticsEndpoint.cs`
- [ ] Create `Endpoints/Feat47_GetSeverityAnalytics/GetSeverityAnalyticsEndpoint.cs`
- [ ] Create `Endpoints/Feat48_GetHotspotRankings/GetHotspotRankingsEndpoint.cs`

### Phase 7: Testing

- [ ] Unit tests for aggregation services
- [ ] Integration tests for aggregation jobs
- [ ] Performance tests with large datasets
- [ ] Idempotency tests (re-run same job)

---

## 🧪 TESTING STRATEGY

### Test Cases

#### TEST CASE 1: Frequency Aggregation - Daily Bucket

**Scenario**: Aggregate frequency for one day

**Setup**: Create test data:
- 5 flood events on 2026-01-15
- 12 threshold exceedances on 2026-01-15

**Expected Result**:
- `event_count = 5`
- `exceed_count = 12`
- One record in `flood_analytics_frequency` for 2026-01-15

#### TEST CASE 2: Idempotency - Re-run Same Job

**Scenario**: Run frequency aggregation twice for same bucket

**Expected Result**:
- First run: Creates record
- Second run: Updates same record (no duplicate)
- `exceed_count` remains 12 (not 24)

#### TEST CASE 3: Severity Aggregation - Missing Data

**Scenario**: Some hours have no readings

**Expected Result**:
- Job completes successfully
- `reading_count` reflects actual count
- Missing intervals logged but don't block job

#### TEST CASE 4: Hotspot Score Calculation

**Scenario**: Calculate hotspots for 30-day period

**Expected Result**:
- Top 20 areas ranked by score
- Score formula: `(frequency * 0.4) + (severity * 0.35) + (duration * 0.25)`
- Rankings stored in `flood_analytics_hotspots`

#### TEST CASE 5: Cache Invalidation

**Scenario**: Re-aggregate data, then query

**Expected Result**:
- Cache invalidated on re-aggregation
- Query returns fresh data from database
- Cache repopulated with new data

---

## ⚡ PERFORMANCE CONSIDERATIONS

### Database Optimization

#### 1. Partitioning (for large analytics tables)

```sql
-- Partition flood_analytics_frequency by month
CREATE TABLE flood_analytics_frequency (
    ...
) PARTITION BY RANGE (time_bucket);

CREATE TABLE flood_analytics_frequency_2026_01 PARTITION OF flood_analytics_frequency
    FOR VALUES FROM ('2026-01-01') TO ('2026-02-01');
```

#### 2. Indexes for Query Performance

```sql
-- Composite index for time-range queries
CREATE INDEX ix_frequency_area_bucket ON flood_analytics_frequency(area_id, time_bucket DESC);

-- Index for hotspot rankings
CREATE INDEX ix_hotspot_score ON flood_analytics_hotspots(score DESC, calculated_at DESC);
```

### Caching Strategy

| Cache Key Pattern | TTL | Invalidation |
|-------------------|-----|--------------|
| `analytics:frequency:{areaId}:{bucketType}:{date}` | 1 hour | On re-aggregation |
| `analytics:severity:{areaId}:{bucketType}:{date}` | 1 hour | On re-aggregation |
| `analytics:hotspots:{periodStart}:{periodEnd}` | 6 hours | On weekly job |

### Job Performance

**Target Metrics:**
- Daily aggregation: < 5 minutes for 100 areas
- Weekly aggregation: < 15 minutes for 100 areas
- Monthly aggregation: < 30 minutes for 100 areas

**Optimization:**
- Parallel processing per area (if possible)
- Batch inserts (bulk insert)
- Use `ON CONFLICT ... DO UPDATE` for idempotency

---

## 📝 IMPLEMENTATION CHECKLIST

### 🟡 Phase 1: Domain Layer
- [ ] Create analytics entities
- [ ] Create repository interfaces
- [ ] Update `AppDbContext`

### 🟡 Phase 2: Infrastructure Layer
- [ ] Implement repositories
- [ ] Create database migrations
- [ ] Add indexes

### 🟡 Phase 3-4: Application Layer
- [ ] Aggregation handlers (FeatG47-49)
- [ ] Query handlers (FeatG50-53)
- [ ] Aggregation services (business logic)

### 🟡 Phase 5: Background Jobs
- [ ] Scheduled aggregation jobs
- [ ] Job scheduling configuration

### 🟡 Phase 6: Presentation Layer
- [ ] Create endpoints
- [ ] Create endpoint DTOs

### 🟡 Phase 7: Testing
- [ ] Unit tests
- [ ] Integration tests
- [ ] Performance tests

---

## 🎯 SUCCESS CRITERIA

### Functional Requirements
- [ ] Scheduled jobs run daily/weekly/monthly
- [ ] Analytics tables populated correctly
- [ ] Idempotent aggregation (re-run safe)
- [ ] Cache invalidation works
- [ ] FE-16 can query analytics tables
- [ ] FE-18 can query hotspot rankings via FeatG53 (⚠️ FE-18 not yet implemented, but endpoint ready)

### Non-Functional Requirements
- [ ] Daily aggregation completes in < 5 minutes
- [ ] Query response time < 200ms (with cache)
- [ ] Job failure rate < 1%
- [ ] Data completeness > 99%

---

**Document Version**: 1.0
**Last Updated**: 2026-01-17
**Author**: FDA Development Team
**Status**: 🟡 Planning
**Next Steps**: Create FE-17 Insight document and diagrams

