# FE-13: Priority Flood Notification System - Frontend Documentation

## Overview
Hệ thống thông báo ngập lụt ưu tiên cho phép người dùng nhận cảnh báo với tốc độ và kênh gửi khác nhau dựa trên **gói đăng ký** (Free, Premium, Monitor). Người dùng trả phí sẽ nhận thông báo nhanh hơn và có nhiều kênh giao tiếp hơn.

---
### Lưu ý: CHỈ TẬP TRUNG VÀO CÁC FLOW CỦA NHẬN THÔNG BÁO, FLOW ĐĂNG KÍ PREMIUM CHỈ ĐỂ TEST ĐỘ DELAY GIỮA BẢN PREMIUM/FREE NẾU CẦN
---

## Priority System Architecture

### 1. Subscription Tiers (Gói đăng ký)

| Tier | Price | Dispatch Delay | Max Retries | Available Channels | Use Case |
|------|-------|----------------|-------------|-------------------|----------|
| **Free** | $0/month | 60-120s | 1 retry | Push, Email | Người dùng cá nhân |
| **Premium** | $9.99/month | 0-20s | 3 retries | Push, Email, SMS | Người dùng trả phí |
| **Monitor** | $49.99/month | 0-10s | 5 retries | Push, Email, SMS | Cơ quan chính phủ, doanh nghiệp |

### 2. Notification Priority Levels

Hệ thống tự động xác định mức độ ưu tiên dựa trên:
- **Alert Severity** (caution/warning/critical)
- **User Tier** (Free/Premium/Monitor)

| Alert Severity | Free User Priority | Premium User Priority | Monitor User Priority |
|----------------|-------------------|----------------------|---------------------|
| Caution | Low | Low | Medium |
| Warning | Low | Medium | High |
| Critical | Medium | High | Critical |

### 3. Dispatch Delay Logic

**Công thức tính delay**:
```
IF user_tier == Monitor:
    delay = (priority >= High) ? 0s : 10s
ELSE IF user_tier == Premium:
    delay = (priority >= High) ? 0s : 20s
ELSE (Free):
    delay = (priority >= Medium) ? 60s : 120s
```

**Ví dụ thực tế**:
- **Monitor user** + Critical alert → 0s delay (gửi ngay lập tức)
- **Premium user** + Warning alert → 0s delay (gửi ngay lập tức)
- **Free user** + Critical alert → 60s delay
- **Free user** + Caution alert → 120s delay

---

## User Flows

### Flow 1: User muốn nâng cấp lên Premium để nhận thông báo nhanh hơn

**Mục đích**: Subscribe gói Premium để giảm thời gian chờ từ 60-120s xuống 0-20s

**Steps**:
1. User vào màn hình "Subscription / Pricing Plans"
2. Hệ thống gọi API: `GET /api/v1/pricing/plans` để hiển thị các gói
3. User chọn "Premium Plan" → Nhấn "Subscribe"
4. Gọi API: `POST /api/v1/plan/subscription/subscribe`
   ```json
   {
     "planCode": "PREMIUM",
     "durationMonths": 12
   }
   ```
5. Backend tự động:
   - Cancel subscription cũ (nếu có)
   - Tạo subscription mới với `tier = Premium`
   - Áp dụng dispatch delay 0-20s cho user này
6. Hiển thị thông báo: "Upgraded to Premium! You'll now receive alerts faster."

**UI Example**:
```
┌─────────────────────────────────────────┐
│  Choose Your Plan                      │
├─────────────────────────────────────────┤
│  ○ FREE                                 │
│    • Push & Email notifications         │
│    • 60-120s delivery delay             │
│    • 1 retry attempt                    │
│                                         │
│  ◉ PREMIUM ($9.99/month)                │
│    • All Free features                  │
│    • SMS notifications                  │
│    • 0-20s delivery delay ⚡            │
│    • 3 retry attempts                   │
│                                         │
│  ○ MONITOR ($49.99/month)               │
│    • All Premium features               │
│    • 0-10s delivery delay ⚡⚡          │
│    • 5 retry attempts                   │
│    • Priority support                   │
│                                         │
│  [Subscribe Now]                        │
└─────────────────────────────────────────┘
```

---

### Flow 2: User xem subscription hiện tại

**Mục đích**: Kiểm tra gói đang dùng, ngày hết hạn, và ưu điểm của tier hiện tại

**Steps**:
1. User vào màn hình "My Subscription"
2. Gọi API: `GET /api/v1/subscription/current`
3. Hiển thị thông tin:
   - Current Tier (Free/Premium/Monitor)
   - Start Date & End Date
   - Dispatch Delay Range
   - Available Channels
   - Next billing date (nếu Premium/Monitor)

**Response Example**:
```json
{
  "success": true,
  "message": "Current subscription retrieved",
  "subscription": {
    "tier": "Premium",
    "planName": "Premium Plan",
    "startDate": "2026-01-15T00:00:00Z",
    "endDate": "2027-01-15T00:00:00Z",
    "status": "active",
    "dispatchDelayRange": "0-20s",
    "availableChannels": ["Push", "Email", "SMS"],
    "maxRetries": 3,
    "daysRemaining": 358
  }
}
```

