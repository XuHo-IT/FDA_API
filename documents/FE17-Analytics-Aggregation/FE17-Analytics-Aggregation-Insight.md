# FE-17: Analytics Aggregation - Technical Analysis

## 📋 Table of Contents

- [Overview](#overview)
- [Strengths](#strengths)
- [Areas for Adjustment](#areas-for-adjustment)
  - [1. Aggregation Strategy & Performance](#1-aggregation-strategy--performance)
  - [2. Data Consistency & Idempotency](#2-data-consistency--idempotency)
  - [3. Cache Strategy](#3-cache-strategy)
  - [4. Job Scheduling & Monitoring](#4-job-scheduling--monitoring)
- [Implementation Plan](#implementation-plan)
- [Database Schema Requirements](#database-schema-requirements)
- [API Endpoints Requirements](#api-endpoints-requirements)
- [Feature Breakdown for Backend](#feature-breakdown-for-backend)
- [Final Recommendations](#final-recommendations)

---

## Overview

**Status:** ✅ Well-designed analytics aggregation system

This feature implements a batch aggregation layer that transforms raw flood sensor data into analytics metrics (frequency, severity, hotspots). It serves as the foundation for FE-16 (History & Trends) and FE-18 (Hotspots Map - ⚠️ Not yet implemented) by providing pre-computed, query-optimized analytics data.

---

## Strengths

**Separation of Concerns:**
- ✅ **Read-Optimized Tables**: Analytics tables are separate from raw data, optimized for queries
- ✅ **Idempotent Design**: Jobs can be re-run safely without double-counting
- ✅ **Time Bucket Granularity**: Supports day/week/month/year for flexible analysis

**Performance Optimization:**
- ✅ **Pre-computed Aggregates**: Avoids scanning millions of raw readings for each query
- ✅ **Cache Layer**: Redis cache for frequently accessed analytics
- ✅ **Indexed Queries**: Optimized indexes on area_id + time_bucket

**Job Management:**
- ✅ **Job Tracking**: Full audit trail of aggregation runs
- ✅ **Error Handling**: Failed jobs logged with error messages
- ✅ **Manual Triggers**: Admin can trigger re-aggregation for data corrections

---

## Areas for Adjustment

### 1. Aggregation Strategy & Performance

**Issue:** Aggregating large datasets (millions of readings) can be slow and memory-intensive.

**Backend Implementation Requirements:**

| Component | Status | Description |
|-----------|--------|-------------|
| Batch Processing | ✅ Required | Process data in chunks (e.g., 10,000 records at a time) |
| Parallel Processing | ❌ Optional | Process multiple areas in parallel (if safe) |
| Incremental Aggregation | ✅ Recommended | Only process new data since last run |
| Database Partitioning | ❌ Future | Partition analytics tables by month/year for large scale |

**Recommendation:**
- **V1**: Implement batch processing with chunking
- **V2**: Add incremental aggregation (track last processed timestamp)
- **V3**: Consider partitioning for tables > 10M rows

**Example Implementation:**
```csharp
public async Task AggregateFrequencyAsync(
    DateTime bucketStart,
    DateTime bucketEnd,
    CancellationToken ct)
{
    const int batchSize = 10000;
    var offset = 0;
    
    while (true)
    {
        var readings = await _readingRepository
            .GetBatchAsync(bucketStart, bucketEnd, offset, batchSize, ct);
        
        if (!readings.Any())
            break;
        
        // Process batch
        var aggregates = CalculateFrequency(readings);
        await _frequencyRepository.BulkUpsertAsync(aggregates, ct);
        
        offset += batchSize;
    }
}
```

---

### 2. Data Consistency & Idempotency

**Issue:** Re-running aggregation jobs must not create duplicates or incorrect counts.

**Backend Implementation Requirements:**

| Component | Status | Description |
|-----------|--------|-------------|
| Upsert Logic | ✅ Required | Use `ON CONFLICT ... DO UPDATE` in PostgreSQL |
| Transaction Scope | ✅ Required | Wrap entire bucket aggregation in transaction |
| Late Data Handling | ✅ Required | Support backfilling historical buckets |
| Data Validation | ✅ Required | Validate aggregated results before persisting |

**Recommendation:**
- Use PostgreSQL `ON CONFLICT` for idempotent upserts
- Wrap each time bucket in a transaction
- Support manual backfill for historical corrections

**Example SQL:**
```sql
INSERT INTO flood_analytics_frequency (
    area_id, time_bucket, bucket_type, event_count, exceed_count
)
VALUES ($1, $2, $3, $4, $5)
ON CONFLICT (area_id, time_bucket, bucket_type)
DO UPDATE SET
    event_count = EXCLUDED.event_count,
    exceed_count = EXCLUDED.exceed_count,
    calculated_at = NOW();
```

---

### 3. Cache Strategy

**Issue:** Analytics queries need to be fast, but cache invalidation must be reliable.

**Backend Implementation Requirements:**

| Component | Status | Description |
|-----------|--------|-------------|
| Cache Keys | ✅ Required | Structured keys: `analytics:{metric}:{areaId}:{bucketType}:{date}` |
| TTL Strategy | ✅ Required | Short TTL (1-6 hours) with job-based invalidation |
| Cache Invalidation | ✅ Required | Invalidate on re-aggregation |
| Cache Warming | ❌ Optional | Pre-populate cache after aggregation |

**Recommendation:**
- Use Redis with structured keys
- Invalidate cache when re-aggregating specific buckets
- Use version-based cache keys for easier invalidation

**Example Implementation:**
```csharp
// Cache key with version
var cacheKey = $"analytics:frequency:{areaId}:{bucketType}:{date}:v{version}";

// Invalidate on re-aggregation
await _cache.RemoveAsync($"analytics:frequency:{areaId}:{bucketType}:*", ct);
```

---

### 4. Job Scheduling & Monitoring

**Issue:** Jobs must run reliably, and failures must be visible to admins.

**Backend Implementation Requirements:**

| Component | Status | Description |
|-----------|--------|-------------|
| Quartz Scheduler | ✅ Required | Use Quartz.NET for job scheduling |
| Job Retry Logic | ✅ Required | Retry failed jobs (max 3 attempts) |
| Job Monitoring | ✅ Required | Dashboard/API to view job status |
| Alerting | ❌ Optional | Email/Slack alerts on job failures |

**Recommendation:**
- Use Quartz.NET for scheduling
- Implement retry logic with exponential backoff
- Provide admin API to view job history and trigger manual runs

**Example Configuration:**
```csharp
// Daily aggregation at 2 AM
services.AddQuartz(q =>
{
    q.ScheduleJob<DailyAggregationJob>(trigger => trigger
        .WithCronSchedule("0 0 2 * * ?")  // 2 AM daily
        .WithIdentity("daily-aggregation"));
});
```

---

## Implementation Plan

### Phase 1: Domain Layer

**Entities/Analytics.cs**
```csharp
public class FloodAnalyticsFrequency : EntityWithId<Guid>
{
    public Guid AreaId { get; set; }
    public DateTime TimeBucket { get; set; }
    public string BucketType { get; set; }  // "day", "week", "month", "year"
    public int EventCount { get; set; }
    public int ExceedCount { get; set; }
    public DateTime CalculatedAt { get; set; }
    
    public virtual Area Area { get; set; }
}

public class FloodAnalyticsSeverity : EntityWithId<Guid>
{
    public Guid AreaId { get; set; }
    public DateTime TimeBucket { get; set; }
    public string BucketType { get; set; }
    public decimal MaxLevel { get; set; }
    public decimal AvgLevel { get; set; }
    public decimal MinLevel { get; set; }
    public int DurationHours { get; set; }
    public int ReadingCount { get; set; }
    public DateTime CalculatedAt { get; set; }
    
    public virtual Area Area { get; set; }
}

public class FloodAnalyticsHotspot : EntityWithId<Guid>
{
    public Guid AreaId { get; set; }
    public decimal Score { get; set; }
    public int Rank { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime CalculatedAt { get; set; }
    
    public virtual Area Area { get; set; }
}
```

**Repositories/IAnalyticsRepository.cs**
```csharp
public interface IFloodAnalyticsFrequencyRepository
{
    Task<List<FloodAnalyticsFrequency>> GetByAreaAndPeriodAsync(
        Guid areaId,
        DateTime startDate,
        DateTime endDate,
        string bucketType,
        CancellationToken ct);
    
    Task BulkUpsertAsync(
        List<FloodAnalyticsFrequency> aggregates,
        CancellationToken ct);
}

public interface IFloodAnalyticsSeverityRepository
{
    Task<List<FloodAnalyticsSeverity>> GetByAreaAndPeriodAsync(
        Guid areaId,
        DateTime startDate,
        DateTime endDate,
        string bucketType,
        CancellationToken ct);
    
    Task BulkUpsertAsync(
        List<FloodAnalyticsSeverity> aggregates,
        CancellationToken ct);
}

public interface IFloodAnalyticsHotspotRepository
{
    Task<List<FloodAnalyticsHotspot>> GetTopHotspotsAsync(
        DateTime periodStart,
        DateTime periodEnd,
        int topN,
        CancellationToken ct);
    
    Task BulkUpsertAsync(
        List<FloodAnalyticsHotspot> hotspots,
        CancellationToken ct);
}
```

---

### Phase 2: Application Layer (CQRS)

**FeatG47_FrequencyAggregation/FrequencyAggregationHandler.cs**
```csharp
public class FrequencyAggregationHandler
    : IRequestHandler<FrequencyAggregationRequest, FrequencyAggregationResponse>
{
    private readonly IFloodEventRepository _eventRepository;
    private readonly ISensorReadingRepository _readingRepository;
    private readonly IFloodAnalyticsFrequencyRepository _frequencyRepository;
    private readonly IAnalyticsJobRunRepository _jobRunRepository;
    
    public async Task<FrequencyAggregationResponse> Handle(
        FrequencyAggregationRequest request,
        CancellationToken ct)
    {
        var jobRun = await CreateJobRunAsync("FREQUENCY_AGG", ct);
        
        try
        {
            // 1. Get time buckets
            var buckets = GenerateTimeBuckets(
                request.StartDate,
                request.EndDate,
                request.BucketType);
            
            // 2. Process each bucket
            foreach (var bucket in buckets)
            {
                await AggregateBucketAsync(
                    bucket.Start,
                    bucket.End,
                    request.BucketType,
                    request.AreaIds,
                    ct);
            }
            
            // 3. Mark job success
            await UpdateJobRunStatusAsync(jobRun.Id, "SUCCESS", ct);
            
            return new FrequencyAggregationResponse
            {
                Success = true,
                JobRunId = jobRun.Id
            };
        }
        catch (Exception ex)
        {
            await UpdateJobRunStatusAsync(
                jobRun.Id,
                "FAILED",
                ex.Message,
                ct);
            throw;
        }
    }
    
    private async Task AggregateBucketAsync(
        DateTime bucketStart,
        DateTime bucketEnd,
        string bucketType,
        List<Guid>? areaIds,
        CancellationToken ct)
    {
        // Get areas to process
        var areas = areaIds != null
            ? await _areaRepository.GetByIdsAsync(areaIds, ct)
            : await _areaRepository.GetAllAsync(ct);
        
        foreach (var area in areas)
        {
            // Count events
            var eventCount = await _eventRepository.CountByAreaAndPeriodAsync(
                area.Id,
                bucketStart,
                bucketEnd,
                ct);
            
            // Count exceedances
            var exceedCount = await _readingRepository.CountExceedancesAsync(
                area.Id,
                bucketStart,
                bucketEnd,
                ct);
            
            // Upsert aggregate
            var aggregate = new FloodAnalyticsFrequency
            {
                AreaId = area.Id,
                TimeBucket = bucketStart,
                BucketType = bucketType,
                EventCount = eventCount,
                ExceedCount = exceedCount,
                CalculatedAt = DateTime.UtcNow
            };
            
            await _frequencyRepository.UpsertAsync(aggregate, ct);
        }
    }
}
```

---

### Phase 3: Presentation Layer (FastEndpoints)

**Endpoints/Feat42_FrequencyAggregation/FrequencyAggregationEndpoint.cs**
```csharp
public class FrequencyAggregationEndpoint
    : Endpoint<FrequencyAggregationRequestDto, FrequencyAggregationResponseDto>
{
    private readonly IMediator _mediator;
    
    public FrequencyAggregationEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public override void Configure()
    {
        Post("/api/v1/analytics/aggregate-frequency");
        Policies("Admin");  // Admin only
    }
    
    public override async Task HandleAsync(
        FrequencyAggregationRequestDto req,
        CancellationToken ct)
    {
        var request = new FrequencyAggregationRequest
        {
            BucketType = req.BucketType,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            AreaIds = req.AreaIds
        };
        
        var result = await _mediator.Send(request, ct);
        
        var response = new FrequencyAggregationResponseDto
        {
            Success = result.Success,
            Message = result.Message,
            Data = new JobRunDto
            {
                JobRunId = result.JobRunId,
                JobType = "FREQUENCY_AGG",
                Status = "RUNNING",
                StartedAt = DateTime.UtcNow
            }
        };
        
        await SendAsync(response, 202, ct);  // 202 Accepted
    }
}
```

---

## Database Schema Requirements

### Recommended Approach: Analytics Tables + Job Management

```sql
-- Analytics output tables (read-optimized)
CREATE TABLE flood_analytics_frequency (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    area_id         UUID NOT NULL REFERENCES areas(id),
    time_bucket     TIMESTAMPTZ NOT NULL,
    bucket_type     VARCHAR(20) NOT NULL,
    event_count     INT DEFAULT 0,
    exceed_count    INT DEFAULT 0,
    calculated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_frequency_area_bucket UNIQUE (area_id, time_bucket, bucket_type)
);

CREATE TABLE flood_analytics_severity (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    area_id         UUID NOT NULL REFERENCES areas(id),
    time_bucket     TIMESTAMPTZ NOT NULL,
    bucket_type     VARCHAR(20) NOT NULL,
    max_level       NUMERIC(14,4),
    avg_level       NUMERIC(14,4),
    min_level       NUMERIC(14,4),
    duration_hours   INT DEFAULT 0,
    reading_count   INT DEFAULT 0,
    calculated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_severity_area_bucket UNIQUE (area_id, time_bucket, bucket_type)
);

CREATE TABLE flood_analytics_hotspots (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    area_id         UUID NOT NULL REFERENCES areas(id),
    score           NUMERIC(14,4) NOT NULL,
    rank            INT,
    period_start    TIMESTAMPTZ NOT NULL,
    period_end      TIMESTAMPTZ NOT NULL,
    calculated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_hotspot_area_period UNIQUE (area_id, period_start, period_end)
);

-- Job management tables
CREATE TABLE analytics_jobs (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    job_type        VARCHAR(50) NOT NULL UNIQUE,
    schedule        VARCHAR(100),
    is_active       BOOLEAN DEFAULT true,
    last_run_at     TIMESTAMPTZ,
    next_run_at     TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE analytics_job_runs (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    job_id          UUID NOT NULL REFERENCES analytics_jobs(id),
    started_at      TIMESTAMPTZ NOT NULL,
    finished_at     TIMESTAMPTZ,
    status          VARCHAR(20) NOT NULL,
    error_message   TEXT,
    records_processed INT,
    records_created  INT,
    execution_time_ms INT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Indexes for performance
CREATE INDEX ix_frequency_area_bucket ON flood_analytics_frequency(area_id, time_bucket DESC);
CREATE INDEX ix_severity_area_bucket ON flood_analytics_severity(area_id, time_bucket DESC);
CREATE INDEX ix_hotspot_score ON flood_analytics_hotspots(score DESC, calculated_at DESC);
CREATE INDEX ix_job_runs_job ON analytics_job_runs(job_id, started_at DESC);
```

---

## API Endpoints Requirements

### FeatG47: Trigger Frequency Aggregation
**Endpoint:** `POST /api/v1/analytics/aggregate-frequency`
**Auth:** Admin

### FeatG48: Trigger Severity Aggregation
**Endpoint:** `POST /api/v1/analytics/aggregate-severity`
**Auth:** Admin

### FeatG49: Trigger Hotspot Aggregation
**Endpoint:** `POST /api/v1/analytics/aggregate-hotspots`
**Auth:** Admin
**⚠️ Note**: Prepared for FE-18 (Hotspots Map) which is not yet implemented. Can be used for reporting/analysis.

### FeatG50: Get Job Status
**Endpoint:** `GET /api/v1/analytics/jobs/{jobRunId}/status`
**Auth:** Admin, Authority

### FeatG51: Get Frequency Analytics
**Endpoint:** `GET /api/v1/analytics/frequency`
**Auth:** User (consumed by FE-16)

### FeatG52: Get Severity Analytics
**Endpoint:** `GET /api/v1/analytics/severity`
**Auth:** User (consumed by FE-16)

### FeatG53: Get Hotspot Rankings
**Endpoint:** `GET /api/v1/analytics/hotspots`
**Auth:** User
**⚠️ Note**: Prepared for FE-18 (Hotspots Map) which is not yet implemented. Endpoint can be used independently for reporting/analysis or integrated when FE-18 is implemented.

---

## Feature Breakdown for Backend

### Analytics Aggregation Features (FeatG47-53)

| Feature | Handler | Endpoint | Auth | Description |
|---------|---------|----------|------|-------------|
| **FeatG47** | `FrequencyAggregationHandler` | `POST /api/v1/analytics/aggregate-frequency` | Admin | Trigger frequency aggregation |
| **FeatG48** | `SeverityAggregationHandler` | `POST /api/v1/analytics/aggregate-severity` | Admin | Trigger severity aggregation |
| **FeatG49** | `HotspotAggregationHandler` | `POST /api/v1/analytics/aggregate-hotspots` | Admin | Trigger hotspot aggregation |
| **FeatG50** | `GetJobStatusHandler` | `GET /api/v1/analytics/jobs/{id}/status` | Admin, Authority | Get job status |
| **FeatG51** | `GetFrequencyAnalyticsHandler` | `GET /api/v1/analytics/frequency` | User | Get frequency data (FE-16) |
| **FeatG52** | `GetSeverityAnalyticsHandler` | `GET /api/v1/analytics/severity` | User | Get severity data (FE-16) |
| **FeatG53** | `GetHotspotRankingsHandler` | `GET /api/v1/analytics/hotspots` | User | Get hotspots (⚠️ Prepared for FE-18, not yet implemented) |

### Data Management Features (FeatG57-66)

**Administrative Area Management (FeatG57-61):**
These features support the analytics aggregation system by providing management capabilities for administrative areas, which are used as the primary grouping dimension for aggregation.

| Feature | Handler | Endpoint | Auth | Description |
|---------|---------|----------|------|-------------|
| **FeatG57** | `CreateAdministrativeAreaHandler` | `POST /api/v1/admin/administrative-areas` | Admin | Create administrative area |
| **FeatG58** | `GetAdministrativeAreasHandler` | `GET /api/v1/admin/administrative-areas` | Admin | List administrative areas |
| **FeatG59** | `GetAdministrativeAreaHandler` | `GET /api/v1/admin/administrative-areas/{id}` | Admin | Get administrative area |
| **FeatG60** | `UpdateAdministrativeAreaHandler` | `PUT /api/v1/admin/administrative-areas/{id}` | Admin | Update administrative area |
| **FeatG61** | `DeleteAdministrativeAreaHandler` | `DELETE /api/v1/admin/administrative-areas/{id}` | Admin | Delete administrative area |

**Flood Event Management (FeatG62-66):**
These features support frequency aggregation by providing management capabilities for flood events, which are counted in the frequency aggregation process.

| Feature | Handler | Endpoint | Auth | Description |
|---------|---------|----------|------|-------------|
| **FeatG62** | `CreateFloodEventHandler` | `POST /api/v1/admin/flood-events` | Admin | Create flood event |
| **FeatG63** | `GetFloodEventsHandler` | `GET /api/v1/admin/flood-events` | Admin | List flood events |
| **FeatG64** | `GetFloodEventHandler` | `GET /api/v1/admin/flood-events/{id}` | Admin | Get flood event |
| **FeatG65** | `UpdateFloodEventHandler` | `PUT /api/v1/admin/flood-events/{id}` | Admin | Update flood event |
| **FeatG66** | `DeleteFloodEventHandler` | `DELETE /api/v1/admin/flood-events/{id}` | Admin | Delete flood event |

**Integration Notes:**
- Administrative areas define geographic boundaries for aggregation grouping
- Flood events are counted in frequency aggregation (`event_count` in `FloodAnalyticsFrequency`)
- When administrative areas or flood events are modified, re-aggregation may be required
- All data management features require Admin role for security

---

## Final Recommendations

1. **Batch Processing**: Implement chunking for large datasets to avoid memory issues.
2. **Idempotency**: Use PostgreSQL `ON CONFLICT` for safe re-runs.
3. **Cache Strategy**: Use Redis with structured keys and TTL-based invalidation.
4. **Job Monitoring**: Provide admin dashboard to view job history and trigger manual runs.
5. **Incremental Aggregation**: Track last processed timestamp to only process new data (V2).

---

## Adjusted Feature Definition

**Feature Name:** FE-17 - Analytics Aggregation

**Scope:**
- Batch aggregation jobs (frequency, severity, hotspots)
- Analytics tables (read-optimized)
- Job management and monitoring
- Query APIs for FE-16 and FE-18 (⚠️ FE-18 not yet implemented, but endpoints prepared)
- Administrative area management (FeatG57-61) - Support data grouping for aggregation
- Flood event management (FeatG62-66) - Support frequency aggregation calculations

---

## Summary

FE-17 provides a critical analytics layer that enables fast queries for history/trends and hotspot visualization. By focusing on idempotency, performance, and job management, the system remains scalable and maintainable as data volume grows.

