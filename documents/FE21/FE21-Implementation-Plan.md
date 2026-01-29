# FE-21: Safe Route Suggestions with GraphHopper

## Context

Tôi đang develop FDA API với Domain-Centric Architecture (Clean Architecture + CQRS).

### Codebase hiện tại:

- **Architecture**: Domain-Centric với 4 layers (Domain, Application, Infrastructure, Presentation)
- **Framework**: ASP.NET Core 8.0 + FastEndpoints (KHÔNG dùng Controllers)
- **Database**: PostgreSQL với EF Core
- **Pattern**: CQRS với MediatR + FluentValidation
- **Authentication**: JWT + Refresh Token (đã hoàn thành)

---

## Feature Specification:

Implement safe route calculation feature với GraphHopper routing engine để tính toán routes tránh các vùng ngập lụt đang active. Feature cho phép:
- Request route từ điểm A đến điểm B
- Tự động avoid active flood zones (dựa trên **Station + SensorReading real-time data**)
- Tạo circle polygon quanh các station đang ngập để gửi cho GraphHopper avoid_polygons
- Cung cấp alternative routes
- Phân tích flood risk cho mỗi route
- Xử lý trường hợp route bị blocked hoàn toàn

---

## Flow tổng quan (đã cập nhật):

```
Station (lat/lng) → SensorReading (latest) → Severity Calculation → Filter (warning+)
    → Circle Polygon (radius by severity) → GraphHopper avoid_polygons → Route Response
```

### Tại sao thay đổi từ FloodEvent sang Station + SensorReading?

| Tiêu chí | FloodEvent (cũ - SAI) | Station + SensorReading (mới - ĐÚNG) |
|----------|----------------------|--------------------------------------|
| Tính chất | Dữ liệu lịch sử | Dữ liệu real-time |
| Geometry | AdministrativeArea.Geometry (admin boundary, phần lớn NULL) | Circle polygon quanh station ngập (luôn có lat/lng) |
| Severity | FloodEvent.PeakLevel (ngưỡng 2.0/3.0) | SensorReading.Value / 100 (ngưỡng 0.1/0.2/0.4 - khớp FeatG31) |
| Consistency | Không khớp FeatG31 | Khớp hoàn toàn FeatG31 + FeatG55 |

---

## Requirements:

1. **Endpoint POST /api/v1/routing/safe-route** - Calculate safe route (authenticated users)
2. **GraphHopper Integration** - Integrate self-hosted GraphHopper routing engine
3. **Flood Avoidance Logic** - Get stations with real-time sensor readings, tạo circle polygons, pass to GraphHopper as avoid_polygons
4. **Route Analysis** - Analyze route distance from flood zones, calculate safety status
5. **Alternative Routes** - Support up to 5 alternative routes with different flood risk scores
6. **Route Profiles** - Support car, bike, foot routing profiles
7. **Mapper Pattern** - Sử dụng ISafeRouteMapper giống các handler khác
8. **Validation**:
   - Lat/Lng within valid ranges (-90 to 90, -180 to 180)
   - RouteProfile must be "car", "bike", or "foot"
   - MaxAlternatives between 0-5
9. **Error Handling**: RouteBlocked status when no safe route available

---

## Technical Details:

### Entities sử dụng:

- **Station** (existing) - Có Latitude, Longitude, ThresholdWarning, ThresholdCritical
- **SensorReading** (existing) - Có Value (double, cm), StationId, MeasuredAt
- **RouteSafetyStatus** (enum) - Safe, Caution, Dangerous, Blocked
- **RouteProfile** (enum) - Car, Bike, Foot

### Business Logic:

#### Phase 1: Get Real-time Flood Data (matching FeatG31)
- Query tất cả Station có tọa độ (Latitude, Longitude not null)
- Get latest SensorReading cho mỗi station (GroupBy StationId, OrderByDescending MeasuredAt)
- Tính severity cho mỗi station:
  - `waterLevel = SensorReading.Value / 100.0` (convert cm → meters)
  - Dùng station custom thresholds nếu có (ThresholdWarning, ThresholdCritical)
  - Default thresholds: `>=0.4m` = critical(3), `>=0.2m` = warning(2), `>=0.1m` = caution(1)
- Lọc chỉ stations có severity >= warning (level >= 2)
- Tạo circle polygon quanh mỗi station ngập:
  - Critical: radius 500m
  - Warning: radius 300m