**UI Example**:
```
┌─────────────────────────────────────────┐
│  Your Subscription                     │
├─────────────────────────────────────────┤
│  💎 PREMIUM PLAN                        │
│                                         │
│  Status: Active ✓                       │
│  Valid until: Jan 15, 2027              │
│  Days remaining: 358                    │
│                                         │
│  Benefits:                              │
│  ⚡ 0-20s delivery delay                │
│  📱 SMS notifications enabled           │
│  🔄 3 retry attempts                    │
│                                         │
│  [Manage Plan] [Cancel Subscription]    │
└─────────────────────────────────────────┘
```

---

### Flow 3: User nhận thông báo với priority khác nhau

**Mục đích**: Hiển thị visual distinction cho alerts với priority cao/thấp

**Steps**:
1. Backend phát hiện mực nước vượt ngưỡng → Trigger alert
2. Backend xác định priority dựa trên:
   - User tier (từ `UserSubscription`)
   - Alert severity (từ `Alert`)
3. Backend tính dispatch delay và gửi notification
4. **Mobile/Web nhận Push notification** với payload:
   ```json
   {
     "notificationId": "notif-uuid-1",
     "alertId": "alert-uuid-1",
     "title": "🚨 CRITICAL FLOOD ALERT",
     "body": "Water level at Trạm Lê Lợi: 35.2 cm (Critical)",
     "priority": "Critical",
     "severity": "critical",
     "stationName": "Trạm Lê Lợi - Pasteur",
     "waterLevel": 35.2,
     "timestamp": "2026-01-22T10:30:00Z"
   }
   ```
5. **Frontend hiển thị notification với style tương ứng**:

**UI Rendering Logic**:
```javascript
function getNotificationStyle(priority) {
  switch (priority) {
    case "Critical":
      return {
        icon: "🚨",
        color: "#DC2626", // red-600
        sound: "alert_critical.mp3",
        vibration: [0, 500, 200, 500],
        priority: "high" // Android notification priority
      };
    case "High":
      return {
        icon: "⚠️",
        color: "#EA580C", // orange-600
        sound: "alert_high.mp3",
        vibration: [0, 300, 100, 300],
        priority: "high"
      };
    case "Medium":
      return {
        icon: "⚡",
        color: "#F59E0B", // yellow-500
        sound: "alert_medium.mp3",
        vibration: [0, 200],
        priority: "default"
      };
    case "Low":
      return {
        icon: "ℹ️",
        color: "#3B82F6", // blue-500
        sound: "alert_low.mp3",
        vibration: [0, 100],
        priority: "low"
      };
  }
}
```

**In-App Notification Card**:
```
┌─────────────────────────────────────────┐
│ 🚨 CRITICAL FLOOD ALERT           [×]  │  ← Red background
├─────────────────────────────────────────┤
│  Trạm Lê Lợi - Pasteur                 │
│  Water Level: 35.2 cm                  │
│  Severity: Critical                     │
│  Priority: Critical ⚡⚡⚡              │  ← Badge with 3 lightning bolts
│                                         │
│  Received: 10:30 AM (0s delay)          │  ← Show actual delay
│  Your Tier: Premium 💎                  │
│                                         │
│  [View Details] [Mark as Read]          │
└─────────────────────────────────────────┘
```

---

### Flow 4: User hủy subscription (downgrade về Free)

**Mục đích**: Cancel Premium/Monitor plan và quay lại Free tier

**Steps**:
1. User vào "Subscription Settings"
2. Nhấn "Cancel Subscription"
3. Hiển thị confirmation dialog:
   ```
   Are you sure you want to cancel Premium?
   
   You will lose:
   • Fast delivery (0-20s → 60-120s delay)
   • SMS notifications
   • Extra retry attempts
   
   [Keep Premium] [Cancel Anyway]
   ```
4. User nhấn "Cancel Anyway"
5. Gọi API: `DELETE /api/v1/plan/subscription/cancel`
   ```json
   {
     "cancelReason": "Too expensive"
   }
   ```
6. Backend:
   - Set subscription status = "cancelled"
   - User tự động chuyển về Free tier
   - Dispatch delay quay lại 60-120s
7. Hiển thị: "Subscription cancelled. You're now on the Free tier."

---

### Flow 5: Admin theo dõi notification delivery performance

**Mục đích**: Admin xem thống kê gửi notification theo tier/priority

**Steps**:
1. Admin vào dashboard "Notification Stats"
2. Gọi API: `GET /api/v1/admin/notifications/stats`
3. Hiển thị biểu đồ:
   - Delivery success rate by tier (Free: 85%, Premium: 95%, Monitor: 99%)
   - Average delivery time by tier (Free: 95s, Premium: 5s, Monitor: 2s)
   - Retry statistics by tier
   - Priority distribution (Low: 40%, Medium: 35%, High: 20%, Critical: 5%)

