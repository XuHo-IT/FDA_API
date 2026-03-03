# FE-21: Safe Route Suggestions with GraphHopper - Implementation Plan

## Overview

Implement safe route calculation with flood avoidance logic by integrating GraphHopper routing engine. This feature allows users to request routes that automatically avoid active flood zones.

**Routing Engine Choice**: GraphHopper (recommended over OSRM/Valhalla due to native avoid_polygons support)

---

## Context

### Current State

- ✅ Clean Architecture with CQRS (MediatR)
- ✅ FloodEvent entity with AdministrativeArea relationship
- ✅ AdministrativeArea with Geometry field (JSON string format)
- ✅ GeoJSON models for Point geometries
- ❌ NO routing/pathfinding logic
- ❌ NO method to get active flood events
- ❌ NO spatial intersection utilities

### Requirements

1. **Routing Service Integration** - Integrate GraphHopper for route calculation
2. **Avoid Flooded Polygons** - Routes must avoid active flood zones
3. **Route Blocked Scenarios** - Handle cases where all routes are blocked

---

## Implementation Phases

This implementation follows 9 phases, starting with GraphHopper setup:

### Feature Groups

- **FeatG71_GraphHopperIntegration** - GraphHopper service wrapper
- **FeatG72_FloodAnalysis** - Flood polygon analysis utilities
- **FeatG73_RequestSafeRoute** - Main safe route feature

---

## PHASE 0: GraphHopper Docker Setup (PREREQUISITE)

**⚠️ CRITICAL: This must be completed first before any code implementation**

### 0.1 System Requirements Check

Verify system meets requirements:

```bash
# Check Docker is installed and running
docker --version
docker ps

# Check available disk space (need ~3GB)
df -h

# Check available RAM (need 4GB for GraphHopper)
free -h  # Linux
# OR
wmic OS get FreePhysicalMemory  # Windows
```

**Minimum Requirements**:

- Docker Desktop installed and running
- 4GB free RAM
- 3GB free disk space
- Internet connection for OSM data download

### 0.2 Project Structure Setup

Create folders in project root:

```bash
cd "d:\Capstone Project\FDA_API"

# Create folders
mkdir graphhopper-data
mkdir scripts
```

### 0.3 Create Docker Compose File

**File**: `docker-compose.graphhopper.yml` (in project root: `d:\Capstone Project\FDA_API\`)

```yaml
version: "3.8"
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
    restart: unless-stopped
```

### 0.4 Download Vietnam OSM Data

**Option A: Using wget (Linux/Mac/Git Bash)**

Create script: `scripts/download-vietnam-osm.sh`

```bash
#!/bin/bash
cd graphhopper-data
echo "Downloading Vietnam OSM data (~600MB)..."
wget https://download.geofabrik.de/asia/vietnam-latest.osm.pbf
echo "Download complete!"
ls -lh vietnam-latest.osm.pbf
```

Run:

```bash
chmod +x scripts/download-vietnam-osm.sh
./scripts/download-vietnam-osm.sh
```

**Option B: Using PowerShell (Windows)**

Create script: `scripts/download-vietnam-osm.ps1`

```powershell
Set-Location graphhopper-data
Write-Host "Downloading Vietnam OSM data (~600MB)..." -ForegroundColor Green
Invoke-WebRequest -Uri "https://download.geofabrik.de/asia/vietnam-latest.osm.pbf" -OutFile "vietnam-latest.osm.pbf"
Write-Host "Download complete!" -ForegroundColor Green
Get-ChildItem vietnam-latest.osm.pbf
```

Run:

```powershell
.\scripts\download-vietnam-osm.ps1
```

**Option C: Manual Download**

1. Open browser: https://download.geofabrik.de/asia/vietnam.html
2. Click "vietnam-latest.osm.pbf" (~600MB)
3. Move downloaded file to `graphhopper-data/` folder

### 0.5 Start GraphHopper Container

**First time startup (will build graph - takes 5-10 minutes):**

```bash
cd "d:\Capstone Project\FDA_API"

# Start container
docker-compose -f docker-compose.graphhopper.yml up -d

# Watch logs (wait for "Started server at HTTP 8989")
docker logs -f fda_graphhopper
```

**Expected log output:**

```
INFO  [2024-01-28 10:00:00] GraphHopper location: /data/vietnam-gh
INFO  [2024-01-28 10:00:05] graph.vertices: 1234567
INFO  [2024-01-28 10:00:10] graph.edges: 2345678
INFO  [2024-01-28 10:05:00] Started server at HTTP 8989
```

**⏱️ Wait time**: 5-10 minutes for first startup (graph building from OSM data)

### 0.6 Verify GraphHopper is Running

**Test 1: Health Check**

```bash
curl http://localhost:8989/health
```

**Expected response:**

```json
{ "status": "ok" }
```

**Test 2: Simple Route Query (Ho Chi Minh City)**

```bash
curl -X POST http://localhost:8989/route \
  -H "Content-Type: application/json" \
  -d '{
    "points": [[106.660172, 10.762622], [106.629664, 10.823099]],
    "profile": "car",
    "points_encoded": false
  }'
