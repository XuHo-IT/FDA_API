# FE-30: Quản lý Mẫu Tin nhắn Cảnh báo (Alert Template Management)

## 1. Tổng quan

**Mục tiêu:** Cho phép Admin tạo, chỉnh sửa các template tin nhắn cảnh báo ngập lụt thay vì hardcode như trước đây.

**Vấn đề hiện tại (trước FE-30):**
- Tin nhắn cảnh báo được hardcode trong code
- Không thể tùy chỉnh nội dung theo nhu cầu
- Không hỗ trợ đa ngôn ngữ (i18n)

**Giải pháp FE-30:**
1. Lưu template trong database
2. CRUD APIs để quản lý templates (Admin only)
3. Template engine hỗ trợ biến động
4. Preview template với sample data trước khi lưu

---

## 2. Các API Endpoints

### Base URL: `/api/v1/admin`

| Method | Endpoint | Mô tả | Quyền |
|--------|----------|--------|--------|
| POST | `/alert-templates` | Tạo mới template | ADMIN |
| GET | `/alert-templates` | Lấy danh sách template | ADMIN |
| GET | `/alert-templates/{id}` | Lấy chi tiết 1 template | ADMIN |
| PUT | `/alert-templates/{id}` | Cập nhật template | ADMIN |
| DELETE | `/alert-templates/{id}` | Xóa template | ADMIN |
| POST | `/alert-templates/{id}/preview` | Preview tin nhắn đã render | ADMIN |

---

## 3. Chi tiết từng API

### 3.1. Tạo mới Template (Create)

**Endpoint:** `POST /api/v1/admin/alert-templates`

**Headers:**
```
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "Critical Push Notification",
  "channel": "Push",
  "severity": "critical",
  "titleTemplate": "⚠️ Cảnh báo ngập lụt nghiêm trọng - {{station_name}}",
  "bodyTemplate": "Mức nước tại {{station_name}} đã đạt {{water_level}} lúc {{time}}. Vượt ngưỡng {{threshold}}m. Vui lòng kiểm tra ngay!",
  "isActive": true,
  "sortOrder": 1
}
```

**Giải thích thuộc tính:**

| Thuộc tính | Kiểu | Bắt buộc | Mô tả |
|------------|------|-----------|--------|
| `name` | string | **Có** | Tên template, ví dụ: "Critical Push Template" |
| `channel` | string | **Có** | Kênh gửi: `Push`, `Email`, `SMS`, `InApp` |
| `severity` | string | Không | Mức độ cảnh báo: `info`, `caution`, `warning`, `critical`. Để null nếu áp dụng cho tất cả |
| `titleTemplate` | string | **Có** | Template cho tiêu đề thông báo |
| `bodyTemplate` | string | **Có** | Template cho nội dung thông báo |
| `isActive` | boolean | Không | Template có đang hoạt động không. Default: `true` |
| `sortOrder` | integer | Không | Thứ tự hiển thị trong danh sách. Default: `0` |

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Alert template created successfully",
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "template": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Critical Push Notification",
    "channel": "Push",
    "severity": "critical",
    "titleTemplate": "⚠️ Cảnh báo ngập lụt nghiêm trọng - {{station_name}}",
    "bodyTemplate": "Mức nước tại {{station_name}} đã đạt {{water_level}} lúc {{time}}. Vượt ngưỡng {{threshold}}m. Vui lòng kiểm tra ngay!",
    "isActive": true,
    "sortOrder": 1,
    "createdBy": "660e8400-e29b-41d4-a716-446655440001",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedBy": null,
    "updatedAt": "2024-01-15T10:30:00Z"
  }
}
```

**Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "Template name is required",
  "id": null,
  "template": null
}
```

---

### 3.2. Lấy danh sách Template (List)

**Endpoint:** `GET /api/v1/admin/alert-templates`

**Headers:**
```
Authorization: Bearer {access_token}
```