---

## APIs Documentation

### 1. Get Pricing Plans (phần này làm sau)
**Endpoint**: `GET /api/v1/pricing/plans`

**Auth**: Not required (public endpoint)

**Response**:
```json
{
  "success": true,
  "message": "Retrieved 3 pricing plans",
  "plans": [
    {
      "id": "plan-uuid-1",
      "code": "FREE",
      "name": "Free Plan",
      "description": "Basic flood alerts for personal use",
      "priceMonth": 0.00,
      "priceYear": 0.00,
      "tier": "Free",
      "features": [
        "Push notifications",
        "Email notifications",
        "60-120s delivery delay",
        "1 retry attempt"
      ]
    },
    {
      "id": "plan-uuid-2",
      "code": "PREMIUM",
      "name": "Premium Plan",
      "description": "Faster alerts with SMS support",
      "priceMonth": 9.99,
      "priceYear": 99.99,
      "tier": "Premium",
      "features": [
        "All Free features",
        "SMS notifications",
        "0-20s delivery delay",
        "3 retry attempts",
        "High priority"
      ]
    },
    {
      "id": "plan-uuid-3",
      "code": "MONITOR",
      "name": "Monitor Plan",
      "description": "Enterprise-grade monitoring for government and business",
      "priceMonth": 49.99,
      "priceYear": 499.99,
      "tier": "Monitor",
      "features": [
        "All Premium features",
        "0-10s delivery delay",
        "5 retry attempts",
        "Critical priority",
        "Government access"
      ]
    }
  ]
}
```

**Response Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `success` | boolean | Trạng thái thành công |
| `message` | string | Thông báo kết quả |
| `plans` | array | Danh sách các gói đăng ký |
| `plans[].id` | GUID | ID của plan |
| `plans[].code` | string | Mã plan (`FREE`, `PREMIUM`, `MONITOR`) |
| `plans[].name` | string | Tên hiển thị |
| `plans[].description` | string | Mô tả ngắn gọn |
| `plans[].priceMonth` | decimal | Giá tháng (USD) |
| `plans[].priceYear` | decimal | Giá năm (USD) |
| `plans[].tier` | string | Tier enum (`Free`, `Premium`, `Monitor`) |
| `plans[].features` | array | Danh sách tính năng của gói |

**Status Codes**:
- `200 OK`: Thành công

**Use Case**: Hiển thị bảng giá để user chọn gói

---

### 2. Subscribe to Plan (G72) (chỉ dùng để test)
**Endpoint**: `POST /api/v1/plan/subscription/subscribe`

**Auth**: Required (JWT Bearer Token, Policy: "User")

**Request**:
```json
{
  "planCode": "PREMIUM",
  "durationMonths": 12
}
```

**Request Fields**:
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `planCode` | string | Yes | Mã plan muốn subscribe (`FREE`, `PREMIUM`, `MONITOR`) |
| `durationMonths` | int | Yes | Số tháng đăng ký (1-120). Default: `12` |

**Response**:
```json
{
  "success": true,
  "message": "Successfully subscribed to Premium Plan",
  "subscription": {
    "subscriptionId": "sub-uuid-1",
    "planCode": "PREMIUM",
    "planName": "Premium Plan",
    "tier": "Premium",
    "startDate": "2026-01-22T00:00:00Z",
    "endDate": "2027-01-22T00:00:00Z",
    "status": "active"
  }
}
```

**Response Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `success` | boolean | `true` nếu subscribe thành công |
| `message` | string | Thông báo kết quả |
| `subscription` | object | Thông tin subscription mới |
| `subscription.subscriptionId` | GUID | ID của subscription |
| `subscription.planCode` | string | Mã plan (`PREMIUM`, `MONITOR`) |
| `subscription.planName` | string | Tên plan |
| `subscription.tier` | string | Tier (`Free`, `Premium`, `Monitor`) |
| `subscription.startDate` | DateTime | Ngày bắt đầu |
| `subscription.endDate` | DateTime | Ngày hết hạn |
| `subscription.status` | string | Trạng thái (`active`, `expired`, `cancelled`) |

**Status Codes**:
- `200 OK`: Subscribe thành công
- `400 Bad Request`: PlanCode không hợp lệ hoặc duration < 1
- `401 Unauthorized`: Chưa đăng nhập
- `404 Not Found`: Plan không tồn tại hoặc inactive

**Business Logic**:
1. Nếu user đã có subscription active → Tự động cancel và tạo mới
2. Free plan: EndDate = StartDate + 100 years (không bao giờ hết hạn)
3. Premium/Monitor: EndDate = StartDate + durationMonths

---

### 3. Get Current Subscription (G71) (chỉ dùng để test)
**Endpoint**: `GET /api/v1/subscription/current`

**Auth**: Required (JWT Bearer Token, Policy: "User")

