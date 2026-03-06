# FE Integration Guide: Road Segment Gradient

## API Response Structure

`GET /api/v1/map/current-status` giờ trả về **2 loại feature** trong cùng một FeatureCollection:

```json
{
  "type": "FeatureCollection",
  "features": [
    // --- LOẠI 1: Point (station markers) ---
    {
      "type": "Feature",
      "geometry": {
        "type": "Point",
        "coordinates": [106.7123, 10.8456]
      },
      "properties": {
        "stationId": "...",
        "stationCode": "ST-01",
        "stationName": "Trạm Nguyễn Văn Linh",
        "roadName": "Nguyễn Văn Linh",
        "waterLevel": 45.2,
        "severity": "critical",
        "severityLevel": 3,
        "markerColor": "#DC2626",
        "alertLevel": "CRITICAL",
        "measuredAt": "2026-03-06T10:00:00Z"
      }
    },

    // --- LOẠI 2: LineString (road segments) ---
    {
      "type": "Feature",
      "geometry": {
        "type": "LineString",
        "coordinates": [
          [106.7123, 10.8456],
          [106.7234, 10.8567]
        ]
      },
      "properties": {
        "roadName": "Nguyễn Văn Linh",
        "startStationId": "...",
        "endStationId": "...",
        "startSeverityLevel": 3,
        "endSeverityLevel": 0,
        "startColor": "#DC2626",
        "endColor": "#10B981"
      }
    }
  ],
  "metadata": {
    "totalStations": 5,
    "roadSegments": 3,
    "stationsWithData": 4,
    "stationsNoData": 1,
    "generatedAt": "2026-03-06T10:00:00Z"
  }
}
```

---

## Severity Color Reference

| severityLevel | severity   | color     | hex       |
|---------------|------------|-----------|-----------|
| 3             | critical   | Red       | `#DC2626` |
| 2             | warning    | Orange    | `#F97316` |
| 1             | caution    | Yellow    | `#FCD34D` |
| 0             | safe       | Green     | `#10B981` |
| -1            | unknown    | Gray      | `#9CA3AF` |

---

## Mapbox Implementation

### Bước 1: Add source với `lineMetrics: true`

`lineMetrics: true` là **bắt buộc** để `line-gradient` hoạt động.

```js
map.addSource('flood-map', {
  type: 'geojson',
  data: apiResponse,       // GeoJsonFeatureCollection từ API
  lineMetrics: true        // BẮT BUỘC cho line-gradient
});
```

### Bước 2: Add layer road gradient (thêm TRƯỚC station layer)

```js
map.addLayer({
  id: 'road-gradient-layer',
  type: 'line',
  source: 'flood-map',
  filter: ['==', ['geometry-type'], 'LineString'],
  layout: {
    'line-cap': 'round',
    'line-join': 'round'
  },
  paint: {
    'line-width': 6,
    'line-gradient': [
      'interpolate', ['linear'],
      ['line-progress'],   // 0.0 tại điểm đầu → 1.0 tại điểm cuối
      0, ['get', 'startColor'],
      1, ['get', 'endColor']
    ]
  }
});
```

### Bước 3: Add layer station markers (thêm SAU để hiện lên trên đường)

```js
map.addLayer({
  id: 'station-layer',
  type: 'circle',
  source: 'flood-map',
  filter: ['==', ['geometry-type'], 'Point'],
  paint: {
    'circle-radius': [
      'interpolate', ['linear'], ['zoom'],
      10, 6,
      15, 10
    ],
    'circle-color': ['get', 'markerColor'],
    'circle-stroke-width': 2,
    'circle-stroke-color': '#ffffff'
  }
});
```

---

## Behavior theo các case

### Case 1: 2 trạm cùng đường, khác severity
```
A (critical, đỏ) ──gradient──► B (safe, xanh)
```
→ LineString gradient đỏ → xanh

### Case 2: 2 trạm cùng đường, cùng severity
```
A (critical, đỏ) ──────────► B (critical, đỏ)
```
→ LineString màu đỏ đồng nhất (startColor = endColor)

### Case 3: 1 trạm có data, 1 trạm no data
```
A (critical, đỏ) ──gradient──► B (no data, gray)
```
→ LineString gradient đỏ → gray

### Case 4: Chỉ có 1 trạm trên đường
```
                  A (critical)
```
→ Không có LineString, chỉ có Point marker

### Case 5: Đoạn đường không có trạm
→ Không render gì (không tô màu)

---

## Update source khi có data mới (SignalR / polling)

```js
// Khi nhận update từ SignalR hoặc re-fetch API
async function refreshFloodMap() {
  const response = await fetch('/api/v1/map/current-status');
  const data = await response.json();

  const source = map.getSource('flood-map');
  if (source) {
    source.setData(data.data);  // data.data là GeoJsonFeatureCollection
  }
}
```

---

## Lưu ý quan trọng

1. **`lineMetrics: true` phải được set khi `addSource`**, không thể update sau — nếu quên phải remove source và add lại.

2. **Thứ tự layer**: road gradient layer phải được add **trước** station layer để station marker hiện lên trên đường.

3. **Filter geometry type**: dùng `['==', ['geometry-type'], 'LineString']` và `['==', ['geometry-type'], 'Point']` để tách 2 loại feature từ cùng 1 source.

4. **Mapbox GL JS version**: `line-gradient` với `['get', 'startColor']` yêu cầu **v2.6+**. Với version cũ hơn, dùng hardcoded color thay vì `['get', 'startColor']`.
