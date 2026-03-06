# FE-33: Calibrate Sensor Data

## A) Nghiệp vụ & Scope

**Mục tiêu:** Hiệu chuẩn dữ liệu cảm biến (Calibration UI + apply offset)

**Nghiệp vụ:**
- Cho phép Admin điều chỉnh offset cho sensor
- Offset được cộng vào giá trị water level khi đọc
- Lịch sử calibration để rollback nếu cần

**Quyết định:** Calibration sẽ lưu vào **Station** entity (thêm fields).

---

## B) Flow chi tiết

### Happy Path:
```
1. Admin gọi PUT /api/v1/stations/{id}/calibration
   → Cập nhật CalibrationOffset = 5cm (đo lệch 5cm)

2. Sensor đọc water_level = 100cm (ESP32 gửi về)
   → Backend tính: ActualValue = 100 + 5 = 105cm
   → Lưu SensorReading.Value = 105 (đã hiệu chuẩn)

3. Admin gọi GET /api/v1/stations/{id}/calibration
   → Xem offset hiện tại và lịch sử

4. Admin gọi POST /api/v1/stations/{id}/calibration/rollback
   → Quay về calibration trước đó
```

### Edge Cases:
- Calibration offset quá lớn (>50cm) → Cảnh báo nhưng vẫn cho phép
- Không có quyền Admin → 403 Forbidden
- Station không tồn tại → 404 Not Found

---

## C) Data/Entities & Trạng thái

### Station (mở rộng - thêm fields):
| Field | Type | Notes |
|-------|------|-------|
| CalibrationOffset | decimal? | Offset (cm) để cộng vào reading - **MỚI** |
| CalibrationFactor | decimal? | Hệ số nhân - **MỚI** |
| LastCalibratedAt | DateTimeOffset? | Lần hiệu chuẩn gần nhất - **MỚI** |
| IsCalibrated | bool | Đã hiệu chuẩn chưa - **MỚI** |

### New Entity: SensorCalibrationHistory
| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| StationId | Guid | FK |
| OldOffset | decimal? | Giá trị cũ |
| NewOffset | decimal? | Giá trị mới |
| Reason | string | Lý do hiệu chuẩn |
| CalibratedBy | Guid | Người thực hiện |
| CalibratedAt | DateTime | Thời điểm |

---

## D) API Contract đề xuất

### Routes:

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/v1/stations/{id}/calibration` | Admin | Get current calibration |
| PUT | `/api/v1/stations/{id}/calibration` | Admin | Update calibration offset |
| GET | `/api/v1/stations/{id}/calibration/history` | Admin | Get calibration history |
| POST | `/api/v1/stations/{id}/calibration/rollback` | Admin | Rollback to previous offset |

### Request/Response:

**PUT /api/v1/stations/{id}/calibration**
```json
// Request
{
  "offset": 5.0,
  "factor": 1.0,
  "reason": "Sensor bị lệch sau khi di chuyển"
}

// Response
{
  "stationId": "uuid",
  "offset": 5.0,
  "factor": 1.0,
  "lastCalibratedAt": "2026-03-03T10:00:00Z",
  "calibratedBy": "uuid-admin"
}
```

**GET /api/v1/stations/{id}/calibration/history**
```json
{
  "items": [
    {
      "id": "uuid",
      "oldOffset": 0,
      "newOffset": 5.0,
      "reason": "Sensor bị lệch",
      "calibratedAt": "2026-03-03T10:00:00Z",
      "calibratedBy": "uuid-admin"
    }
  ]
}
```

---

## E) Plan triển khai

### Phase 1: Entity & Database (FeatG115)
- [ ] Thêm fields vào Station entity: CalibrationOffset, CalibrationFactor, LastCalibratedAt, IsCalibrated
- [ ] Tạo entity SensorCalibrationHistory
- [ ] Create migration

### Phase 2: Application Layer - CQRS (FeatG116)
- [ ] UpdateStationCalibrationHandler - cập nhật offset
- [ ] GetStationCalibrationHandler - lấy calibration hiện tại
- [ ] GetStationCalibrationHistoryHandler - lấy lịch sử

### Phase 3: Application Layer - CQRS (FeatG117)
- [ ] RollbackStationCalibrationHandler - rollback

### Phase 4: Presentation Layer - Endpoints (FeatG118)
- [ ] UpdateCalibrationEndpoint + DTOs
- [ ] GetCalibrationEndpoint + DTOs

### Phase 5: Presentation Layer - Endpoints (FeatG119)
- [ ] GetCalibrationHistoryEndpoint + DTOs
- [ ] RollbackCalibrationEndpoint + DTOs

### Phase 6: Integration
- [ ] Sửa CreateSensorReadingHandler - áp dụng calibration khi lưu reading:
  ```
  actualValue = rawValue * factor + offset
  ```

---

## F) Test Plan

### Unit Tests:
| Test Case | Input | Expected |
|-----------|-------|----------|
| Apply offset | value=100, offset=5 | actual=105 |
| Apply factor | value=100, factor=1.1 | actual=110 |
| Apply both | value=100, offset=5, factor=1.1 | actual=115.5 |
| No calibration | value=100, offset=null | actual=100 |

### Integration Tests:
| Test Case | Flow | Expected |
|-----------|------|----------|
| Update calibration | PUT với offset=5 | Offset updated |
| Get calibration | GET | Trả về offset hiện tại |
| Rollback | POST rollback | Offset restored to old value |
| Calibration history | GET history | List of changes |

---

## Files to Create/Modify:

### New:
- `SensorCalibrationHistory.cs` - entity
- `ISensorCalibrationHistoryRepository.cs`
- Handlers: UpdateCalibration, GetCalibration, GetHistory, Rollback
- Endpoints: các endpoint calibration

### Modify:
- `Station.cs` - thêm fields: CalibrationOffset, CalibrationFactor, LastCalibratedAt, IsCalibrated
- `CreateSensorReadingHandler.cs` - áp dụng calibration khi lưu

### Dependencies:
- Liên quan đến: Station entity, SensorReading entity
- Tiền đề: FE-32 (Monitor Sensor Status)
