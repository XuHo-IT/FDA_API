# FE-12: Alert Notification System - Frontend Documentation

## Overview
Hệ thống cảnh báo ngập lụt tự động gửi thông báo cho người dùng khi mực nước vượt ngưỡng nguy hiểm. Người dùng có thể tùy chỉnh cách nhận thông báo qua Push, Email, hoặc SMS.

---

## User Flows

### Flow 1: Đăng ký nhận cảnh báo cho một khu vực
**Mục đích**: Người dùng bật/tắt cảnh báo cho một Area đã tạo trước đó

**Steps**:
1. User tạo Area (đã có ở FE trước)
2. Hệ thống tự động tạo subscription với cài đặt mặc định (không cần gọi API)
3. User có thể cập nhật cài đặt cảnh báo (xem Flow 2)

**Note**: Subscription được tạo tự động khi user tạo Area, không cần subscribe thủ công.

---

### Flow 2: Cập nhật cài đặt cảnh báo
**Mục đích**: Thay đổi kênh thông báo (Push/Email/SMS), mức độ nghiêm trọng tối thiểu, giờ im lặng

**Steps**:
1. User vào màn hình "My Areas"
2. Chọn một Area → Nhấn "Alert Settings"
3. Cập nhật:
   - Min Severity (caution/warning/critical)
   - Channels (Push ✅, Email ✅, SMS ❌)
   - Quiet Hours (22:00 - 06:00)
4. Nhấn "Save"
5. Gọi API: `PUT /api/v1/areas/{areaId}/alert-preferences`

---

### Flow 3: Xem lịch sử cảnh báo
**Mục đích**: Xem các cảnh báo đã nhận trong quá khứ

**Steps**:
1. User vào màn hình "Alert History"
2. Hệ thống gọi API: `GET /api/v1/alerts/history`
3. Hiển thị danh sách alerts với:
   - Station name, water level, severity
   - Thời gian trigger và resolve
   - Trạng thái gửi notification (sent/failed)

---

### Flow 4: Admin xem tổng quan subscriptions
**Mục đích**: Admin theo dõi người dùng đã đăng ký nhận cảnh báo

**Steps**:
1. Admin vào trang "User Subscriptions"
2. Gọi API: `GET /api/v1/admin/alerts/subscriptions`
3. Hiển thị danh sách users và cài đặt của họ

---

## APIs Documentation

### 1. Update Alert Preferences (G41)
**Endpoint**: `PUT /api/v1/areas/{areaId}/alert-preferences`

**Auth**: Required (JWT Bearer Token)

**Request**:
```json
{
  "minSeverity": "warning",
  "enablePush": true,
  "enableEmail": false,
  "enableSms": false,
  "quietHoursStart": "22:00:00",
  "quietHoursEnd": "06:00:00"
}
```

**Request Fields**:
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `minSeverity` | string | No | Mức nghiêm trọng tối thiểu để nhận thông báo. Values: `"caution"`, `"warning"`, `"critical"`. Default: `"warning"` |
| `enablePush` | boolean | No | Bật/tắt thông báo Push (Firebase). Default: `true` |
| `enableEmail` | boolean | No | Bật/tắt thông báo Email. Default: `false` |
| `enableSms` | boolean | No | Bật/tắt thông báo SMS (Twilio). Default: `false` |
| `quietHoursStart` | TimeSpan | No | Giờ bắt đầu chế độ im lặng (không gửi thông báo). Format: `"HH:mm:ss"` |
| `quietHoursEnd` | TimeSpan | No | Giờ kết thúc chế độ im lặng. Format: `"HH:mm:ss"` |

**Response**:
```json
{
  "success": true,
  "message": "Alert preferences updated successfully"
}
```

**Response Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `success` | boolean | `true` nếu update thành công, `false` nếu có lỗi |
| `message` | string | Thông báo kết quả hoặc lỗi |

**Status Codes**:
- `200 OK`: Update thành công
- `400 Bad Request`: Invalid input (severity không hợp lệ)
- `401 Unauthorized`: Chưa đăng nhập
- `404 Not Found`: Area không tồn tại hoặc không thuộc về user

---

### 2. Get My Subscriptions (G67)
**Endpoint**: `GET /api/v1/alerts/subscriptions/me`

**Auth**: Required (JWT Bearer Token)

**Response**:
```json
{
  "success": true,
  "message": "Retrieved 3 subscriptions",
  "subscriptions": [
    {
      "subscriptionId": "sub-uuid-1",
      "areaId": "area-uuid-1",
      "areaName": "Khu vực nhà tôi",
      "stationId": null,
      "minSeverity": "warning",
      "enablePush": true,
      "enableEmail": false,
      "enableSms": false,
      "quietHoursStart": "22:00:00",
      "quietHoursEnd": "06:00:00"
    }
  ]
}
```

