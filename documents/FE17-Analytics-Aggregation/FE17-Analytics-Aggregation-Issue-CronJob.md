# Issue: Analytics Aggregation Cron Jobs Not Executing on Schedule (FE-17)

> **Issue ID**: FE-17-ISSUE-001  
> **Created**: 2026-01-17  
> **Status**: 🔴 Open  
> **Priority**: High  
> **Component**: Analytics Aggregation (FE-17)  
> **Affected Features**: FeatG47 (Frequency Aggregation), FeatG48 (Severity Aggregation), FeatG49 (Hotspot Aggregation)

---

## 📋 Issue Summary

The analytics aggregation jobs (Frequency, Severity, and Hotspot) are **not executing automatically on their scheduled times**. According to FE-17 documentation, these jobs should run:
- **Daily at 2 AM** (Frequency & Severity aggregation)
- **Weekly on Monday** (Hotspot aggregation)
- **Monthly on 1st** (Monthly aggregation)

However, the current implementation only supports **manual triggering** via API endpoints. The scheduled/recurring job configuration is **missing**.

---

## Scheduling Strategy (Official Decision)

- The system uses **Hangfire Recurring Jobs** for scheduled analytics aggregation.
- Quartz.NET is reserved only for system-level or complex scheduling scenarios (e.g., clustered triggers, calendar-based exclusions, complex dependencies); not used for FE-17 analytics.
- No external OS cron or external scheduler is used.

---

## Timezone Policy

- All scheduled jobs are defined in **Vietnam Time (`Asia/Ho_Chi_Minh`)**.
- Hangfire `RecurringJobOptions.TimeZone` is explicitly set for every recurring job.
- No manual UTC offset conversion is applied inside cron expressions.

> Vietnam has no DST → stable and predictable schedules.

---

## Cron Expression Format (Hangfire)

- Hangfire uses **5-field cron**: `Minute Hour Day Month DayOfWeek`.
- Quartz 6/7-field cron is **not** used for FE-17 jobs.
- Prefer Hangfire `Cron.*` helpers over raw strings to avoid mistakes.

---

## Job Execution & Dependency Injection

- Recurring jobs must **not** rely on singletons that wrap scoped services.
- Hangfire resolves job dependencies from DI per execution scope; prefer **generic AddOrUpdate<T>()** so DI is fresh each run.
- Jobs trigger aggregation via **application-level handlers (MediatR)**; scheduling layer contains no business logic.
- Do **not** register a scheduler class as Singleton if it injects scoped services (e.g., `IMediator`, `DbContext`).

---

## Date Calculation Rule

- Scheduling layer does **not** compute business date ranges.
- Date ranges are computed **inside the application handlers** at execution time.
- Scheduled jobs only express intent (e.g., “run daily aggregation”), the handler derives the exact period:
  - Daily job → handler processes **yesterday** (completed day)
  - Weekly job → handler processes **previous completed week** (Mon 00:00 → Sun 23:59)
  - Monthly job → handler processes **previous completed month**

---

## Idempotency & Re-run Policy

- Aggregation jobs must be **idempotent**.
- Re-runs (manual or retry) must **not** create duplicates.
- Data is overwritten/upserted by `(Period + Area + MetricType)`.

---

## Manual vs Scheduled Execution

- Manual API-triggered aggregation and scheduled aggregation call the **same application handlers**.
- Scheduling layer holds **no business logic**; it only triggers the handlers.

---

## Monitoring & Verification

- All recurring jobs must be visible in the **Hangfire Dashboard**.
- Execution history is used to verify execution time, duration, failures, and retries.

---

## Summary Table

| Aspect            | Decision                          |
|-------------------|-----------------------------------|
| Scheduler         | Hangfire Recurring Jobs           |
| Timezone          | Asia/Ho_Chi_Minh                  |
| Cron Format       | Hangfire 5-field                  |
| Date Calculation  | At job execution time             |
| Execution Layer   | MediatR handlers                  |
| Idempotency       | Required (upsert/overwrite)       |
| Dashboard         | Hangfire                          |

---

## 🔍 Root Cause Analysis

### Current Implementation Status

1. **Hangfire is Configured**: Hangfire is properly set up in `ServiceExtensions.cs` with PostgreSQL storage
2. **Background Jobs Exist**: The background job classes are implemented:
   - `FrequencyAggregationBackgroundJob`
   - `SeverityAggregationBackgroundJob`
   - `HotspotAggregationBackgroundJob`
3. **Manual Triggering Works**: Jobs can be triggered manually via API endpoints:
   - `POST /api/v1/analytics/aggregate-frequency`
   - `POST /api/v1/analytics/aggregate-severity`
   - `POST /api/v1/analytics/aggregate-hotspots`
4. **❌ Missing**: Recurring job registration using `RecurringJob.AddOrUpdate()`

### Evidence

