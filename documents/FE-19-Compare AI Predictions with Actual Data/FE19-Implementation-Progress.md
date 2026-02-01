# FE-19 Implementation Progress

## ✅ Completed

### Phase 1: Domain Layer
- ✅ `PredictionLog` entity created
- ✅ `PredictionLogConfiguration` created
- ✅ `IPredictionLogRepository` interface created
- ✅ `AppDbContext` updated with `DbSet<PredictionLog>`

### Phase 2: Infrastructure Layer
- ✅ `PgsqlPredictionLogRepository` implementation created
- ✅ Repository registered in `ServiceExtensions.cs`

### Phase 3: Application Layer (Partial)
- ✅ **FeatG76: LogPrediction** - COMPLETE
  - ✅ `LogPredictionRequest.cs`
  - ✅ `LogPredictionResponse.cs`
  - ✅ `LogPredictionHandler.cs`
  - ✅ `LogPredictionRequestValidator.cs`
  - ✅ `PredictionLogStatusCode.cs`
  - ✅ `FDAAPI.App.FeatG76_LogPrediction.csproj`

## 🚧 In Progress / TODO

### Phase 3: Application Layer (Remaining)
- ⏳ **FeatG77: GetPredictionComparisons**
  - Need: Request, Response, Handler, Validator, StatusCode enum
  - Pattern: Similar to `GetAlertHistory` with pagination

- ⏳ **FeatG78: GetPredictionAccuracyStats**
  - Need: Request, Response, Handler, Validator
  - Pattern: Similar to `GetFloodStatistics` with aggregation

- ⏳ **FeatG79: VerifyPredictionsBackgroundJob**
  - Need: Background job runner class (similar to `FrequencyAggregationRunner`)
  - Need: Hangfire recurring job registration
  - Pattern: Query pending predictions, verify with sensor readings, update entities

### Phase 4: Mapper Layer
- ⏳ Create `PredictionLogMapper` in `FDAAPI.App.Common/Services/Mapping/`
- ⏳ Map `PredictionLog` entity to DTOs

### Phase 5: Presentation Layer
- ⏳ **FeatG76 Endpoint**: `POST /api/v1/internal/log-prediction`
  - Need: Endpoint class, RequestDto, ResponseDto
  - Auth: AllowAnonymous or API Key

- ⏳ **FeatG77 Endpoint**: `GET /api/v1/predictions/comparisons`
  - Need: Endpoint class, RequestDto, ResponseDto
  - Auth: Policies("User")

- ⏳ **FeatG78 Endpoint**: `GET /api/v1/predictions/accuracy-stats`
  - Need: Endpoint class, RequestDto, ResponseDto
  - Auth: Policies("User")

### Phase 6: Configuration
- ⏳ Register FeatG76-78 assemblies in `ServiceExtensions.cs` (AddApplicationServices)
- ⏳ Register Hangfire recurring job for FeatG79 in `Program.cs` or extension method
  - Schedule: Every 10 minutes
  - Timezone: Asia/Ho_Chi_Minh

### Phase 7: Database Migration
- ⏳ Create EF Core migration for `prediction_logs` table
- ⏳ Run migration on dev/UAT databases

### Phase 8: Documentation
- ⏳ Update insight document with implementation details
- ⏳ Create test cases document
- ⏳ Update API documentation

## 📝 Implementation Notes

### FeatG79 Background Job Logic
1. Query `GetPendingVerificationAsync(DateTime.UtcNow, limit: 100)`
2. For each prediction:
   - Get area from `AreaId`
   - Query sensor readings in time range `[StartTime, EndTime]` for stations within area radius
   - Calculate `ActualWaterLevel = MAX(Value)`
   - Determine `IsCorrect` based on logic:
     - If `actualWaterLevel < 0.2m` AND `predictedProb < 0.3` → Correct
     - If `actualWaterLevel >= 0.2m` AND `predictedProb >= 0.3` → Correct
     - Otherwise → Incorrect
   - Calculate `AccuracyScore`
   - Update `PredictionLog` with verification results

### Sensor Reading Query by Area
- Need to query stations within area radius (use `IStationRepository.GetStationsWithinRadiusAsync`)
- Then query sensor readings for those stations in time range
- Use `ISensorReadingRepository.GetByStationAndTimeRangeAsync` or create new method

### Next Steps
1. Complete FeatG77 and FeatG78 handlers
2. Create mapper for PredictionLog
3. Create FastEndpoints for all 3 features
4. Register Hangfire job for FeatG79
5. Create and run migration
6. Test end-to-end flow

