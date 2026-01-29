# FE-08 – View Flood Conditions by Sensor Stations

> **Feature Name**: View Flood Conditions by Sensor Stations  
> **Goal**: Cho phép người dùng xem nhanh tình trạng ngập lụt theo từng trạm cảm biến (sensor station)  
> **Scope**: Backend API (FastEndpoints + CQRS) + phục vụ Web/Mobile map view  
> **Related Domain**: `stations`, `sensor_readings`, `sensor_daily_agg`, `flood_statistics`, `alerts`

---

## 1. Bối cảnh & Vấn đề cần giải quyết

Hiện tại hệ thống đã có:

- Quản lý trạm đo (`stations`) – tạo/sửa/xoá, xem chi tiết (FeatG23–27)
- Lưu trữ dữ liệu đo (`sensor_readings`, `sensor_daily_agg`, `water_levels`)
- Các feature phân tích nâng cao như:
  - FE-15 – View Flood Alert History
  - FE-16 – View Flood History & Trends
  - FE-17 – Analytics Aggregation
  - FE-234 – Flood Evaluation Per Monitored Area

**Thiếu**: 1 API đơn giản, tối ưu cho **màn hình “Danh sách trạm + tình trạng ngập hiện tại”** để:

- Người dùng (citizen / authority) mở app là thấy ngay **mức nước + mức độ nguy cơ** tại mỗi trạm.
- Có thể lọc theo:
  - Khu vực (bounding box map)
  - Mức độ nguy cơ (safe / caution / warning / critical)
  - Trạng thái trạm (active / offline / maintenance)
- Tối ưu cho map overlay (markers màu theo severity).

FE-08 sẽ tập trung vào **luồng đọc (Query)**, không chỉnh sửa dữ liệu, tận dụng dữ liệu đã được thu thập/bổ sung bởi các feature và job khác.

---

## 2. Mục tiêu nghiệp vụ

- **Mục tiêu chính**: 
  - Cho phép người dùng xem **tình trạng ngập hiện tại** theo từng trạm cảm biến trên bản đồ hoặc dạng danh sách.

- **Use cases chính**:
  1. Người dùng mở màn hình “Stations Flood Conditions”:
     - Thấy danh sách trạm + mức nước hiện tại + mức độ nguy cơ.
  2. Người dùng phóng to/thu nhỏ bản đồ:
     - Gửi bounding box lên backend để chỉ lấy trạm nằm trong viewport.
  3. Người dùng lọc theo mức độ nguy cơ:
     - Chỉ hiển thị trạm `warning` / `critical`.

---

## 3. Nguồn dữ liệu & Quy tắc nghiệp vụ

### 3.1 Bảng dữ liệu liên quan

- `stations`
  - Thông tin vị trí, code, status, last_seen_at…
- `sensor_readings`
  - Dữ liệu đo tức thời từ sensor (water level, rain…)
- `sensor_daily_agg`
  - Tổng hợp theo ngày (`max_level`, `min_level`, `avg_level`, `rainfall_total`)
- `flood_statistics`
  - Thống kê chiều sâu ngập, số giờ ngập theo ngày/trạm
- `alert_rules` + `alerts`
  - Quy tắc cảnh báo & logs cảnh báo (có thể dùng để hiển thị thêm context).

### 3.2 Định nghĩa “Flood Condition by Station”

Đối với mỗi trạm, backend trả về một `StationFloodCondition` bao gồm:

- Thông tin trạm:
  - `stationId`, `code`, `name`, `roadName`, `direction`
  - `latitude`, `longitude`
  - `stationStatus` (active / offline / maintenance)
- Dữ liệu “hiện tại”:
  - `latestMeasuredAt` – timestamp đo mới nhất (từ `sensor_readings` hoặc `water_levels`)
  - `waterLevel` – mực nước (đã hiệu chỉnh)
  - `unit` – m đơn vị (m/cm tuỳ chuẩn hệ thống)
- Thông tin đánh giá ngập:
  - `severity` – `safe` | `caution` | `warning` | `critical`
  - `severityScore` – thang 0–3 (0: safe, 3: critical)
  - `lastAlertAt` – thời điểm alert gần nhất (nếu có)

