# Plan: Road Segment Gradient GeoJSON for Map Current Status

## Context
Feature FE-06 requires the map to display flood severity as a color gradient along roads. Mapbox `line-gradient` paint property (requires `lineMetrics: true` on source) interpolates color along a LineString. The BE needs to return LineString features — one per adjacent station pair on the same road — with `startColor`/`endColor` properties.

**Problem 1**: Station entity has `RoadName` but no ordering field → solve with nearest-neighbor proximity sort, no DB migration needed.

**Problem 2**: `GeoJsonGeometry.Coordinates` is `decimal[]` and is used directly in `RouteFloodAnalyzer.cs` (`.Length`, indexer `[i]`) — changing to `object` would break compilation there.

---

## Approach: Add Separate LineString Geometry Class

Do NOT modify `GeoJsonGeometry` (used by routing feature). Instead, add a parallel `LineStringGeometry` class specifically for road segments. `GeoJsonFeature` already uses `object? Properties` pattern — apply the same for geometry by adding a dedicated subclass.

---

## Files to Modify

### 1. `src/Core/Application/FDAAPI.App.Common/DTOs/GeoJsonModels.cs`

Add a new `LineStringGeometry` class (do NOT touch existing `GeoJsonGeometry`):

```csharp
/// <summary>
/// GeoJSON LineString Geometry — for road segment gradient features
/// </summary>
public class LineStringGeometry
{
    public string Type { get; set; } = "LineString";
    public decimal[][] Coordinates { get; set; } = Array.Empty<decimal[]>();
}
```

`GeoJsonFeature.Geometry` is currently typed as `GeoJsonGeometry`. Change it to `object` so it can hold either `GeoJsonGeometry` (Point) or `LineStringGeometry` (LineString):

```csharp
public class GeoJsonFeature
{
    public string Type { get; set; } = "Feature";
    public object Geometry { get; set; } = new GeoJsonGeometry();  // was: GeoJsonGeometry
    public object? Properties { get; set; }
}
```

> **Impact check**: `GeoJsonFeature.Geometry` was assigned but never read with typed access in other handlers — only constructed and returned as JSON. Changing to `object` is safe because `System.Text.Json` serializes polymorphically.

### 2. `src/Core/Application/FDAAPI.App.Common/DTOs/StationFloodStatus.cs`

Add coordinates so the handler can access them when building LineString features:

```csharp
public decimal? Latitude { get; set; }
public decimal? Longitude { get; set; }
```

### 3. `src/Core/Application/FDAAPI.App.FeatG31_GetMapCurrentStatus/GetMapCurrentStatusHandler.cs`

**A. Populate Latitude/Longitude** in the `stationStatuses` projection (step 3):
```csharp
Latitude = station.Latitude,
Longitude = station.Longitude,
```

**B. Add road segment generation** after building Point features, before returning:
```csharp
// 5. Build road segment LineString features
var roadSegmentFeatures = new List<GeoJsonFeature>();

var stationsWithCoords = stationStatuses
    .Where(s => s.Latitude.HasValue && s.Longitude.HasValue && !string.IsNullOrEmpty(s.RoadName))
    .GroupBy(s => s.RoadName);

foreach (var road in stationsWithCoords)
{
    var stationList = road.ToList();
    if (stationList.Count < 2) continue;

    var ordered = OrderStationsByProximity(stationList);

    for (int i = 0; i < ordered.Count - 1; i++)
    {
        var stA = ordered[i];
        var stB = ordered[i + 1];

        roadSegmentFeatures.Add(new GeoJsonFeature
        {
            Type = "Feature",
            Geometry = new LineStringGeometry
            {
                Coordinates = new[]
                {
                    new[] { stA.Longitude!.Value, stA.Latitude!.Value },
                    new[] { stB.Longitude!.Value, stB.Latitude!.Value }
                }
            },
            Properties = new
            {
                roadName = stA.RoadName,
                startStationId = stA.StationId,
                endStationId = stB.StationId,
                startSeverityLevel = stA.SeverityLevel,
                endSeverityLevel = stB.SeverityLevel,
                startColor = GetMarkerColor(stA.SeverityLevel),
                endColor = GetMarkerColor(stB.SeverityLevel)
            }
        });
    }
}
```

**C. Combine all features** and update response:
```csharp
var allFeatures = features.Concat(roadSegmentFeatures).ToList();

return new GetMapCurrentStatusResponse
{
    ...
    Data = new GeoJsonFeatureCollection
    {
        Type = "FeatureCollection",
        Features = allFeatures,
        Metadata = new
        {
            totalStations = features.Count,
            roadSegments = roadSegmentFeatures.Count,
            stationsWithData = features.Count(f => ((dynamic)f.Properties!).waterLevel != null),
            stationsNoData = features.Count(f => ((dynamic)f.Properties!).waterLevel == null),
            generatedAt = DateTime.UtcNow,
            ...
        }
    }
};
```

