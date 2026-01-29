# 📊 PHÂN TÍCH FE-21: REQUEST SAFE ROUTE SUGGESTIONS

> **Feature Analysis**: Routing service integration with flood avoidance logic
> **Author**: System Analysis
> **Date**: 2026-01-28
> **Status**: Planning Phase

---

## 🎯 YÊU CẦU CHỨC NĂNG

### 1. **Routing Service Integration**
Tích hợp dịch vụ tìm đường để tính toán route từ điểm A đến điểm B

### 2. **Avoid Flooded Polygons Logic**
Tránh các vùng ngập lụt (dựa trên dữ liệu flood từ hệ thống)

### 3. **Test: Route Blocked Scenarios**
Xử lý trường hợp đường bị chặn hoàn toàn bởi lũ

---

## 🏗️ KIẾN TRÚC HIỆN TẠI

### **Điểm Mạnh**

✅ **Clean Architecture** với CQRS pattern (MediatR)
✅ **PostgreSQL + PostGIS** - Hỗ trợ spatial queries
✅ **Feature-based organization** - Dễ mở rộng
✅ **Haversine distance calculation** - Đã có sẵn
✅ **GeoJSON support** - Chuẩn cho mapping
✅ **FastEndpoints** - REST API nhanh và nhẹ

### **Dữ Liệu Geo-Spatial Hiện Có**

1. **Area** (User-defined): Point + Radius (100-150m)
2. **AdministrativeArea**: PostGIS Geometry (Polygon) - lưu dạng JSON string
3. **FloodEvent**: Link tới AdministrativeArea (có StartTime, EndTime, PeakLevel)
4. **Station**: Point coordinates với water level data

### **Hạn Chế Hiện Tại**

❌ **KHÔNG có routing/pathfinding logic**
❌ **KHÔNG có graph network** cho đường giao thông
❌ **PostGIS geometry chưa được dùng tối đa** (chỉ lưu trữ, chưa query spatial)
❌ **Flood polygons chưa rõ ràng** - chỉ có FloodEvent link với AdministrativeArea

---

## 🔍 SO SÁNH ROUTING ENGINES

| Tiêu chí | **OSRM** | **GraphHopper** | **Valhalla** |
|---------|----------|-----------------|--------------|
| **License** | BSD-2-Clause (Free) | Apache 2.0 (Free) | MIT (Free) |
| **Ngôn ngữ** | C++ | Java | C++ |
| **API Wrapper .NET** | ✅ HTTP Client dễ | ✅ HTTP Client dễ | ✅ HTTP Client dễ |
| **Self-hosted** | ✅ Docker ready | ✅ Docker ready | ✅ Docker ready |
| **Custom routing profiles** | ⚠️ Hạn chế | ✅ Rất linh hoạt | ✅ Linh hoạt |
| **Avoid polygons** | ❌ KHÔNG hỗ trợ trực tiếp | ✅ Custom weighting | ✅ Custom costing |
| **Performance** | 🚀 Rất nhanh (C++) | 🏃 Nhanh (Java) | 🚀 Rất nhanh (C++) |
| **Community** | ⭐⭐⭐⭐⭐ Lớn nhất | ⭐⭐⭐⭐ Tốt | ⭐⭐⭐ Trung bình |
| **Documentation** | ⭐⭐⭐⭐⭐ Xuất sắc | ⭐⭐⭐⭐⭐ Xuất sắc | ⭐⭐⭐ Tốt |
| **Vietnam OSM data** | ✅ Tốt | ✅ Tốt | ✅ Tốt |

---

## 🎖️ ĐỀ XUẤT: **GRAPHHOPPER** (Recommended)

### **Lý Do Chọn GraphHopper**

#### ✅ **1. Custom Weighting & Avoid Areas**

```json
// GraphHopper hỗ trợ avoid polygons TRỰC TIẾP
{
  "points": [[106.123, 10.456], [106.789, 10.654]],
  "avoid_polygons": {
    "type": "Polygon",
    "coordinates": [[[lng1,lat1], [lng2,lat2], ...]]
  }
}
```

**OSRM & Valhalla**: Cần custom implementation phức tạp hơn

#### ✅ **2. Flexible Routing Profiles**

- **car, bike, foot, hike** - Built-in
- **Custom profiles** với JSON config
- **Weighting functions** - Tránh ngập lụt bằng cách tăng cost

