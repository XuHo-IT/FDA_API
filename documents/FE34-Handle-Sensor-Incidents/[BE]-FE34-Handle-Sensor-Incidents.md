# FE-34: Handle Sensor Incidents

## A) Nghiệp vụ & Scope

**Mục tiêu:** Xử lý sự cố cảm biến (tạo/phân công/giải quyết)

**Nghiệp vụ:**
- Tạo mới entity SensorIncident (tách biệt với Alert - cảnh báo ngập)
- Incident types: hardware_fault, tampering, maintenance, offline
- Workflow: Create → Assign → Resolve
- Disable sensor khỏi map/alerts khi faulty

**Quyết định:** Tạo entity riêng SensorIncident (không mở rộng Alert).

---

## B) Flow chi tiết

### Happy Path:
```
1. System phát hiện sự cố (FE-32):
   - Station offline > 24h
   - Sensor readings bất thường

2. Admin tạo Incident:
   POST /api/v1/incidents
   → Type: "hardware_fault", "tampering", "maintenance", "offline"
   → Status: "open"

3. Admin/Manager phân công:
   PUT /api/v1/incidents/{id}/assign
   → AssignedTo: user_id
   → Status: "in_progress"

4. Người được phân công xử lý:
   PUT /api/v1/incidents/{id}/resolve
   → Resolution: "fixed", "wont_fix", "duplicate"
   → Notes: "Đã thay sensor mới"
   → Status: "resolved"

5. System re-enable station:
   → Station.Status = "active" (nếu trước đó bị disabled)
```

### Edge Cases:
- Incident trùng lặp →标记为 duplicate
- Station đã bị disable → Khi resolve cần quyết định có enable lại không
- Không có quyền → 403 Forbidden

---

## C) Data/Entities & Trạng thái

### New Entity: SensorIncident

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| StationId | Guid | FK |
| IncidentType | string | hardware_fault, tampering, maintenance, offline |
| Status | string | open, in_progress, resolved, closed |
| Priority | string | low, medium, high, critical |
| Title | string | Tiêu đề |
| Description | string | Mô tả chi tiết |
| AssignedTo | Guid? | Người được phân công |
| AssignedAt | DateTime? | Thời điểm phân công |
| ResolvedAt | DateTime? | Thời điểm giải quyết |
| Resolution | string? | fixed, wont_fix, duplicate |
| ResolutionNotes | string? | Ghi chú giải quyết |
| CreatedBy | Guid | |
| CreatedAt | DateTime | |
| UpdatedBy | Guid | |
| UpdatedAt | DateTime | |

### Incident Status Flow:
```
open → in_progress → resolved → closed
         ↓
      (wont_fix/duplicate = resolved)
```

### Station Behavior:
- Khi incident created với type "hardware_fault" hoặc "offline":
  - Station.IsIncidentActive = true (tạm disable)
  - Station không hiển thị trên map
  - Station không tạo Alert

---

## D) API Contract đề xuất

### Routes:

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/v1/incidents` | Admin | List incidents (with filters) |
| POST | `/api/v1/incidents` | Admin | Create incident |
| GET | `/api/v1/incidents/{id}` | Admin | Get incident detail |
| PUT | `/api/v1/incidents/{id}` | Admin | Update incident |
| PUT | `/api/v1/incidents/{id}/assign` | Admin | Assign to user |
| PUT | `/api/v1/incidents/{id}/resolve` | Admin/Assigned | Resolve incident |
| PUT | `/api/v1/incidents/{id}/close` | Admin | Close incident |

### Request/Response:

**POST /api/v1/incidents**
```json
// Request
{
  "stationId": "uuid",
  "incidentType": "hardware_fault",
  "priority": "high",
  "title": "Sensor không gửi dữ liệu",
  "description": "Station không phản hồi từ 48 giờ qua"
}

// Response (201 Created)
{
  "id": "uuid",
  "stationId": "uuid",
  "incidentType": "hardware_fault",
  "status": "open",
  "priority": "high",
  "title": "Sensor không gửi dữ liệu",
  "createdAt": "2026-03-03T10:00:00Z"
}
```

**PUT /api/v1/incidents/{id}/assign**
```json
// Request
{
  "assignedTo": "uuid-of-technician"
}

