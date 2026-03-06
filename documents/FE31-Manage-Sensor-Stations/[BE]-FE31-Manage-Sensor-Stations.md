# FE-31: Manage Station Components

## A) Nghiệp vụ & Scope

**Mục tiêu:** CRUD các thành phần/phần cứng trong một trạm đo (station)

**Nghiệp vụ:**
- Mỗi station có nhiều component: ESP32 (MCU), SRT04 (sensor siêu âm đo nước), sensor nhiệt độ, pin (battery), loa (speaker), chip 4G/GSM...
- Quản lý thông tin chi tiết của từng component
- Track component status và health

---

## B) Data/Entities

### Entity: StationComponent

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| StationId | Guid | FK |
| ComponentType | string? | esp32, srt04, temperature_sensor, battery, speaker, gsm_module, solar_panel... |
| Name | string? | Tên component |
| Model | string? | Model |
| SerialNumber | string? | Serial number |
| FirmwareVersion | string? | Phiên bản firmware |
| Status | string? | active, inactive, faulty |
| InstalledAt | DateTimeOffset? | Ngày lắp đặt |
| LastMaintenanceAt | DateTimeOffset? | Lần bảo trì gần nhất |
| Notes | string? | Ghi chú |

### Component Types:
- `esp32` - ESP32 microcontroller
- `srt04` - SRT04 ultrasonic sensor
- `temperature_sensor` - Temperature sensor
- `battery` - Battery/power supply
- `speaker` - Speaker/buzzer
- `gsm_module` - 4G/GSM module
- `solar_panel` - Solar panel
- `rain_sensor` - Rain sensor

---

## C) API Contract

### Routes:

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/v1/stations/{stationId}/components` | Admin | Create component |
| GET | `/api/v1/stations/{stationId}/components` | Admin/User | List all components |
| GET | `/api/v1/stations/{stationId}/components/{id}` | Admin/User | Get component detail |
| PUT | `/api/v1/stations/{stationId}/components/{id}` | Admin | Update component |
| DELETE | `/api/v1/stations/{stationId}/components/{id}` | Admin | Delete component |

---

## D) Plan triển khai

### FeatG105: StationComponentCreate - Create
- [x] Entity `StationComponent.cs`
- [x] Repository `IStationComponentRepository.cs`
- [x] DbSet in AppDbContext
- [x] Migration
- [x] CreateStationComponentHandler + Request
- [x] CreateStationComponentEndpoint + DTOs

### FeatG106: StationComponentUpdate - Update
- [x] UpdateStationComponentHandler + Request
- [x] UpdateStationComponentEndpoint + DTOs

### FeatG107: StationComponentDelete - Delete
- [x] DeleteStationComponentHandler + Request
- [x] DeleteStationComponentEndpoint + DTOs

### FeatG108: StationComponentList - List
- [x] GetStationComponentsHandler + Request
- [x] GetStationComponentsEndpoint + DTOs

### FeatG109: StationComponentGet - Get By ID
- [x] GetStationComponentByIdHandler + Request
- [x] GetStationComponentByIdEndpoint + DTOs

---

## E) File Structure (tách theo style)

```
src/Core/Application/
├── FDAAPI.App.FeatG105_StationComponentCreate/
│   ├── StationComponentCommands.cs (CreateStationComponentRequest, Handler, Response)
│   └── FDAAPI.App.FeatG105_StationComponentCreate.csproj
│
├── FDAAPI.App.FeatG106_StationComponentUpdate/
│   ├── UpdateStationComponentRequest.cs
│   ├── UpdateStationComponentHandler.cs
│   ├── UpdateStationComponentResponse.cs
│   └── FDAAPI.App.FeatG106_StationComponentUpdate.csproj
│
├── FDAAPI.App.FeatG107_StationComponentDelete/
│   ├── DeleteStationComponentRequest.cs
│   ├── DeleteStationComponentHandler.cs
│   ├── DeleteStationComponentResponse.cs
│   └── FDAAPI.App.FeatG107_StationComponentDelete.csproj
│
├── FDAAPI.App.FeatG108_StationComponentList/
│   ├── GetStationComponentsRequest.cs
│   ├── GetStationComponentsHandler.cs
│   ├── GetStationComponentsResponse.cs
│   └── FDAAPI.App.FeatG108_StationComponentList.csproj
│
└── FDAAPI.App.FeatG109_StationComponentGet/
    ├── GetStationComponentByIdRequest.cs
    ├── GetStationComponentByIdHandler.cs
    ├── GetStationComponentByIdResponse.cs
    └── FDAAPI.App.FeatG109_StationComponentGet.csproj

src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/Endpoints/
├── FeatG105_StationComponentCreate/
│   ├── CreateStationComponentEndpoint.cs
│   └── DTOs/
├── FeatG106_StationComponentUpdate/
│   ├── UpdateStationComponentEndpoint.cs
│   └── DTOs/
├── FeatG107_StationComponentDelete/
│   ├── DeleteStationComponentEndpoint.cs
│   └── DTOs/
├── FeatG108_StationComponentList/
│   ├── GetStationComponentsEndpoint.cs
│   └── DTOs/
└── FeatG109_StationComponentGet/
    ├── GetStationComponentByIdEndpoint.cs
    └── DTOs/
```

---

## F) Test Cases

| Test Case | Flow | Expected |
|-----------|------|----------|
| Create component | POST with valid data | 201 Created |
| Create duplicate type | POST with existing type | 400 Bad Request |
| Update component | PUT with new data | 200 OK |
| Delete component | DELETE existing | 200 OK |
| Get by ID | GET /{id} | 200 OK |
| List components | GET / | List of components |