```

**Expected**: JSON response with `paths` array containing route data

**Test 3: Check Container Status**

```bash
docker ps | grep fda_graphhopper
```

**Expected**: Container status should be "Up"

### 0.7 GraphHopper Management Commands

```bash
# Stop GraphHopper
docker-compose -f docker-compose.graphhopper.yml down

# Start GraphHopper (subsequent starts are fast - graph already built)
docker-compose -f docker-compose.graphhopper.yml up -d

# View logs
docker logs fda_graphhopper

# Restart GraphHopper
docker-compose -f docker-compose.graphhopper.yml restart

# Remove GraphHopper (keeps data)
docker-compose -f docker-compose.graphhopper.yml down

# Remove GraphHopper and data (complete cleanup)
docker-compose -f docker-compose.graphhopper.yml down -v
rm -rf graphhopper-data/vietnam-gh
```

### 0.8 Troubleshooting

**Problem**: Container exits immediately

```bash
# Check logs
docker logs fda_graphhopper

# Common issue: OSM file not found
# Solution: Verify file exists
ls -lh graphhopper-data/vietnam-latest.osm.pbf
```

**Problem**: Out of memory error

```bash
# Reduce Java heap size in docker-compose.graphhopper.yml
# Change: -Xmx4g -Xms2g
# To: -Xmx2g -Xms1g
```

**Problem**: Port 8989 already in use

```bash
# Find what's using port 8989
netstat -ano | findstr :8989  # Windows
lsof -i :8989  # Linux/Mac

# Option 1: Kill the process
# Option 2: Change port in docker-compose.graphhopper.yml
# ports:
#   - "8990:8989"  # Use 8990 instead
```

**Problem**: Slow graph building (>15 minutes)

- Normal for first time with full Vietnam data
- Subsequent starts use cached graph (fast)
- To speed up: Use smaller region (e.g., Ho Chi Minh City only)

### 0.9 Acceptance Criteria

✅ Docker container `fda_graphhopper` is running
✅ Health check returns `{"status": "ok"}`
✅ Simple route query returns valid JSON with paths
✅ Logs show "Started server at HTTP 8989"
✅ Port 8989 is accessible from host machine

**ONLY proceed to Phase 1 after completing Phase 0 successfully!**

---

## PHASE 1: Domain Layer Extensions

### 1.1 Extend IFloodEventRepository

**File**: `src/Core/Domain/FDAAPI.Domain.RelationalDb/Repositories/IFloodEventRepository.cs`

Add method to get active flood events:

```csharp
Task<List<FloodEvent>> GetActiveFloodEventsAsync(CancellationToken ct = default);
```

### 1.2 Extend IAdministrativeAreaRepository

No changes needed - existing `GetByIdAsync` and `GetByIdsAsync` are sufficient.

### 1.3 Create New Enums

**File**: `src/Core/Domain/FDAAPI.Domain.RelationalDb/Enums/RouteSafetyStatus.cs`

```csharp
public enum RouteSafetyStatus
{
    Safe = 0,
    Caution = 1,
    Dangerous = 2,
    Blocked = 3
}
```

**File**: `src/Core/Domain/FDAAPI.Domain.RelationalDb/Enums/RouteProfile.cs`

```csharp
public enum RouteProfile
{
    Car,
    Bike,
    Foot
}
```

---

## PHASE 2: Infrastructure Layer - Repositories

### 2.1 Implement GetActiveFloodEventsAsync

**File**: `src/External/Infrastructure/Persistence/FDAAPI.Infra.Persistence/Repositories/PgsqlFloodEventRepository.cs`

Add implementation:

```csharp
public async Task<List<FloodEvent>> GetActiveFloodEventsAsync(CancellationToken ct = default)
{
    var now = DateTime.UtcNow;
    return await _context.FloodEvents
        .AsNoTracking()
        .Include(f => f.AdministrativeArea)
        .Where(f => f.StartTime <= now && f.EndTime >= now)
        .ToListAsync(ct);
}
```

---

## PHASE 3: Infrastructure Layer - GraphHopper Service

### 3.1 Create GraphHopper Models

**Folder**: `src/External/Infrastructure/Services/FDAAPI.Infra.Services/Routing/Models/`

**GraphHopperRouteRequest.cs**:

```csharp
public class GraphHopperRouteRequest
{
    public decimal[][] Points { get; set; } = Array.Empty<decimal[]>();
    public string Profile { get; set; } = "car";
    public List<GeoJsonGeometry>? AvoidPolygons { get; set; }
    public AlternativeRouteConfig? AlternativeRoute { get; set; }
    public bool PointsEncoded { get; set; } = false;
    public bool Instructions { get; set; } = true;
}