// Response
{
  "id": "uuid",
  "status": "in_progress",
  "assignedTo": "uuid-of-technician",
  "assignedAt": "2026-03-03T10:30:00Z"
}
```

**PUT /api/v1/incidents/{id}/resolve**
```json
// Request
{
  "resolution": "fixed",
  "resolutionNotes": "Đã thay sensor mới, hoạt động bình thường",
  "reEnableStation": true
}

// Response
{
  "id": "uuid",
  "status": "resolved",
  "resolvedAt": "2026-03-03T11:00:00Z",
  "resolution": "fixed",
  "stationReEnabled": true
}
```

---

## E) Plan triển khai

### Phase 1: Entity & Repository (FeatG120)
- [x] Create SensorIncident entity
- [x] Create IIncidentRepository interface
- [x] Add DbSet to AppDbContext
- [x] Create migration

### Phase 2: Application Layer - CQRS (FeatG121)
- [ ] CreateIncidentHandler
- [ ] GetIncidentsHandler (list with filters)
- [ ] GetIncidentByIdHandler

### Phase 3: Application Layer - CQRS (FeatG122)
- [ ] UpdateIncidentHandler
- [ ] AssignIncidentHandler
- [ ] ResolveIncidentHandler
- [ ] CloseIncidentHandler

### Phase 4: Presentation Layer - Endpoints (FeatG123)
- [ ] CreateIncidentEndpoint + DTOs
- [ ] GetIncidentsEndpoint + DTOs
- [ ] GetIncidentByIdEndpoint + DTOs

### Phase 5: Presentation Layer - Endpoints (FeatG124)
- [ ] UpdateIncidentEndpoint + DTOs
- [ ] AssignIncidentEndpoint + DTOs
- [ ] ResolveIncidentEndpoint + DTOs
- [ ] CloseIncidentEndpoint + DTOs

### Phase 6: Integration với Station (FeatG125)
- [x] Thêm field IsIncidentActive vào Station entity
- [ ] Sửa AlertProcessingJob - skip station nếu IsIncidentActive = true
- [ ] Sửa Station map endpoint - exclude nếu IsIncidentActive = true
- [ ] Auto-create incident khi station offline > 24h (từ FE-32)

### Phase 7: Notification (FeatG126)
- [ ] Gửi notification cho người được assign

---

## F) Test Plan

### Unit Tests:
| Test Case | Input | Expected |
|-----------|-------|----------|
| Status transition | open → assign → in_progress | Valid |
| Invalid transition | open → resolve | Invalid (phải qua in_progress) |
| Station disable | incident created | Station.IsIncidentActive = true |
| Station re-enable | resolve with reEnableStation=true | Station.IsIncidentActive = false |

### Integration Tests:
| Test Case | Flow | Expected |
|-----------|------|----------|
| Create incident | POST với valid data | 201 Created, station disabled |
| Assign incident | PUT /assign | 200 OK, status = in_progress |
| Resolve incident | PUT /resolve | 200 OK, status = resolved |
| List incidents | GET ?status=open | Filtered list |
| Auto-create | Station offline 25h | Incident tự động được tạo |

---

## Files to Create/Modify:

### New:
- `SensorIncident.cs` - entity ✅
- `ISensorIncidentRepository.cs` - repository interface ✅
- `PgsqlSensorIncidentRepository.cs` - repository implementation ✅
- `FDAAPI.App.FeatG121_IncidentCreate/*` (sau khi tách)
- `FDAAPI.App.FeatG122_IncidentList/*` (sau khi tách)
- `FDAAPI.App.FeatG123_IncidentUpdate/*` (sau khi tách)
- Endpoints: IncidentManagement/*

### Modify:
- `Station.cs` - thêm IsIncidentActive field ✅
- `AlertProcessingJob.cs` - skip nếu station có incident
- `MqttIngestionJob` hoặc station status job - auto-create incident khi offline lâu

### Dependencies:
- Liên quan đến: Station entity, User entity
- Tiền đề: FE-32 (Monitor Sensor Status - phát hiện offline)