**Response**:
```json
{
  "success": true,
  "message": "Current subscription retrieved",
  "subscription": {
    "subscriptionId": "sub-uuid-1",
    "userId": "user-uuid-1",
    "planId": "plan-uuid-2",
    "planCode": "PREMIUM",
    "planName": "Premium Plan",
    "tier": "Premium",
    "startDate": "2026-01-15T00:00:00Z",
    "endDate": "2027-01-15T00:00:00Z",
    "status": "active",
    "renewMode": "manual",
    "daysRemaining": 358,
    "benefits": {
      "dispatchDelayRange": "0-20s",
      "availableChannels": ["Push", "Email", "SMS"],
      "maxRetries": 3,
      "priorityLevel": "Medium to High"
    }
  }
}
```

**Response Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `success` | boolean | Trạng thái thành công |
| `message` | string | Thông báo kết quả |
| `subscription` | object? | Subscription hiện tại (null nếu chưa có hoặc đã hết hạn) |
| `subscription.subscriptionId` | GUID | ID của subscription |
| `subscription.tier` | string | Tier hiện tại (`Free`, `Premium`, `Monitor`) |
| `subscription.startDate` | DateTime | Ngày bắt đầu |
| `subscription.endDate` | DateTime | Ngày hết hạn |
| `subscription.status` | string | Trạng thái (`active`, `expired`, `cancelled`) |
| `subscription.renewMode` | string | Chế độ gia hạn (`manual`, `auto`) |
| `subscription.daysRemaining` | int | Số ngày còn lại đến khi hết hạn |
| `subscription.benefits` | object | Ưu đãi của tier hiện tại |
| `subscription.benefits.dispatchDelayRange` | string | Khoảng delay gửi notification (VD: `"0-20s"`) |
| `subscription.benefits.availableChannels` | array | Danh sách kênh có thể dùng |
| `subscription.benefits.maxRetries` | int | Số lần retry tối đa khi gửi thất bại |
| `subscription.benefits.priorityLevel` | string | Mức ưu tiên notification |

**Status Codes**:
- `200 OK`: Thành công (có thể subscription = null nếu user chưa subscribe)
- `401 Unauthorized`: Chưa đăng nhập

**Use Case**: 
- Hiển thị thông tin subscription trên profile
- Check tier trước khi hiển thị features
- Hiển thị nút "Upgrade to Premium" nếu đang là Free

---

### 4. Cancel Subscription (G73) (chỉ dùng để test)
**Endpoint**: `DELETE /api/v1/plan/subscription/cancel`

**Auth**: Required (JWT Bearer Token, Policy: "User")

**Request**:
```json
{
  "cancelReason": "Too expensive"
}
```

**Request Fields**:
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `cancelReason` | string | No | Lý do hủy (optional, để feedback) |

**Response**:
```json
{
  "success": true,
  "message": "Successfully cancelled Premium Plan. You are now on the Free tier.",
  "cancelledSubscription": {
    "subscriptionId": "sub-uuid-1",
    "planName": "Premium Plan",
    "previousTier": "Premium",
    "cancelledAt": "2026-01-22T15:30:00Z",
    "cancelReason": "Too expensive"
  }
}
```

**Response Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `success` | boolean | `true` nếu cancel thành công |
| `message` | string | Thông báo kết quả |
| `cancelledSubscription` | object | Thông tin subscription đã cancel |
| `cancelledSubscription.subscriptionId` | GUID | ID của subscription bị hủy |
| `cancelledSubscription.planName` | string | Tên plan đã cancel |
| `cancelledSubscription.previousTier` | string | Tier trước khi cancel |
| `cancelledSubscription.cancelledAt` | DateTime | Thời điểm cancel |
| `cancelledSubscription.cancelReason` | string? | Lý do hủy (nếu có) |

**Status Codes**:
- `200 OK`: Cancel thành công
- `400 Bad Request`: Đang ở Free tier (không thể cancel Free plan)
- `401 Unauthorized`: Chưa đăng nhập
- `404 Not Found`: Không có subscription active để cancel

**Business Logic**:
1. Không thể cancel Free plan (đã là tier thấp nhất)
2. Sau khi cancel, user tự động về Free tier
3. Dispatch delay tăng lên 60-120s
4. Mất quyền truy cập SMS channel

---

### 5. Get Alert History with Priority Info (G40 - Enhanced) (đã làm - có thêm thuộc tính priority)
**Endpoint**: `GET /api/v1/alerts/history`