public class AlternativeRouteConfig
{
    public int MaxPaths { get; set; } = 3;
    public double MaxWeightFactor { get; set; } = 1.4;
}
```

**GraphHopperRouteResponse.cs**:

```csharp
public class GraphHopperRouteResponse
{
    public List<GraphHopperPath> Paths { get; set; } = new();
}

public class GraphHopperPath
{
    public double Distance { get; set; }
    public long Time { get; set; }
    public GeoJsonGeometry Geometry { get; set; } = new();
    public List<GraphHopperInstruction> Instructions { get; set; } = new();
}

public class GraphHopperInstruction
{
    public double Distance { get; set; }
    public int Time { get; set; }
    public int Sign { get; set; }
    public string Text { get; set; } = string.Empty;
}
```

### 3.2 Create GraphHopper Service Interface

**File**: `src/Core/Application/FDAAPI.App.Common/Services/IGraphHopperService.cs`

```csharp
public interface IGraphHopperService
{
    Task<GraphHopperRouteResponse> GetRouteAsync(
        GraphHopperRouteRequest request,
        CancellationToken ct = default);
}
```

### 3.3 Implement GraphHopper Service

**File**: `src/External/Infrastructure/Services/FDAAPI.Infra.Services/Routing/GraphHopperService.cs`

```csharp
public class GraphHopperService : IGraphHopperService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GraphHopperService> _logger;

    public GraphHopperService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GraphHopperService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        var baseUrl = _configuration["GraphHopper:BaseUrl"] ?? "http://localhost:8989";
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromMilliseconds(
            _configuration.GetValue<int>("GraphHopper:Timeout", 30000));
    }

    public async Task<GraphHopperRouteResponse> GetRouteAsync(
        GraphHopperRouteRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var requestBody = BuildGraphHopperRequest(request);
            var response = await _httpClient.PostAsJsonAsync("/route", requestBody, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GraphHopperRouteResponse>(ct);
            return result ?? throw new InvalidOperationException("Empty response from GraphHopper");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "GraphHopper API request failed");
            throw new InvalidOperationException("Routing service unavailable", ex);
        }
    }

    private object BuildGraphHopperRequest(GraphHopperRouteRequest request)
    {
        var ghRequest = new
        {
            points = request.Points,
            profile = request.Profile,
            points_encoded = request.PointsEncoded,
            instructions = request.Instructions,
            alternative_route = request.AlternativeRoute != null ? new
            {
                max_paths = request.AlternativeRoute.MaxPaths,
                max_weight_factor = request.AlternativeRoute.MaxWeightFactor
            } : null,
            avoid_polygons = request.AvoidPolygons?.Select(p => new
            {
                type = p.Type,
                coordinates = ConvertCoordinatesToArray(p.Coordinates)
            }).ToList()
        };

        return ghRequest;
    }

    private decimal[][][] ConvertCoordinatesToArray(decimal[] coords)
    {
        // Convert flat coordinate array to polygon format
        // Assuming coords is [lng1, lat1, lng2, lat2, ...]
        var points = new List<decimal[]>();
        for (int i = 0; i < coords.Length; i += 2)
        {
            points.Add(new[] { coords[i], coords[i + 1] });
        }
        return new[] { points.ToArray() };
    }
}
```

### 3.4 Configuration

**File**: `src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/appsettings.json`

Add section:

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

## PHASE 4: Application Layer - Flood Analysis Service

### 4.1 Create Flood Analysis Models

**File**: `src/Core/Application/FDAAPI.App.Common/Models/Routing/FloodPolygon.cs`

```csharp
public class FloodPolygon
{
    public Guid FloodEventId { get; set; }
    public GeoJsonGeometry Geometry { get; set; } = new();
    public FloodEvent FloodEvent { get; set; } = null!;
}
```

### 4.2 Create Route Flood Analyzer Interface

**File**: `src/Core/Application/FDAAPI.App.Common/Services/IRouteFloodAnalyzer.cs`

```csharp
public interface IRouteFloodAnalyzer
{
    List<FloodWarningDto> AnalyzeRoute(
        GeoJsonGeometry routeGeometry,
        List<FloodPolygon> floodPolygons);

