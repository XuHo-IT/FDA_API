# FE-24: Advanced Route Optimization

## Context

Tôi đang develop FDA API với Domain-Centric Architecture (Clean Architecture + CQRS).

### Codebase hiện tại:

- **Architecture**: Domain-Centric với 4 layers (Domain, Application, Infrastructure, Presentation)
- **Framework**: ASP.NET Core 8.0 + FastEndpoints (KHÔNG dùng Controllers)
- **Database**: PostgreSQL với EF Core
- **Pattern**: CQRS với MediatR + FluentValidation
- **Authentication**: JWT + Refresh Token (đã hoàn thành)

### Existing Feature (FE-21):

FE-21 đã implement safe route calculation với GraphHopper:
- `POST /api/v1/routing/safe-route` — tính toán route tránh vùng ngập lụt
- 3 parallel GraphHopper calls (safe, fastest, shortest)
- Flood polygon generation từ Station + SensorReading data
- GeoJSON FeatureCollection response
- Route profiles: car, bike, foot

**FE-24 builds on top of FE-21** bằng cách thêm advanced optimization constraints và performance testing.

---

## Feature Specification:

Implement advanced route optimization feature cho phép:
- Thêm **waypoints** (điểm trung gian) vào route
- Tối ưu hóa route theo **time window** (thời gian bắt đầu ảnh hưởng flood prediction)
- **Caching** route results để giảm latency cho identical requests
- **Performance benchmarks**: đảm bảo response time < 3 giây cho 95% requests
- **Consistency tests**: cùng input luôn cho cùng output (deterministic routing)

---

## Flow tổng quan:

```
Request (start, end, waypoints?, departureTime?)
    → Validate input
    → Check cache (key = hash of coordinates + profile + departureTime)
    → [Cache HIT] → Return cached response
    → [Cache MISS] → Build flood polygons (with time-based severity projection)
        → Call GraphHopper with waypoints support
        → Analyze route safety
        → Cache result (TTL = 5 minutes)
        → Return GeoJSON FeatureCollection
```

---

## Requirements:

1. **Endpoint POST /api/v1/routing/optimized-route** — Advanced route with optimization constraints (authenticated users)
2. **Waypoints Support** — Allow 0-5 intermediate points between start and end
3. **Departure Time** — Optional departure time for time-aware flood severity projection
4. **Response Caching** — Cache route results in-memory (IMemoryCache) with 5-minute TTL
5. **Route Comparison Metadata** — Return detailed comparison data (time saved, distance saved, flood risk delta)
6. **Performance Target** — 95th percentile response time < 3000ms (cached < 100ms)
7. **Deterministic Results** — Same input always produces same output within cache window
8. **Backward Compatible** — FE-21 endpoint remains unchanged, FE-24 is a NEW endpoint

---

## Technical Details:

### Entities sử dụng:

- **Station** (existing) — Có Latitude, Longitude, ThresholdWarning, ThresholdCritical
- **SensorReading** (existing) — Có Value (double, cm), StationId, MeasuredAt
- **RouteSafetyStatus** (existing enum) — Safe, Caution, Dangerous, Blocked
- **RouteProfile** (existing enum) — Car, Bike, Foot
- **NO new database entities needed**

### Business Logic:

#### Phase 1: Request Validation & Cache Check
- Validate tất cả input fields (start, end, waypoints, profile, departureTime)
- Compute cache key = SHA256 hash of sorted coordinates + profile + departureTime (rounded to 5-minute window)
- Check IMemoryCache → if HIT, return cached response immediately
- Log cache hit/miss ratio

#### Phase 2: Waypoints Support
- Convert waypoints thành GraphHopper multi-point request:
  - `points = [[startLng, startLat], [wp1Lng, wp1Lat], ..., [endLng, endLat]]`
