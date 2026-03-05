# Task: Implement SignalR Real-time Flood Map (FE-06)

## Context

Backend dùng **SignalR** (không phải Socket.IO). Hub endpoint: `/hubs/flood-data`

Codebase đang dùng pattern Socket.IO (`socketClient.emit/on/off`).
Task này cần tạo một **SignalR client riêng** song song, KHÔNG thay thế socket client cũ.

---

## BE API cần biết

### REST API — Initial load
```
GET /api/map/current-status
Query params (optional):
  - minLat, maxLat, minLng, maxLng  → viewport bounding box
  - status = "active"               → default

Response: GeoJSON FeatureCollection
{
  "type": "FeatureCollection",
  "features": [
    {
      "type": "Feature",
      "geometry": { "type": "Point", "coordinates": [longitude, latitude] },
      "properties": {
        "stationId": "guid",
        "stationCode": "string",
        "stationName": "string",
        "locationDesc": "string",
        "roadName": "string",
        "waterLevel": number | null,   // đơn vị: cm
        "distance": number | null,
        "sensorHeight": number | null,
        "unit": "cm",
        "measuredAt": "ISO8601 string | null",
        "severity": "safe" | "caution" | "warning" | "critical" | "unknown",
        "severityLevel": 0 | 1 | 2 | 3 | -1,
        "markerColor": "#hex",
        "alertLevel": "SAFE" | "CAUTION" | "WARNING" | "CRITICAL" | "NO DATA",
        "stationStatus": "active" | "offline" | "maintenance",
        "lastSeenAt": "ISO8601 string | null"
      }
    }
  ],
  "metadata": {
    "totalStations": number,
    "stationsWithData": number,
    "stationsNoData": number,
    "generatedAt": "ISO8601 string"
  }
}
```

### SignalR Hub
```
URL: /hubs/flood-data
Package: @microsoft/signalr

Hub Methods (client → server):
  - SubscribeToStation(stationId: string)    → join group cho 1 trạm cụ thể
  - UnsubscribeFromStation(stationId: string)

Server Events (server → client):
  - "ReceiveSensorUpdate"  → broadcast tới TẤT CẢ clients
  - "ReceiveStationUpdate" → chỉ tới clients đã subscribe station đó

Payload (cả 2 events giống nhau):
{
  "type": "sensor_update",
  "timestamp": "ISO8601",
  "data": {
    "stationId": "guid",
    "stationCode": "string",
    "stationName": "string",
    "latitude": number,
    "longitude": number,
    "waterLevel": number,
    "distance": number,
    "sensorHeight": number,
    "unit": "cm",
    "status": number,
    "severity": "safe" | "caution" | "warning" | "critical",
    "severityLevel": 0 | 1 | 2 | 3,
    "markerColor": "#hex",
    "alertLevel": "SAFE" | "CAUTION" | "WARNING" | "CRITICAL",
    "measuredAt": "ISO8601"
  }
}
```

---

## Severity/Color mapping (để render map)

| severityLevel | severity   | markerColor | alertLevel |
|---------------|------------|-------------|------------|
| -1            | unknown    | #9CA3AF     | NO DATA    |
| 0             | safe       | #10B981     | SAFE       |
| 1             | caution    | #FCD34D     | CAUTION    |
| 2             | warning    | #F97316     | WARNING    |
| 3             | critical   | #DC2626     | CRITICAL   |

---

## Cấu trúc file cần tạo

Theo pattern của codebase (xem websocket.md), tạo các file sau:

```
src/
├── models/
│   └── flood-map/
│       └── entity/
│           ├── enum/
│           │   └── flood-severity.enum.ts          [1]
│           └── flood-station.entity.ts             [2]
│
├── lib/
│   └── signalr-client.ts                           [3]  ← SignalR connection singleton
│
├── services/
│   └── gateways/
│       └── flood-map-gateway.service.ts            [4]  ← Listen functions only (no emit for this feature)
│
├── hooks/
│   └── sockets/
│       └── flood-map.socket.ts                     [5]  ← useGetSensorUpdateSocket
│
└── constants/
    └── query-keys.ts                               [6]  ← thêm floodMap key
```

---

## [1] `models/flood-map/entity/enum/flood-severity.enum.ts`