    RouteSafetyStatus CalculateSafetyStatus(List<FloodWarningDto> warnings);
}
```

### 4.3 Implement Route Flood Analyzer

**File**: `src/External/Infrastructure/Services/FDAAPI.Infra.Services/Routing/RouteFloodAnalyzer.cs`

```csharp
public class RouteFloodAnalyzer : IRouteFloodAnalyzer
{
    private const double DANGER_THRESHOLD_METERS = 100;
    private const double CAUTION_THRESHOLD_METERS = 500;

    public List<FloodWarningDto> AnalyzeRoute(
        GeoJsonGeometry routeGeometry,
        List<FloodPolygon> floodPolygons)
    {
        var warnings = new List<FloodWarningDto>();

        foreach (var floodPolygon in floodPolygons)
        {
            var distance = CalculateMinimumDistance(routeGeometry, floodPolygon.Geometry);

            if (distance <= CAUTION_THRESHOLD_METERS)
            {
                warnings.Add(new FloodWarningDto
                {
                    FloodEventId = floodPolygon.FloodEventId,
                    AdministrativeAreaName = floodPolygon.FloodEvent.AdministrativeArea?.Name ?? "Unknown",
                    Severity = DetermineSeverity(floodPolygon.FloodEvent.PeakLevel),
                    PeakLevel = floodPolygon.FloodEvent.PeakLevel ?? 0,
                    FloodPolygon = floodPolygon.Geometry,
                    DistanceFromRouteMeters = (decimal)distance
                });
            }
        }

        return warnings;
    }

    public RouteSafetyStatus CalculateSafetyStatus(List<FloodWarningDto> warnings)
    {
        if (!warnings.Any())
            return RouteSafetyStatus.Safe;

        var minDistance = warnings.Min(w => w.DistanceFromRouteMeters);
        var hasCritical = warnings.Any(w => w.Severity == "critical");

        if (minDistance < (decimal)DANGER_THRESHOLD_METERS || hasCritical)
            return RouteSafetyStatus.Dangerous;

        if (minDistance < (decimal)CAUTION_THRESHOLD_METERS)
            return RouteSafetyStatus.Caution;

        return RouteSafetyStatus.Safe;
    }

    private double CalculateMinimumDistance(GeoJsonGeometry route, GeoJsonGeometry polygon)
    {
        // Simplified distance calculation
        // For production: use spatial library like NetTopologySuite
        // For now: calculate distance from route midpoint to polygon centroid

        if (route.Coordinates.Length < 2 || polygon.Coordinates.Length < 2)
            return double.MaxValue;

        var routeMidpoint = new
        {
            Lng = (double)route.Coordinates[0],
            Lat = (double)route.Coordinates[1]
        };

        var polygonCentroid = new
        {
            Lng = (double)polygon.Coordinates[0],
            Lat = (double)polygon.Coordinates[1]
        };

        return CalculateHaversineDistance(
            routeMidpoint.Lat, routeMidpoint.Lng,
            polygonCentroid.Lat, polygonCentroid.Lng);
    }

