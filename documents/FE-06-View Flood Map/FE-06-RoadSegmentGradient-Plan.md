# Plan: Road Segment Gradient GeoJSON for Map Current Status

## Phase 1 (DONE): Basic LineString Features
- Added `GeoJsonLineStringGeometry` class and `GeoJsonFeature.Geometry` as `object`
- Added `Latitude`/`Longitude` to `StationFloodStatus`
- Handler generates LineString features connecting station pairs on same road
- Nearest-neighbor ordering for stations along a road

## Phase 2 (CURRENT): GraphHopper Route Geometry

### Problem
Phase 1 LineStrings only have 2 coordinates (start/end) → renders as straight lines ignoring actual road shape. Doesn't follow roads through intersections.

### Solution
Use self-hosted GraphHopper (already in project) to get actual road geometry between station pairs. GraphHopper returns multi-point LineString that follows real road paths.

---

## Files to Modify

### 1. `src/Core/Application/FDAAPI.App.Common/DTOs/GeoJsonModels.cs`
**Rename** `LineStringGeometry` → `GeoJsonLineStringGeometry` to avoid conflict with `Routing.LineStringGeometry` (`double[][]`).

### 2. `src/Core/Application/FDAAPI.App.FeatG31_GetMapCurrentStatus/GetMapCurrentStatusHandler.cs`

**A. Inject `IGraphHopperService`** (from `FDAAPI.App.Common.Services`, already DI-registered):
```csharp
private readonly IGraphHopperService _graphHopperService;
```

**B. Replace step 5** — call GraphHopper for each station pair (parallel calls):
```csharp
// For each station pair, get road geometry from GraphHopper
var routeResponse = await _graphHopperService.GetRouteAsync(new GraphHopperRouteRequest
{
    Points = new[] {
        new[] { stA.Longitude!.Value, stA.Latitude!.Value },
        new[] { stB.Longitude!.Value, stB.Latitude!.Value }
    },
    Profile = "car",
    Instructions = false
}, ct);

// Convert double[][] → decimal[][] for GeoJSON output
var path = routeResponse.Paths.FirstOrDefault();
var routeCoords = path?.Points.Coordinates
    .Select(c => new[] { (decimal)c[0], (decimal)c[1] })
    .ToArray();
```

**C. Fallback**: If GraphHopper fails → use straight line (2 points), no crash.

---

## Existing Infrastructure Reused

| Component | Location |
|-----------|----------|
| `IGraphHopperService` | `App.Common/Services/IGraphHopperService.cs` |
| `GraphHopperService` | `Infra.Services/Routing/GraphHopperService.cs` |
| `GraphHopperRouteRequest` | `App.Common/Models/Routing/GraphHopperRouteRequest.cs` |
| DI registration | `Infra.Configuration/ServiceExtensions.cs:211` |
| GraphHopper config | `appsettings.json` → `http://localhost:8989` |

---

## Response Structure (same shape, more coordinates)

Before (Phase 1 — straight line):
```json
"coordinates": [[106.7123, 10.8456], [106.7234, 10.8567]]
```

After (Phase 2 — follows road):
```json
"coordinates": [
  [106.7123, 10.8456],
  [106.7130, 10.8460],
  [106.7145, 10.8470],
  [106.7200, 10.8510],
  [106.7234, 10.8567]
]
```

**Properties unchanged** — FE code doesn't need changes.

---

## Verification

1. Build project → no compile errors
2. Ensure GraphHopper self-hosted is running at `http://localhost:8989`
3. Call `GET /api/v1/map/current-status`
4. Verify LineString coordinates have multiple points following road shape
5. If GraphHopper is down → fallback to straight lines (no crash)