**Gợi ý tính severity (phiên bản đơn giản V1)**:

- Dựa vào mực nước hiện tại so với ngưỡng (được cấu hình từ:
  - `alert_rules` theo từng trạm, hoặc
  - cấu hình global trong Application layer).
- Logic ví dụ:

- `waterLevel <= 0` → `safe` (0)
- `0 < waterLevel < T_warning` → `caution` (1)
- `T_warning <= waterLevel < T_critical` → `warning` (2)
- `waterLevel >= T_critical` → `critical` (3)

Chi tiết logic threshold có thể được refine sau bởi các feature analytics (FE-17, FE-234), nhưng FE-08 chỉ **consume** kết quả/threshold, không chịu trách nhiệm training hoặc tính toán phức tạp.

---

## 4. API Specification (đề xuất)

FE-08 chủ yếu là **read-only APIs** phục vụ UI map/list.

### 4.1 Endpoint 1 – List Station Flood Conditions

- **Method**: `GET`
- **Route**: `/api/v1/stations/conditions`
- **Auth**:
  - `AllowAnonymous()` cho public data
  - Hoặc `Policies("User")` nếu muốn giới hạn cho user đã login

#### Query Params

- `bounds` *(optional)*: `"minLat,minLng,maxLat,maxLng"`
- `severity` *(optional)*: `safe|caution|warning|critical` (có thể multi-value)
- `status` *(optional)*: `active|offline|maintenance`
- `page` *(optional)*: số trang (default: 1)
- `size` *(optional)*: số record/trang (default: 100, max: 500)

#### Response 200 OK (ví dụ)

```json
{
  "success": true,
  "message": "Station flood conditions retrieved successfully",
  "statusCode": 200,
  "data": {
    "total": 2,
    "items": [
      {
        "stationId": "11111111-1111-1111-1111-111111111111",
        "code": "ST_DN_01",
        "name": "Cau Nguyen Hue",
        "roadName": "Nguyen Hue",
        "direction": "road section",
        "latitude": 10.762622,
        "longitude": 106.660172,
        "stationStatus": "active",
        "latestMeasuredAt": "2026-01-29T10:30:00Z",
        "waterLevel": 2.5,
        "unit": "m",
        "severity": "warning",
        "severityScore": 2,
        "lastAlertAt": "2026-01-29T10:25:00Z"
      },
      {
        "stationId": "22222222-2222-2222-2222-222222222222",
        "code": "ST_DN_02",
        "name": "Cau Tran Hung Dao",
        "roadName": "Tran Hung Dao",
        "direction": "road section",
        "latitude": 10.772622,
        "longitude": 106.670172,
        "stationStatus": "active",
        "latestMeasuredAt": "2026-01-29T10:28:00Z",
        "waterLevel": 0.3,
        "unit": "m",
        "severity": "caution",
        "severityScore": 1,
        "lastAlertAt": null
      }
    ]
  }
}
```

### 4.2 Endpoint 2 – Station Flood Condition Detail

- **Method**: `GET`
- **Route**: `/api/v1/stations/{id}/condition`
- **Auth**: tương tự Endpoint 1

#### Response 200 OK (ví dụ)

```json
{
  "success": true,
  "message": "Station flood condition retrieved successfully",
  "statusCode": 200,
  "data": {
    "stationId": "11111111-1111-1111-1111-111111111111",
    "code": "ST_DN_01",
    "name": "Cau Nguyen Hue",
    "roadName": "Nguyen Hue",
    "direction": "road section",
    "latitude": 10.762622,
    "longitude": 106.660172,
    "stationStatus": "active",
    "latestMeasuredAt": "2026-01-29T10:30:00Z",
    "waterLevel": 2.5,
    "unit": "m",
    "severity": "warning",
    "severityScore": 2,
    "lastAlertAt": "2026-01-29T10:25:00Z",
    "recentReadings": [
      {
        "measuredAt": "2026-01-29T10:30:00Z",
        "waterLevel": 2.5,
        "qualityFlag": "ok"
      },
      {
        "measuredAt": "2026-01-29T10:25:00Z",
        "waterLevel": 2.3,
        "qualityFlag": "ok"
      }
    ]
  }
}
```