    private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // Earth radius in meters
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    private string DetermineSeverity(decimal? peakLevel)
    {
        if (!peakLevel.HasValue) return "caution";
        if (peakLevel >= 3.0m) return "critical";
        if (peakLevel >= 2.0m) return "warning";
        return "caution";
    }
}
```

---

## PHASE 5: Application Layer - FeatG73_RequestSafeRoute

### 5.1 Create DTOs

**File**: `src/Core/Application/FDAAPI.App.Common/DTOs/RouteDto.cs`

```csharp
public class RouteDto
{
    public GeoJsonGeometry Geometry { get; set; } = new();
    public decimal DistanceMeters { get; set; }
    public int DurationSeconds { get; set; }
    public List<RouteInstructionDto> Instructions { get; set; } = new();
    public decimal FloodRiskScore { get; set; }
}

public class RouteInstructionDto
{
    public decimal Distance { get; set; }
    public int Time { get; set; }
    public string Text { get; set; } = string.Empty;
}
```

**File**: `src/Core/Application/FDAAPI.App.Common/DTOs/FloodWarningDto.cs`

```csharp
public class FloodWarningDto
{
    public Guid FloodEventId { get; set; }
    public string AdministrativeAreaName { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public decimal PeakLevel { get; set; }
    public GeoJsonGeometry FloodPolygon { get; set; } = new();
    public decimal DistanceFromRouteMeters { get; set; }
}
```

### 5.2 Create StatusCode Enum

**File**: `src/Core/Application/FDAAPI.App.Common/Models/Routing/SafeRouteStatusCode.cs`

```csharp
public enum SafeRouteStatusCode
{
    Success = 200,
    BadRequest = 400,
    Unauthorized = 401,
    NotFound = 404,
    RouteBlocked = 422,
    ServiceUnavailable = 503,
    UnknownError = 500
}
```

### 5.3 Create Request Model

**Folder**: `src/Core/Application/FDAAPI.App.FeatG73_RequestSafeRoute/`

**File**: `CreateSafeRouteRequest.cs`

```csharp
public sealed record CreateSafeRouteRequest(
    Guid UserId,
    decimal StartLatitude,
    decimal StartLongitude,
    decimal EndLatitude,
    decimal EndLongitude,
    string RouteProfile,
    int MaxAlternatives,
    bool AvoidFloodedAreas
) : IFeatureRequest<SafeRouteResponse>;
```

### 5.4 Create Response Model

**File**: `SafeRouteResponse.cs`

```csharp
public class SafeRouteResponse : IFeatureResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public SafeRouteStatusCode StatusCode { get; set; }
    public SafeRouteData? Data { get; set; }
}

public class SafeRouteData
{
    public RouteDto PrimaryRoute { get; set; } = new();
    public List<RouteDto> AlternativeRoutes { get; set; } = new();
    public List<FloodWarningDto> FloodWarnings { get; set; } = new();
    public RouteSafetyStatus SafetyStatus { get; set; }
}
```

### 5.5 Create Validator

**File**: `CreateSafeRouteRequestValidator.cs`

```csharp
public class CreateSafeRouteRequestValidator : AbstractValidator<CreateSafeRouteRequest>
{
    public CreateSafeRouteRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.StartLatitude)
            .InclusiveBetween(-90, 90).WithMessage("Start latitude must be between -90 and 90");

        RuleFor(x => x.StartLongitude)
            .InclusiveBetween(-180, 180).WithMessage("Start longitude must be between -180 and 180");

        RuleFor(x => x.EndLatitude)
            .InclusiveBetween(-90, 90).WithMessage("End latitude must be between -90 and 90");

        RuleFor(x => x.EndLongitude)
            .InclusiveBetween(-180, 180).WithMessage("End longitude must be between -180 and 180");

        RuleFor(x => x.RouteProfile)
            .NotEmpty().WithMessage("Route profile is required")
            .Must(p => p == "car" || p == "bike" || p == "foot")
            .WithMessage("Route profile must be 'car', 'bike', or 'foot'");

        RuleFor(x => x.MaxAlternatives)
            .InclusiveBetween(0, 5).WithMessage("Max alternatives must be between 0 and 5");
    }
}
```

### 5.6 Create Handler

**File**: `CreateSafeRouteHandler.cs`

```csharp
public class CreateSafeRouteHandler : IRequestHandler<CreateSafeRouteRequest, SafeRouteResponse>
{
    private readonly IGraphHopperService _graphHopper;
    private readonly IFloodEventRepository _floodRepo;
    private readonly IAdministrativeAreaRepository _adminAreaRepo;
    private readonly IRouteFloodAnalyzer _floodAnalyzer;
    private readonly ILogger<CreateSafeRouteHandler> _logger;