#### ✅ **3. .NET Integration**

```csharp
// Dễ dàng tích hợp với HttpClient
public interface IGraphHopperService
{
    Task<RouteResponse> GetRouteAsync(RouteRequest request, CancellationToken ct);
    Task<RouteResponse> GetSafeRouteAsync(
        decimal startLat, decimal startLng,
        decimal endLat, decimal endLng,
        List<FloodPolygon> floodedAreas,
        CancellationToken ct);
}
```

#### ✅ **4. Alternative Routes**

```json
{
  "algorithm": "alternative_route",
  "alternative_route.max_paths": 3,
  "alternative_route.max_weight_factor": 1.4
}
```

**Use case**: Nếu route 1 bị ngập → suggest route 2, 3

#### ✅ **5. Docker Deployment**

```yaml
services:
  graphhopper:
    image: graphhopper/graphhopper:latest
    volumes:
      - ./graphhopper-data:/data
    ports:
      - "8989:8989"
    environment:
      - JAVA_OPTS=-Xmx2g -Xms2g
```

#### ✅ **6. Vietnam OSM Data**

```bash
# Download Vietnam map
wget https://download.geofabrik.de/asia/vietnam-latest.osm.pbf

# GraphHopper import
docker run -v ./data:/data graphhopper/graphhopper \
  --input /data/vietnam-latest.osm.pbf \
  --output /data/vietnam-gh
```

---

## 🛠️ KIẾN TRÚC ĐỀ XUẤT

### **Architecture Diagram**

```
[Frontend]
    ↓ POST /api/v1/routing/safe-route
[FastEndpoint: RequestSafeRouteEndpoint]
    ↓ CreateSafeRouteRequest
[MediatR Handler: RequestSafeRouteHandler]
    ↓
    ├─→ [IFloodEventRepository] → Get active flood events
    ├─→ [IAdministrativeAreaRepository] → Get flood polygons
    ├─→ [IGraphHopperService] → Call external GraphHopper API
    └─→ [RouteFloodAnalyzer] → Validate route safety
    ↓
[Response: SafeRouteResponse]
    - Route geometry (GeoJSON LineString)
    - Distance, duration
    - Flood warnings
    - Alternative routes
```

### **Data Models**

#### **Request**

```csharp
// File: CreateSafeRouteRequest.cs
public sealed record CreateSafeRouteRequest : IFeatureRequest<SafeRouteResponse>
{
    public Guid UserId { get; init; }
    public decimal StartLatitude { get; init; }
    public decimal StartLongitude { get; init; }
    public decimal EndLatitude { get; init; }
    public decimal EndLongitude { get; init; }
    public string RouteProfile { get; init; } = "car"; // car, bike, foot
    public int MaxAlternatives { get; init; } = 3;
    public bool AvoidFloodedAreas { get; init; } = true;
}
```

#### **Response**

```csharp
// File: SafeRouteResponse.cs
public sealed record SafeRouteResponse : IFeatureResponse
{
    public bool Success { get; init; }
    public string Message { get; init; }
    public SafeRouteStatusCode StatusCode { get; init; }
    public SafeRouteData? Data { get; init; }
}

public sealed record SafeRouteData
{
    public RouteDto PrimaryRoute { get; init; }
    public List<RouteDto> AlternativeRoutes { get; init; }
    public List<FloodWarningDto> FloodWarnings { get; init; }
    public RouteSafetyStatus SafetyStatus { get; init; } // Safe, Caution, Dangerous, Blocked
}

public sealed record RouteDto
{
    public GeoJsonGeometry Geometry { get; init; } // LineString
    public decimal DistanceMeters { get; init; }
    public int DurationSeconds { get; init; }
    public List<RouteInstruction> Instructions { get; init; }
    public decimal FloodRiskScore { get; init; } // 0-100
}

public sealed record FloodWarningDto
{
    public Guid FloodEventId { get; init; }
    public string AdministrativeAreaName { get; init; }
    public string Severity { get; init; } // "critical", "warning", "caution"
    public decimal PeakLevel { get; init; }
    public GeoJsonGeometry FloodPolygon { get; init; }
    public decimal DistanceFromRouteMeters { get; init; }
}
```

---