**Response Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `success` | boolean | Trạng thái thành công |
| `message` | string | Thông báo kết quả |
| `subscriptions` | array | Danh sách subscriptions của user |
| `subscriptions[].subscriptionId` | GUID | ID của subscription |
| `subscriptions[].areaId` | GUID? | ID của Area (nếu subscribe theo area) |
| `subscriptions[].areaName` | string | Tên Area |
| `subscriptions[].stationId` | GUID? | ID của Station (nếu subscribe trực tiếp station - chỉ admin) |
| `subscriptions[].minSeverity` | string | Mức nghiêm trọng tối thiểu (`"caution"`, `"warning"`, `"critical"`) |
| `subscriptions[].enablePush` | boolean | Có bật Push notification không |
| `subscriptions[].enableEmail` | boolean | Có bật Email không |
| `subscriptions[].enableSms` | boolean | Có bật SMS không |
| `subscriptions[].quietHoursStart` | TimeSpan? | Giờ bắt đầu im lặng |
| `subscriptions[].quietHoursEnd` | TimeSpan? | Giờ kết thúc im lặng |

**Status Codes**:
- `200 OK`: Thành công
- `401 Unauthorized`: Chưa đăng nhập

---

### 3. Get Alert History (G40)
**Endpoint**: `GET /api/v1/alerts/history`

**Auth**: Required (JWT Bearer Token)

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `areaId` | GUID | No | Lọc theo Area cụ thể |
| `severity` | string | No | Lọc theo mức nghiêm trọng (`caution`, `warning`, `critical`) |
| `fromDate` | DateTime | No | Lọc từ ngày (ISO 8601: `2026-01-01T00:00:00Z`) |
| `toDate` | DateTime | No | Lọc đến ngày |
| `pageNumber` | int | No | Trang hiện tại (default: `1`) |
| `pageSize` | int | No | Số items mỗi trang (default: `10`) |

**Request Example**:
```
GET /api/v1/alerts/history?areaId=area-uuid-1&severity=warning&pageNumber=1&pageSize=20
```