    public CreateSafeRouteHandler(
        IGraphHopperService graphHopper,
        IFloodEventRepository floodRepo,
        IAdministrativeAreaRepository adminAreaRepo,
        IRouteFloodAnalyzer floodAnalyzer,
        ILogger<CreateSafeRouteHandler> logger)
    {
        _graphHopper = graphHopper;
        _floodRepo = floodRepo;
        _adminAreaRepo = adminAreaRepo;
        _floodAnalyzer = floodAnalyzer;
        _logger = logger;
    }

    public async Task<SafeRouteResponse> Handle(
        CreateSafeRouteRequest request,
        CancellationToken ct)
    {
        try
        {
            // 1. Get active flood events
            var activeFloods = await _floodRepo.GetActiveFloodEventsAsync(ct);
            _logger.LogInformation("Found {Count} active flood events", activeFloods.Count);

            // 2. Build flood polygons list
            var floodPolygons = new List<FloodPolygon>();
            foreach (var flood in activeFloods)
            {
                if (flood.AdministrativeArea?.Geometry != null)
                {
                    try
                    {
                        var geometry = JsonSerializer.Deserialize<GeoJsonGeometry>(
                            flood.AdministrativeArea.Geometry);

                        if (geometry != null)
                        {
                            floodPolygons.Add(new FloodPolygon
                            {
                                FloodEventId = flood.Id,
                                Geometry = geometry,
                                FloodEvent = flood
                            });
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse geometry for flood event {Id}", flood.Id);
                    }
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
                Profile = request.RouteProfile.ToLower(),
                AvoidPolygons = request.AvoidFloodedAreas
                    ? floodPolygons.Select(p => p.Geometry).ToList()
                    : null,
                AlternativeRoute = request.MaxAlternatives > 0
                    ? new AlternativeRouteConfig { MaxPaths = request.MaxAlternatives }
                    : null
            };

            var routeResponse = await _graphHopper.GetRouteAsync(routeRequest, ct);

            // 4. Analyze route safety
            if (routeResponse.Paths == null || !routeResponse.Paths.Any())
            {
                return new SafeRouteResponse
                {
                    Success = false,
                    Message = "No route found. All paths may be blocked by flooding.",
                    StatusCode = SafeRouteStatusCode.RouteBlocked
                };
            }

            var primaryRoute = routeResponse.Paths.First();
            var floodWarnings = _floodAnalyzer.AnalyzeRoute(primaryRoute.Geometry, floodPolygons);
            var safetyStatus = _floodAnalyzer.CalculateSafetyStatus(floodWarnings);

            // 5. Build response
            return new SafeRouteResponse
            {
                Success = true,
                Message = "Route calculated successfully",
                StatusCode = SafeRouteStatusCode.Success,
                Data = new SafeRouteData
                {
                    PrimaryRoute = MapToRouteDto(primaryRoute, floodWarnings),
                    AlternativeRoutes = routeResponse.Paths.Skip(1)
                        .Select(p => MapToRouteDto(p, _floodAnalyzer.AnalyzeRoute(p.Geometry, floodPolygons)))
                        .ToList(),
                    FloodWarnings = floodWarnings,
                    SafetyStatus = safetyStatus
                }
            };
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "GraphHopper service error");
            return new SafeRouteResponse
            {
                Success = false,
                Message = "Routing service is currently unavailable",
                StatusCode = SafeRouteStatusCode.ServiceUnavailable
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in safe route calculation");
            return new SafeRouteResponse
            {
                Success = false,
                Message = "An error occurred while calculating the route",
                StatusCode = SafeRouteStatusCode.UnknownError
            };
        }
    }

    private RouteDto MapToRouteDto(GraphHopperPath path, List<FloodWarningDto> warnings)
    {
        var floodRiskScore = CalculateFloodRiskScore(warnings);

        return new RouteDto
        {
            Geometry = path.Geometry,
            DistanceMeters = (decimal)path.Distance,
            DurationSeconds = (int)(path.Time / 1000), // Convert from ms to seconds
            Instructions = path.Instructions.Select(i => new RouteInstructionDto
            {
                Distance = (decimal)i.Distance,
                Time = i.Time,
                Text = i.Text
            }).ToList(),
            FloodRiskScore = floodRiskScore
        };
    }

    private decimal CalculateFloodRiskScore(List<FloodWarningDto> warnings)
    {
        if (!warnings.Any()) return 0;

        var criticalCount = warnings.Count(w => w.Severity == "critical");
        var warningCount = warnings.Count(w => w.Severity == "warning");
        var cautionCount = warnings.Count(w => w.Severity == "caution");

        var score = (criticalCount * 40) + (warningCount * 20) + (cautionCount * 10);
        return Math.Min(100, score);
    }
}
```

---

## PHASE 6: Presentation Layer - FastEndpoints

### 6.1 Create Endpoint DTOs

**Folder**: `src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/Endpoints/Feat73_RequestSafeRoute/DTOs/`

**File**: `RequestSafeRouteRequestDto.cs`

```csharp
public class RequestSafeRouteRequestDto
{
    public decimal StartLatitude { get; set; }
    public decimal StartLongitude { get; set; }
    public decimal EndLatitude { get; set; }
    public decimal EndLongitude { get; set; }
    public string RouteProfile { get; set; } = "car";
    public int MaxAlternatives { get; set; } = 3;
    public bool AvoidFloodedAreas { get; set; } = true;
}
```

**File**: `SafeRouteResponseDto.cs`

```csharp
public class SafeRouteResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public SafeRouteData? Data { get; set; }
}
```

### 6.2 Create Endpoint

**File**: `RequestSafeRouteEndpoint.cs`

```csharp
public class RequestSafeRouteEndpoint : Endpoint<RequestSafeRouteRequestDto, SafeRouteResponseDto>
{
    private readonly IMediator _mediator;