#### Phase 2: Call GraphHopper
- Build GraphHopper API request with:
  - Start/End coordinates
  - Route profile (car/bike/foot)
  - AvoidPolygons (circle polygons from flood data)
  - AlternativeRoute config (max_paths)
- POST to GraphHopper `/route` endpoint
- Parse response paths (geometry, distance, time, instructions)

#### Phase 3: Analyze Route Safety
- For each route path:
  - Calculate Haversine distance from route to each flooded station
  - Generate FloodWarning if distance < 1000m
  - Calculate flood risk score (0-100) based on warnings
- Determine overall RouteSafetyStatus:
  - Safe: No warnings
  - Caution: Severity level >= 2
  - Dangerous: Severity level >= 3 (critical)
  - Blocked: No route found

#### Phase 4: Return Response (using Mapper)
- Dùng ISafeRouteMapper.MapToRouteDto() để map GraphHopperPath → RouteDto
- Primary route with safety info
- Alternative routes (if requested)
- List of flood warnings with station info
- Overall safety status

### Authorization Requirements:

- **POST /api/v1/routing/safe-route**: `Policies("User")` - Any authenticated user

### Validation Rules:

- **UserId**: Required (from JWT)
- **StartLatitude**: Required, -90 to 90
- **StartLongitude**: Required, -180 to 180
- **EndLatitude**: Required, -90 to 90
- **EndLongitude**: Required, -180 to 180
- **RouteProfile**: Required, must be "car", "bike", or "foot"
- **MaxAlternatives**: 0 to 5
- **AvoidFloodedAreas**: Boolean (default true)

### Database Changes:

**NO database migration needed**. All existing entities support this feature:
- Station has Latitude, Longitude, ThresholdWarning, ThresholdCritical
- SensorReading has Value, StationId, MeasuredAt
- Only sử dụng AppDbContext trực tiếp (matching FeatG31 pattern)

### External Service Integration:

#### GraphHopper Setup
- **Deployment**: Docker container (self-hosted)
- **Image**: `israelhikingmap/graphhopper:latest`
- **Data**: Vietnam OSM data (~600MB download)
- **Memory**: 4GB RAM minimum
- **Port**: 8989
- **API**: RESTful HTTP API

#### Configuration (appsettings.json):
```json
{
  "GraphHopper": {
    "BaseUrl": "http://localhost:8989",
    "ApiKey": "",
    "DefaultProfile": "car",
    "Timeout": 30000
  }
}
```

---

## Implementation (đã hoàn thành):

### **Phase 1 - Domain Layer**

**Deliverables**:

- [x] Extend IFloodEventRepository với method `GetActiveFloodEventsAsync()` (giữ lại cho backward compat)
- [x] Create enum `RouteSafetyStatus` in `Enums/RouteSafetyStatus.cs`
- [x] Create enum `RouteProfile` in `Enums/RouteProfile.cs`

**Files**:
- `src/Core/Domain/FDAAPI.Domain.RelationalDb/Repositories/IFloodEventRepository.cs`
- `src/Core/Domain/FDAAPI.Domain.RelationalDb/Enums/RouteSafetyStatus.cs`
- `src/Core/Domain/FDAAPI.Domain.RelationalDb/Enums/RouteProfile.cs`

---

### **Phase 2 - Infrastructure Layer (Repositories)**

**Deliverables**:

- [x] Implement GetActiveFloodEventsAsync in PgsqlFloodEventRepository (giữ lại)

**Files**:
- `src/External/Infrastructure/Persistence/FDAAPI.Infra.Persistence/Repositories/PgsqlFloodEventRepository.cs`

**Note**: Handler mới KHÔNG dùng FloodEventRepository. Thay vào đó dùng AppDbContext trực tiếp để query Station + SensorReading (matching FeatG31 pattern).

---

### **Phase 3 - Infrastructure Layer (GraphHopper Service)**

**Deliverables**:

- [x] Create GraphHopper models in `FDAAPI.App.Common/Models/Routing/`
  - `GraphHopperRouteRequest.cs` (with AlternativeRouteConfig)
  - `GraphHopperRouteResponse.cs` (with GraphHopperPath, GraphHopperInstruction)
- [x] Create service interface: `IGraphHopperService` in `FDAAPI.App.Common/Services/`
- [x] Implement service: `GraphHopperService` in `FDAAPI.Infra.Services/Routing/`
  - Inject HttpClient + IConfiguration
  - POST to `/route` endpoint
  - BuildGraphHopperRequest() with avoid_polygons serialization
  - ConvertCoordinatesToArray() - flat array → polygon format
  - Handle HttpRequestException → InvalidOperationException

