# FE-31: Quản lý Thành phần Trạm (Station Component Management)

## 1. Tổng quan

**Mục tiêu:** Cho phép người dùng quản lý các thành phần/phần cứng trong một trạm đo (station).

**Nghiệp vụ:**
- Mỗi station có nhiều component: ESP32 (MCU), SRT04 (sensor siêu âm đo nước), sensor nhiệt độ, pin (battery), loa (speaker), chip 4G/GSM...
- Quản lý thông tin chi tiết của từng component
- Theo dõi trạng thái và tình trạng hoạt động của component

---

## 2. Các API Endpoints

### Base URL: `/api/v1`

| Method | Endpoint | Mô tả | Quyền |
|--------|----------|--------|--------|
| POST | `/stations/{stationId}/components` | Tạo mới component | ADMIN |
| GET | `/stations/{stationId}/components` | Lấy danh sách component | ADMIN, USER |
| GET | `/stations/{stationId}/components/{id}` | Lấy chi tiết 1 component | ADMIN, USER |
| PUT | `/stations/{stationId}/components/{id}` | Cập nhật component | ADMIN |
| DELETE | `/stations/{stationId}/components/{id}` | Xóa component | ADMIN |

---

## 3. Chi tiết từng API

### 3.1. Tạo mới Component (Create)

**Endpoint:** `POST /api/v1/stations/{stationId}/components`

**Headers:**
```
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "componentType": "esp32",
  "name": "ESP32 Main Controller",
  "model": "ESP32-WROOM-32",
  "serialNumber": "ESP32-001234",
  "firmwareVersion": "1.0.0",
  "notes": "Main controller for sensor data collection"
}
```

**Giải thích thuộc tính:**

| Thuộc tính | Kiểu | Bắt buộc | Mô tả |
|------------|------|-----------|--------|
| `componentType` | string | **Có** | Loại component. Các giá trị hợp lệ: `esp32`, `srt04`, `temperature_sensor`, `battery`, `speaker`, `gsm_module`, `solar_panel`, `rain_sensor` |
| `name` | string | Không | Tên của component, giúp dễ nhận diện |
| `model` | string | Không | Model của thiết bị |
| `serialNumber` | string | Không | Số serial của thiết bị |
| `firmwareVersion` | string | Không | Phiên bản firmware hiện tại |
| `notes` | string | Không | Ghi chú thêm về component |

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Component created successfully",
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "component": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stationId": "660e8400-e29b-41d4-a716-446655440001",
    "componentType": "esp32",
    "name": "ESP32 Main Controller",
    "model": "ESP32-WROOM-32",
    "serialNumber": "ESP32-001234",
    "firmwareVersion": "1.0.0",
    "status": "active",
    "installedAt": "2024-01-15T10:30:00Z",
    "lastMaintenanceAt": null,
    "notes": "Main controller for sensor data collection",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  }
}
```

**Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "Component type is required",
  "id": null,
  "component": null
}
```

---

### 3.2. Lấy danh sách Component (List)

**Endpoint:** `GET /api/v1/stations/{stationId}/components`

**Headers:**
```
Authorization: Bearer {access_token}
```