- GraphHopper natively supports multiple points → route qua tất cả points theo thứ tự
- **Important**: Khi có waypoints, alternative_route KHÔNG khả dụng (GraphHopper limitation)
  - Nếu có waypoints → chỉ gọi 2 requests (safe route + shortest route), KHÔNG gọi normal route với alternatives

#### Phase 3: Time-Based Flood Severity Projection
- Nếu `departureTime` được cung cấp:
  - Tính thời gian di chuyển ước tính (estimated travel time) từ GraphHopper response
  - Cho mỗi flood zone, kiểm tra xem station có **trend** tăng hay giảm:
    - Query last 3 sensor readings (ordered by MeasuredAt DESC)
    - Nếu readings đang **tăng** → tăng severity lên 1 level (e.g., warning → critical)
    - Nếu readings đang **giảm** → giữ nguyên severity (không giảm level)
  - Trend calculation: `trend = (reading[0].Value - reading[2].Value) / reading[2].Value`
    - Nếu trend > 0.1 (tăng >10%) → severity +1
    - Nếu trend <= 0.1 → giữ nguyên
- Nếu `departureTime` = null → sử dụng logic FE-21 hiện tại (không projection)

#### Phase 4: Route Optimization & Comparison
- Gọi GraphHopper (reuse existing IGraphHopperService):
  - Safe route: flexible mode + avoid flood polygons
  - Shortest route: flexible mode + distance_influence=200
  - Normal route with alternatives: CH mode (chỉ khi KHÔNG có waypoints)
- Build comparison metadata cho mỗi alternative vs safe route:
  - `timeSavedSeconds` = alternativeTime - safeRouteTime (negative = slower)
  - `distanceSavedMeters` = alternativeDistance - safeRouteDistance
  - `floodRiskDelta` = alternativeRiskScore - safeRouteRiskScore (positive = riskier)

#### Phase 5: Cache & Return
- Cache response trong IMemoryCache với TTL = 5 phút
- Return GeoJSON FeatureCollection (same format as FE-21) + extended metadata

### Authorization Requirements:

- **POST /api/v1/routing/optimized-route**: `Policies("User")` — Any authenticated user

### Validation Rules:

- **UserId**: Required (from JWT)
- **StartLatitude**: Required, -90 to 90
- **StartLongitude**: Required, -180 to 180
- **EndLatitude**: Required, -90 to 90
- **EndLongitude**: Required, -180 to 180
- **RouteProfile**: Required, must be "car", "bike", or "foot"
- **MaxAlternatives**: 0 to 5 (default 3)
- **AvoidFloodedAreas**: Boolean (default true)
- **Waypoints**: Optional, max 5 items, each with valid lat (-90 to 90) and lng (-180 to 180)
- **DepartureTime**: Optional, must be in future (if provided), max 24 hours ahead

### Database Changes:

**NO database migration needed.** Feature sử dụng:
- Existing Station + SensorReading tables (same as FE-21)
- IMemoryCache cho caching (in-process, no external dependency)

### External Service Integration:

Reuse existing GraphHopper setup (same as FE-21):
- **Container**: `fda_graphhopper` on port 8989
- **API**: POST `/route` with multi-point support
- **Config**: `GraphHopper:BaseUrl` from appsettings.json

---

## Reuse from FE-21:

Các components sau được **reuse hoàn toàn**, KHÔNG tạo mới:

| Component | File | Reuse |
|-----------|------|-------|
| IGraphHopperService | `FDAAPI.App.Common/Services/IGraphHopperService.cs` | 100% reuse |
| GraphHopperService | `FDAAPI.Infra.Services/Routing/GraphHopperService.cs` | 100% reuse |
| IRouteFloodAnalyzer | `FDAAPI.App.Common/Services/IRouteFloodAnalyzer.cs` | Extend with new method |
| RouteFloodAnalyzer | `FDAAPI.Infra.Services/Routing/RouteFloodAnalyzer.cs` | Extend with trend analysis |
| ISafeRouteMapper | `FDAAPI.App.Common/Services/Mapping/ISafeRouteMapper.cs` | 100% reuse |
| SafeRouteMapper | `FDAAPI.App.Common/Services/Mapping/SafeRouteMapper.cs` | 100% reuse |
| GraphHopperRouteRequest | `FDAAPI.App.Common/Models/Routing/GraphHopperRouteRequest.cs` | 100% reuse (already supports multi-point) |
| GraphHopperRouteResponse | `FDAAPI.App.Common/Models/Routing/GraphHopperRouteResponse.cs` | 100% reuse |
| FloodPolygon | `FDAAPI.App.Common/Models/Routing/FloodPolygon.cs` | 100% reuse |
| FloodWarningDto | `FDAAPI.App.Common/DTOs/FloodWarningDto.cs` | 100% reuse |
| RouteProfile enum | `FDAAPI.Domain.RelationalDb/Enums/RouteProfile.cs` | 100% reuse |
| RouteSafetyStatus enum | `FDAAPI.Domain.RelationalDb/Enums/RouteSafetyStatus.cs` | 100% reuse |
| SafeRouteStatusCode enum | `FDAAPI.App.Common/Models/Routing/SafeRouteStatusCode.cs` | Reuse (rename → RouteStatusCode hoặc reuse directly) |

---

## New Components to Create:

### 1. Application Layer — `FDAAPI.App.FeatG24_OptimizedRoute/`

| File | Description |
|------|-------------|
| `OptimizedRouteRequest.cs` | Sealed record with waypoints + departureTime |
| `OptimizedRouteResponse.cs` | Response with extended comparison metadata |
| `OptimizedRouteHandler.cs` | Handler with caching + trend analysis + waypoints |
| `OptimizedRouteRequestValidator.cs` | FluentValidation with waypoint + time rules |

### 2. Presentation Layer — `Endpoints/Feat24_OptimizedRoute/`

| File | Description |
|------|-------------|
| `OptimizedRouteEndpoint.cs` | FastEndpoint for POST /api/v1/routing/optimized-route |
| `DTOs/OptimizedRouteRequestDto.cs` | Request DTO with waypoints array |
| `DTOs/OptimizedRouteResponseDto.cs` | Response DTO |

### 3. Extend Existing Services

| File | Change |
|------|--------|
| `IRouteFloodAnalyzer.cs` | Add `BuildFloodPolygonsWithTrend()` method |
| `RouteFloodAnalyzer.cs` | Implement trend-based severity projection |
| `ServiceExtensions.cs` | Register new feature assembly + IMemoryCache |

---

## Response Format:

### Success Response (GeoJSON FeatureCollection):

```json
{
  "success": true,
  "message": "Optimized route calculated successfully",
  "statusCode": 200,
  "data": {
    "type": "FeatureCollection",
    "features": [
      {
        "type": "Feature",
        "geometry": {
          "type": "LineString",
          "coordinates": [[106.660172, 10.762622], ...]
        },
        "properties": {
          "name": "safeRoute",
          "distanceMeters": 8945.2,
          "durationSeconds": 720,
          "floodRiskScore": 25,
          "instructions": [...]
        }
      },
      {
        "type": "Feature",
        "geometry": {
          "type": "LineString",
          "coordinates": [[106.660172, 10.762622], ...]
        },
        "properties": {
          "name": "alternativeRoute_1",
          "distanceMeters": 9200.5,
          "durationSeconds": 780,
          "floodRiskScore": 40,
          "comparison": {
            "timeSavedSeconds": -60,
            "distanceSavedMeters": -255.3,
            "floodRiskDelta": 15
          },
          "instructions": [...]
        }
      },
      {
        "type": "Feature",
        "geometry": {
          "type": "Polygon",
          "coordinates": [[[106.665, 10.762], ...]]
        },
        "properties": {
          "name": "floodZone",
          "stationId": "guid-here",
          "stationCode": "STN001",
          "stationName": "Station Name",
          "severity": "warning",
          "severityLevel": 2,
          "waterLevel": 175.5,
          "unit": "cm",
          "trendDirection": "rising",
          "originalSeverity": "warning",
          "projectedSeverity": "critical",
          "latitude": 10.765,
          "longitude": 106.668,
          "distanceFromRouteMeters": 450.75
        }
      }
    ],
    "metadata": {
      "safetyStatus": "Caution",
      "totalFloodZones": 3,
      "alternativeRouteCount": 2,
      "generatedAt": "2026-01-30T12:34:56Z",
      "cached": false,
      "waypointCount": 2,
      "departureTime": "2026-01-30T13:00:00Z",
      "floodTrendApplied": true
    }
  }
}
```