**Files**:
- `src/Core/Application/FDAAPI.App.Common/Models/Routing/GraphHopperRouteRequest.cs`
- `src/Core/Application/FDAAPI.App.Common/Models/Routing/GraphHopperRouteResponse.cs`
- `src/Core/Application/FDAAPI.App.Common/Services/IGraphHopperService.cs`
- `src/External/Infrastructure/Services/FDAAPI.Infra.Services/Routing/GraphHopperService.cs`

---

### **Phase 4 - Application Layer (Flood Analysis Service) - ĐÃ CẬP NHẬT**

**Deliverables**:

- [x] Create FloodPolygon model (Station-based, NOT FloodEvent-based)
  - Fields: StationId, StationName, StationCode, Latitude, Longitude, WaterLevel, Severity, SeverityLevel, Geometry
- [x] Create IRouteFloodAnalyzer interface với 3 methods:
  - `BuildFloodPolygons(stations, latestReadings)` - tạo circle polygons từ station data
  - `AnalyzeRoute(routeGeometry, floodPolygons)` - phân tích route safety
  - `CalculateSafetyStatus(warnings)` - tính overall status
- [x] Implement RouteFloodAnalyzer:
  - `CalculateFloodSeverity()` - matching FeatG31 + station custom thresholds (FeatG55)
  - `CreateCirclePolygon()` - tạo circular GeoJSON Polygon (32 points)
  - `CalculateHaversineDistance()` - tính khoảng cách route-station
  - Circle radius: critical=500m, warning=300m
  - Chỉ tạo polygon cho severity >= warning (level >= 2)

**Files**:
- `src/Core/Application/FDAAPI.App.Common/Models/Routing/FloodPolygon.cs`
- `src/Core/Application/FDAAPI.App.Common/Services/IRouteFloodAnalyzer.cs`
- `src/External/Infrastructure/Services/FDAAPI.Infra.Services/Routing/RouteFloodAnalyzer.cs`

---

### **Phase 5 - Application Layer (FeatG74_RequestSafeRoute) - ĐÃ CẬP NHẬT**

**Note**: Dùng FeatG74 (vì FeatG73 đã dùng cho CancelSubscription)

**Deliverables**:

- [x] Create DTOs (đã cập nhật cho Station-based):
  - `RouteDto.cs` - Geometry, DistanceMeters, DurationSeconds, Instructions, FloodRiskScore
  - `FloodWarningDto.cs` - StationId, StationName, StationCode, Severity, SeverityLevel, WaterLevel, Unit, Lat/Lng, FloodPolygon, DistanceFromRouteMeters
- [x] Create StatusCode enum: `SafeRouteStatusCode.cs`
- [x] Implement Request: `CreateSafeRouteRequest.cs` (sealed record)
- [x] Implement Response: `SafeRouteResponse.cs` with SafeRouteData
- [x] Implement Validator: `CreateSafeRouteRequestValidator.cs`
- [x] Implement Handler: `CreateSafeRouteHandler.cs` (ĐÃ CẬP NHẬT)
  - **Inject**: IGraphHopperService, IStationRepository, IRouteFloodAnalyzer, ISafeRouteMapper, AppDbContext, ILogger
  - **KHÔNG còn dùng**: IFloodEventRepository, IAdministrativeAreaRepository
  - Logic:
    1. Query stations có tọa độ (AppDbContext, matching FeatG31)
    2. Get latest SensorReading per station (GroupBy + OrderByDescending)
    3. Build flood polygons via `_floodAnalyzer.BuildFloodPolygons()`
    4. Call GraphHopper with avoid_polygons
    5. Analyze route safety
    6. Build response using `_mapper.MapToRouteDto()`

**Files**:
- `src/Core/Application/FDAAPI.App.FeatG74_RequestSafeRoute/CreateSafeRouteRequest.cs`
- `src/Core/Application/FDAAPI.App.FeatG74_RequestSafeRoute/SafeRouteResponse.cs`
- `src/Core/Application/FDAAPI.App.FeatG74_RequestSafeRoute/CreateSafeRouteRequestValidator.cs`
- `src/Core/Application/FDAAPI.App.FeatG74_RequestSafeRoute/CreateSafeRouteHandler.cs`
- `src/Core/Application/FDAAPI.App.Common/DTOs/RouteDto.cs`
- `src/Core/Application/FDAAPI.App.Common/DTOs/FloodWarningDto.cs`
- `src/Core/Application/FDAAPI.App.Common/Models/Routing/SafeRouteStatusCode.cs`