## 🔧 IMPLEMENTATION PLAN

### **Phase 1: GraphHopper Integration** (FeatG71)

```
Files to create:
1. src/External/Infrastructure/Services/FDAAPI.Infra.Services/Routing/
   ├── IGraphHopperService.cs
   ├── GraphHopperService.cs
   ├── Models/GraphHopperRequest.cs
   ├── Models/GraphHopperResponse.cs
   └── GraphHopperConfiguration.cs

2. appsettings.json
   "GraphHopper": {
     "BaseUrl": "http://localhost:8989",
     "ApiKey": "",  // Optional if self-hosted
     "DefaultProfile": "car",
     "Timeout": 30000
   }
```

### **Phase 2: Flood Polygon Logic** (FeatG72)

```
Files to create:
1. src/Core/Application/FDAAPI.App.Common/Services/
   ├── IRouteFloodAnalyzer.cs
   └── RouteFloodAnalyzer.cs

Logic:
- Get active FloodEvents (StartTime <= Now <= EndTime)
- Retrieve AdministrativeArea.Geometry for each event
- Parse JSON geometry to GeoJSON Polygon
- Check route-polygon intersection using PostGIS or in-memory
```

### **Phase 3: Safe Route Feature** (FeatG73)

```
Files structure:
1. src/Core/Application/FDAAPI.App.FeatG73_RequestSafeRoute/
   ├── CreateSafeRouteRequest.cs
   ├── CreateSafeRouteRequestValidator.cs
   ├── CreateSafeRouteHandler.cs
   └── SafeRouteResponse.cs

2. src/External/Presentation/.../Endpoints/Feat73_RequestSafeRoute/
   ├── RequestSafeRouteEndpoint.cs
   └── DTOs/
       ├── RequestSafeRouteRequestDto.cs
       └── SafeRouteResponseDto.cs
```

### **Handler Logic**

```csharp
public class CreateSafeRouteHandler : IRequestHandler<CreateSafeRouteRequest, SafeRouteResponse>
{
    private readonly IGraphHopperService _graphHopper;
    private readonly IFloodEventRepository _floodRepo;
    private readonly IAdministrativeAreaRepository _adminAreaRepo;
    private readonly IRouteFloodAnalyzer _floodAnalyzer;

    public async Task<SafeRouteResponse> Handle(CreateSafeRouteRequest request, CancellationToken ct)
    {
        // 1. Get active flood events
        var activeFloods = await _floodRepo.GetActiveFloodEventsAsync(ct);

        // 2. Build flood polygons list
        var floodPolygons = new List<FloodPolygon>();
        foreach (var flood in activeFloods)
        {
            var area = await _adminAreaRepo.GetByIdAsync(flood.AdministrativeAreaId, ct);
            if (area?.Geometry != null)
            {
                var polygon = JsonSerializer.Deserialize<GeoJsonGeometry>(area.Geometry);
                floodPolygons.Add(new FloodPolygon
                {
                    Geometry = polygon,
                    FloodEvent = flood
                });
            }
        }

        // 3. Call GraphHopper with avoid_polygons
        var routeRequest = new GraphHopperRouteRequest
        {
            Points = new[]
            {
                new[] { request.StartLongitude, request.StartLatitude },
                new[] { request.EndLongitude, request.EndLatitude }
            },
            Profile = request.RouteProfile,
            AvoidPolygons = request.AvoidFloodedAreas
                ? floodPolygons.Select(p => p.Geometry).ToList()
                : null,
            AlternativeRoute = new AlternativeRouteConfig
            {
                MaxPaths = request.MaxAlternatives
            }
        };

        var routeResponse = await _graphHopper.GetRouteAsync(routeRequest, ct);

        // 4. Analyze route safety
        var primaryRoute = routeResponse.Paths.FirstOrDefault();
        if (primaryRoute == null)
        {
            return new SafeRouteResponse
            {
                Success = false,
                Message = "No route found. All paths may be blocked by flooding.",
                StatusCode = SafeRouteStatusCode.RouteBlocked
            };
        }

        var floodWarnings = _floodAnalyzer.AnalyzeRoute(
            primaryRoute.Geometry,
            floodPolygons);

        var safetyStatus = CalculateSafetyStatus(floodWarnings);

        // 5. Build response
        return new SafeRouteResponse
        {
            Success = true,
            Message = "Route calculated successfully",
            StatusCode = SafeRouteStatusCode.Success,
            Data = new SafeRouteData
            {
                PrimaryRoute = MapToRouteDto(primaryRoute),
                AlternativeRoutes = routeResponse.Paths.Skip(1)
                    .Select(MapToRouteDto).ToList(),
                FloodWarnings = floodWarnings,
                SafetyStatus = safetyStatus
            }
        };
    }
}
```