---

## Expected Implementation:

### Phase 1 - Domain Layer

**Deliverables**:
- [ ] NO new entities needed
- [ ] NO database changes needed

**Acceptance Criteria**:
- Existing Station, SensorReading entities đủ dùng
- Existing enums (RouteProfile, RouteSafetyStatus) đủ dùng

---

### Phase 2 - Infrastructure Layer (Extend Existing Services)

**Deliverables**:
- [ ] Extend `IRouteFloodAnalyzer` với method `BuildFloodPolygonsWithTrend()`
- [ ] Implement trend analysis trong `RouteFloodAnalyzer.cs`:
  - Query last 3 readings per station
  - Calculate trend direction
  - Adjust severity if rising trend
- [ ] Register `IMemoryCache` trong `ServiceExtensions.cs` (nếu chưa có)

**Acceptance Criteria**:
- `BuildFloodPolygonsWithTrend()` nhận thêm parameter `Dictionary<Guid, List<SensorReading>> recentReadings`
- Trend calculation: `(reading[0] - reading[2]) / reading[2]` → if >0.1 → severity +1
- Original `BuildFloodPolygons()` method KHÔNG bị thay đổi (backward compatible)

---

### Phase 3 - Application Layer

**Deliverables**:
- [ ] Create project `src/Core/Application/FDAAPI.App.FeatG24_OptimizedRoute/`
- [ ] Create `.csproj` with references to Domain + App.Common
- [ ] Implement `OptimizedRouteRequest.cs` (sealed record):
  ```csharp
  public sealed record OptimizedRouteRequest(
      Guid UserId,
      decimal StartLatitude,
      decimal StartLongitude,
      decimal EndLatitude,
      decimal EndLongitude,
      string RouteProfile,
      int MaxAlternatives,
      bool AvoidFloodedAreas,
      List<WaypointDto>? Waypoints,
      DateTime? DepartureTime
  ) : IFeatureRequest<OptimizedRouteResponse>;
  ```
- [ ] Implement `OptimizedRouteResponse.cs`:
  - Reuse `SafeRouteGeoJsonData` structure
  - Extend `SafeRouteMetadata` → `OptimizedRouteMetadata` with:
    - `bool Cached`
    - `int WaypointCount`
    - `DateTime? DepartureTime`
    - `bool FloodTrendApplied`
- [ ] Implement `OptimizedRouteRequestValidator.cs`:
  - All FE-21 validations +
  - Waypoints: max 5 items, each lat/lng valid range
  - DepartureTime: if provided, must be future, max 24h ahead
- [ ] Implement `OptimizedRouteHandler.cs`:
  - Inject: IGraphHopperService, IRouteFloodAnalyzer, ISafeRouteMapper, AppDbContext, IMemoryCache, ILogger
  - Cache key computation
  - Waypoint-aware GraphHopper calls
  - Trend-based flood analysis (if departureTime provided)
  - Route comparison metadata calculation

**Acceptance Criteria**:
- Request model là sealed record
- Validator covers tất cả new fields
- Handler reuse existing services qua interfaces
- Cache key is deterministic (same input → same key)
- Waypoints mode disables alternative_route (GraphHopper limitation)