**Auth**: Required (JWT Bearer Token)

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `areaId` | GUID | No | Lọc theo Area cụ thể |
| `severity` | string | No | Lọc theo mức nghiêm trọng (`caution`, `warning`, `critical`) |
| `priority` | string | No | **NEW** - Lọc theo priority (`Low`, `Medium`, `High`, `Critical`) |
| `fromDate` | DateTime | No | Lọc từ ngày (ISO 8601) |
| `toDate` | DateTime | No | Lọc đến ngày |
| `pageNumber` | int | No | Trang hiện tại (default: `1`) |
| `pageSize` | int | No | Số items mỗi trang (default: `10`) |

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
      "severity": "critical",
      "priority": "Critical",
      "waterLevel": 35.2,
      "message": "Water level exceeded critical threshold",
      "triggeredAt": "2026-01-22T10:30:00Z",
      "resolvedAt": "2026-01-22T12:00:00Z",
      "status": "resolved",
      "notifications": [
        {
          "notificationId": "notif-uuid-1",
          "channel": "Push",
          "channelName": "Push",
          "priority": "Critical",
          "priorityName": "Critical",
          "status": "sent",
          "sentAt": "2026-01-22T10:30:02Z",
          "deliveredAt": "2026-01-22T10:30:05Z",
          "errorMessage": null,
          "title": "🚨 CRITICAL FLOOD ALERT",
          "actualDelay": "2s"
        },
        {
          "notificationId": "notif-uuid-2",
          "channel": "SMS",
          "channelName": "SMS",
          "priority": "Critical",
          "priorityName": "Critical",
          "status": "sent",
          "sentAt": "2026-01-22T10:30:03Z",
          "deliveredAt": "2026-01-22T10:30:08Z",
          "errorMessage": null,
          "title": null,
          "actualDelay": "3s"
        }
      ]
    }
  ],
  "totalCount": 45,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

**Response Fields (New/Modified)**:
| Field | Type | Description |
|-------|------|-------------|
| `alerts[].priority` | enum | **NEW** - Priority level (`Low`, `Medium`, `High`, `Critical`) |
| `alerts[].notifications[].priority` | enum | **NEW** - Priority của notification cụ thể |
| `alerts[].notifications[].priorityName` | string | **NEW** - Tên priority dạng string |
| `alerts[].notifications[].title` | string? | **NEW** - Tiêu đề notification (dynamic dựa trên priority) |
| `alerts[].notifications[].actualDelay` | string | **NEW** - Thời gian delay thực tế (VD: `"2s"`, `"65s"`) |

**Priority Badge Display Logic**:
```javascript
function getPriorityBadge(priority) {
  switch (priority) {
    case "Critical":
      return {
        text: "CRITICAL",
        emoji: "🚨",
        color: "red",
        badges: "⚡⚡⚡"
      };
    case "High":
      return {
        text: "HIGH",
        emoji: "⚠️",
        color: "orange",
        badges: "⚡⚡"
      };
    case "Medium":
      return {
        text: "MEDIUM",
        emoji: "⚡",
        color: "yellow",
        badges: "⚡"
      };
    case "Low":
      return {
        text: "LOW",
        emoji: "ℹ️",
        color: "blue",
        badges: ""
      };
  }
}
```

---

## Data Flow Diagrams

### Flow 1: Subscription & Priority Assignment

```
User                Frontend              Backend               Database
  |                    |                     |                     |
  |-- Click "Subscribe Premium" ----------->|                     |
  |                    |                     |                     |
  |                    |-- POST /plan/subscription/subscribe ----->|
  |                    |    { planCode: "PREMIUM" }                |
  |                    |                     |                     |
  |                    |                     |-- Query Plan ------>|
  |                    |                     |<-- Plan Details ----|
  |                    |                     |                     |
  |                    |                     |-- Get Active Sub -->|
  |                    |                     |<-- Old Sub (if any)|
  |                    |                     |                     |
  |                    |                     |-- Cancel Old Sub -->|
  |                    |                     |<-- Success ---------|
  |                    |                     |                     |
  |                    |                     |-- Create New Sub -->|
  |                    |                     |<-- Sub Created -----|
  |                    |                     |                     |
  |                    |<-- 200 OK: { tier: "Premium" } ----------|
  |                    |                     |                     |
  |<-- Show "Upgraded!" |                     |                     |
```

---

### Flow 2: Alert Trigger with Priority Routing

```
Station         Backend                    PriorityService        Database
  |                |                             |                    |
  |-- Water Level |                             |                    |
  |    reaches    |                             |                    |
  |    critical   |                             |                    |
  |               |                             |                    |
  |-------------->|-- Create Alert ------------>|                    |
  |               |   { severity: "critical" }  |                    |
  |               |                             |                    |
  |               |-- Get Subscribed Users -----|                    |
  |               |<-- [User1, User2, User3] ---|                    |
  |               |                             |                    |
  |               |                             |                    |
  | For each user:|                             |                    |
  |               |-- Get User Tier ----------->|                    |
  |               |<-- { tier: "Premium" } -----|                    |
  |               |                             |                    |
  |               |-- Calculate Priority ------>|                    |
  |               |   (severity + tier)         |                    |
  |               |<-- { priority: "High" } ----|                    |
  |               |                             |                    |
  |               |-- Get Dispatch Delay ------>|                    |
  |               |<-- { delay: 0s } -----------|                    |
  |               |                             |                    |
  |               |-- Get Available Channels -->|                    |
  |               |<-- [Push, SMS, Email] ------|                    |
  |               |                             |                    |
  |               |-- Create NotificationLog -->|                    |
  |               |   { priority: "High",       |                    |
  |               |     scheduledAt: now + 0s,  |                    |
  |               |     channels: [Push, SMS] } |                    |
  |               |                             |                    |
  |               |-- Save to DB -------------->|                    |
  |               |<-- Success -----------------|                    |
```