**Query Parameters:** Không có (lấy tất cả)

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Alert templates retrieved successfully",
  "templates": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Critical Push Notification",
      "channel": "Push",
      "severity": "critical",
      "titleTemplate": "⚠️ Cảnh báo ngập lụt nghiêm trọng - {{station_name}}",
      "bodyTemplate": "Mức nước tại {{station_name}} đã đạt {{water_level}} lúc {{time}}.",
      "isActive": true,
      "sortOrder": 1,
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-15T10:30:00Z"
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "name": "Warning Email Template",
      "channel": "Email",
      "severity": "warning",
      "titleTemplate": "Cảnh báo ngập lụt - {{station_name}}",
      "bodyTemplate": "Kính gửi quý khách,\n\nMức nước tại {{station_name}} đã đạt {{water_level}}.",
      "isActive": true,
      "sortOrder": 2,
      "createdAt": "2024-01-14T09:00:00Z",
      "updatedAt": "2024-01-14T09:00:00Z"
    }
  ]
}
```

**Giải thích thuộc tính Response:**

| Thuộc tính | Mô tả |
|------------|-------|
| `id` | ID duy nhất của template |
| `name` | Tên template |
| `channel` | Kênh gửi: Push, Email, SMS, InApp |
| `severity` | Mức độ: info, caution, warning, critical |
| `titleTemplate` | Template tiêu đề |
| `bodyTemplate` | Template nội dung |
| `isActive` | Trạng thái hoạt động |
| `sortOrder` | Thứ tự hiển thị |
| `createdAt` | Ngày tạo |
| `updatedAt` | Ngày cập nhật cuối |

---

### 3.3. Lấy chi tiết 1 Template (Get By ID)

**Endpoint:** `GET /api/v1/admin/alert-templates/{id}`

**Headers:**
```
Authorization: Bearer {access_token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Alert template retrieved successfully",
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "template": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Critical Push Notification",
    "channel": "Push",
    "severity": "critical",
    "titleTemplate": "⚠️ Cảnh báo ngập lụt nghiêm trọng - {{station_name}}",
    "bodyTemplate": "Mức nước tại {{station_name}} đã đạt {{water_level}} lúc {{time}}. Vượt ngưỡng {{threshold}}m. Vui lòng kiểm tra ngay!",
    "isActive": true,
    "sortOrder": 1,
    "createdBy": "660e8400-e29b-41d4-a716-446655440001",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedBy": "660e8400-e29b-41d4-a716-446655440001",
    "updatedAt": "2024-01-20T14:00:00Z"
  }
}
```

**Response (404 Not Found):**
```json
{
  "success": false,
  "message": "Alert template not found",
  "id": null,
  "template": null
}
```

---

### 3.4. Cập nhật Template (Update)

**Endpoint:** `PUT /api/v1/admin/alert-templates/{id}`

**Headers:**
```
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "Critical Push Notification - Updated",
  "channel": "Push",
  "severity": "critical",
  "titleTemplate": "🚨 CẢNH BÁO NGẬP - {{station_name}}",
  "bodyTemplate": "Cảnh báo! Mức nước tại {{station_name}} đã đạt {{water_level}}m. Vượt ngưỡng {{threshold}}m lúc {{time}}.",
  "isActive": true,
  "sortOrder": 1
}
```

**Giải thích thuộc tính:**

| Thuộc tính | Kiểu | Bắt buộc | Mô tả |
|------------|------|-----------|--------|
| `name` | string | **Có** | Tên template |
| `channel` | string | **Có** | Kênh gửi |
| `severity` | string | **Có** | Mức độ (có thể null) |
| `titleTemplate` | string | **Có** | Template tiêu đề |
| `bodyTemplate` | string | **Có** | Template nội dung |
| `isActive` | boolean | **Có** | Trạng thái hoạt động |
| `sortOrder` | integer | **Có** | Thứ tự |

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Alert template updated successfully",
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "template": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Critical Push Notification - Updated",
    "channel": "Push",
    "severity": "critical",
    "titleTemplate": "🚨 CẢNH BÁO NGẬP - {{station_name}}",
    "bodyTemplate": "Cảnh báo! Mức nước tại {{station_name}} đã đạt {{water_level}}m.",
    "isActive": true,
    "sortOrder": 1,
    "createdBy": "660e8400-e29b-41d4-a716-446655440001",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedBy": "660e8400-e29b-41d4-a716-446655440001",
    "updatedAt": "2024-01-25T16:45:00Z"
  }
}
```

---

### 3.5. Xóa Template (Delete)

**Endpoint:** `DELETE /api/v1/admin/alert-templates/{id}`

**Headers:**
```
Authorization: Bearer {access_token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Alert template deleted successfully"
}
```

**Response (404 Not Found):**
```json
{
  "success": false,
  "message": "Alert template not found"
}
```

---

### 3.6. Preview Template (Preview)

**Endpoint:** `POST /api/v1/admin/alert-templates/{id}/preview`

**Headers:**
```
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "stationName": "Trạm đo Quận 1",
  "waterLevel": "3.5m",
  "waterLevelRaw": 3.5,
  "severity": "critical",
  "time": "2024-01-15 14:30:00",
  "threshold": 3.0,
  "address": "123 Đường Nguyễn Huệ, Quận 1, TP.HCM",
  "message": "Mức nước vượt ngưỡng báo động"
}
```

**Giải thích thuộc tính Preview:**