---

### Phase 4 - Mapper Layer

**Deliverables**:
- [ ] Extend `ISafeRouteMapper` với method `BuildRouteFeatureWithComparison()` (optional — có thể reuse existing method + add comparison ở handler level)
- [ ] Hoặc: Add comparison data trực tiếp trong handler (simpler approach)

**Acceptance Criteria**:
- Comparison metadata (timeSaved, distanceSaved, floodRiskDelta) có trong response
- Flood zone features có thêm `trendDirection`, `originalSeverity`, `projectedSeverity` nếu trend applied

---

### Phase 5 - Presentation Layer

**Deliverables**:
- [ ] Create folder `Endpoints/Feat24_OptimizedRoute/`
- [ ] Implement `OptimizedRouteEndpoint.cs`:
  - Route: `POST /api/v1/routing/optimized-route`
  - Policies: `"User"`
  - Tags: `"Routing", "Optimization"`
- [ ] Create `DTOs/OptimizedRouteRequestDto.cs`:
  ```csharp
  public class OptimizedRouteRequestDto
  {
      public decimal StartLatitude { get; set; }
      public decimal StartLongitude { get; set; }
      public decimal EndLatitude { get; set; }
      public decimal EndLongitude { get; set; }
      public string RouteProfile { get; set; } = "car";
      public int MaxAlternatives { get; set; } = 3;
      public bool AvoidFloodedAreas { get; set; } = true;
      public List<WaypointDto>? Waypoints { get; set; }
      public DateTime? DepartureTime { get; set; }
  }
  ```
- [ ] Create `DTOs/OptimizedRouteResponseDto.cs`

**Acceptance Criteria**:
- Endpoint inject IMediator
- JWT userId extraction from claims
- Proper HTTP status code mapping

---

### Phase 6 - Configuration & Registration

**Deliverables**:
- [ ] Update `ServiceExtensions.cs`:
  - `AddApplicationServices()`: Add `typeof(OptimizedRouteRequest).Assembly`
  - `AddInfrastructureServices()`: Add `services.AddMemoryCache()` (nếu chưa có)
- [ ] Verify DI container — NO new repository/mapper registrations needed (reuse existing)

**Acceptance Criteria**:
- Feature assembly registered cho MediatR + FluentValidation
- IMemoryCache available for injection
- No breaking changes to existing registrations

---

### Phase 7 - Performance Testing & Verification

**Deliverables**:
- [ ] Build solution: `dotnet build "d:\Capstone Project\FDA_API\FDA_Api.sln"`
- [ ] Manual performance test:
  - Test 1: First request (cold cache) — expect < 3000ms
  - Test 2: Same request again (warm cache) — expect < 100ms
  - Test 3: Request with waypoints — expect < 3000ms
  - Test 4: Request with departureTime — expect < 3000ms
- [ ] Consistency test:
  - Send same request 3 times → all responses should be identical
  - Verify cache key determinism

**Test Data**:

```bash
# Test 1: Basic optimized route (no waypoints)
curl -X POST http://localhost:5000/api/v1/routing/optimized-route \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "startLatitude": 10.762622,
    "startLongitude": 106.660172,
    "endLatitude": 10.823099,
    "endLongitude": 106.629664,
    "routeProfile": "car",
    "maxAlternatives": 3,
    "avoidFloodedAreas": true
  }'

# Test 2: With waypoints
curl -X POST http://localhost:5000/api/v1/routing/optimized-route \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "startLatitude": 10.762622,
    "startLongitude": 106.660172,
    "endLatitude": 10.823099,
    "endLongitude": 106.629664,
    "routeProfile": "car",
    "maxAlternatives": 3,
    "avoidFloodedAreas": true,
    "waypoints": [
      { "latitude": 10.790000, "longitude": 106.645000 },
      { "latitude": 10.810000, "longitude": 106.635000 }
    ]
  }'

# Test 3: With departure time
curl -X POST http://localhost:5000/api/v1/routing/optimized-route \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "startLatitude": 10.762622,
    "startLongitude": 106.660172,
    "endLatitude": 10.823099,
    "endLongitude": 106.629664,
    "routeProfile": "bike",
    "maxAlternatives": 2,
    "avoidFloodedAreas": true,
    "departureTime": "2026-01-30T14:00:00Z"
  }'

# Test 4: Cache consistency (repeat Test 1 immediately)
# Expected: identical response with "cached": true in metadata
```