---

### Flow 3: Notification Dispatch with Retry

```
NotificationJob   NotificationLog      FCM/SMS/Email       Database
      |                  |                    |                |
      |-- Poll Pending --|                    |                |
      |<-- Log (0s) -----|                    |                |
      |                  |                    |                |
      |-- Wait 0s -------|                    |                |
      |                  |                    |                |
      |-- Send Push -----|------------------>|                |
      |                  |                    |-- FCM API ---> (Network)
      |                  |                    |<-- 200 OK ----|
      |                  |                    |                |
      |<-- Success ------|                    |                |
      |                  |                    |                |
      |-- Update Log ----|                    |                |
      |   { status: "sent",                   |                |
      |     sentAt: now } |                   |                |
      |                  |                    |                |
      |-- Save to DB ----|                    |--------------->|
      |                  |                    |                |
      |                  |                    |                |
      | IF FAILED:       |                    |                |
      |-- Update Log ----|                    |                |
      |   { status: "pending_retry",          |                |
      |     retryCount: 1,                    |                |
      |     updatedAt: now + 15m }            |                |
      |                  |                    |                |
      |-- Wait 15 min ---|                    |                |
      |-- Retry Send ----|------------------>|                |
      |                  |                    |                |
```

---

## Priority Calculation Matrix

### Formula

```
priority = calculatePriority(severity, userTier)

IF userTier == Monitor:
    IF severity == "critical" → Critical
    IF severity == "warning"  → High
    IF severity == "caution"  → Medium

ELSE IF userTier == Premium:
    IF severity == "critical" → High
    IF severity == "warning"  → Medium
    IF severity == "caution"  → Low

ELSE (Free):
    IF severity == "critical" → Medium
    IF severity == "warning"  → Low
    IF severity == "caution"  → Low
```

### Visual Matrix

| Severity ↓ / Tier → | Free | Premium | Monitor |
|---------------------|------|---------|---------|
| **Critical** | 🟡 Medium | 🟠 High | 🔴 Critical |
| **Warning** | 🔵 Low | 🟡 Medium | 🟠 High |
| **Caution** | 🔵 Low | 🔵 Low | 🟡 Medium |

---

## UI/UX Guidelines

### 1. Pricing Plans Screen

**Components**:
- 3 cards (Free, Premium, Monitor)
- Highlight "Most Popular" (Premium)
- Badge "Current Plan" nếu đã subscribe
- Price display (monthly/yearly toggle)
- Feature comparison table

**Sample**:
```
┌─────────────────────────────────────────────────────────────┐
│  Choose Your Plan                                          │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌───────────┐  ┌───────────────┐  ┌───────────┐          │
│  │   FREE    │  │   PREMIUM ★   │  │  MONITOR  │          │
│  │           │  │ Most Popular  │  │           │          │
│  ├───────────┤  ├───────────────┤  ├───────────┤          │
│  │   $0      │  │    $9.99      │  │  $49.99   │          │
│  │  /month   │  │   /month      │  │  /month   │          │
│  ├───────────┤  ├───────────────┤  ├───────────┤          │
│  │ ✓ Push    │  │ ✓ All Free    │  │ ✓ All     │          │
│  │ ✓ Email   │  │ ✓ SMS         │  │   Premium │          │
│  │ • 60-120s │  │ ⚡ 0-20s      │  │ ⚡⚡ 0-10s │          │
│  │   delay   │  │    delay      │  │    delay  │          │
│  │ • 1 retry │  │ • 3 retries   │  │ • 5 retry │          │
│  ├───────────┤  ├───────────────┤  ├───────────┤          │
│  │ [Current] │  │ [Subscribe]   │  │[Subscribe]│          │
│  └───────────┘  └───────────────┘  └───────────┘          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

### 2. Alert Notification Card (with Priority)

**Priority-based Styling**:
```
// Critical Alert
┌─────────────────────────────────────────┐
│ 🚨 CRITICAL FLOOD ALERT           [×]  │  ← Background: #DC2626 (red)
├─────────────────────────────────────────┤  ← Bold border
│  Trạm Lê Lợi - Pasteur                 │
│  Water Level: 35.2 cm                  │
│  Severity: Critical                     │
│  Priority: Critical ⚡⚡⚡              │  ← 3 lightning bolts
│                                         │
│  Delivered in: 2s ⚡                    │  ← Highlight fast delivery
│  Your Tier: Premium 💎                  │
│                                         │
│  [View Details] [Dismiss]               │
└─────────────────────────────────────────┘