---

## 5. Thiết kế backend (Clean Architecture)

### 5.1 Application Layer (CQRS)

Đề xuất 2 feature CQRS:

- `GetStationFloodConditions` (list):
  - Request: chứa filter (bounds, severity, status, paging)
  - Response: list `StationFloodConditionDto` + pagination info
  - Handler: 
    - Lấy danh sách trạm theo bounding box + status từ `IStationRepository`
    - Lấy bản ghi `sensor_readings` mới nhất theo station (hoặc `IWaterLevelRepository`)
    - Map thành DTO + tính severity.

- `GetStationFloodConditionDetail` (detail):
  - Request: `stationId`
  - Response: 1 `StationFloodConditionDetailDto`
  - Handler:
    - Lấy `Station` từ `IStationRepository`
    - Lấy reading mới nhất + vài reading gần nhất để hiển thị trend ngắn hạn
    - Lấy alert gần nhất (nếu cần) từ `alerts`
    - Tính severity tương tự endpoint list.

### 5.2 Repositories & Entities

- **Entities** (Domain):
  - `Station`
  - `Sensor` / `WaterLevel` / `SensorReading`
  - `AlertRule`, `Alert`

- **Repositories** (Domain Interfaces):
  - `IStationRepository`
  - `IWaterLevelRepository` hoặc `ISensorReadingRepository`
  - (Optional) `IAlertRepository`

### 5.3 Presentation Layer (FastEndpoints)

- Endpoint 1: `ViewFloodConditionsByStationsEndpoint`
  - Route: `GET /api/v1/stations/conditions`
  - DTOs: `ViewFloodConditionsByStationsRequestDto`, `ViewFloodConditionsByStationsResponseDto`, `StationFloodConditionDto`
  - Inject `IMediator`

- Endpoint 2: `ViewFloodConditionDetailEndpoint`
  - Route: `GET /api/v1/stations/{id}/condition`
  - DTOs: `ViewFloodConditionDetailResponseDto`, `RecentReadingDto`

Các endpoint này sẽ:

- Parse query/path params → build Application Request
- Gọi `_mediator.Send(...)`
- Map Application Response → HTTP DTO
- Trả JSON về cho client.

---

## 6. Yêu cầu phi chức năng

- **Hiệu năng**:
  - Endpoint list cần tối ưu để load nhanh cho map (có thể lên đến vài trăm trạm)
  - Có thể:
    - Giới hạn `size` tối đa (vd: 500)
    - Dùng bounding box filter ngay tại SQL (lat/lng)
    - Cache ngắn hạn (Redis) cho khu vực nhiều người xem.

- **Độ trễ**:
  - Mục tiêu < 500ms cho request list trong điều kiện dữ liệu trung bình.

- **An toàn & quyền truy cập**:
  - Dữ liệu trạm & mực nước có thể để public (`AllowAnonymous`) nếu không nhạy cảm.
  - Nếu cần hạn chế, dùng `Policies("User")` tương tự các endpoint user-facing khác.

- **Tính mở rộng**:
  - Thiết kế DTO và Handler sao cho sau này có thể:
    - Thêm trường như `rainIntensity`, `predictionLevel`, `riskScoreAI`
    - Kết hợp với analytics từ FE-17, FE-234 mà không phá vỡ API cũ.

---

## 7. Kết luận

FE-08 cung cấp lớp API đọc đơn giản nhưng rất quan trọng cho trải nghiệm người dùng:

- Kết nối trực tiếp **sensor data → bản đồ trạm → người dùng**.
- Tái sử dụng các bảng và pattern đã có (`stations`, `sensor_readings`, CQRS + FastEndpoints).
- Là nền tảng cho các feature UI như:
  - Map overlay theo màu severity
  - Popup chi tiết trạm
  - Kết nối tới lịch sử cảnh báo/analytics (FE-15, FE-16, FE-17, FE-234).

Class Diagram và Sequence Diagram tương ứng cho FE-08 sẽ được lưu tại:

- `documents/Class.Diagram/FE08_ViewFloodConditionsBySensorStations.puml`
- `documents/Sequence.Diagram/FE08_ViewFloodConditionsBySensorStations.puml`