**Acceptance Criteria**:
- 0 build errors
- Cold cache response < 3000ms
- Warm cache response < 100ms
- Consistent results for identical inputs

---

### Phase 8 - Documentation

**Deliverables**:
- [ ] Create `documents/FE-24/FE-24-Complete-Documentation.md`
- [ ] Include: API examples, test cases, architecture notes

---

## WaypointDto Definition:

```csharp
// In FDAAPI.App.Common/DTOs/WaypointDto.cs
namespace FDAAPI.App.Common.DTOs
{
    public class WaypointDto
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}
```

---

## Cache Key Strategy:

```csharp
// Pseudo-code for deterministic cache key
var keyParts = new List<string>
{
    $"{request.StartLatitude:F6}",
    $"{request.StartLongitude:F6}",
    $"{request.EndLatitude:F6}",
    $"{request.EndLongitude:F6}",
    request.RouteProfile,
    request.AvoidFloodedAreas.ToString()
};

if (request.Waypoints != null)
{
    foreach (var wp in request.Waypoints)
        keyParts.Add($"{wp.Latitude:F6},{wp.Longitude:F6}");
}

if (request.DepartureTime.HasValue)
{
    // Round to 5-minute window for cache grouping
    var rounded = new DateTime(
        request.DepartureTime.Value.Ticks / (TimeSpan.TicksPerMinute * 5) * (TimeSpan.TicksPerMinute * 5),
        DateTimeKind.Utc);
    keyParts.Add(rounded.ToString("O"));
}

var rawKey = string.Join("|", keyParts);
var cacheKey = $"route:{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)))}";
```

---

## Important Notes:

### DO's:
- Reuse existing IGraphHopperService, IRouteFloodAnalyzer, ISafeRouteMapper
- Reuse existing GeoJSON response format
- Keep FE-21 endpoint unchanged (backward compatible)
- Use IMemoryCache (built-in ASP.NET Core, no external dependency)
- Round departureTime to 5-min window cho cache efficiency

### DON'Ts:
- DON'T modify FE-21 handler hoặc endpoint
- DON'T create new database tables
- DON'T use external cache (Redis) — IMemoryCache is sufficient
- DON'T call alternative_route when waypoints exist (GraphHopper limitation)
- DON'T decrease severity in trend analysis (only increase or keep same)

---

## Questions to Clarify:

1. Should waypoint order be optimized (TSP/traveling salesman)?
   → **Answer**: No, waypoints are used in the order provided by the user.
2. Should cache be shared across users?
   → **Answer**: Yes, route results are not user-specific (same coordinates → same route).
3. Should we support real-time WebSocket updates for route conditions?
   → **Answer**: No, not in this phase. Polling is sufficient.
4. Should flood trend use more than 3 readings?
   → **Answer**: No, 3 readings are sufficient for short-term trend detection.

---

## Build & Verify Commands:

### Build Solution
```bash
dotnet build "d:\Capstone Project\FDA_API\FDA_Api.sln"
```

### Run Application
```bash
cd "d:\Capstone Project\FDA_API\src\External\Presentation\FDAAPI.Presentation.FastEndpointBasedApi"
dotnet run
```

---

**Last Updated**: 2026-01-30
**Depends On**: FE-21 (Safe Route Suggestions) — must be complete
**Status**: SPEC READY — awaiting implementation