---

## 🧪 TESTING STRATEGY

### **Test Cases**

#### **1. Normal Route (No Floods)**

```
Input: StartPoint, EndPoint (no active floods)
Expected: Success, SafetyStatus = Safe, 0 warnings
```

#### **2. Route with Nearby Flood**

```
Input: StartPoint, EndPoint (flood 200m away)
Expected: Success, SafetyStatus = Caution, warning with distance
```

#### **3. Route Through Flooded Area**

```
Input: StartPoint, EndPoint (flood on direct path)
Expected: Success with detour, SafetyStatus = Dangerous, warnings
```

#### **4. Route Completely Blocked**

```
Input: StartPoint in flood zone, EndPoint outside
Expected: RouteBlocked status code, suggest waiting or alternative transport
```

#### **5. Multiple Alternative Routes**

```
Input: StartPoint, EndPoint, MaxAlternatives = 3
Expected: 3 routes with different flood risk scores
```

---

## 📈 PERFORMANCE CONSIDERATIONS

### **Caching Strategy**

```csharp
// Redis cache key pattern
string cacheKey = $"route:{startLat}:{startLng}:{endLat}:{endLng}:{profile}:{floodVersion}";

// Flood data versioning
string floodVersion = ComputeFloodDataHash(activeFloods); // MD5 hash
```

### **GraphHopper Response Time**

- **Local deployment**: ~50-200ms (fast)
- **Cloud API**: ~200-500ms
- **Cache hit**: <10ms

### **PostGIS Spatial Query**

```sql
-- Efficient polygon intersection check
SELECT f.id, a.name, a.geometry
FROM flood_events f
JOIN administrative_areas a ON f.administrative_area_id = a.id
WHERE f.end_time >= NOW()
  AND ST_Intersects(
    ST_GeomFromGeoJSON(a.geometry),
    ST_LineFromText('LINESTRING(...)')
  );
```

---

## 🚀 DEPLOYMENT

### **Docker Compose**

```yaml
version: '3.8'
services:
  graphhopper:
    image: graphhopper/graphhopper:8.0
    container_name: fda_graphhopper
    volumes:
      - ./graphhopper-data:/data
    ports:
      - "8989:8989"
    environment:
      - JAVA_OPTS=-Xmx4g -Xms2g
    command: >
      --input /data/vietnam-latest.osm.pbf
      --output /data/vietnam-gh
      --host 0.0.0.0
      --port 8989
```

### **Environment Variables**

```env
GRAPHHOPPER_BASE_URL=http://graphhopper:8989
GRAPHHOPPER_TIMEOUT_MS=30000
ROUTING_CACHE_TTL_SECONDS=3600
```

---

## 🔄 ALTERNATIVE: Nếu Chọn OSRM

### **Ưu điểm**

- Nhanh hơn GraphHopper (~30% faster)
- Community lớn hơn
- Docker image nhẹ hơn

### **Nhược điểm**

- **KHÔNG hỗ trợ avoid_polygons**
- Phải implement custom logic:
  1. Gọi OSRM để lấy route
  2. Check route-polygon intersection manually
  3. Nếu intersect → tính waypoints to detour
  4. Gọi lại OSRM với waypoints
  5. Lặp lại cho đến khi route an toàn

### **Complexity**

- GraphHopper: **Low** (built-in avoid)
- OSRM: **High** (custom implementation)

---

## 📊 KẾT LUẬN & ĐỀ XUẤT CUỐI CÙNG

### **🏆 Recommendation: GraphHopper**

**Lý do:**

1. ✅ **Avoid polygons native support** - Quan trọng nhất cho FE-21
2. ✅ **Flexible custom weighting** - Scale tốt cho future features
3. ✅ **Alternative routes** - Tốt cho UX
4. ✅ **Easy .NET integration** - Ít code hơn, maintain dễ hơn
5. ✅ **Self-hosted** - Data privacy, no API costs
6. ✅ **Vietnam OSM data** - Quality tốt