---

### **Phase 5.5 - Mapper (MỚI)**

**Deliverables**:

- [x] Create ISafeRouteMapper interface in `FDAAPI.App.Common/Services/Mapping/`
  - `MapToRouteDto(GraphHopperPath path, List<FloodWarningDto> warnings)` → RouteDto
- [x] Implement SafeRouteMapper in `FDAAPI.App.Common/Services/Mapping/`
  - Map GraphHopperPath → RouteDto
  - Map GraphHopperInstruction → RouteInstructionDto
  - Calculate FloodRiskScore (critical*40 + warning*20 + caution*10, max 100)

**Files**:
- `src/Core/Application/FDAAPI.App.Common/Services/Mapping/ISafeRouteMapper.cs`
- `src/Core/Application/FDAAPI.App.Common/Services/Mapping/SafeRouteMapper.cs`

---

### **Phase 6 - Presentation Layer (FastEndpoints)**

**Deliverables**:

- [x] Create folder: `Endpoints/Feat74_RequestSafeRoute/`
- [x] Create DTOs:
  - `RequestSafeRouteRequestDto.cs`
  - `SafeRouteResponseDto.cs`
- [x] Implement Endpoint: `RequestSafeRouteEndpoint.cs`
  - Route: `POST /api/v1/routing/safe-route`
  - Policies: `Policies("User")`
  - Tags: "Routing", "Safety"
  - Extract UserId from JWT
  - Map DTO to MediatR Request
  - Send via IMediator
  - Return response with appropriate HTTP status code

**Files**:
- `src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/Endpoints/Feat74_RequestSafeRoute/RequestSafeRouteEndpoint.cs`
- `src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/Endpoints/Feat74_RequestSafeRoute/DTOs/RequestSafeRouteRequestDto.cs`
- `src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/Endpoints/Feat74_RequestSafeRoute/DTOs/SafeRouteResponseDto.cs`

---

### **Phase 7 - Configuration & Registration - ĐÃ CẬP NHẬT**

**Deliverables**:

- [x] Update `ServiceExtensions.cs`:
  - **AddApplicationServices()**:
    - Add `using FDAAPI.App.FeatG74_RequestSafeRoute;`
    - Add `typeof(CreateSafeRouteRequest).Assembly` to assemblies array
  - **AddInfrastructureServices()**:
    - Register: `services.AddHttpClient<IGraphHopperService, GraphHopperService>();`
    - Register: `services.AddScoped<IRouteFloodAnalyzer, RouteFloodAnalyzer>();`
    - Register: `services.AddScoped<ISafeRouteMapper, SafeRouteMapper>();`
- [x] Update `appsettings.json`:
  - Add GraphHopper configuration section

**Files**:
- `src/External/Infrastructure/Common/FDAAPI.Infra.Configuration/ServiceExtensions.cs`
- `src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/appsettings.json`

---

### **Phase 8 - Docker Setup & Testing**

**Deliverables**:

- [x] Create `docker-compose.graphhopper.yml` in project root
  - Image: `israelhikingmap/graphhopper:latest`
  - Volume: `./graphhopper-data:/data`
  - Port: 8989
  - Command: `--input /data/vietnam-260128.osm.pbf -o /data/vietnam-gh --host 0.0.0.0 --port 8989`
  - Memory: 4GB (JAVA_OPTS=-Xmx4g -Xms2g)
- [ ] Test GraphHopper health: `curl http://localhost:8989/health`
- [x] Build solution: `dotnet build` - **0 errors, 0 warnings (FE-21 related)**

**Files**:
- `docker-compose.graphhopper.yml`

**Test Cases**:

#### TEST CASE 1: Normal Route (No Flooded Stations)

**Scenario**: Request route khi không có station nào ngập (tất cả sensor readings < 20cm)

**cURL**:
```bash
curl -X POST http://localhost:5000/api/v1/routing/safe-route \
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
```