// Low Priority Alert
┌─────────────────────────────────────────┐
│ ℹ️  Flood Watch                   [×]  │  ← Background: #EFF6FF (light blue)
├─────────────────────────────────────────┤
│  Trạm Nguyễn Huệ                        │
│  Water Level: 18.5 cm                  │
│  Severity: Caution                      │
│  Priority: Low                          │
│                                         │
│  Delivered in: 115s                     │
│  Your Tier: Free                        │
│                                         │
│  [View Details] [Dismiss]               │
└─────────────────────────────────────────┘
```

---

### 3. Subscription Status Widget

Hiển thị compact trên header/sidebar:

```
┌─────────────────────────┐
│ 💎 PREMIUM              │
│ Active until Jan 2027   │
│ ⚡ 0-20s delay          │
│ [Manage]                │
└─────────────────────────┘

// OR for Free users:
┌─────────────────────────┐
│ 🆓 FREE PLAN            │
│ Upgrade for faster      │
│ delivery (0-20s)        │
│ [Upgrade Now]           │
└─────────────────────────┘
```

---

### 4. Alert History with Priority Filter

```
┌─────────────────────────────────────────────────────┐
│  Alert History                           [+ Filter] │
├─────────────────────────────────────────────────────┤
│  Filters: [All Severity ▼] [All Priority ▼]        │
│           [Last 7 days ▼]                           │
├─────────────────────────────────────────────────────┤
│                                                     │
│  🚨 CRITICAL - Trạm Lê Lợi           Jan 22, 10:30 │
│     Priority: Critical ⚡⚡⚡         Delivered: 2s  │
│     [View Details]                                  │
│  ─────────────────────────────────────────────────  │
│  ⚠️  WARNING - Trạm Pasteur          Jan 21, 15:20 │
│     Priority: Medium ⚡              Delivered: 18s │
│     [View Details]                                  │
│  ─────────────────────────────────────────────────  │
│  ℹ️  CAUTION - Trạm Nguyễn Huệ       Jan 20, 08:45 │
│     Priority: Low                    Delivered: 95s │
│     [View Details]                                  │
│                                                     │
│  [Load More]                        Page 1 of 5     │
└─────────────────────────────────────────────────────┘
```

---

## Testing Checklist

### For Frontend Team:

#### Subscription Flow
- [ ] Display pricing plans correctly
- [ ] Handle subscribe API call
- [ ] Show confirmation after successful subscription
- [ ] Display current tier in profile/dashboard
- [ ] Handle cancel subscription flow
- [ ] Show downgrade warning when cancelling

#### Priority Notification Display
- [ ] Parse priority field from notification payload
- [ ] Apply correct background color by priority
- [ ] Display lightning bolt badges (1-3 bolts)
- [ ] Play different sounds by priority
- [ ] Set correct vibration pattern
- [ ] Show actual delivery delay

#### Alert History
- [ ] Display priority badge on each alert
- [ ] Filter by priority (Low/Medium/High/Critical)
- [ ] Show channel icons (Push/Email/SMS)
- [ ] Display delivery status (sent/failed/delivered)
- [ ] Show actual delay time
- [ ] Highlight failures with error messages

#### Mobile Push Notification
- [ ] Receive FCM payload with priority field
- [ ] Set Android notification priority correctly
- [ ] Display heads-up notification for Critical alerts
- [ ] Navigate to alert details on tap
- [ ] Group notifications by station/area

---

## Common Errors & Troubleshooting

| Error | Status | Cause | Solution |
|-------|--------|-------|----------|
| "Plan not found" | 404 | PlanCode sai hoặc plan inactive | Dùng `FREE`, `PREMIUM`, `MONITOR` (uppercase) |
| "Cannot cancel Free plan" | 400 | User đang ở Free tier | Không hiển thị nút Cancel nếu đang Free |
| "No active subscription" | 404 | User chưa subscribe hoặc đã expire | Call `GET /subscription/current` để check trước |
| "Invalid duration" | 400 | `durationMonths < 1` hoặc `> 120` | Validate input: 1 ≤ duration ≤ 120 |
| Priority không hiển thị | 200 | Response có priority nhưng không render | Check mapping `priority` → `getPriorityBadge()` |

---

## Performance Considerations

### 1. Notification Dispatch Optimization

**Problem**: Free users có delay 60-120s → có thể tích luỹ hàng nghìn notifications

**Solution**:
- Backend dùng background job (NotificationDispatchJob) để xử lý queue
- Ưu tiên gửi Critical/High priority trước (ORDER BY priority DESC)
- Free users với Low priority xếp cuối queue

### 2. Mobile Battery Impact

**Problem**: High priority notifications có vibration + sound → tốn pin

**Solution**:
- Chỉ dùng heads-up notification cho Critical/High priority
- Low/Medium priority: silent notification hoặc grouped
- Implement "Quiet Hours" từ UserAlertSubscription

### 3. Retry Policy

**Problem**: Network errors có thể gây retry spam

**Solution**:
- Exponential backoff: 5min → 15min → 45min
- Max retries dựa trên tier (Free: 1, Premium: 3, Monitor: 5)
- Sau khi hết retries → mark as "failed" và không gửi lại

---

## Frontend Integration Examples

### React Native - Handle Priority Notification

```javascript
import messaging from '@react-native-firebase/messaging';
import { getNotificationStyle } from './utils/priorityHelper';