**Query Parameters:** Không có (lấy tất cả)

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Components retrieved successfully",
  "components": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "stationId": "660e8400-e29b-41d4-a716-446655440001",
      "componentType": "esp32",
      "name": "ESP32 Main Controller",
      "model": "ESP32-WROOM-32",
      "serialNumber": "ESP32-001234",
      "firmwareVersion": "1.0.0",
      "status": "active",
      "installedAt": "2024-01-15T10:30:00Z",
      "lastMaintenanceAt": "2024-06-01T14:00:00Z",
      "notes": "Main controller",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-06-01T14:00:00Z"
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440002",
      "stationId": "660e8400-e29b-41d4-a716-446655440001",
      "componentType": "srt04",
      "name": "Ultrasonic Sensor",
      "model": "HC-SR04",
      "serialNumber": "SR04-005678",
      "firmwareVersion": null,
      "status": "active",
      "installedAt": "2024-01-15T10:35:00Z",
      "lastMaintenanceAt": null,
      "notes": "Water level measurement",
      "createdAt": "2024-01-15T10:35:00Z",
      "updatedAt": "2024-01-15T10:35:00Z"
    }
  ]
}
```

**Giải thích thuộc tính Response:**

| Thuộc tính | Mô tả |
|------------|-------|
| `id` | ID duy nhất của component |
| `stationId` | ID của station chứa component |
| `componentType` | Loại component |
| `name` | Tên component |
| `model` | Model thiết bị |
| `serialNumber` | Số serial |
| `firmwareVersion` | Phiên bản firmware |
| `status` | Trạng thái: `active`, `inactive`, `faulty` |
| `installedAt` | Ngày lắp đặt |
| `lastMaintenanceAt` | Ngày bảo trì gần nhất |
| `notes` | Ghi chú |
| `createdAt` | Ngày tạo |
| `updatedAt` | Ngày cập nhật cuối |

---

### 3.3. Lấy chi tiết 1 Component (Get By ID)

**Endpoint:** `GET /api/v1/stations/{stationId}/components/{id}`

**Headers:**
```
Authorization: Bearer {access_token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Component retrieved successfully",
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "component": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stationId": "660e8400-e29b-41d4-a716-446655440001",
    "componentType": "esp32",
    "name": "ESP32 Main Controller",
    "model": "ESP32-WROOM-32",
    "serialNumber": "ESP32-001234",
    "firmwareVersion": "1.0.0",
    "status": "active",
    "installedAt": "2024-01-15T10:30:00Z",
    "lastMaintenanceAt": "2024-06-01T14:00:00Z",
    "notes": "Main controller for sensor data collection",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-06-01T14:00:00Z"
  }
}
```

**Response (404 Not Found):**
```json
{
  "success": false,
  "message": "Component not found",
  "id": null,
  "component": null
}
```

---

### 3.4. Cập nhật Component (Update)

**Endpoint:** `PUT /api/v1/stations/{stationId}/components/{id}`

**Headers:**
```
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "componentType": "esp32",
  "name": "ESP32 Main Controller - Updated",
  "model": "ESP32-WROOM-32E",
  "serialNumber": "ESP32-001234",
  "firmwareVersion": "1.1.0",
  "status": "active",
  "notes": "Firmware updated to v1.1.0"
}
```

**Giải thích thuộc tính:**

| Thuộc tính | Kiểu | Bắt buộc | Mô tả |
|------------|------|-----------|--------|
| `componentType` | string | **Có** | Loại component |
| `name` | string | **Có** | Tên component |
| `model` | string | Không | Model thiết bị |
| `serialNumber` | string | Không | Số serial |
| `firmwareVersion` | string | Không | Phiên bản firmware mới |
| `status` | string | **Có** | Trạng thái: `active`, `inactive`, `faulty` |
| `notes` | string | Không | Ghi chú |

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Component updated successfully",
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "component": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stationId": "660e8400-e29b-41d4-a716-446655440001",
    "componentType": "esp32",
    "name": "ESP32 Main Controller - Updated",
    "model": "ESP32-WROOM-32E",
    "serialNumber": "ESP32-001234",
    "firmwareVersion": "1.1.0",
    "status": "active",
    "installedAt": "2024-01-15T10:30:00Z",
    "lastMaintenanceAt": "2024-06-01T14:00:00Z",
    "notes": "Firmware updated to v1.1.0",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-07-20T09:15:00Z"
  }
}
```

---

### 3.5. Xóa Component (Delete)

**Endpoint:** `DELETE /api/v1/stations/{stationId}/components/{id}`

**Headers:**
```
Authorization: Bearer {access_token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Component deleted successfully"
}
```

**Response (404 Not Found):**
```json
{
  "success": false,
  "message": "Component not found"
}
```

---

## 4. Các loại Component (Component Types)

| Giá trị | Mô tả |
|---------|-------|
| `esp32` | ESP32 microcontroller - Bộ điều khiển trung tâm |
| `srt04` | HC-SR04 ultrasonic sensor - Sensor siêu âm đo mực nước |
| `temperature_sensor` | Cảm biến nhiệt độ |
| `battery` | Pin/Power supply |
| `speaker` | Loa/Buzzer - Cảnh báo âm thanh |
| `gsm_module` | Module 4G/GSM - Truyền dữ liệu |
| `solar_panel` | Tấm năng lượng mặt trời |
| `rain_sensor` | Cảm biến mưa |

---

## 5. Các trạng thái Component (Status)

| Giá trị | Mô tả |
|---------|-------|
| `active` | Hoạt động bình thường |
| `inactive` | Không hoạt động |
| `faulty` | Bị lỗi/Cần bảo trì |

---

## 6. Flow sử dụng cho Frontend

### 6.1. Flow xem danh sách component của một station

```
1. User chọn một station từ danh sách
2. Frontend gọi GET /api/v1/stations/{stationId}/components
3. Hiển thị danh sách component với các thông tin: tên, loại, trạng thái
4. User có thể lọc theo trạng thái (active/inactive/faulty)
```