| Thuộc tính | Kiểu | Mô tả |
|------------|------|-------|
| `stationName` | string | Tên trạm để replace {{station_name}} |
| `waterLevel` | string | Mức nước + đơn vị để replace {{water_level}} |
| `waterLevelRaw` | number | Chỉ số để replace {{water_level_raw}} |
| `severity` | string | Mức độ để replace {{severity}} |
| `time` | string | Thời gian để replace {{time}} |
| `threshold` | number | Ngưỡng cảnh báo để replace {{threshold}} |
| `address` | string | Địa chỉ để replace {{address}} |
| `message` | string | Message để replace {{message}} |

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Template preview generated successfully",
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "rendered": {
    "title": "⚠️ Cảnh báo ngập lụt nghiêm trọng - Trạm đo Quận 1",
    "body": "Mức nước tại Trạm đo Quận 1 đã đạt 3.5m lúc 2024-01-15 14:30:00. Vượt ngưỡng 3.0m. Vui lòng kiểm tra ngay!"
  }
}
```

---

## 4. Các biến sử dụng trong Template (Variables)

Template hỗ trợ các biến sau để replace động:

| Biến | Mô tả | Ví dụ output |
|------|-------|--------------|
| `{{station_name}}` | Tên trạm | "Trạm đo Quận 1" |
| `{{water_level}}` | Mức nước + đơn vị | "3.5m" |
| `{{water_level_raw}}` | Chỉ số mức nước | "3.5" |
| `{{severity}}` | Mức độ cảnh báo | "critical", "warning" |
| `{{time}}` | Thời gian | "2024-01-15 14:30:00" |
| `{{threshold}}` | Ngưỡng cảnh báo | "3.0" |
| `{{address}}` | Địa chỉ trạm | "123 Nguyễn Huệ, Q1, TP.HCM" |
| `{{message}}` | Alert message | "Mức nước vượt ngưỡng" |

---

## 5. Kênh gửi (Channel)

| Giá trị | Mô tả |
|---------|-------|
| `Push` | Push notification (Firebase/FCM) |
| `Email` | Email notification |
| `SMS` | Tin nhắn SMS |
| `InApp` | Thông báo trong app |

---

## 6. Mức độ cảnh báo (Severity)

| Giá trị | Mô tả |
|---------|-------|
| `info` | Thông tin |
| `caution` | Cảnh báo nhẹ |
| `warning` | Cảnh báo |
| `critical` | Nghiêm trọng |

**Lưu ý:** Khi `severity` để `null`, template sẽ áp dụng cho tất cả các mức độ.

---

## 7. Flow sử dụng cho Frontend

### 7.1. Flow xem danh sách template

```
1. Admin truy cập trang quản lý Alert Template
2. Frontend gọi GET /api/v1/admin/alert-templates
3. Hiển thị danh sách template với các cột: Tên, Kênh, Mức độ, Trạng thái
4. Có thể lọc theo kênh (Push/Email/SMS/InApp)
5. Có thể lọc theo mức độ (critical/warning/info)
```

### 7.2. Flow tạo mới template

```
1. Admin click nút "Tạo mới Template"
2. Hiển thị form với các trường:
   - Tên template
   - Chọn kênh (dropdown)
   - Chọn mức độ (dropdown, có thể để "Tất cả")
   - Template tiêu đề (hỗ trợ biến)
   - Template nội dung (hỗ trợ biến)
   - Checkbox "Kích hoạt"
   - Thứ tự
3. Admin có thể click "Preview" để xem trước
4. Click "Lưu" -> POST /api/v1/admin/alert-templates
5. Nếu thành công -> Redirect về danh sách
```

### 7.3. Flow chỉnh sửa template

```
1. Admin click vào template trong danh sách
2. Hiển thị form chỉnh sửa với thông tin hiện tại
3. Admin sửa thông tin
4. Có thể click "Preview" để xem trước
5. Click "Lưu" -> PUT /api/v1/admin/alert-templates/{id}
```

### 7.4. Flow xóa template

```
1. Admin click nút xóa trên template
2. Hiển thị confirm dialog: "Bạn có chắc muốn xóa template X?"
3. Admin xác nhận
4. Frontend gọi DELETE /api/v1/admin/alert-templates/{id}
5. Nếu thành công -> Xóa khỏi danh sách
```

### 7.5. Flow Preview template

```
1. Admin click nút "Preview" trên form tạo/sửa
2. Hiển thị modal với các trường nhập sample data
3. Admin nhập dữ liệu mẫu (hoặc dùng mặc định)
4. Click "Xem trước"
5. Frontend gọi POST /api/v1/admin/alert-templates/{id}/preview
6. Hiển thị title và body đã được render
```

---

## 8. Ví dụ Code (Frontend)

### 8.1. Gọi API lấy danh sách template

```javascript
async function getAlertTemplates() {
  const token = localStorage.getItem('access_token');

  const response = await fetch(
    '/api/v1/admin/alert-templates',
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
    return data.templates;
  } else {
    throw new Error(data.message);
  }
}
```

### 8.2. Gọi API tạo mới template

```javascript
async function createAlertTemplate(templateData) {
  const token = localStorage.getItem('access_token');

  const response = await fetch(
    '/api/v1/admin/alert-templates',
    {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(templateData)
    }
  );

  const data = await response.json();

  if (data.success) {
    return data.template;
  } else {
    throw new Error(data.message);
  }
}

