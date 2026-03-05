# FE-16 – View Flood History & Trends

> **Feature Name**: View Flood History & Trends (Timeseries Charts)
> **Created**: 2026-01-16
> **Status**: 🟡 Planning
> **Backend Features**: FeatG39 (GetFloodHistory), FeatG40 (GetFloodTrends), FeatG41 (GetFloodStatistics)
> **Priority**: High
> **Related**: FE-09 (Monitored Areas), FE-30/31 (Map Status)

---

## 📋 TABLE OF CONTENTS

1. [Executive Summary](#executive-summary)
2. [Feature Analysis](#feature-analysis)
3. [Domain Model](#domain-model)
4. [API Specifications](#api-specifications)
5. [Timeseries Storage Strategy](#timeseries-storage-strategy)
6. [Implementation Plan](#implementation-plan)
7. [Testing Strategy](#testing-strategy)
8. [Performance Considerations](#performance-considerations)

---

## 📊 EXECUTIVE SUMMARY

### Feature Overview

**Problem**: Users and authorities need to view historical flood data and trends to:
- Understand flooding patterns over time (day/week/month/year)
- Make informed decisions about flood preparedness
- Analyze which areas are most affected
- Compare current conditions to historical data

**Solution**: Provide API endpoints and chart-ready data for:
- Historical water level readings with time-range filters
- Aggregated statistics (min/max/avg) by day/week/month
- Trend analysis with comparison to previous periods
- Missing data interval detection for data quality

### Backend Features to Implement

| Feature | Endpoint | Type | Description | Status |
|---------|----------|------|-------------|--------|
| **FeatG39** | `GET /api/v1/flood-history` | Query | Get historical readings with time filters | 🟡 New |
| **FeatG40** | `GET /api/v1/flood-trends` | Query | Get aggregated trends (day/week/month) | 🟡 New |
| **FeatG41** | `GET /api/v1/flood-statistics` | Query | Get min/max/avg statistics by station | 🟡 New |

### Key Requirements

1. **Timeseries Query API**: Fetch historical sensor readings with flexible time ranges
2. **Aggregation Endpoints**: Pre-computed or on-demand aggregations for charts
3. **Chart-Ready Format**: Response format optimized for Web/Mobile chart libraries
4. **Performance**: Handle large datasets efficiently (millions of readings)
5. **Missing Interval Detection**: Identify gaps in sensor data

---

## 🔍 FEATURE ANALYSIS

### Key Requirements

#### 1. UI Requirements (Charts - Web/Mobile)

| Chart Type | Time Range | Data Points | Use Case |
|------------|------------|-------------|----------|
| **Line Chart** | Last 24 hours | Hourly readings | Real-time monitoring |
| **Line Chart** | Last 7 days | Daily aggregates | Weekly trends |
| **Bar Chart** | Last 30 days | Daily max/avg | Monthly analysis |
| **Comparison Chart** | Custom range | Multiple stations | Station comparison |
| **Heatmap** | Last year | Daily flood hours | Seasonal patterns |

#### 2. API Requirements

- **Time Range Filters**: startDate, endDate, granularity (hour/day/week/month)
- **Station Filters**: Single station, multiple stations, area-based
- **Aggregation Types**: raw, hourly, daily, weekly, monthly
- **Response Format**: Array of {timestamp, value, metadata} for charts
- **Pagination**: Support for large datasets with cursor-based pagination

#### 3. Business Rules

| Rule | Description | Status |
|------|-------------|--------|
| **Max Time Range** | 1 year for raw data, 5 years for aggregated | 🟡 New |
| **Min Granularity** | 5 minutes for raw, 1 hour for aggregated | 🟡 New |
| **Data Retention** | Raw: 1 year, Daily aggregates: 5 years | 🟡 New |
| **Missing Data Flags** | Mark intervals with no readings | 🟡 New |
| **Quality Flags** | Include data quality indicators | ✅ Exists in sensor_readings |

---

## 🗄️ DOMAIN MODEL

### Existing Tables

#### 1. `sensor_readings` (Primary Data Store)

```sql
-- Already exists - raw sensor data
CREATE TABLE sensor_readings (
    id              UUID PRIMARY KEY,
    station_id      UUID NOT NULL REFERENCES stations(id),
    value           DOUBLE PRECISION NOT NULL,  -- Water level
    distance        DOUBLE PRECISION,           -- Distance measurement
    sensor_height   DOUBLE PRECISION,           -- Sensor height
    unit            VARCHAR(10) DEFAULT 'cm',
    status          INT,
    measured_at     TIMESTAMPTZ NOT NULL,

    created_by      UUID NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL,
    updated_by      UUID NOT NULL,
    updated_at      TIMESTAMPTZ NOT NULL
);

-- Existing indexes (optimized for timeseries queries)
CREATE INDEX ix_sensor_readings_station ON sensor_readings(station_id);
CREATE INDEX ix_sensor_readings_measured_at ON sensor_readings(measured_at);
CREATE INDEX ix_sensor_readings_station_time ON sensor_readings(station_id, measured_at);
```

#### 2. `sensor_daily_agg` (Pre-computed Aggregates - from db.md)

```sql
-- Defined in db.md but NOT yet implemented
CREATE TABLE sensor_daily_agg (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    station_id      UUID NOT NULL REFERENCES stations(id),
    date            DATE NOT NULL,
    max_level       NUMERIC(14,4),
    min_level       NUMERIC(14,4),
    avg_level       NUMERIC(14,4),
    rainfall_total  NUMERIC(14,4),
    reading_count   INT DEFAULT 0,           -- Number of readings in the day

    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT uq_daily_agg_station_date UNIQUE (station_id, date)
);

CREATE INDEX ix_daily_agg_station_date ON sensor_daily_agg(station_id, date);
```

#### 3. `flood_statistics` (Summary Statistics - from db.md)

```sql
-- Defined in db.md but NOT yet implemented
CREATE TABLE flood_statistics (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    station_id      UUID NOT NULL REFERENCES stations(id),
    date            DATE NOT NULL,
    avg_depth_m     NUMERIC(14,4),
    max_depth_m     NUMERIC(14,4),
    flood_hours     INT,                      -- Hours with water level > threshold

    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT uq_flood_stats_station_date UNIQUE (station_id, date)
);

CREATE INDEX ix_flood_stats_station_date ON flood_statistics(station_id, date);
```

### New Entities to Create

#### FloodHistoryDto (Response DTO)

```csharp
public class FloodHistoryDto
{
    public Guid StationId { get; set; }
    public string StationName { get; set; } = string.Empty;
    public string StationCode { get; set; } = string.Empty;
    public List<FloodDataPointDto> DataPoints { get; set; } = new();
    public FloodHistoryMetadata Metadata { get; set; } = new();
}

public class FloodDataPointDto
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }              // Water level in cm
    public double? ValueMeters { get; set; }       // Water level in meters
    public string? QualityFlag { get; set; }       // ok, suspect, bad
    public string? Severity { get; set; }          // safe, caution, warning, critical
}

public class FloodHistoryMetadata
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Granularity { get; set; } = "raw";   // raw, hourly, daily, weekly, monthly
    public int TotalDataPoints { get; set; }
    public int MissingIntervals { get; set; }
    public DateTime? LastUpdated { get; set; }
}
```

#### FloodTrendDto (Aggregated Response)

```csharp
public class FloodTrendDto
{
    public Guid StationId { get; set; }
    public string StationName { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;  // "2026-01", "2026-W03", "2026-01-16"
    public string Granularity { get; set; } = "daily";
    public List<FloodTrendDataPoint> DataPoints { get; set; } = new();
}

public class FloodTrendDataPoint
{
    public string Period { get; set; } = string.Empty;  // "2026-01-16", "2026-W03", "2026-01"
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public double MaxLevel { get; set; }
    public double MinLevel { get; set; }
    public double AvgLevel { get; set; }
    public int ReadingCount { get; set; }
    public int FloodHours { get; set; }                 // Hours above threshold
    public double? RainfallTotal { get; set; }
    public string PeakSeverity { get; set; } = "safe";  // Highest severity in period
}
```

#### FloodStatisticsDto (Summary Statistics)

```csharp
public class FloodStatisticsDto
{
    public Guid StationId { get; set; }
    public string StationName { get; set; } = string.Empty;
    public string StationCode { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    // Summary Statistics
    public double MaxWaterLevel { get; set; }
    public double MinWaterLevel { get; set; }
    public double AvgWaterLevel { get; set; }
    public int TotalFloodHours { get; set; }
    public int TotalReadings { get; set; }
    public int MissingIntervals { get; set; }

    // Severity Breakdown
    public int HoursSafe { get; set; }
    public int HoursCaution { get; set; }
    public int HoursWarning { get; set; }
    public int HoursCritical { get; set; }

    // Trend Comparison (vs previous period)
    public double? AvgLevelChange { get; set; }         // +/- percentage
    public double? FloodHoursChange { get; set; }       // +/- percentage
}
```

---

## 🔌 API SPECIFICATIONS

### FeatG39: Get Flood History (Raw/Hourly Data)

**Endpoint**: `GET /api/v1/flood-history`
**Authorization**: `Policies("User")` - Authenticated users
**Pattern**: MediatR + Mapper

#### Request Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `stationId` | UUID | No | null | Filter by station (null = all) |
| `stationIds` | UUID[] | No | null | Filter by multiple stations |
| `areaId` | UUID | No | null | Filter by user's area (nearby stations) |
| `startDate` | DateTime | No | -24h | Start of time range |
| `endDate` | DateTime | No | now | End of time range |
| `granularity` | string | No | "raw" | raw, hourly, daily |
| `limit` | int | No | 1000 | Max data points per station |
| `cursor` | string | No | null | Pagination cursor |

#### Request Example

```http
GET /api/v1/flood-history?stationId=550e8400-e29b-41d4-a716-446655440000&startDate=2026-01-15T00:00:00Z&endDate=2026-01-16T00:00:00Z&granularity=hourly
Authorization: Bearer {access_token}
```

#### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Flood history retrieved successfully",
  "statusCode": 200,
  "data": {
    "stationId": "550e8400-e29b-41d4-a716-446655440000",
    "stationName": "Station Ben Nghe",
    "stationCode": "ST_DN_01",
    "dataPoints": [
      {
        "timestamp": "2026-01-15T00:00:00Z",
        "value": 125.5,
        "valueMeters": 1.255,
        "qualityFlag": "ok",
        "severity": "caution"
      },
      {
        "timestamp": "2026-01-15T01:00:00Z",
        "value": 142.3,
        "valueMeters": 1.423,
        "qualityFlag": "ok",
        "severity": "caution"
      },
      {
        "timestamp": "2026-01-15T02:00:00Z",
        "value": 215.8,
        "valueMeters": 2.158,
        "qualityFlag": "ok",
        "severity": "warning"
      }
    ],
    "metadata": {
      "startDate": "2026-01-15T00:00:00Z",
      "endDate": "2026-01-16T00:00:00Z",
      "granularity": "hourly",
      "totalDataPoints": 24,
      "missingIntervals": 0,
      "lastUpdated": "2026-01-16T10:30:00Z"
    }
  },
  "pagination": {
    "hasMore": false,
    "nextCursor": null,
    "totalCount": 24
  }
}
```

#### Error Responses

**400 Bad Request - Invalid Time Range**
```json
{
  "success": false,
  "message": "Time range cannot exceed 1 year for raw data",
  "statusCode": 400
}
```

**404 Not Found - Station Not Found**
```json
{
  "success": false,
  "message": "Station not found",
  "statusCode": 404
}
```

---

### FeatG40: Get Flood Trends (Aggregated Data)

**Endpoint**: `GET /api/v1/flood-trends`
**Authorization**: `Policies("User")` - Authenticated users
**Pattern**: MediatR + Mapper

#### Request Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `stationId` | UUID | Yes | - | Station to analyze |
| `period` | string | No | "last30days" | last7days, last30days, last90days, last365days, custom |
| `startDate` | DateTime | No | - | Required if period=custom |
| `endDate` | DateTime | No | - | Required if period=custom |
| `granularity` | string | No | "daily" | daily, weekly, monthly |
| `compareWithPrevious` | bool | No | false | Include comparison with previous period |

#### Request Example

```http
GET /api/v1/flood-trends?stationId=550e8400-e29b-41d4-a716-446655440000&period=last30days&granularity=daily&compareWithPrevious=true
Authorization: Bearer {access_token}
```

#### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Flood trends retrieved successfully",
  "statusCode": 200,
  "data": {
    "stationId": "550e8400-e29b-41d4-a716-446655440000",
    "stationName": "Station Ben Nghe",
    "period": "last30days",
    "granularity": "daily",
    "dataPoints": [
      {
        "period": "2025-12-17",
        "periodStart": "2025-12-17T00:00:00Z",
        "periodEnd": "2025-12-17T23:59:59Z",
        "maxLevel": 215.8,
        "minLevel": 85.2,
        "avgLevel": 142.3,
        "readingCount": 288,
        "floodHours": 4,
        "rainfallTotal": 25.5,
        "peakSeverity": "warning"
      },
      {
        "period": "2025-12-18",
        "periodStart": "2025-12-18T00:00:00Z",
        "periodEnd": "2025-12-18T23:59:59Z",
        "maxLevel": 180.5,
        "minLevel": 75.8,
        "avgLevel": 125.6,
        "readingCount": 288,
        "floodHours": 2,
        "rainfallTotal": 12.3,
        "peakSeverity": "caution"
      }
    ],
    "comparison": {
      "previousPeriodStart": "2025-11-17T00:00:00Z",
      "previousPeriodEnd": "2025-12-16T23:59:59Z",
      "avgLevelChange": -12.5,
      "floodHoursChange": -25.0,
      "peakLevelChange": -8.3
    },
    "summary": {
      "totalFloodHours": 85,
      "avgWaterLevel": 138.5,
      "maxWaterLevel": 320.5,
      "daysWithFlooding": 18,
      "mostAffectedDay": "2025-12-25"
    }
  }
}
```

---

### FeatG41: Get Flood Statistics (Summary)

**Endpoint**: `GET /api/v1/flood-statistics`
**Authorization**: `Policies("User")` - Authenticated users
**Pattern**: MediatR + Mapper

#### Request Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `stationId` | UUID | No | null | Filter by station |
| `stationIds` | UUID[] | No | null | Filter by multiple stations |
| `areaId` | UUID | No | null | Filter by user's area |
| `period` | string | No | "last30days" | Time period for statistics |
| `includeBreakdown` | bool | No | true | Include severity breakdown |

#### Request Example

```http
GET /api/v1/flood-statistics?stationId=550e8400-e29b-41d4-a716-446655440000&period=last30days&includeBreakdown=true
Authorization: Bearer {access_token}
```

#### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Flood statistics retrieved successfully",
  "statusCode": 200,
  "data": {
    "stationId": "550e8400-e29b-41d4-a716-446655440000",
    "stationName": "Station Ben Nghe",
    "stationCode": "ST_DN_01",
    "periodStart": "2025-12-17T00:00:00Z",
    "periodEnd": "2026-01-16T00:00:00Z",
    "summary": {
      "maxWaterLevel": 320.5,
      "minWaterLevel": 45.2,
      "avgWaterLevel": 138.5,
      "totalFloodHours": 85,
      "totalReadings": 8640,
      "missingIntervals": 12
    },
    "severityBreakdown": {
      "hoursSafe": 548,
      "hoursCaution": 72,
      "hoursWarning": 68,
      "hoursCritical": 32
    },
    "comparison": {
      "avgLevelChange": -12.5,
      "floodHoursChange": -25.0
    },
    "dataQuality": {
      "completeness": 99.86,
      "missingIntervals": [
        {
          "start": "2025-12-20T14:00:00Z",
          "end": "2025-12-20T15:00:00Z",
          "durationMinutes": 60
        }
      ]
    }
  }
}
```

---

## 📊 TIMESERIES STORAGE STRATEGY

### Strategy Overview

| Data Type | Storage | Retention | Query Pattern | Use Case |
|-----------|---------|-----------|---------------|----------|
| **Raw Readings** | sensor_readings | 1 year | Point-in-time queries | Debugging, detailed analysis |
| **Hourly Aggregates** | sensor_hourly_agg (new) | 2 years | Intraday trends | 24h/7d charts |
| **Daily Aggregates** | sensor_daily_agg | 5 years | Long-term trends | 30d/90d/1y charts |
| **Monthly Aggregates** | flood_statistics | Forever | Historical analysis | Multi-year comparison |

### Aggregation Pipeline

```
[IoT Sensor] → [Raw Reading] → [PostgreSQL: sensor_readings]
                                        ↓ (every hour)
                               [Aggregation Job]
                                        ↓
                        [PostgreSQL: sensor_hourly_agg]
                                        ↓ (daily at midnight)
                               [Aggregation Job]
                                        ↓
                         [PostgreSQL: sensor_daily_agg]
                                        ↓ (monthly on 1st)
                               [Aggregation Job]
                                        ↓
                         [PostgreSQL: flood_statistics]
```

### New Table: `sensor_hourly_agg`

```sql
CREATE TABLE sensor_hourly_agg (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    station_id      UUID NOT NULL REFERENCES stations(id),
    hour_start      TIMESTAMPTZ NOT NULL,  -- Truncated to hour
    max_level       NUMERIC(14,4),
    min_level       NUMERIC(14,4),
    avg_level       NUMERIC(14,4),
    reading_count   INT DEFAULT 0,
    quality_score   NUMERIC(5,2),          -- % of valid readings

    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT uq_hourly_agg_station_hour UNIQUE (station_id, hour_start)
);

CREATE INDEX ix_hourly_agg_station_hour ON sensor_hourly_agg(station_id, hour_start);
```

### Aggregation Job (Background Service)

```csharp
// Quartz job to run every hour
public class HourlyAggregationJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var lastHour = DateTime.UtcNow.AddHours(-1).TruncateToHour();

        // Query raw readings for the last hour
        var readings = await _readingRepository.GetByHourAsync(lastHour);

        // Group by station and calculate aggregates
        var aggregates = readings
            .GroupBy(r => r.StationId)
            .Select(g => new SensorHourlyAgg
            {
                StationId = g.Key,
                HourStart = lastHour,
                MaxLevel = g.Max(r => r.Value),
                MinLevel = g.Min(r => r.Value),
                AvgLevel = g.Average(r => r.Value),
                ReadingCount = g.Count(),
                QualityScore = g.Count(r => r.QualityFlag == "ok") * 100.0 / g.Count()
            });

        await _hourlyAggRepository.BulkInsertAsync(aggregates);
    }
}
```

### Query Optimization

#### 1. Time-Range Query with Granularity Selection

```csharp
public async Task<List<FloodDataPointDto>> GetFloodHistoryAsync(
    Guid stationId,
    DateTime startDate,
    DateTime endDate,
    string granularity,
    CancellationToken ct)
{
    var timeRange = endDate - startDate;

    // Auto-select data source based on range and granularity
    if (granularity == "raw" && timeRange <= TimeSpan.FromDays(7))
    {
        // Use raw readings for short ranges
        return await GetFromRawReadings(stationId, startDate, endDate, ct);
    }
    else if (granularity == "hourly" || timeRange <= TimeSpan.FromDays(30))
    {
        // Use hourly aggregates for medium ranges
        return await GetFromHourlyAgg(stationId, startDate, endDate, ct);
    }
    else
    {
        // Use daily aggregates for long ranges
        return await GetFromDailyAgg(stationId, startDate, endDate, ct);
    }
}
```

#### 2. Missing Interval Detection

```csharp
public List<MissingInterval> DetectMissingIntervals(
    List<FloodDataPointDto> dataPoints,
    TimeSpan expectedInterval)
{
    var missing = new List<MissingInterval>();

    for (int i = 1; i < dataPoints.Count; i++)
    {
        var gap = dataPoints[i].Timestamp - dataPoints[i-1].Timestamp;

        if (gap > expectedInterval * 1.5) // 50% tolerance
        {
            missing.Add(new MissingInterval
            {
                Start = dataPoints[i-1].Timestamp,
                End = dataPoints[i].Timestamp,
                DurationMinutes = (int)gap.TotalMinutes
            });
        }
    }

    return missing;
}
```

---

## 🚀 IMPLEMENTATION PLAN

### Phase 1: Domain Layer

- [ ] Create `SensorHourlyAgg` entity
- [ ] Create `ISensorHourlyAggRepository` interface
- [ ] Create `IFloodStatisticsRepository` interface
- [ ] Update `AppDbContext` with new DbSets
- [ ] Create EF Core configurations

### Phase 2: Infrastructure Layer

- [ ] Implement `PgsqlSensorHourlyAggRepository`
- [ ] Implement `PgsqlFloodStatisticsRepository`
- [ ] Add time-range query methods to `ISensorReadingRepository`
- [ ] Create database migrations for new tables

### Phase 3: Application Layer (FeatG39 - GetFloodHistory)

- [ ] Create project `FDAAPI.App.FeatG39_GetFloodHistory`
- [ ] Create `GetFloodHistoryRequest.cs` (sealed record)
- [ ] Create `GetFloodHistoryResponse.cs`
- [ ] Create `GetFloodHistoryHandler.cs` (MediatR)
- [ ] Create `GetFloodHistoryRequestValidator.cs` (FluentValidation)
- [ ] Create `FloodHistoryStatusCode.cs` enum

### Phase 4: Application Layer (FeatG40 - GetFloodTrends)

- [ ] Create project `FDAAPI.App.FeatG40_GetFloodTrends`
- [ ] Create `GetFloodTrendsRequest.cs`
- [ ] Create `GetFloodTrendsResponse.cs`
- [ ] Create `GetFloodTrendsHandler.cs`
- [ ] Create `GetFloodTrendsRequestValidator.cs`
- [ ] Implement period comparison logic

### Phase 5: Application Layer (FeatG41 - GetFloodStatistics)

- [ ] Create project `FDAAPI.App.FeatG41_GetFloodStatistics`
- [ ] Create `GetFloodStatisticsRequest.cs`
- [ ] Create `GetFloodStatisticsResponse.cs`
- [ ] Create `GetFloodStatisticsHandler.cs`
- [ ] Create `GetFloodStatisticsRequestValidator.cs`
- [ ] Implement missing interval detection

### Phase 6: Mapper Layer

- [ ] Create `IFloodHistoryMapper` interface
- [ ] Implement `FloodHistoryMapper`
- [ ] Create DTOs in `FDAAPI.App.Common/DTOs/`
- [ ] Register mappers in `ServiceExtensions`

### Phase 7: Presentation Layer

- [ ] Create `Endpoints/Feat39_GetFloodHistory/GetFloodHistoryEndpoint.cs`
- [ ] Create `Endpoints/Feat40_GetFloodTrends/GetFloodTrendsEndpoint.cs`
- [ ] Create `Endpoints/Feat41_GetFloodStatistics/GetFloodStatisticsEndpoint.cs`
- [ ] Create request/response DTOs for each endpoint

### Phase 8: Background Jobs (Aggregation)

- [ ] Create `HourlyAggregationJob.cs` (Quartz)
- [ ] Create `DailyAggregationJob.cs` (Quartz)
- [ ] Register jobs in `ServiceExtensions`
- [ ] Add job scheduling configuration

### Phase 9: Database & Testing

- [ ] Create database migrations
- [ ] Apply migrations
- [ ] Create seed data scripts
- [ ] Write integration tests
- [ ] Performance testing with large datasets

---

## 🧪 TESTING STRATEGY

### Test Cases

#### TEST CASE 1: Get Flood History - Last 24 Hours

**Scenario**: User requests hourly water level data for the last 24 hours

**cURL**:
```bash
curl -X GET "http://localhost:5000/api/v1/flood-history?stationId=550e8400-e29b-41d4-a716-446655440000&granularity=hourly" \
  -H "Authorization: Bearer {access_token}"
```

**Expected Response (200 OK)**:
- 24 data points (1 per hour)
- Each point has timestamp, value, severity
- Metadata includes totalDataPoints = 24

**Validation**:
- ✅ Data points are sorted by timestamp ASC
- ✅ No gaps in hourly data
- ✅ Severity calculated correctly

---

#### TEST CASE 2: Get Flood History - Time Range Exceeds Limit

**Scenario**: User requests 2 years of raw data (exceeds 1 year limit)

**cURL**:
```bash
curl -X GET "http://localhost:5000/api/v1/flood-history?stationId=550e8400-e29b-41d4-a716-446655440000&startDate=2024-01-01&endDate=2026-01-01&granularity=raw" \
  -H "Authorization: Bearer {access_token}"
```

**Expected Response (400 Bad Request)**:
```json
{
  "success": false,
  "message": "Time range cannot exceed 1 year for raw data. Use 'daily' granularity for longer ranges.",
  "statusCode": 400
}
```

---

#### TEST CASE 3: Get Flood Trends - Monthly Comparison

**Scenario**: User requests monthly trends with comparison to previous period

**cURL**:
```bash
curl -X GET "http://localhost:5000/api/v1/flood-trends?stationId=550e8400-e29b-41d4-a716-446655440000&period=last30days&granularity=daily&compareWithPrevious=true" \
  -H "Authorization: Bearer {access_token}"
```

**Expected Response (200 OK)**:
- 30 data points (1 per day)
- Comparison object with percentage changes
- Summary with total flood hours

---

#### TEST CASE 4: Get Flood Statistics - Multiple Stations

**Scenario**: User requests statistics for multiple stations

**cURL**:
```bash
curl -X GET "http://localhost:5000/api/v1/flood-statistics?stationIds=550e8400-e29b-41d4-a716-446655440000,550e8400-e29b-41d4-a716-446655440001&period=last7days" \
  -H "Authorization: Bearer {access_token}"
```

**Expected Response (200 OK)**:
- Array of statistics for each station
- Each includes severity breakdown
- Missing intervals detected

---

#### TEST CASE 5: Missing Interval Detection

**Scenario**: Test that gaps in sensor data are detected

**Setup**: Create readings with 2-hour gap in data

**cURL**:
```bash
curl -X GET "http://localhost:5000/api/v1/flood-statistics?stationId=550e8400-e29b-41d4-a716-446655440000&period=last24hours" \
  -H "Authorization: Bearer {access_token}"
```

**Expected Response (200 OK)**:
```json
{
  "dataQuality": {
    "completeness": 91.67,
    "missingIntervals": [
      {
        "start": "2026-01-15T14:00:00Z",
        "end": "2026-01-15T16:00:00Z",
        "durationMinutes": 120
      }
    ]
  }
}
```

---

#### TEST CASE 6: Performance - Large Dataset

**Scenario**: Query 1 year of daily data (365 data points)

**cURL**:
```bash
curl -X GET "http://localhost:5000/api/v1/flood-trends?stationId=550e8400-e29b-41d4-a716-446655440000&period=last365days&granularity=daily" \
  -H "Authorization: Bearer {access_token}"
```

**Performance Criteria**:
- ✅ Response time < 500ms
- ✅ Memory usage < 50MB
- ✅ No database timeouts

**Database Check**:
```sql
-- Verify index usage
EXPLAIN ANALYZE
SELECT * FROM sensor_daily_agg
WHERE station_id = '550e8400-e29b-41d4-a716-446655440000'
AND date >= '2025-01-16' AND date <= '2026-01-16'
ORDER BY date;
```

---

## ⚡ PERFORMANCE CONSIDERATIONS

### Database Optimization

#### 1. Partitioning (for large datasets)

```sql
-- Partition sensor_readings by month
CREATE TABLE sensor_readings (
    id              UUID NOT NULL,
    station_id      UUID NOT NULL,
    measured_at     TIMESTAMPTZ NOT NULL,
    value           DOUBLE PRECISION NOT NULL,
    -- other columns...
) PARTITION BY RANGE (measured_at);

-- Create monthly partitions
CREATE TABLE sensor_readings_2026_01 PARTITION OF sensor_readings
    FOR VALUES FROM ('2026-01-01') TO ('2026-02-01');

CREATE TABLE sensor_readings_2026_02 PARTITION OF sensor_readings
    FOR VALUES FROM ('2026-02-01') TO ('2026-03-01');
```

#### 2. Indexes for Timeseries Queries

```sql
-- Composite index for time-range queries
CREATE INDEX ix_readings_station_time ON sensor_readings(station_id, measured_at DESC);

-- BRIN index for large tables (more efficient than B-tree for timeseries)
CREATE INDEX ix_readings_measured_at_brin ON sensor_readings USING BRIN (measured_at);

-- Partial index for recent data
CREATE INDEX ix_readings_recent ON sensor_readings(station_id, measured_at)
    WHERE measured_at > NOW() - INTERVAL '30 days';
```

### Caching Strategy

| Cache Key | TTL | Invalidation |
|-----------|-----|--------------|
| `flood_history:{stationId}:{granularity}:{date}` | 5 min | On new reading |
| `flood_trends:{stationId}:{period}` | 1 hour | Hourly job |
| `flood_statistics:{stationId}:{period}` | 6 hours | Daily job |

### Query Optimization

#### 1. Use Aggregation Tables

```csharp
// Choose data source based on time range
if (timeRange <= TimeSpan.FromHours(24))
    return await QueryRawReadings(stationId, startDate, endDate);
else if (timeRange <= TimeSpan.FromDays(30))
    return await QueryHourlyAgg(stationId, startDate, endDate);
else
    return await QueryDailyAgg(stationId, startDate, endDate);
```

#### 2. Pagination for Large Results

```csharp
// Cursor-based pagination
public async Task<(List<FloodDataPointDto> Data, string? NextCursor)> GetPagedHistory(
    Guid stationId,
    DateTime startDate,
    string? cursor,
    int limit = 1000)
{
    var query = _context.SensorReadings
        .Where(r => r.StationId == stationId && r.MeasuredAt >= startDate)
        .OrderBy(r => r.MeasuredAt);

    if (cursor != null)
    {
        var cursorTime = DecodeCursor(cursor);
        query = query.Where(r => r.MeasuredAt > cursorTime);
    }

    var results = await query.Take(limit + 1).ToListAsync();

    var hasMore = results.Count > limit;
    var data = results.Take(limit).ToList();
    var nextCursor = hasMore ? EncodeCursor(data.Last().MeasuredAt) : null;

    return (data, nextCursor);
}
```

---

## 📝 IMPLEMENTATION CHECKLIST

### 🟡 Phase 1: Domain Layer
- [ ] Create `SensorHourlyAgg` entity
- [ ] Create `FloodStatistics` entity (if not exists)
- [ ] Create repository interfaces
- [ ] Update `AppDbContext`

### 🟡 Phase 2: Infrastructure Layer
- [ ] Implement repositories
- [ ] Create database migrations
- [ ] Add new indexes

### 🟡 Phase 3-5: Application Layer
- [ ] FeatG39: GetFloodHistory handler
- [ ] FeatG40: GetFloodTrends handler
- [ ] FeatG41: GetFloodStatistics handler
- [ ] Create validators
- [ ] Create StatusCode enums

### 🟡 Phase 6: Mapper Layer
- [ ] Create DTOs
- [ ] Create mappers
- [ ] Register in DI

### 🟡 Phase 7: Presentation Layer
- [ ] Create endpoints
- [ ] Create endpoint DTOs

### 🟡 Phase 8: Background Jobs
- [ ] HourlyAggregationJob
- [ ] DailyAggregationJob
- [ ] Job scheduling

### 🟡 Phase 9: Testing
- [ ] Unit tests
- [ ] Integration tests
- [ ] Performance tests

---

## 🎯 SUCCESS CRITERIA

### Functional Requirements
- [ ] Users can query historical flood data with time filters
- [ ] Users can view aggregated trends (day/week/month)
- [ ] Users can get summary statistics
- [ ] Missing data intervals are detected and reported
- [ ] Comparison with previous period is available

### Non-Functional Requirements
- [ ] API response time < 500ms for 30-day queries
- [ ] API response time < 2s for 1-year queries
- [ ] Aggregation jobs complete within 5 minutes
- [ ] Data completeness > 99%
- [ ] Chart-ready JSON format

---

**Document Version**: 1.0
**Last Updated**: 2026-01-16
**Author**: FDA Development Team
**Status**: 🟡 Planning
**Next Steps**: Create implementation plan with detailed code