```ts
export enum EFloodSeverity {
  UNKNOWN = "unknown",
  SAFE = "safe",
  CAUTION = "caution",
  WARNING = "warning",
  CRITICAL = "critical",
}

export enum EFloodSeverityLevel {
  NO_DATA = -1,
  SAFE = 0,
  CAUTION = 1,
  WARNING = 2,
  CRITICAL = 3,
}

export const SEVERITY_COLOR: Record<EFloodSeverityLevel, string> = {
  [EFloodSeverityLevel.NO_DATA]: "#9CA3AF",
  [EFloodSeverityLevel.SAFE]: "#10B981",
  [EFloodSeverityLevel.CAUTION]: "#FCD34D",
  [EFloodSeverityLevel.WARNING]: "#F97316",
  [EFloodSeverityLevel.CRITICAL]: "#DC2626",
};
```

---

## [2] `models/flood-map/entity/flood-station.entity.ts`

```ts
import { EFloodSeverity, EFloodSeverityLevel } from "./enum/flood-severity.enum";

export interface FloodStationProperties {
  stationId: string;
  stationCode: string;
  stationName: string;
  locationDesc: string;
  roadName: string;
  waterLevel: number | null;
  distance: number | null;
  sensorHeight: number | null;
  unit: string;
  measuredAt: string | null;
  severity: EFloodSeverity;
  severityLevel: EFloodSeverityLevel;
  markerColor: string;
  alertLevel: string;
  stationStatus: "active" | "offline" | "maintenance";
  lastSeenAt: string | null;
}

export interface FloodStationFeature {
  type: "Feature";
  geometry: {
    type: "Point";
    coordinates: [number, number]; // [longitude, latitude]
  };
  properties: FloodStationProperties;
}

export interface FloodMapFeatureCollection {
  type: "FeatureCollection";
  features: FloodStationFeature[];
  metadata: {
    totalStations: number;
    stationsWithData: number;
    stationsNoData: number;
    generatedAt: string;
  };
}

// SignalR realtime update payload
export interface SensorUpdatePayload {
  type: "sensor_update";
  timestamp: string;
  data: {
    stationId: string;
    stationCode: string;
    stationName: string;
    latitude: number;
    longitude: number;
    waterLevel: number;
    distance: number;
    sensorHeight: number;
    unit: string;
    status: number;
    severity: EFloodSeverity;
    severityLevel: EFloodSeverityLevel;
    markerColor: string;
    alertLevel: string;
    measuredAt: string;
  };
}

// Viewport bounding box for initial load
export interface MapViewportBounds {
  minLat: number;
  maxLat: number;
  minLng: number;
  maxLng: number;
}
```

---

## [3] `lib/signalr-client.ts`

**QUAN TRỌNG**: File này khác hoàn toàn với `socket-client.ts` (Socket.IO).
Dùng `@microsoft/signalr`, KHÔNG dùng `socket.io-client`.

```ts
import * as signalR from "@microsoft/signalr";

const SIGNALR_HUB_URL = process.env.NEXT_PUBLIC_API_URL + "/hubs/flood-data";
// Mobile (Expo): const SIGNALR_HUB_URL = process.env.EXPO_PUBLIC_API_URL + "/hubs/flood-data";

let connection: signalR.HubConnection | null = null;

export const getSignalRConnection = (): signalR.HubConnection => {
  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(SIGNALR_HUB_URL)
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // retry intervals ms
      .configureLogging(signalR.LogLevel.Warning)
      .build();
  }
  return connection;
};

export const startSignalRConnection = async (): Promise<void> => {
  const conn = getSignalRConnection();
  if (conn.state === signalR.HubConnectionState.Disconnected) {
    await conn.start();
  }
};

export const stopSignalRConnection = async (): Promise<void> => {
  if (connection && connection.state !== signalR.HubConnectionState.Disconnected) {
    await connection.stop();
  }
};
```

---

## [4] `services/gateways/flood-map-gateway.service.ts`