**File**: `src/External/Infrastructure/Common/FDAAPI.Infra.Configuration/ServiceExtensions.cs`

```csharp
// Hangfire for on-demand background jobs (analytics aggregation)
services.AddHangfire(config => config
    .UsePostgreSqlStorage(connectionString, ...));

services.AddHangfireServer(options => { ... });
```

**Missing Code**: No `RecurringJob.AddOrUpdate()` calls to register scheduled jobs.

**Documentation Reference**: `documents/FE17-Analytics-Aggregation.md` (Line 128)
```
**Job Scheduling** | Daily at 2 AM, Weekly on Monday, Monthly on 1st | 🟡 New
```

**Expected Behavior** (from documentation):
```
[Scheduler (Cron/Quartz)]
   ↓ (daily at 2 AM / weekly on Monday / monthly on 1st)
[Analytics Aggregation Service]
```

**Actual Behavior**: Jobs only run when manually triggered via API.

---

## 🎯 Impact

### Functional Impact
- ❌ Analytics data is not automatically updated on schedule
- ❌ Users may see stale analytics data until manual aggregation is triggered
- ❌ System does not meet the documented requirements for scheduled aggregation
- ❌ Cache may become stale without regular updates

### Business Impact
- 📊 Analytics dashboards may show outdated information
- ⚠️ Flood risk assessments may be based on incomplete data
- 🔄 Manual intervention required to keep analytics current
- 📈 Historical trend analysis may have gaps

---

## 🔧 Suggested Solutions

### Solution 1: Implement Hangfire Recurring Jobs (Recommended)

**Approach**: Use Hangfire's `RecurringJob.AddOrUpdate()` to register scheduled jobs that automatically trigger the aggregation handlers.

#### Implementation Steps (DI-safe)

1. **Register Recurring Jobs (DI-safe, no singleton scheduler)**

   **File**: `src/External/Infrastructure/Common/FDAAPI.Infra.Configuration/ServiceExtensions.cs`

   ```csharp
   var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");

   // Frequency – daily 02:00 VN time
   RecurringJob.AddOrUpdate<FrequencyAggregationRunner>(
       "frequency-aggregation-daily",
       runner => runner.RunDailyAsync(),
       Cron.Daily(2),
       new RecurringJobOptions { TimeZone = vnTimeZone });

   // Severity – daily 02:15 VN time
   RecurringJob.AddOrUpdate<SeverityAggregationRunner>(
       "severity-aggregation-daily",
       runner => runner.RunDailyAsync(),
       Cron.Daily(2, 15),
       new RecurringJobOptions { TimeZone = vnTimeZone });

   // Hotspot – weekly Monday 03:00 VN time (previous completed week handled inside handler)
   RecurringJob.AddOrUpdate<HotspotAggregationRunner>(
       "hotspot-aggregation-weekly",
       runner => runner.RunWeeklyAsync(),
       Cron.Weekly(DayOfWeek.Monday, 3),
       new RecurringJobOptions { TimeZone = vnTimeZone });

   // Monthly – day 1 at 04:00 VN time (previous completed month handled inside handler)
   RecurringJob.AddOrUpdate<FrequencyAggregationRunner>(
       "frequency-aggregation-monthly",
       runner => runner.RunMonthlyAsync(),
       Cron.Monthly(1, 4),
       new RecurringJobOptions { TimeZone = vnTimeZone });
   ```

2. **Runner classes resolve scoped services per execution**

   **Pattern** (example):

   ```csharp
   public class FrequencyAggregationRunner
   {
       private readonly IMediator _mediator;
       private readonly ILogger<FrequencyAggregationRunner> _logger;

       public FrequencyAggregationRunner(IMediator mediator, ILogger<FrequencyAggregationRunner> logger)
       {
           _mediator = mediator;
           _logger = logger;
       }

       public async Task RunDailyAsync()
       {
           _logger.LogInformation("Starting scheduled daily frequency aggregation");
           await _mediator.Send(new FrequencyAggregationCommand { Mode = FrequencyAggregationMode.Daily });
       }

       public async Task RunMonthlyAsync()
       {
           _logger.LogInformation("Starting scheduled monthly frequency aggregation");
           await _mediator.Send(new FrequencyAggregationCommand { Mode = FrequencyAggregationMode.Monthly });
       }
   }
   ```

   - Handlers compute date ranges (daily/weekly/monthly) at execution time.
   - No dates are captured during job registration.
   - Runners are transient/scoped per execution; no singleton schedulers wrapping scoped dependencies.
   - Recurring job registration is invoked **after app startup** (e.g., in `Program.cs` after `app.Build()`, or via a lightweight `IHostedService` that only registers cron). Do not place registration in the request pipeline.

> ⚠️ Deployment note: Linux containers must have `tzdata` installed, otherwise `TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh")` will throw.