**Expected Response (200 OK)**:
```json
{
  "success": true,
  "message": "Route calculated successfully",
  "statusCode": 200,
  "data": {
    "primaryRoute": {
      "geometry": { "type": "LineString", "coordinates": [...] },
      "distanceMeters": 12450,
      "durationSeconds": 1230,
      "floodRiskScore": 0,
      "instructions": [...]
    },
    "alternativeRoutes": [],
    "floodWarnings": [],
    "safetyStatus": "Safe"
  }
}
```

**Validation**:
- Status code 200
- SafetyStatus = "Safe"
- floodWarnings array is empty
- floodRiskScore = 0
- No avoid_polygons sent to GraphHopper

#### TEST CASE 2: Route with Nearby Flooded Station

**Scenario**: Có station ngập gần route (sensor reading >= 20cm = warning)

**Expected Response (200 OK)**:
```json
{
  "success": true,
  "message": "Route calculated successfully",
  "statusCode": 200,
  "data": {
    "primaryRoute": {
      "floodRiskScore": 20
    },
    "floodWarnings": [
      {
        "stationId": "...",
        "stationName": "Station ABC",
        "stationCode": "ST-001",
        "severity": "warning",
        "severityLevel": 2,
        "waterLevel": 25.5,
        "unit": "cm",
        "latitude": 10.77,
        "longitude": 106.65,
        "distanceFromRouteMeters": 450
      }
    ],
    "safetyStatus": "Caution"
  }
}
```

**Validation**:
- SafetyStatus = "Caution"
- floodWarnings has station info (NOT flood event info)
- Circle polygon (300m radius) sent to GraphHopper avoid_polygons
- Route avoids the flooded station area

#### TEST CASE 3: Route with Critical Flooding

**Scenario**: Có station ngập nghiêm trọng (sensor reading >= 40cm = critical)

**Expected**:
- SafetyStatus = "Dangerous"
- Circle polygon 500m radius quanh station
- FloodRiskScore cao (>= 40)

#### TEST CASE 4: Route Completely Blocked

**Scenario**: Tất cả routes bị blocked bởi flood zones

**Expected Response (422 RouteBlocked)**:
```json
{
  "success": false,
  "message": "No route found. All paths may be blocked by flooding.",
  "statusCode": 422
}
```

#### TEST CASE 5: Invalid Coordinates

**Expected Response (400 Bad Request)** - handled by FluentValidation

#### TEST CASE 6: GraphHopper Service Unavailable

**Setup**: Stop GraphHopper container

**Expected Response (503 Service Unavailable)**:
```json
{
  "success": false,
  "message": "Routing service is currently unavailable",
  "statusCode": 503
}
```

---

## Important Notes:

### DO's:
- **ALWAYS** dùng Station + SensorReading real-time data (KHÔNG dùng FloodEvent)
- **ALWAYS** match severity thresholds với FeatG31 (waterLevel/100: 0.1/0.2/0.4)
- **ALWAYS** support station custom thresholds (ThresholdWarning, ThresholdCritical) như FeatG55
- **ALWAYS** dùng ISafeRouteMapper cho DTO mapping (giống pattern các handler khác)
- **ALWAYS** dùng AppDbContext trực tiếp cho queries (matching FeatG31 pattern)
- **ALWAYS** handle GraphHopper service errors gracefully
- **ALWAYS** log GraphHopper API calls for debugging
- **ALWAYS** validate coordinates before calling GraphHopper

### DON'Ts:
- **DON'T** dùng FloodEvent để xác định vùng ngập (đó là dữ liệu lịch sử)
- **DON'T** dùng AdministrativeArea.Geometry (phần lớn NULL, chỉ là admin boundary)
- **DON'T** hardcode severity thresholds khác với FeatG31
- **DON'T** call GraphHopper directly from handler (use IGraphHopperService)
- **DON'T** hardcode GraphHopper URL (use configuration)
- **DON'T** expose internal GraphHopper errors to client

### Best Practices:
1. **Consistency**: Severity calculation khớp hoàn toàn FeatG31 + FeatG55
2. **Service Abstraction**: IGraphHopperService allows mocking for tests
3. **Mapper Pattern**: ISafeRouteMapper giống IFloodEventMapper, IStationMapper, etc.
4. **Error Handling**: Catch HttpRequestException → ServiceUnavailable
5. **Circle Polygons**: 32-point approximation, radius tùy severity
6. **Distance Calculation**: Haversine formula (có thể upgrade NetTopologySuite sau)

---

## File Summary (tất cả files của FE-21):