```ts
import { getSignalRConnection, startSignalRConnection } from "@/lib/signalr-client";
import { SensorUpdatePayload } from "@/models/flood-map/entity/flood-station.entity";

export const FLOOD_MAP_EVENTS = {
  RECEIVE_SENSOR_UPDATE: "ReceiveSensorUpdate",
  RECEIVE_STATION_UPDATE: "ReceiveStationUpdate",
} as const;

// ─── Listen functions ────────────────────────────────────────────────────────

export const onSensorUpdate = (
  callback: (payload: SensorUpdatePayload) => void,
) => {
  const conn = getSignalRConnection();
  conn.on(FLOOD_MAP_EVENTS.RECEIVE_SENSOR_UPDATE, callback);
  return () => conn.off(FLOOD_MAP_EVENTS.RECEIVE_SENSOR_UPDATE, callback);
};

export const onStationUpdate = (
  callback: (payload: SensorUpdatePayload) => void,
) => {
  const conn = getSignalRConnection();
  conn.on(FLOOD_MAP_EVENTS.RECEIVE_STATION_UPDATE, callback);
  return () => conn.off(FLOOD_MAP_EVENTS.RECEIVE_STATION_UPDATE, callback);
};

// ─── Subscribe actions (client → server) ────────────────────────────────────

export const subscribeToStation = async (stationId: string): Promise<void> => {
  await startSignalRConnection();
  const conn = getSignalRConnection();
  await conn.invoke("SubscribeToStation", stationId);
};

export const unsubscribeFromStation = async (stationId: string): Promise<void> => {
  const conn = getSignalRConnection();
  if (conn.state === "Connected") {
    await conn.invoke("UnsubscribeFromStation", stationId);
  }
};
```

---

## [5] `hooks/sockets/flood-map.socket.ts`

Dùng lại `useSocketSubscription` từ `hooks/sockets/common/use-socket-subscription.ts`.

```ts
import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";

import { QUERY_KEYS } from "@/constants/query-keys";
import { useSocketSubscription } from "@/hooks/sockets/common/use-socket-subscription";
import {
  FloodMapFeatureCollection,
  SensorUpdatePayload,
} from "@/models/flood-map/entity/flood-station.entity";
import { ListenEventParams } from "@/types/socket";
import {
  onSensorUpdate,
  startSignalRConnection,
  stopSignalRConnection,
} from "@/services/gateways/flood-map-gateway.service";

// ─── Connection lifecycle hook ────────────────────────────────────────────────

export const useFloodMapSignalR = () => {
  useEffect(() => {
    startSignalRConnection().catch(console.error);
    return () => {
      stopSignalRConnection().catch(console.error);
    };
  }, []);
};

// ─── Listen hook — auto update React Query cache ──────────────────────────────

export const useFloodMapSensorUpdateSocket = (
  params?: Omit<ListenEventParams<SensorUpdatePayload>, "callback">,
) => {
  const queryClient = useQueryClient();

  useSocketSubscription({
    ...params,
    subscription: onSensorUpdate,
    callback: (payload: SensorUpdatePayload) => {
      // Update cache: tìm feature trong FeatureCollection và update properties
      queryClient.setQueriesData<FloodMapFeatureCollection>(
        { queryKey: [QUERY_KEYS.floodMap.currentStatus()] },
        (old) => {
          if (!old) return old;
          return {
            ...old,
            features: old.features.map((feature) => {
              if (feature.properties.stationId !== payload.data.stationId) {
                return feature;
              }
              return {
                ...feature,
                properties: {
                  ...feature.properties,
                  waterLevel: payload.data.waterLevel,
                  severity: payload.data.severity,
                  severityLevel: payload.data.severityLevel,
                  markerColor: payload.data.markerColor,
                  alertLevel: payload.data.alertLevel,
                  measuredAt: payload.data.measuredAt,
                },
              };
            }),
          };
        },
      );
    },
  });
};

// ─── Custom callback hook (nếu component muốn tự xử lý) ──────────────────────

export const useGetSensorUpdateSocket = ({
  callback,
  ...params
}: ListenEventParams<SensorUpdatePayload>) => {
  useSocketSubscription({
    ...params,
    subscription: onSensorUpdate,
    callback,
  });
};
```

---

## [6] `constants/query-keys.ts` — thêm vào QUERY_KEYS