#### Cron / Schedule Reference (Hangfire)

> ⚠️ Hangfire uses **5-field** cron (not Quartz 6-field). Prefer built-in helpers. Use raw strings only if no helper exists.

| Schedule              | Hangfire Helper            | Description (Vietnam Time) |
|-----------------------|----------------------------|----------------------------|
| Daily at 2:00 AM      | `Cron.Daily(2)`            | Every day at 02:00 VN time |
| Daily at 2:15 AM      | `Cron.Daily(2, 15)`        | Every day at 02:15 VN time |
| Weekly Monday 3:00 AM | `Cron.Weekly(DayOfWeek.Monday, 3)` | Every Monday at 03:00 VN time |
| Monthly 1st, 4:00 AM  | `Cron.Monthly(1, 4)`       | 1st day of month at 04:00 VN time |

#### Advantages
- ✅ Uses existing Hangfire infrastructure
- ✅ Jobs visible in Hangfire dashboard
- ✅ Can be paused/resumed from dashboard
- ✅ Automatic retry on failure (if configured)
- ✅ No additional dependencies

#### Considerations
- ⚠️ Requires Hangfire server to be running
- ⚠️ Jobs are stored in Hangfire's PostgreSQL schema
- ⚠️ Timezone must be explicitly set (project uses `Asia/Ho_Chi_Minh` to avoid manual UTC conversion)

---

## 🧪 Testing Strategy

### Test Cases

1. **Scheduled Job Execution**
   - ✅ Verify jobs execute at correct times
   - ✅ Verify jobs process correct date ranges (yesterday for daily, last week for weekly, etc.)
   - ✅ Verify jobs create `AnalyticsJobRun` records

2. **Job Failure Handling**
   - ✅ Verify failed jobs are logged
   - ✅ Verify retry mechanism works (if configured)
   - ✅ Verify no partial data is written on failure

3. **Concurrent Execution**
   - ✅ Verify manual trigger doesn't conflict with scheduled job

4. **Timezone Handling**
   - ✅ Verify jobs use Asia/Ho_Chi_Minh consistently
   - ✅ Verify date calculations are correct for daily/weekly/monthly ranges

5. **Hangfire Dashboard Verification**
   - ✅ Verify recurring jobs appear in Hangfire dashboard
   - ✅ Verify job execution history is visible
   - ✅ Verify jobs can be manually triggered/paused from dashboard

---

## 📝 Implementation Checklist

### For Solution 1 (Hangfire Recurring Jobs)

- [ ] Register Hangfire recurring jobs (no scheduler singleton; use `RecurringJob.AddOrUpdate<T>()`)
- [ ] Ensure runners resolve scoped services per execution
- [ ] Implement trigger methods for each job type
- [ ] Initialize recurring jobs in `Program.cs` (after `app.Build()`) or a lightweight registrar that does not inject scoped services
- [ ] Add logging for job execution
- [ ] Test job execution at scheduled times
- [ ] Verify jobs appear in Hangfire dashboard
- [ ] Update documentation

---

## 🔄 Migration Plan

1. **Phase 1: Implementation**
   - Implement chosen solution
   - Add comprehensive logging
   - Test in development environment

2. **Phase 2: Testing**
   - Test scheduled execution
   - Test failure scenarios
   - Test concurrent execution
   - Verify data correctness

3. **Phase 3: Deployment**
   - Deploy to UAT environment
   - Monitor job execution for 1 week
   - Verify analytics data is updated correctly
   - Deploy to production

4. **Phase 4: Monitoring**
   - Set up alerts for job failures
   - Monitor job execution times
   - Review analytics data freshness

---

## 📚 Related Documentation

- **FE-17 Main Document**: `documents/FE17-Analytics-Aggregation.md`
- **FE-17 Insight**: `documents/FE17-Analytics-Aggregation-Insight.md`
- **FE-17 Testing**: `documents/FE17-Analytics-Aggregation-Testing.md`
- **Hangfire Documentation**: https://docs.hangfire.io/

---

## 🎯 Success Criteria

- ✅ Jobs execute automatically at scheduled times
- ✅ Analytics data is updated daily without manual intervention
- ✅ Jobs are visible and manageable via dashboard
- ✅ Failed jobs are logged and can be retried
- ✅ No conflicts between scheduled and manual job execution
- ✅ System meets documented requirements for scheduled aggregation

---

## 📌 Notes

- **Current Workaround**: Administrators must manually trigger aggregation jobs via API endpoints
- **Recommended Solution**: Solution 1 (Hangfire Recurring Jobs) for consistency with existing Hangfire usage
- **Timezone**: All scheduled times use `Asia/Ho_Chi_Minh` (no DST)

---

**Document Version**: 1.0  
**Last Updated**: 2026-01-17  
**Author**: FDA Development Team  
**Status**: 🔴 Open - Awaiting Implementation