    public RequestSafeRouteEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/v1/routing/safe-route");
        Policies("User"); // Authenticated users only
        Summary(s =>
        {
            s.Summary = "Request safe route with flood avoidance";
            s.Description = "Calculate route from start to end point while avoiding active flood zones. Returns primary route and alternatives.";
            s.ExampleRequest = new RequestSafeRouteRequestDto
            {
                StartLatitude = 10.762622m,
                StartLongitude = 106.660172m,
                EndLatitude = 10.823099m,
                EndLongitude = 106.629664m,
                RouteProfile = "car",
                MaxAlternatives = 3,
                AvoidFloodedAreas = true
            };
        });
        Tags("Routing", "Safety");
    }

    public override async Task HandleAsync(RequestSafeRouteRequestDto req, CancellationToken ct)
    {
        // Extract UserId from JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim == null)
        {
            await SendAsync(new SafeRouteResponseDto
            {
                Success = false,
                Message = "Unauthorized",
                StatusCode = 401
            }, 401, ct);
            return;
        }

        var userId = Guid.Parse(userIdClaim.Value);

        // Create MediatR request
        var command = new CreateSafeRouteRequest(
            userId,
            req.StartLatitude,
            req.StartLongitude,
            req.EndLatitude,
            req.EndLongitude,
            req.RouteProfile,
            req.MaxAlternatives,
            req.AvoidFloodedAreas
        );

        // Send to handler
        var result = await _mediator.Send(command, ct);

        // Map to DTO
        var response = new SafeRouteResponseDto
        {
            Success = result.Success,
            Message = result.Message,
            StatusCode = (int)result.StatusCode,
            Data = result.Data
        };

        // Send response
        var httpStatusCode = result.Success ? 200 : (int)result.StatusCode;
        await SendAsync(response, httpStatusCode, ct);
    }
}
```

---

## PHASE 7: Configuration & Registration

### 7.1 Register Services

**File**: `src/External/Infrastructure/Common/FDAAPI.Infra.Configuration/ServiceExtensions.cs`

**In AddApplicationServices():**

```csharp
using FDAAPI.App.FeatG73_RequestSafeRoute;