**Response**:
```json
{
  "success": true,
  "message": "Retrieved 15 alerts",
  "alerts": [
    {
      "alertId": "alert-uuid-1",
      "stationId": "station-uuid-1",
      "stationName": "Trạm Lê Lợi - Pasteur",
      "stationCode": "STN-001",
      "severity": "warning",
      "priority": "Medium",
      "waterLevel": 25.5,
      "message": "Water level exceeded warning threshold",
      "triggeredAt": "2026-01-20T10:30:00Z",
      "resolvedAt": "2026-01-20T12:00:00Z",
      "status": "resolved",
      "notifications": [
        {
          "notificationId": "notif-uuid-1",
          "channel": "Push",
          "status": "sent",
          "sentAt": "2026-01-20T10:31:00Z",
          "deliveredAt": "2026-01-20T10:31:05Z",
          "errorMessage": null
        },
        {
          "notificationId": "notif-uuid-2",
          "channel": "Email",
          "status": "failed",
          "sentAt": "2026-01-20T10:31:00Z",
          "deliveredAt": null,
          "errorMessage": "SMTP connection timeout"
        }
      ]
    }
  ],
  "totalCount": 45,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

**Response Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `success` | boolean | Trạng thái thành công |
| `message` | string | Thông báo kết quả |
| `alerts` | array | Danh sách cảnh báo |
| `alerts[].alertId` | GUID | ID của alert |
| `alerts[].stationName` | string | Tên trạm đo |
| `alerts[].stationCode` | string | Mã trạm (VD: `STN-001`) |
| `alerts[].severity` | string | Mức độ nghiêm trọng (`caution`, `warning`, `critical`) |
| `alerts[].priority` | enum | Priority level (`Low`, `Medium`, `High`, `Critical`) |
| `alerts[].waterLevel` | decimal | Mực nước (cm) |
| `alerts[].message` | string | Nội dung cảnh báo |
| `alerts[].triggeredAt` | DateTime | Thời điểm kích hoạt cảnh báo |
| `alerts[].resolvedAt` | DateTime? | Thời điểm mực nước trở về bình thường (null nếu chưa giải quyết) |
| `alerts[].status` | string | Trạng thái (`open`, `resolved`) |
| `alerts[].notifications` | array | Danh sách notifications đã gửi |
| `alerts[].notifications[].channel` | enum | Kênh gửi (`Push`, `Email`, `SMS`, `InApp`) |
| `alerts[].notifications[].status` | string | Trạng thái gửi (`pending`, `sent`, `failed`, `delivered`) |
| `alerts[].notifications[].sentAt` | DateTime? | Thời điểm gửi |
| `alerts[].notifications[].deliveredAt` | DateTime? | Thời điểm nhận được (nếu có confirmation) |
| `alerts[].notifications[].errorMessage` | string? | Thông báo lỗi nếu gửi thất bại |
| `totalCount` | int | Tổng số alerts (tất cả trang) |
| `pageNumber` | int | Trang hiện tại |
| `pageSize` | int | Số items mỗi trang |
| `totalPages` | int | Tổng số trang |

**Status Codes**:
- `200 OK`: Thành công
- `401 Unauthorized`: Chưa đăng nhập

---

### 4. Delete Subscription (G68)
**Endpoint**: `DELETE /api/v1/alerts/subscriptions/{subscriptionId}`

**Auth**: Required (JWT Bearer Token)

**Response**:
```json
{
  "success": true,
  "message": "Subscription deleted successfully"
}
```

**Status Codes**:
- `200 OK`: Xóa thành công
- `401 Unauthorized`: Chưa đăng nhập
- `403 Forbidden`: Subscription không thuộc về user
- `404 Not Found`: Subscription không tồn tại

---

### 5. Admin: Get All Subscriptions (G69)
**Endpoint**: `GET /api/v1/admin/alerts/subscriptions`

**Auth**: Required (Role: Admin, Authority)

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `userId` | GUID | No | Lọc theo user cụ thể |
| `areaId` | GUID | No | Lọc theo area |
| `pageNumber` | int | No | Trang hiện tại (default: `1`) |
| `pageSize` | int | No | Số items mỗi trang (default: `20`) |

**Response**:
```json
{
  "success": true,
  "message": "Retrieved 50 subscriptions",
  "subscriptions": [
    {
      "subscriptionId": "sub-uuid-1",
      "userId": "user-uuid-1",
      "userEmail": "user@example.com",
      "areaId": "area-uuid-1",
      "areaName": "Khu vực Q1",
      "minSeverity": "warning",
      "enablePush": true,
      "enableEmail": false,
      "enableSms": false,
      "createdAt": "2026-01-15T08:00:00Z"
    }
  ],
  "totalCount": 120,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 6
}
```

**Status Codes**:
- `200 OK`: Thành công
- `401 Unauthorized`: Chưa đăng nhập
- `403 Forbidden`: Không có quyền Admin

---

### 6. Admin: Get Alert Statistics (G70)
**Endpoint**: `GET /api/v1/admin/alerts/stats`

**Auth**: Required (Role: Admin, Authority)

**Response**:
```json
{
  "success": true,
  "message": "Statistics retrieved successfully",
  "stats": {
    "totalAlerts": 150,
    "activeAlerts": 5,
    "resolvedAlerts": 145,
    "totalNotifications": 450,
    "notificationsSent": 420,
    "notificationsFailed": 30,
    "alertsBySeverity": {
      "caution": 80,
      "warning": 60,
      "critical": 10
    },
    "notificationsByChannel": {
      "Push": 300,
      "Email": 120,
      "SMS": 30
    },
    "totalSubscriptions": 75,
    "activeSubscriptions": 70
  }
}
```

**Status Codes**:
- `200 OK`: Thành công
- `401 Unauthorized`: Chưa đăng nhập
- `403 Forbidden`: Không có quyền Admin

---

## UI/UX Guidelines

### 1. Alert Settings Screen
**Components**:
- Toggle switches cho Push/Email/SMS
- Dropdown cho Min Severity (Caution/Warning/Critical)
- Time pickers cho Quiet Hours
- Save button

**Sample**:
```
┌─────────────────────────────────────┐
│  Alert Settings - Khu vực Q1       │
├─────────────────────────────────────┤
│                                     │
│  Notification Channels:             │
│  🔔 Push Notification      [✓]      │
│  📧 Email                  [ ]      │
│  📱 SMS                    [ ]      │
│                                     │
│  Minimum Severity:                  │
│  [Warning ▼]                        │
│                                     │
│  Quiet Hours:                       │
│  From: [22:00] To: [06:00]          │
│                                     │
│  [Save Changes]                     │
└─────────────────────────────────────┘
```

### 2. Alert History Screen
**Components**:
- List view with cards
- Filters (Severity, Date Range)
- Pagination
- Status badges (Open/Resolved)

**Sample Card**:
```
┌─────────────────────────────────────┐
│ 🔴 WARNING                          │
│ Trạm Lê Lợi - Pasteur              │
│ Water Level: 25.5 cm               │
│ Jan 20, 2026 10:30 AM               │
│                                     │
│ Notifications:                      │
│ ✓ Push (Sent)                       │
│ ✗ Email (Failed: Timeout)           │
│                                     │
│ Status: Resolved at 12:00 PM        │
└─────────────────────────────────────┘
```

---

## Testing Checklist

### For Mobile App Team:
- [ ] Integrate Firebase FCM SDK
- [ ] Handle Push notification payload
- [ ] Display in-app notification UI
- [ ] Navigate to Alert History on notification tap

### For Web Team:
- [ ] Implement Alert Settings form
- [ ] Implement Alert History table with filters
- [ ] Add pagination controls
- [ ] Test time zone conversions (UTC to local)

### Common:
- [ ] Handle 401 Unauthorized (redirect to login)
- [ ] Show loading states
- [ ] Display error messages
- [ ] Validate Min Severity enum values

---

## Common Errors

| Error | Status | Cause | Solution |
|-------|--------|-------|----------|
| "Unauthorized" | 401 | JWT token expired | Refresh token and retry |
| "Area not found" | 404 | Area đã bị xóa | Refresh area list |
| "Invalid severity" | 400 | Truyền severity không hợp lệ | Chỉ dùng: `caution`, `warning`, `critical` |
| "No subscription found" | 404 | Area chưa có subscription | Auto-tạo khi tạo Area |
