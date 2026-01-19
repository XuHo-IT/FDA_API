# Frontend Documentation - FE-12: Alert Preferences & Management

> **Target Audience**: Frontend Developers (Mobile & Web)  
> **Backend Dependencies**: FeatG39, G40, G41 (Alert APIs)  
> **Priority**: HIGH  
> **Estimated Effort**: 5-7 days

---

## 📋 TABLE OF CONTENTS

1. [Overview](#1-overview)
2. [API Endpoints Reference](#2-api-endpoints-reference)
3. [User Flows](#3-user-flows)
4. [UI Components to Build](#4-ui-components-to-build)
5. [Sample Code Implementation](#5-sample-code-implementation)
6. [Error Handling](#6-error-handling)
7. [Testing Checklist](#7-testing-checklist)
8. [Design Guidelines](#8-design-guidelines)

---

## 1. OVERVIEW

### What is FE-12?

FE-12 là hệ thống **Alert Engine** (backend tự động) giám sát cảm biến nước lũ 24/7 và gửi cảnh báo cho người dùng qua:
- 📱 **Push Notification** (Firebase Cloud Messaging)
- 📧 **Email** (SMTP)
- 💬 **SMS** (Twilio/other providers)

### Your Responsibilities (Frontend Team)

Bạn **KHÔNG CẦN** xử lý logic gửi thông báo (backend tự động làm). Nhiệm vụ của bạn là:

✅ **Build UI để user cấu hình:**
- Đăng ký nhận cảnh báo cho khu vực cụ thể
- Chọn kênh nhận thông báo (Push/Email/SMS)
- Đặt mức độ nghiêm trọng tối thiểu (Warning, Critical)

✅ **Build UI để user xem:**
- Lịch sử cảnh báo đã nhận
- Trạng thái gửi thông báo (Sent, Failed)

✅ **Build Admin UI:**
- Xem danh sách user đã đăng ký
- Xem cài đặt thông báo của từng user
- Thống kê hiệu suất gửi thông báo

---

## 2. API ENDPOINTS REFERENCE

### 2.1. Subscribe to Alerts (Đăng Ký Cảnh Báo)

**Endpoint**: `POST /api/v1/alerts/subscriptions `  
**Auth**: Required (JWT Bearer Token)  
**Role**: User, Admin

**Request Body**:
{
  "stationId": "uuid-of-station",
  "areaId": "uuid-of-area",
  "minSeverity": "warning",
  "enablePush": true,
  "enableEmail": true,
  "enableSms": false
}**Field Descriptions**:
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `stationId` | UUID | ✅ | ID của trạm cảm biến (từ API GET /api/v1/stations) |
| `areaId` | UUID | ❌ | ID khu vực người dùng (từ API GET /api/v1/areas/me) |
| `minSeverity` | String | ✅ | Mức độ tối thiểu: `"info"`, `"caution"`, `"warning"`, `"critical"` |
| `enablePush` | Boolean | ✅ | Bật/tắt Push Notification |
| `enableEmail` | Boolean | ✅ | Bật/tắt Email |
| `enableSms` | Boolean | ✅ | Bật/tắt SMS |

**Success Response (201 Created)**:
{
  "success": true,
  "message": "Subscription created successfully",
  "statusCode": 201,
  "data": {
    "subscriptionId": "uuid-123",
    "userId": "uuid-456",
    "stationId": "uuid-789",
    "stationName": "Trạm 1 - Nguyễn Văn Linh",
    "minSeverity": "warning",
    "channels": ["Push", "Email"],
    "createdAt": "2026-01-19T10:30:00Z"
  }
}**Error Response (400 Bad Request)**:
{
  "success": false,
  "message": "Station not found",
  "statusCode": 400
}**Error Response (409 Conflict)**:
{
  "success": false,
  "message": "You have already subscribed to this station",
  "statusCode": 409,
  "data": {
    "existingSubscriptionId": "uuid-existing"
  }
}---

### 2.2. Get My Subscriptions (Danh Sách Đăng Ký Của Tôi)

**Endpoint**: `GET /api/v1/alerts/subscriptions/me`  
**Auth**: Required (JWT Bearer Token)  
**Role**: User, Admin

**Query Parameters**: None

**Success Response (200 OK)**:
{
  "success": true,
  "message": "Retrieved successfully",
  "statusCode": 200,
  "data": {
    "subscriptions": [
      {
        "subscriptionId": "uuid-1",
        "stationId": "uuid-station-1",
        "stationName": "Trạm 1 - Nguyễn Văn Linh",
        "areaId": "uuid-area-1",
        "areaName": "Nhà tôi - Quận 7",
        "minSeverity": "warning",
        "enablePush": true,
        "enableEmail": true,
        "enableSms": false,
        "isActive": true,
        "createdAt": "2026-01-15T08:00:00Z"
      },
      {
        "subscriptionId": "uuid-2",
        "stationId": "uuid-station-2",
        "stationName": "Trạm 2 - Võ Văn Kiệt",
        "areaId": null,
        "areaName": null,
        "minSeverity": "critical",
        "enablePush": true,
        "enableEmail": false,
        "enableSms": true,
        "isActive": true,
        "createdAt": "2026-01-16T09:30:00Z"
      }
    ],
    "totalCount": 2
  }
}---

### 2.3. Update Alert Preferences (Cập Nhật Cài Đặt)

**Endpoint**: `PUT /api/v1/alerts/preferences/{subscriptionId}`  
**Auth**: Required (JWT Bearer Token)  
**Role**: User, Admin

**Path Parameter**:
- `subscriptionId`: UUID của subscription cần update

**Request Body**:
{
  "minSeverity": "critical",
  "enablePush": true,
  "enableEmail": false,
  "enableSms": true
}**Success Response (200 OK)**:
{
  "success": true,
  "message": "Preferences updated successfully",
  "statusCode": 200,
  "data": {
    "subscriptionId": "uuid-123",
    "minSeverity": "critical",
    "channels": ["Push", "SMS"],
    "updatedAt": "2026-01-19T11:00:00Z"
  }
}---

### 2.4. Delete Subscription (Hủy Đăng Ký)

**Endpoint**: `DELETE /api/v1/alerts/subscriptions/{subscriptionId}`  
**Auth**: Required (JWT Bearer Token)  
**Role**: User, Admin

**Success Response (200 OK)**:
{
  "success": true,
  "message": "Subscription deleted successfully",
  "statusCode": 200
}---

### 2.5. Get Alert History (Lịch Sử Cảnh Báo)

**Endpoint**: `GET /api/v1/alerts/history`  
**Auth**: Required (JWT Bearer Token)  
**Role**: User, Admin

**Query Parameters**:
| Parameter | Type | Required | Description | Example |
|-----------|------|----------|-------------|---------|
| `page` | Integer | ❌ | Số trang (default: 1) | `1` |
| `pageSize` | Integer | ❌ | Số item/trang (default: 20, max: 100) | `20` |
| `severity` | String | ❌ | Lọc theo mức độ | `warning` |
| `stationId` | UUID | ❌ | Lọc theo trạm | `uuid-123` |
| `fromDate` | ISO8601 | ❌ | Từ ngày | `2026-01-01T00:00:00Z` |
| `toDate` | ISO8601 | ❌ | Đến ngày | `2026-01-19T23:59:59Z` |

**Success Response (200 OK)**:
{
  "success": true,
  "message": "Retrieved successfully",
  "statusCode": 200,
  "data": {
    "alerts": [
      {
        "alertId": "uuid-alert-1",
        "stationId": "uuid-station-1",
        "stationName": "Trạm 1 - Nguyễn Văn Linh",
        "severity": "warning",
        "priority": "Medium",
        "message": "Water level exceeded 2.5m threshold",
        "currentValue": 2.8,
        "unit": "m",
        "triggeredAt": "2026-01-19T10:30:00Z",
        "resolvedAt": null,
        "status": "open",
        "notifications": [
          {
            "channel": "Push",
            "status": "sent",
            "sentAt": "2026-01-19T10:30:15Z",
            "deliveredAt": "2026-01-19T10:30:18Z"
          },
          {
            "channel": "Email",
            "status": "sent",
            "sentAt": "2026-01-19T10:30:20Z",
            "deliveredAt": "2026-01-19T10:30:45Z"
          }
        ]
      },
      {
        "alertId": "uuid-alert-2",
        "stationId": "uuid-station-2",
        "stationName": "Trạm 2 - Võ Văn Kiệt",
        "severity": "critical",
        "priority": "High",
        "message": "Water level exceeded 3.5m threshold - CRITICAL",
        "currentValue": 3.9,
        "unit": "m",
        "triggeredAt": "2026-01-18T14:00:00Z",
        "resolvedAt": "2026-01-18T18:30:00Z",
        "status": "resolved",
        "notifications": [
          {
            "channel": "Push",
            "status": "sent",
            "sentAt": "2026-01-18T14:00:10Z",
            "deliveredAt": "2026-01-18T14:00:12Z"
          },
          {
            "channel": "SMS",
            "status": "failed",
            "sentAt": "2026-01-18T14:00:15Z",
            "deliveredAt": null,
            "errorMessage": "Phone number not verified",
            "retryCount": 3
          }
        ]
      }
    ],
    "pagination": {
      "currentPage": 1,
      "pageSize": 20,
      "totalPages": 3,
      "totalCount": 45
    }
  }
}---

### 2.6. Admin: Get All User Subscriptions

**Endpoint**: `GET /api/v1/admin/alerts/subscriptions`  
**Auth**: Required (JWT Bearer Token)  
**Role**: Admin only

**Query Parameters**:
| Parameter | Type | Description |
|-----------|------|-------------|
| `page` | Integer | Số trang |
| `pageSize` | Integer | Số item/trang |
| `userId` | UUID | Lọc theo user ID |
| `stationId` | UUID | Lọc theo trạm |

**Success Response (200 OK)**:
{
  "success": true,
  "message": "Retrieved successfully",
  "statusCode": 200,
  "data": {
    "subscriptions": [
      {
        "subscriptionId": "uuid-1",
        "userId": "uuid-user-1",
        "userEmail": "user1@example.com",
        "userPhone": "+84901234567",
        "stationId": "uuid-station-1",
        "stationName": "Trạm 1 - Nguyễn Văn Linh",
        "minSeverity": "warning",
        "enablePush": true,
        "enableEmail": true,
        "enableSms": false,
        "isActive": true,
        "totalAlertsReceived": 15,
        "lastAlertAt": "2026-01-19T10:30:00Z",
        "createdAt": "2026-01-10T08:00:00Z"
      }
    ],
    "pagination": {
      "currentPage": 1,
      "pageSize": 50,
      "totalPages": 10,
      "totalCount": 487
    }
  }
}---

### 2.7. Admin: Get Notification Statistics

**Endpoint**: `GET /api/v1/admin/alerts/stats`  
**Auth**: Required (JWT Bearer Token)  
**Role**: Admin only

**Query Parameters**:
| Parameter | Type | Description |
|-----------|------|-------------|
| `fromDate` | ISO8601 | Từ ngày (default: 24h trước) |
| `toDate` | ISO8601 | Đến ngày (default: hiện tại) |

**Success Response (200 OK)**:
{
  "success": true,
  "message": "Retrieved successfully",
  "statusCode": 200,
  "data": {
    "period": {
      "from": "2026-01-18T12:00:00Z",
      "to": "2026-01-19T12:00:00Z"
    },
    "alerts": {
      "total": 45,
      "bySeverity": {
        "info": 10,
        "caution": 15,
        "warning": 18,
        "critical": 2
      },
      "byStatus": {
        "open": 12,
        "resolved": 33
      }
    },
    "notifications": {
      "totalCreated": 135,
      "totalSent": 128,
      "totalFailed": 7,
      "totalPending": 0,
      "byChannel": {
        "Push": {
          "sent": 45,
          "failed": 0,
          "successRate": 100
        },
        "Email": {
          "sent": 42,
          "failed": 3,
          "successRate": 93.3
        },
        "SMS": {
          "sent": 41,
          "failed": 4,
          "successRate": 91.1
        }
      },
      "avgDeliveryTimeSeconds": 2.5,
      "pendingRetries": 0
    },
    "users": {
      "totalSubscribers": 487,
      "activeSubscribers": 465,
      "newSubscribers24h": 12
    }
  }
}---

## 3. USER FLOWS

### 3.1. Flow: User Đăng Ký Cảnh Báo
┌─────────────────────────────────────────────────────────────┐
│ SCREEN 1: Chọn Khu Vực/Trạm Muốn Giám Sát                   │
├─────────────────────────────────────────────────────────────┤
│ 1. User mở app/web                                          │
│ 2. Navigate to "Alert Settings" hoặc "Cảnh Báo"            │
│ 3. Tap "Thêm Cảnh Báo Mới" (+)                             │
│ 4. Hiển thị map hoặc list danh sách trạm                   │
│ 5. User chọn trạm (có thể link với khu vực đã tạo)        │
└──────────────────────────┬──────────────────────────────────┘
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ SCREEN 2: Cấu Hình Thông Báo                                │
├─────────────────────────────────────────────────────────────┤
│ Form:                                                       │
│ ┌────────────────────────────────────────────────────────┐ │
│ │ Trạm: Trạm 1 - Nguyễn Văn Linh                        │ │
│ │                                                         │ │
│ │ Mức Độ Tối Thiểu:                                      │ │
│ │ ○ Info (Thông tin)                                     │ │
│ │ ○ Caution (Cẩn trọng)                                  │ │
│ │ ● Warning (Cảnh báo) ← Selected                        │ │
│ │ ○ Critical (Khẩn cấp)                                  │ │
│ │                                                         │ │
│ │ Kênh Nhận Thông Báo:                                   │ │
│ │ ☑ Push Notification (Khuyến nghị)                      │ │
│ │ ☑ Email (abc@example.com)                              │ │
│ │ ☐ SMS (+84901234567) [Yêu cầu xác thực số điện thoại] │ │
│ │                                                         │ │
│ │ [ Hủy ]           [ Lưu Cài Đặt ]                      │ │
│ └────────────────────────────────────────────────────────┘ │
└──────────────────────────┬──────────────────────────────────┘
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ ACTION: API Call                                            │
├─────────────────────────────────────────────────────────────┤
│ POST /api/v1/alerts/subscriptions                           │
│ {                                                           │
│   "stationId": "uuid-station-1",                           │
│   "minSeverity": "warning",                                │
│   "enablePush": true,                                      │
│   "enableEmail": true,                                     │
│   "enableSms": false                                       │
│ }                                                           │
└──────────────────────────┬──────────────────────────────────┘
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ SUCCESS: Show Confirmation                                  │
├─────────────────────────────────────────────────────────────┤
│ ✅ "Đã đăng ký cảnh báo thành công!"                       │
│                                                             │
│ Bạn sẽ nhận thông báo qua:                                 │
│ • Push Notification                                        │
│ • Email (abc@example.com)                                  │
│                                                             │
│ Khi mực nước vượt mức Cảnh báo (Warning) trở lên.         │
│                                                             │
│ [ OK ]                                                      │
└─────────────────────────────────────────────────────────────┘

### 3.2. Flow: User Xem & Chỉnh Sửa Cài Đặt
┌─────────────────────────────────────────────────────────────┐
│ SCREEN: Danh Sách Cảnh Báo Đã Đăng Ký                       │
├─────────────────────────────────────────────────────────────┤
│ API: GET /api/v1/alerts/subscriptions/me                   │
│                                                             │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ 📍 Trạm 1 - Nguyễn Văn Linh                            │ │
│ │    Khu vực: Nhà tôi - Quận 7                           │ │
│ │    Mức độ: ⚠️ Warning                                  │ │
│ │    Kênh: 📱 Push, 📧 Email                             │ │
│ │    [Chỉnh Sửa] [Xóa]                                   │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ 📍 Trạm 2 - Võ Văn Kiệt                                │ │
│ │    Khu vực: Không có                                   │ │
│ │    Mức độ: 🚨 Critical                                 │ │
│ │    Kênh: 📱 Push, 💬 SMS                               │ │
│ │    [Chỉnh Sửa] [Xóa]                                   │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ [ + Thêm Cảnh Báo Mới ]                                     │
└─────────────────────────────────────────────────────────────┘
                           │
                           │ User tap [Chỉnh Sửa]
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ MODAL: Chỉnh Sửa Cài Đặt                                    │
├─────────────────────────────────────────────────────────────┤
│ Trạm: Trạm 1 - Nguyễn Văn Linh (không thể đổi)             │
│                                                             │
│ Mức Độ Tối Thiểu:                                          │
│ ○ Info  ○ Caution  ● Warning  ○ Critical                   │
│                                                             │
│ Kênh Nhận Thông Báo:                                       │
│ ☑ Push Notification                                        │
│ ☐ Email                                                     │
│ ☑ SMS                                                       │
│                                                             │
│ [ Hủy ]           [ Lưu Thay Đổi ]                         │
└──────────────────────────┬──────────────────────────────────┘
                           ▼
                   PUT /api/v1/alerts/subscriptions/{id}
                           ▼
                   ✅ "Đã cập nhật thành công!"


### 3.3. Flow: User Xem Lịch Sử Cảnh Báo
┌─────────────────────────────────────────────────────────────┐
│ SCREEN: Lịch Sử Cảnh Báo                                    │
├─────────────────────────────────────────────────────────────┤
│ API: GET /api/v1/alerts/history?page=1&pageSize=20         │
│                                                             │
│ [Filter: All Severity ▼] [Station: All ▼] [Date Range]    │
│                                                             │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ ⚠️ WARNING · 19/01/2026 10:30                          │ │
│ │ Trạm 1 - Nguyễn Văn Linh                               │ │
│ │ Mực nước: 2.8m (Vượt ngưỡng 2.5m)                      │ │
│ │ Đã gửi: 📱 Push ✅ · 📧 Email ✅                        │ │
│ │ Trạng thái: 🟢 Đang diễn ra                            │ │
│ │ [Xem Chi Tiết]                                          │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ 🚨 CRITICAL · 18/01/2026 14:00                         │ │
│ │ Trạm 2 - Võ Văn Kiệt                                   │ │
│ │ Mực nước: 3.9m (Vượt ngưỡng 3.5m)                      │ │
│ │ Đã gửi: 📱 Push ✅ · 💬 SMS ❌ (Thất bại)              │ │
│ │ Trạng thái: ✅ Đã giải quyết (18:30)                   │ │
│ │ [Xem Chi Tiết]                                          │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ [Previous] Page 1 of 3 [Next]                              │
└─────────────────────────────────────────────────────────────┘

9. FIREBASE CLOUD MESSAGING (FCM) SETUP
9.1. Initialize FCM (Web)
// firebase-config.ts
import { initializeApp } from 'firebase/app';
import { getMessaging, getToken, onMessage } from 'firebase/messaging';

const firebaseConfig = {
  apiKey: process.env.REACT_APP_FIREBASE_API_KEY,
  authDomain: process.env.REACT_APP_FIREBASE_AUTH_DOMAIN,
  projectId: process.env.REACT_APP_FIREBASE_PROJECT_ID,
  messagingSenderId: process.env.REACT_APP_FIREBASE_MESSAGING_SENDER_ID,
  appId: process.env.REACT_APP_FIREBASE_APP_ID
};

const app = initializeApp(firebaseConfig);
export const messaging = getMessaging(app);

11. DEPLOYMENT CHECKLIST
11.1. Environment Variables
# Frontend .env
REACT_APP_API_BASE_URL=https://api.floodwarning.com
REACT_APP_FIREBASE_API_KEY=xxx
REACT_APP_FIREBASE_AUTH_DOMAIN=xxx
REACT_APP_FIREBASE_PROJECT_ID=xxx
REACT_APP_FIREBASE_MESSAGING_SENDER_ID=xxx
REACT_APP_FIREBASE_APP_ID=xxx
REACT_APP_FIREBASE_VAPID_KEY=xxx

Most Used APIs
# Get my subscriptions
GET /api/v1/alerts/subscriptions/me

# Create subscription
POST /api/v1/alerts/subscriptions 

# Update preferences
PUT /api/v1/alerts/subscriptions/{id}

# Delete subscription
DELETE /api/v1/alerts/subscriptions/{id}

# Get alert history
GET /api/v1/alerts/history?page=1&pageSize=20