```ts
floodMap: {
  all: () => ["floodMap"],
  currentStatus: (bounds?: { minLat: number; maxLat: number; minLng: number; maxLng: number }) =>
    bounds
      ? [...QUERY_KEYS.floodMap.all(), "currentStatus", bounds]
      : [...QUERY_KEYS.floodMap.all(), "currentStatus"],
},
```

---

## REST API hook (useQuery)

Tạo file `hooks/queries/flood-map.query.ts`:

```ts
import { useQuery } from "@tanstack/react-query";
import { QUERY_KEYS } from "@/constants/query-keys";
import { FloodMapFeatureCollection, MapViewportBounds } from "@/models/flood-map/entity/flood-station.entity";

const fetchFloodMapStatus = async (
  bounds?: MapViewportBounds,
): Promise<FloodMapFeatureCollection> => {
  const params = new URLSearchParams({ status: "active" });
  if (bounds) {
    params.set("minLat", String(bounds.minLat));
    params.set("maxLat", String(bounds.maxLat));
    params.set("minLng", String(bounds.minLng));
    params.set("maxLng", String(bounds.maxLng));
  }
  const res = await fetch(`/api/map/current-status?${params}`);
  if (!res.ok) throw new Error("Failed to fetch flood map status");
  const json = await res.json();
  return json.data;
};

export const useFloodMapStatus = (bounds?: MapViewportBounds) => {
  return useQuery({
    queryKey: QUERY_KEYS.floodMap.currentStatus(bounds),
    queryFn: () => fetchFloodMapStatus(bounds),
    staleTime: 30_000, // 30s — vì realtime update qua SignalR
    refetchOnWindowFocus: false,
  });
};
```

---

## Sử dụng trong component

```tsx
// FloodMapScreen.tsx (Web hoặc Mobile đều dùng cùng pattern)

export const FloodMapScreen = () => {
  const [bounds, setBounds] = useState<MapViewportBounds | undefined>();

  // 1. Kết nối SignalR khi mount
  useFloodMapSignalR();

  // 2. Load initial data từ REST API
  const { data: geoJson, isLoading } = useFloodMapStatus(bounds);

  // 3. Lắng nghe realtime updates — tự update React Query cache
  useFloodMapSensorUpdateSocket();

  // 4. Khi user pan/zoom map → update bounds (debounce 400ms)
  const handleViewportChange = useDebouncedCallback((newBounds: MapViewportBounds) => {
    setBounds(newBounds);
  }, 400);

  // 5. Render map với geoJson.features
  // ...
};
```

---

## Lưu ý quan trọng

### Cả Web và Mobile

1. **Install package**: `npm install @microsoft/signalr`
   (React Native cũng dùng cùng package này — không cần package khác)

2. **signalr-client.ts là singleton** — chỉ tạo 1 connection duy nhất cho toàn app.

3. **useFloodMapSignalR** phải được gọi ở component cha (Screen/Page level), không gọi nhiều lần.

4. **useSocketSubscription** tái sử dụng từ `hooks/sockets/common/use-socket-subscription.ts` — KHÔNG tạo mới.

### Chỉ Mobile (React Native / Expo)

5. Thay `process.env.NEXT_PUBLIC_API_URL` → `process.env.EXPO_PUBLIC_API_URL`

6. React Native **không có** `window` object. SignalR package v7+ hỗ trợ RN native. Nếu gặp lỗi, thêm polyfill:
   ```ts
   // Thêm vào đầu signalr-client.ts trước import signalR
   global.XMLHttpRequest = global.originalXMLHttpRequest ?? global.XMLHttpRequest;
   ```

7. Khi app vào background (AppState = 'background'), nên `stopSignalRConnection()` và reconnect khi foreground lại để tiết kiệm battery:
   ```ts
   // Trong signalr-client.ts hoặc hook riêng
   import { AppState } from "react-native";

   AppState.addEventListener("change", (state) => {
     if (state === "background") stopSignalRConnection();
     if (state === "active") startSignalRConnection();
   });
   ```

### Chỉ Web (Next.js)

8. Nếu dùng Server Components: `signalr-client.ts` phải là `"use client"` module.

9. Với Mapbox Web: dùng `geoJson.features` làm source data, update bằng `map.getSource('stations').setData(geoJson)` khi cache thay đổi.