### Core files (đã implement):
| # | File | Layer | Status |
|---|------|-------|--------|
| 1 | `Enums/RouteSafetyStatus.cs` | Domain | Done |
| 2 | `Enums/RouteProfile.cs` | Domain | Done |
| 3 | `Models/Routing/FloodPolygon.cs` | Application | Done (Station-based) |
| 4 | `Models/Routing/GraphHopperRouteRequest.cs` | Application | Done |
| 5 | `Models/Routing/GraphHopperRouteResponse.cs` | Application | Done |
| 6 | `Models/Routing/SafeRouteStatusCode.cs` | Application | Done |
| 7 | `DTOs/RouteDto.cs` | Application | Done |
| 8 | `DTOs/FloodWarningDto.cs` | Application | Done (Station-based) |
| 9 | `Services/IGraphHopperService.cs` | Application | Done |
| 10 | `Services/IRouteFloodAnalyzer.cs` | Application | Done (3 methods) |
| 11 | `Services/Mapping/ISafeRouteMapper.cs` | Application | Done |
| 12 | `Services/Mapping/SafeRouteMapper.cs` | Application | Done |
| 13 | `FeatG74_RequestSafeRoute/CreateSafeRouteRequest.cs` | Application | Done |
| 14 | `FeatG74_RequestSafeRoute/SafeRouteResponse.cs` | Application | Done |
| 15 | `FeatG74_RequestSafeRoute/CreateSafeRouteRequestValidator.cs` | Application | Done |
| 16 | `FeatG74_RequestSafeRoute/CreateSafeRouteHandler.cs` | Application | Done (Station-based + Mapper) |
| 17 | `Routing/GraphHopperService.cs` | Infrastructure | Done |
| 18 | `Routing/RouteFloodAnalyzer.cs` | Infrastructure | Done (Circle polygon + FeatG31 severity) |
| 19 | `ServiceExtensions.cs` | Infrastructure | Done (3 registrations) |
| 20 | `Feat74_RequestSafeRoute/RequestSafeRouteEndpoint.cs` | Presentation | Done |
| 21 | `Feat74_RequestSafeRoute/DTOs/RequestSafeRouteRequestDto.cs` | Presentation | Done |
| 22 | `Feat74_RequestSafeRoute/DTOs/SafeRouteResponseDto.cs` | Presentation | Done |
| 23 | `docker-compose.graphhopper.yml` | Root | Done |

### Build Status: **0 Errors**

---

## Build & Verify Commands:

### Setup GraphHopper
```bash
# Start GraphHopper (OSM data already in graphhopper-data/)
docker-compose -f docker-compose.graphhopper.yml up -d

# Check health
curl http://localhost:8989/health
```

### Build Solution
```bash
dotnet build "d:\Capstone Project\FDA_API\FDA_Api.sln"
```

### Run Application
```bash
cd "d:\Capstone Project\FDA_API\src\External\Presentation\FDAAPI.Presentation.FastEndpointBasedApi"
dotnet run
```

### Test Endpoint
```bash
# Get auth token first
TOKEN=$(curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@test.com","password":"password"}' \
  | jq -r '.data.accessToken')

# Test safe route endpoint
curl -X POST http://localhost:5000/api/v1/routing/safe-route \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "startLatitude": 10.762622,
    "startLongitude": 106.660172,
    "endLatitude": 10.823099,
    "endLongitude": 106.629664,
    "routeProfile": "car",
    "maxAlternatives": 3,
    "avoidFloodedAreas": true
  }'
```

---

## Key Points Summary:

- Use **GraphHopper** (self-hosted Docker container, israelhikingmap/graphhopper:latest)
- **NO database migrations** needed
- Data source: **Station + SensorReading** (real-time, matching FeatG31)
- **KHÔNG dùng FloodEvent** (dữ liệu lịch sử, không phù hợp)
- Severity thresholds: **0.1m/0.2m/0.4m** (matching FeatG31 + FeatG55 custom thresholds)
- Flood zone: **Circle polygon** quanh station ngập (300-500m radius)
- Implement **ISafeRouteMapper** (matching project mapper pattern)
- Use **MediatR** pattern (IRequestHandler)
- Use **FluentValidation** for input validation
- Feature group: **FeatG74_RequestSafeRoute** (FeatG73 đã dùng cho CancelSubscription)
- Handle **service unavailable** scenarios gracefully

---

**Implementation Status**: COMPLETE - Build thành công 0 errors.