messaging().onMessage(async remoteMessage => {
  const { 
    title, 
    body, 
    priority, 
    severity, 
    waterLevel,
    stationName 
  } = remoteMessage.data;

  const style = getNotificationStyle(priority);

  // Display local notification with priority styling
  await notifee.displayNotification({
    title: `${style.emoji} ${title}`,
    body: body,
    android: {
      channelId: `alert-${priority.toLowerCase()}`, // Different channel per priority
      color: style.color,
      priority: style.androidPriority, // 'high' for Critical/High
      sound: style.sound,
      vibrationPattern: style.vibration,
      largeIcon: 'ic_flood_alert',
      badge: priority === 'Critical' ? 1 : 0,
      showTimestamp: true,
      importance: priority === 'Critical' ? 5 : 4, // Max for Critical
    },
    ios: {
      sound: style.sound,
      critical: priority === 'Critical', // iOS Critical Alerts (bypass Do Not Disturb)
      criticalVolume: 1.0,
    },
  });

  // Navigate to alert details if app is foreground
  if (remoteMessage.data.alertId) {
    navigation.navigate('AlertDetails', { 
      alertId: remoteMessage.data.alertId 
    });
  }
});
```

---

### React Web - Subscription Management

```typescript
// components/SubscriptionCard.tsx
import { useState, useEffect } from 'react';
import { subscriptionApi } from '@/api/subscription';

export function SubscriptionCard() {
  const [currentSub, setCurrentSub] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchCurrentSubscription();
  }, []);

  async function fetchCurrentSubscription() {
    try {
      const response = await subscriptionApi.getCurrent();
      setCurrentSub(response.subscription);
    } catch (error) {
      console.error('Failed to fetch subscription:', error);
    } finally {
      setLoading(false);
    }
  }

  async function handleUpgrade(planCode: string) {
    try {
      const response = await subscriptionApi.subscribe({
        planCode,
        durationMonths: 12
      });
      
      if (response.success) {
        toast.success(`Upgraded to ${response.subscription.planName}! 🎉`);
        await fetchCurrentSubscription(); // Refresh
      }
    } catch (error) {
      toast.error('Failed to upgrade subscription');
    }
  }

  async function handleCancel() {
    const confirmed = await showCancelDialog();
    if (!confirmed) return;

    try {
      const response = await subscriptionApi.cancel({
        cancelReason: 'User requested'
      });
      
      if (response.success) {
        toast.info('Subscription cancelled. Back to Free tier.');
        await fetchCurrentSubscription(); // Refresh
      }
    } catch (error) {
      toast.error('Failed to cancel subscription');
    }
  }

  if (loading) return <Skeleton />;

  return (
    <Card>
      <CardHeader>
        <h3>Your Subscription</h3>
      </CardHeader>
      <CardBody>
        {currentSub ? (
          <>
            <Badge color={getTierColor(currentSub.tier)}>
              {currentSub.tier}
            </Badge>
            <p>Active until: {formatDate(currentSub.endDate)}</p>
            <p>Dispatch Delay: {currentSub.benefits.dispatchDelayRange}</p>
            <p>Max Retries: {currentSub.benefits.maxRetries}</p>
            
            {currentSub.tier !== 'Free' && (
              <Button onClick={handleCancel} variant="outline">
                Cancel Subscription
              </Button>
            )}
            
            {currentSub.tier === 'Free' && (
              <Button onClick={() => handleUpgrade('PREMIUM')}>
                Upgrade to Premium
              </Button>
            )}
          </>
        ) : (
          <p>No active subscription</p>
        )}
      </CardBody>
    </Card>
  );
}
```

---

## Summary

**FE-13** implements a **3-tier priority system** (Free/Premium/Monitor) that differentiates notification delivery based on:
1. **Dispatch Delay**: 0-10s (Monitor) → 0-20s (Premium) → 60-120s (Free)
2. **Available Channels**: Monitor/Premium có SMS, Free chỉ có Push/Email
3. **Retry Attempts**: 5 (Monitor) → 3 (Premium) → 1 (Free)
4. **Priority Calculation**: Tier + Severity → Priority Level → Visual Display

**Key APIs**:
- `GET /api/v1/pricing/plans` - List plans
- `POST /api/v1/plan/subscription/subscribe` - Subscribe
- `GET /api/v1/subscription/current` - Current tier
- `DELETE /api/v1/plan/subscription/cancel` - Cancel
- `GET /api/v1/alerts/history` - History with priority

**Frontend Responsibilities**:
- Display pricing comparison
- Handle subscription flow
- Parse priority from notification payload
- Apply visual styling by priority
- Show actual delivery delay
- Implement priority filtering

---

## Next Steps

1. Implement subscription UI components
2. Integrate FCM SDK with priority handling
3. Add priority badges to alert history
4. Test notification delivery with different tiers
5. Implement cancel subscription flow
6. Add analytics tracking for tier conversions

