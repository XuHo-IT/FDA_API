# FE-32: Monitor Sensor & Device Status

## A) Nghiệp vụ & Scope

**Mục tiêu:** Giám sát trạng thái sensor/device (heartbeat, battery, reliability)

**Nghiệp vụ:**
- Theo dõi heartbeat/last-seen của station
- Theo dõi battery level (ESP32 gửi về)
- Theo dõi signal strength (RSSI)
- Phát hiện offline devices

**Quyết định:** Battery/Signal sẽ được lưu vào **SensorReading** (không tạo entity mới). Khi cần kiểm tra battery của station, lấy SensorReading mới nhất.

**NOTE:** ESP32 sẽ gửi thêm battery/signal trong MQTT message - cần cập nhật sau.

---

## B) Flow chi tiết

### Happy Path:
```
1. ESP32 gửi MQTT message:
   {
     "station_id": "xxx",
     "water_level": 2.5,
     "distance": 100,
     "sensor_height": 300,
     "status": 0,
     "battery": 85,      // % pin
     "rssi": -45         // signal strength
   }

2. MqttIngestionJob nhận message
   → Lưu SensorReading (bao gồm battery, rssi)
   → Cập nhật Station.LastSeenAt = now

3. Background job kiểm tra offline:
   → Nếu LastSeenAt > 10 phút → Status = "offline"
   → Tạo SensorIncident nếu cần (FE-34)
```

### Edge Cases:
- Không có battery/rssi trong message → OK, vẫn xử lý water level
- Station không tồn tại → Log warning, bỏ qua
- MQTT disconnect → Job tự reconnect

---

## C) Data/Entities & Trạng thái

### SensorReading (mở rộng):
| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| StationId | Guid | FK |
| Value | double | Water level |
| Distance | double | |
| SensorHeight | double | |
| Unit | string | default: "cm" |
| Status | int | 0=normal, 1=warning, 2=critical |
| BatteryLevel | int? | Battery % (0-100) - **MỚI** |
| SignalStrength | int? | RSSI (dBm) - **MỚI** |
| MeasuredAt | DateTime | |

### Station (existing):
| Field | Type | Notes |
|-------|------|-------|
| Status | string | active, offline, maintenance |
| LastSeenAt | DateTimeOffset? | **Dùng để phát hiện offline** |

### Station Status Logic:
```
- active: LastSeenAt <= 10 phút
- offline: LastSeenAt > 10 phút
- maintenance: Được set thủ công
```

---

## D) API Contract đề xuất

### Routes:

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/v1/stations/{id}/status` | User+ | Get station device status |
| GET | `/api/v1/stations/status/online` | User+ | List online stations |
| GET | `/api/v1/stations/status/offline` | User+ | List offline stations |

### Response:

**GET /api/v1/stations/{id}/status**
```json
{
  "stationId": "uuid",
  "stationName": "Trạm Nguyễn Trãi",
  "status": "active",
  "lastSeenAt": "2026-03-03T10:30:00Z",
  "batteryLevel": 85,
  "signalStrength": -45,
  "lastReading": {
    "waterLevel": 2.5,
    "measuredAt": "2026-03-03T10:28:00Z"
  },
  "offlineDurationMinutes": null
}
```

**GET /api/v1/stations/status/offline**
```json
{
  "items": [
    {
      "stationId": "uuid",
      "stationName": "Trạm ABC",
      "lastSeenAt": "2026-03-03T08:00:00Z",
      "offlineDurationMinutes": 150
    }
  ],
  "total": 1
}
```

---

## E) Plan triển khai

### Phase 1: MQTT Message Update (FeatG110)
- [ ] Update MqttSensorDataDto - thêm BatteryLevel, SignalStrength
- [ ] Update CreateSensorReadingRequest - thêm parameters
- [ ] Update CreateSensorReadingHandler - lưu battery/signal

### Phase 2: Station Status Logic (FeatG111)
- [ ] Update MqttIngestionJob - cập nhật Station.LastSeenAt
- [ ] Tạo Background Job: CheckStationStatusJob
  - Chạy mỗi 5 phút
  - Kiểm tra LastSeenAt > 10 phút → set Status = "offline"

### Phase 3: API Endpoints (FeatG112)
- [ ] GET /stations/{id}/status - trả về device status
- [ ] GET /stations/status/online - filter Status = "active"
- [ ] GET /stations/status/offline - filter Status = "offline"

### Phase 4: Station Component Status (FeatG113)
- [ ] Track component health từ readings
- [ ] Update component status based on readings

### Phase 5: Integration với FE-34 (FeatG114)
- [ ] Khi station offline > 6h → Tạo SensorIncident (FE-34)

---

## F) Test Plan

### Unit Tests:
| Test Case | Input | Expected |
|-----------|-------|----------|
| Status calculation | LastSeenAt = now | "active" |
| Status calculation | LastSeenAt = 15 phút trước | "offline" |
| Offline duration | 2 giờ trước | 120 minutes |

### Integration Tests:
| Test Case | Flow | Expected |
|-----------|------|----------|
| MQTT with battery | Gửi message có battery | Lưu vào DB |
| Station goes offline | Không có reading 15 phút | Status = "offline" |
| Get status | GET /stations/xxx/status | Trả về battery, signal |

---

## Files to Create/Modify:

### New:
- `SensorIncidentCheckJob.cs` - Background job kiểm tra offline
- StationStatusEndpoint.cs - API endpoints

### Modify:
- `MqttSensorDataDto.cs` - thêm battery, signal
- `CreateSensorReadingRequest.cs` - thêm parameters
- `CreateSensorReadingHandler.cs` - lưu battery/signal
- `MqttIngestionJob.cs` - cập nhật Station.LastSeenAt
- `StationRepository.cs` - thêm methods lọc theo status

### Dependencies:
- Liên quan đến: Station entity, SensorReading entity
- Tiền đề cho: FE-34 (khi offline quá lâu → tạo incident)

---

## REMINDER CHO USER:
> **NOTE:** ESP32 khi bắn tín hiệu về sẽ có thêm thuộc tính battery và rssi. Bạn sẽ thêm sau. Khi đó cần cập nhật:
> 1. MQTT message format từ ESP32
> 2. MqttSensorDataDto trong backend