### 6.2. Flow thêm mới component

```
1. User click nút "Thêm Component"
2. Hiển thị form với các trường:
   - Chọn loại component (dropdown)
   - Nhập tên, model, serial number, firmware
   - Thêm ghi chú (nếu có)
3. Frontend gọi POST /api/v1/stations/{stationId}/components
4. Nếu thành công -> Hiển thị thông báo, reload danh sách
5. Nếu lỗi -> Hiển thị message lỗi
```

### 6.3. Flow cập nhật component

```
1. User click vào component trong danh sách
2. Hiển thị form chỉnh sửa với thông tin hiện tại
3. User sửa thông tin và click "Lưu"
4. Frontend gọi PUT /api/v1/stations/{stationId}/components/{id}
5. Cập nhật lại danh sách
```

### 6.4. Flow xóa component

```
1. User click nút xóa trên component
2. Hiển thị confirm dialog: "Bạn có chắc muốn xóa component X?"
3. User xác nhận
4. Frontend gọi DELETE /api/v1/stations/{stationId}/components/{id}
5. Nếu thành công -> Xóa khỏi danh sách, hiển thị thông báo
```

---

## 7. Ví dụ Code (Frontend)

### 7.1. Gọi API lấy danh sách component

```javascript
async function getComponents(stationId) {
  const token = localStorage.getItem('access_token');

  const response = await fetch(
    `/api/v1/stations/${stationId}/components`,
    {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    }
  );

  const data = await response.json();

  if (data.success) {
    return data.components;
  } else {
    throw new Error(data.message);
  }
}
```

### 7.2. Gọi API tạo mới component

```javascript
async function createComponent(stationId, componentData) {
  const token = localStorage.getItem('access_token');

  const response = await fetch(
    `/api/v1/stations/${stationId}/components`,
    {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(componentData)
    }
  );

  const data = await response.json();

  if (data.success) {
    return data.component;
  } else {
    throw new Error(data.message);
  }
}

// Sử dụng
const newComponent = await createComponent('station-uuid', {
  componentType: 'esp32',
  name: 'ESP32 Main Controller',
  model: 'ESP32-WROOM-32',
  serialNumber: 'ESP32-001234',
  firmwareVersion: '1.0.0'
});
```

### 7.3. Gọi API cập nhật component

```javascript
async function updateComponent(stationId, componentId, componentData) {
  const token = localStorage.getItem('access_token');

  const response = await fetch(
    `/api/v1/stations/${stationId}/components/${componentId}`,
    {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(componentData)
    }
  );

  const data = await response.json();

  if (data.success) {
    return data.component;
  } else {
    throw new Error(data.message);
  }
}
```

### 7.4. Gọi API xóa component

```javascript
async function deleteComponent(stationId, componentId) {
  const token = localStorage.getItem('access_token');

  const response = await fetch(
    `/api/v1/stations/${stationId}/components/${componentId}`,
    {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${token}`
      }
    }
  );

  const data = await response.json();

  if (!data.success) {
    throw new Error(data.message);
  }
}
```

---

## 8. Lưu ý quan trọng

1. **Authentication:** Tất cả các API (trừ GET công khai) đều cần Bearer token trong header
2. **Authorization:**
   - ADMIN: Có thể thực hiện tất cả các thao tác (CRUD)
   - USER: Chỉ có thể xem (GET)
3. **Validation:**
   - `componentType` là bắt buộc khi tạo/cập nhật
   - `name` là bắt buộc khi cập nhật
   - `status` phải là một trong các giá trị: `active`, `inactive`, `faulty`
4. **Error Handling:** Luôn kiểm tra trường `success` trong response để xem API có thành công không

---

## 9. Màn hình gợi ý cho Frontend

### 9.1. Danh sách Component
- Hiển thị bảng với các cột: Tên, Loại, Model, Serial, Trạng thái, Ngày lắp đặt
- Có nút thêm mới (chỉ Admin)
- Mỗi dòng có nút sửa, xóa (chỉ Admin)
- Click vào dòng để xem chi tiết

### 9.2. Form thêm/sửa Component
- Dropdown chọn loại component
- Input text cho các trường thông tin
- Dropdown chọn trạng thái (khi sửa)
- Nút Lưu và Hủy

### 9.3. Chi tiết Component
- Hiển thị tất cả thông tin của component
- Có nút sửa và xóa (chỉ Admin)
- Hiển thị lịch sử bảo trì (nếu có)