**D. Add helper `OrderStationsByProximity`**:
```csharp
private List<StationFloodStatus> OrderStationsByProximity(List<StationFloodStatus> stations)
{
    var remaining = stations.ToList();
    var ordered = new List<StationFloodStatus>();
    var current = remaining.MinBy(s => s.Longitude);
    remaining.Remove(current!);
    ordered.Add(current!);

    while (remaining.Count > 0)
    {
        var next = remaining.MinBy(s =>
            Math.Pow((double)(s.Latitude! - current!.Latitude!), 2) +
            Math.Pow((double)(s.Longitude! - current!.Longitude!), 2));
        remaining.Remove(next!);
        ordered.Add(next!);
        current = next;
    }
    return ordered;
}
```

---

## Impact on Existing Code

| File | Uses `GeoJsonGeometry.Coordinates` directly? | Impact |
|------|----------------------------------------------|--------|
| `RouteFloodAnalyzer.cs` | YES — `.Length`, `[i]` indexer | **NOT touched**, `GeoJsonGeometry` class unchanged |
| `GraphHopperRouteResponse.cs` | YES — assigns `decimal[]` to `Coordinates` | **NOT touched** |
| `SafeRouteMapper.cs` | Builds anonymous object (not `GeoJsonGeometry`) | Not affected |
| `GetMapCurrentStatusHandler.cs` | Only assigns, never reads typed | Unaffected — `Geometry = new GeoJsonGeometry { ... }` still works when type is `object` |

**Only breaking change to existing class**: `GeoJsonFeature.Geometry` type from `GeoJsonGeometry` → `object`. Safe because it's never read with typed access in the codebase — only assigned and serialized to JSON.

---

## Response Structure After Change

```json
{
  "type": "FeatureCollection",
  "features": [
    {
      "type": "Feature",
      "geometry": { "type": "Point", "coordinates": [106.7, 10.8] },
      "properties": { "stationId": "...", "markerColor": "#10B981", ... }
    },
    {
      "type": "Feature",
      "geometry": {
        "type": "LineString",
        "coordinates": [[106.7, 10.8], [106.71, 10.81]]
      },
      "properties": {
        "roadName": "Nguyễn Văn Linh",
        "startStationId": "...",
        "endStationId": "...",
        "startSeverityLevel": 0,
        "endSeverityLevel": 3,
        "startColor": "#10B981",
        "endColor": "#DC2626"
      }
    }
  ],
  "metadata": {
    "totalStations": 5,
    "roadSegments": 3,
    "stationsWithData": 4,
    "stationsNoData": 1,
    "generatedAt": "..."
  }
}
```

---

## FE Usage (Mapbox)

```js
map.addSource('flood-map', {
  type: 'geojson',
  data: featureCollection,
  lineMetrics: true  // Required for line-gradient
});

// Road gradient layer (renders BEFORE station points)
map.addLayer({
  id: 'road-gradient',
  type: 'line',
  source: 'flood-map',
  filter: ['==', '$type', 'LineString'],
  paint: {
    'line-width': 5,
    'line-gradient': [
      'interpolate', ['linear'], ['line-progress'],
      0, ['get', 'startColor'],
      1, ['get', 'endColor']
    ]
  }
});

// Station circle layer
map.addLayer({
  id: 'stations',
  type: 'circle',
  source: 'flood-map',
  filter: ['==', '$type', 'Point'],
  paint: {
    'circle-color': ['get', 'markerColor'],
    'circle-radius': 8
  }
});
```

> **Note**: `line-gradient` with `['get', 'startColor']` requires Mapbox GL JS v2.6+.

---

## Severity Color Reference

| Level | Severity | Color  | Hex       |
|-------|----------|--------|-----------|
| 3     | Critical | Red    | `#DC2626` |
| 2     | Warning  | Orange | `#F97316` |
| 1     | Caution  | Yellow | `#FCD34D` |
| 0     | Safe     | Green  | `#10B981` |
| -1    | No data  | Gray   | `#9CA3AF` |

---

## Verification

1. Build project → no compile errors
2. Call `GET /api/v1/map/current-status`
3. Verify `features` contains both `Point` and `LineString` entries
4. Verify `metadata.roadSegments` count is correct
5. Edge cases: single station on road → no segment; no `RoadName` → no segment