**Trade-offs:**

- Performance hơi chậm hơn OSRM (~20-30%) → Acceptable với caching
- Java runtime → Hơi nặng hơn C++ (OSRM/Valhalla) → OK với 4GB RAM

---

## 📅 IMPLEMENTATION TIMELINE

| Phase | Task | Effort | Priority |
|-------|------|--------|----------|
| 1 | GraphHopper Docker setup | 1 day | High |
| 2 | IGraphHopperService + Tests | 2 days | High |
| 3 | RouteFloodAnalyzer logic | 2 days | High |
| 4 | FeatG73 Handler + Validator | 2 days | High |
| 5 | Endpoint + DTOs | 1 day | High |
| 6 | Integration tests (blocked scenarios) | 2 days | High |
| 7 | Performance optimization + caching | 1 day | Medium |
| **Total** | **~11 days** | | |

---

## 💡 SAMPLE API USAGE

### **Request**

```http
POST /api/v1/routing/safe-route
Authorization: Bearer {token}
Content-Type: application/json

{
  "startLatitude": 10.762622,
  "startLongitude": 106.660172,
  "endLatitude": 10.823099,
  "endLongitude": 106.629664,
  "routeProfile": "car",
  "maxAlternatives": 3,
  "avoidFloodedAreas": true
}
```

### **Response**

```json
{
  "success": true,
  "message": "Route calculated successfully",
  "statusCode": "Success",
  "data": {
    "primaryRoute": {
      "geometry": {
        "type": "LineString",
        "coordinates": [[106.660172, 10.762622], ...]
      },
      "distanceMeters": 12450,
      "durationSeconds": 1230,
      "floodRiskScore": 15.5,
      "instructions": [...]
    },
    "alternativeRoutes": [...],
    "floodWarnings": [
      {
        "floodEventId": "...",
        "administrativeAreaName": "Phường Bến Nghé, Quận 1",
        "severity": "warning",
        "peakLevel": 2.3,
        "distanceFromRouteMeters": 85
      }
    ],
    "safetyStatus": "Caution"
  }
}
```

---

## 🔗 REFERENCES

### **File Locations**

| Aspect | File Path |
|--------|-----------|
| Area Entity | [src/Core/Domain/FDAAPI.Domain.RelationalDb/Entities/Area.cs](../src/Core/Domain/FDAAPI.Domain.RelationalDb/Entities/Area.cs) |
| FloodEvent Entity | [src/Core/Domain/FDAAPI.Domain.RelationalDb/Entities/FloodEvent.cs](../src/Core/Domain/FDAAPI.Domain.RelationalDb/Entities/FloodEvent.cs) |
| AdministrativeArea Entity | [src/Core/Domain/FDAAPI.Domain.RelationalDb/Entities/AdministrativeArea.cs](../src/Core/Domain/FDAAPI.Domain.RelationalDb/Entities/AdministrativeArea.cs) |
| CreateAreaHandler (Reference) | [src/Core/Application/FDAAPI.App.FeatG32_AreaCreate/CreateAreaHandler.cs](../src/Core/Application/FDAAPI.App.FeatG32_AreaCreate/CreateAreaHandler.cs) |
| GeoJSON Models | [src/Core/Application/FDAAPI.App.Common/DTOs/GeoJsonModels.cs](../src/Core/Application/FDAAPI.App.Common/DTOs/GeoJsonModels.cs) |

### **External Resources**

- [GraphHopper Documentation](https://docs.graphhopper.com/)
- [GraphHopper API Reference](https://docs.graphhopper.com/api/)
- [Vietnam OSM Data](https://download.geofabrik.de/asia/vietnam.html)
- [PostGIS Spatial Functions](https://postgis.net/docs/reference.html)

---

## ✅ NEXT STEPS

1. Review và approval của team cho routing engine choice
2. Setup GraphHopper Docker container
3. Implement Phase 1: GraphHopper Service Integration
4. Implement Phase 2: Flood Polygon Logic
5. Implement Phase 3: Safe Route Feature
6. Unit & Integration Testing
7. Performance Optimization & Caching
8. Documentation & Deployment

---

**End of Analysis Document**