var assemblies = new[]
{
    typeof(CreateSafeRouteRequest).Assembly,
    // ... other assemblies
};
```

**In AddInfrastructureServices():**

```csharp
// GraphHopper service
services.AddHttpClient<IGraphHopperService, GraphHopperService>();

// Flood analyzer
services.AddScoped<IRouteFloodAnalyzer, RouteFloodAnalyzer>();
```

---

## PHASE 8: Docker & Deployment Setup

### 8.1 Create Docker Compose for GraphHopper

**File**: `docker-compose.graphhopper.yml` (in project root)

```yaml
version: "3.8"
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
    restart: unless-stopped
```

### 8.2 Download Vietnam OSM Data

Create script: `scripts/setup-graphhopper.sh`

```bash
#!/bin/bash
mkdir -p graphhopper-data
cd graphhopper-data
wget https://download.geofabrik.de/asia/vietnam-latest.osm.pbf
cd ..
docker-compose -f docker-compose.graphhopper.yml up -d
```

---

## Critical Files to Modify

1. **Domain Layer**:
   - `src/Core/Domain/FDAAPI.Domain.RelationalDb/Repositories/IFloodEventRepository.cs` - Add GetActiveFloodEventsAsync

2. **Infrastructure Layer**:
   - `src/External/Infrastructure/Persistence/FDAAPI.Infra.Persistence/Repositories/PgsqlFloodEventRepository.cs` - Implement GetActiveFloodEventsAsync

3. **Configuration**:
   - `src/External/Infrastructure/Common/FDAAPI.Infra.Configuration/ServiceExtensions.cs` - Register services
   - `src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/appsettings.json` - Add GraphHopper config

---

## Testing & Verification

### Test Case 1: Normal Route (No Floods)

**Request**:

```bash
curl -X POST http://localhost:5000/api/v1/routing/safe-route \
  -H "Authorization: Bearer {token}" \
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

**Expected**: Success, SafetyStatus = Safe, 0 flood warnings

### Test Case 2: Route Blocked Scenario

Create test flood event covering the route path, verify RouteBlocked status.

### Verification Steps

1. ✅ Start GraphHopper container: `docker-compose -f docker-compose.graphhopper.yml up -d`
2. ✅ Verify GraphHopper: `curl http://localhost:8989/health`
3. ✅ Build solution: `dotnet build`
4. ✅ Run application: `dotnet run`
5. ✅ Test endpoint with various scenarios
6. ✅ Check logs for GraphHopper API calls

---

## Notes & Considerations

### Spatial Analysis Limitation

Current implementation uses simplified distance calculation (Haversine from route midpoint to polygon centroid). For production, consider:

- NetTopologySuite for accurate polygon-line intersection
- PostGIS spatial queries for database-level filtering

### GraphHopper Deployment

- Self-hosted recommended for data privacy
- Vietnam OSM data: ~600MB download, ~2GB processed
- Memory requirement: 4GB RAM minimum
- First route calculation may be slow (graph building)

### Performance Optimization

- Add Redis caching for routes (cache key includes flood data hash)
- Cache TTL: 1 hour (routes change when flood events change)
- Consider background job to update flood polygon cache

### Future Enhancements

1. Real-time route updates via SignalR when flood events change
2. Traffic data integration
3. Multi-modal routing (combine car + walking)
4. Route history and favorites

---

## Dependencies

No new NuGet packages required. All functionality uses:

- ✅ Existing HttpClient (for GraphHopper API)
- ✅ System.Text.Json (for serialization)
- ✅ EF Core (existing repositories)
- ✅ MediatR (existing CQRS)
- ✅ FluentValidation (existing validation)

Optional for production:

- NetTopologySuite (for advanced spatial operations)