// Sử dụng
const newTemplate = await createAlertTemplate({
  name: "Critical Push Notification",
  channel: "Push",
  severity: "critical",
  titleTemplate: "⚠️ Cảnh báo ngập lụt - {{station_name}}",
  bodyTemplate: "Mức nước tại {{station_name}} đã đạt {{water_level}}.",
  isActive: true,
  sortOrder: 1
});
```

### 8.3. Gọi API cập nhật template

```javascript
async function updateAlertTemplate(templateId, templateData) {
  const token = localStorage.getItem('access_token');

  const response = await fetch(
    `/api/v1/admin/alert-templates/${templateId}`,
    {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(templateData)
    }
  );

  const data = await response.json();

  if (data.success) {
    return data.template;
  } else {
    throw new Error(data.message);
  }
}
```

### 8.4. Gọi API xóa template

```javascript
async function deleteAlertTemplate(templateId) {
  const token = localStorage.getItem('access_token');

  const response = await fetch(
    `/api/v1/admin/alert-templates/${templateId}`,
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

### 8.5. Gọi API preview template

```javascript
async function previewAlertTemplate(templateId, previewData) {
  const token = localStorage.getItem('access_token');

  const response = await fetch(
    `/api/v1/admin/alert-templates/${templateId}/preview`,
    {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(previewData)
    }
  );

  const data = await response.json();

  if (data.success) {
    return data.rendered;
  } else {
    throw new Error(data.message);
  }
}

// Sử dụng
const rendered = await previewAlertTemplate('template-id', {
  stationName: 'Trạm đo Quận 1',
  waterLevel: '3.5m',
  waterLevelRaw: 3.5,
  severity: 'critical',
  time: '2024-01-15 14:30:00',
  threshold: 3.0,
  address: '123 Nguyễn Huệ, Q1, TP.HCM',
  message: 'Mức nước vượt ngưỡng'
});

// rendered.title -> "⚠️ Cảnh báo ngập lụt - Trạm đo Quận 1"
// rendered.body -> "Mức nước tại Trạm đo Quận 1 đã đạt 3.5m..."
```

---

## 9. Lưu ý quan trọng

1. **Authentication:** Tất cả API đều cần Bearer token trong header
2. **Authorization:** Chỉ ADMIN mới có quyền truy cập các API này
3. **Validation:**
   - `name`, `channel`, `titleTemplate`, `bodyTemplate` là bắt buộc
   - `channel` phải là một trong: Push, Email, SMS, InApp
   - `severity` (nếu có) phải là một trong: info, caution, warning, critical
4. **Template Variables:** Khi hiển thị form, nên show hướng dẫn các biến có thể sử dụng
5. **Preview:** Nên khuyến khích Admin sử dụng Preview trước khi lưu để đảm bảo template hiển thị đúng
6. **Error Handling:** Luôn kiểm tra trường `success` trong response

---

## 10. Màn hình gợi ý cho Frontend

### 10.1. Danh sách Template
- Bảng hiển thị: Tên, Kênh, Mức độ, Trạng thái, Thứ tự, Ngày tạo
- Có dropdown lọc theo kênh
- Có dropdown lọc theo mức độ
- Nút "Thêm mới" (chỉ Admin)
- Mỗi dòng có nút Sửa, Xóa, Preview (chỉ Admin)
- Click vào dòng để xem chi tiết

### 10.2. Form tạo/sửa Template
- Input text: Tên template
- Dropdown: Chọn kênh (Push/Email/SMS/InApp)
- Dropdown: Chọn mức độ (Info/Caution/Warning/Critical/All)
- Textarea: Template tiêu đề (có hint các biến)
- Textarea: Template nội dung (có hint các biến)
- Checkbox: Kích hoạt
- Input number: Thứ tự
- Nút Preview -> Mở modal preview
- Nút Lưu và Hủy

### 10.3. Modal Preview
- Form nhập dữ liệu mẫu (hoặc dùng mặc định)
- Nút "Xem trước"
- Hiển thị kết quả: Title và Body đã render
- Nút Đóng

### 10.4. Chi tiết Template
- Hiển thị tất cả thông tin
- Nút Sửa, Xóa, Preview (chỉ Admin)
- Hiển thị lịch sử: Ngày tạo, người tạo, ngày cập nhật, người cập nhật
