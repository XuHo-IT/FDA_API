# FE-11: Receive Area-based Flood Updates
## Tài liệu API dành cho Frontend

---

## Mô Tả Tính Năng
- **FE-11**: Nhận cập nhật lũ lụt theo khu vực (Area)
- **Logic**: Cập nhật status theo từng khu vực: `Normal` / `Watch` / `Warning`
- **In-app updates feed**: Hiển thị danh sách các khu vực mà user đã đăng ký theo dõi (My Areas)
- **Test**: Tần suất cập nhật, không spam thông báo

---

## 1. API Endpoints

### 1.1. Lấy Danh Sách My Areas (Các khu vực user đã tạo)

**Endpoint:**
```
GET /api/v1/areas/me
Authorization: Bearer <token>
```

**Response:**
```json
{
  "success": true,
  "message": "User areas retrieved successfully",
  "statusCode": 200,
  "areas": [
    {
      "id": "guid",
      "name": "Khu vực Quận 1",
      "latitude": 10.7769,
      "longitude": 106.7009,
      "radiusMeters": 500,
      "addressText": "Quận 1, TP.HCM"
    }
  ],
  "totalCount": 1
}
```

---

### 1.2. Lấy Flood Status Của Một Area

**Endpoint:**
```
GET /api/v1/area/areas/{areaId}/status
```
> ⚠️ **Lưu ý**: Endpoint này `AllowAnonymous` - không cần token

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `areaId` | GUID | ID của area cần lấy status |

**Response:**
```json
{
  "success": true,
  "message": "Area status evaluated successfully",
  "data": {
    "areaId": "guid",
    "status": "Normal",
    "severityLevel": 0,
    "summary": "Area status is currently Normal. All nearby sensors report safe levels.",
    "contributingStations": [
      {
        "stationId": "guid",
        "stationCode": "ST001",
        "distance": 150.5,
        "waterLevel": 0.5,
        "severity": "safe",
        "weight": 0
      }
    ],
    "evaluatedAt": "2026-03-04T10:30:00Z"
  }
}
```

---

## 2. Flood Status Values

| Status | SeverityLevel | Mô Tả |
|--------|---------------|-------|
| `Normal` | 0 | Mực nước an toàn |
| `Watch` | 2 | Cảnh báo - mực nước cao |
| `Warning` | 3 | Nguy hiểm - mực nước nguy hiểm |
| `Unknown` | -1 | Không có dữ liệu cảm biến |

---

## 3. Logic Tính Toán Status

### 3.1. Cách Tính
1. **Tìm stations trong bán kính**: Lấy tất cả các station active trong `radiusMeters` của area
2. **Lấy dữ liệu cảm biến mới nhất**: Gọi API lấy readings của các stations gần nhất
3. **Tính severity**:
   - `critical` (weight=3): waterLevel >= `ThresholdCritical`
   - `warning` (weight=2): waterLevel >= `ThresholdWarning`
   - `safe` (weight=0): waterLevel > 0 và < ThresholdWarning
   - `unknown` (weight=-1): không có data
4. **Xác định status**: Lấy max weight của các stations

### 3.2. Công Thức
```
Status = Max(contributingStations[].Weight)
- Weight = 3 → Warning
- Weight = 2 → Watch
- Weight = 0 → Normal
- Weight = -1 → Unknown
```

### 3.3. Caching
- **Cache duration**: 30 giây
- Key format: `area_status_{areaId}`
- Tránh spam API - FE nên cache response ở client

---

## 4. Luồng Xử Lý Notifications (Background Job)

### 4.1. Quy Trình Gửi Thông Báo
```
Alert Created → Find Subscriptions (Area-based) → Filter by User Tier
→ Check Severity Threshold → Check Quiet Hours → Create Notification Logs
→ Dispatch (Push/Email/SMS/In-App)
```

### 4.2. Area-Based Subscription Logic
- Khi Alert được tạo cho một Station
- Tìm tất cả Areas chứa Station đó (dựa vào lat/long + radius)
- Lấy subscriptions của các Areas đó
- Gửi notification cho user đã subscribe

### 4.3. Spam Prevention
- **Cooldown**: Có cơ chế cooldown giữa các notifications
- **User Tier**: Tùy thuộc vào subscription tier
- **Quiet Hours**: User có thể cài đặt giờ im lặng
- **Severity Threshold**: User chỉ nhận notification khi đạt mức severity tối thiểu

---

## 5. Hướng Dẫn Test Cho FE

### 5.1. Test Cases

#### TC01: Lấy danh sách My Areas
```bash
GET /api/v1/areas/me
Authorization: Bearer <user_token>
```
**Expected:**
- Trả về danh sách các areas user đã tạo
- TotalCount phản ánh đúng số lượng

#### TC02: Lấy status của một area (Normal)
```bash
GET /api/v1/area/areas/{areaId}/status
```
**Expected:**
- Status = "Normal" khi tất cả stations gần đều báo "safe"

#### TC03: Lấy status của một area (Watch)
**Setup:** Tạo alert với mức warning cho station trong area
**Expected:**
- Status = "Watch"
- Summary mô tả: "Watch: High water level detected at Station X"

#### TC04: Lấy status của một area (Warning)
**Setup:** Tạo alert với mức critical cho station trong area
**Expected:**
- Status = "Warning"
- Summary mô tả: "Warning: Critical water level detected at Station X"

#### TC05: Area không có station trong bán kính
**Expected:**
- Status = "Unknown"
- Summary: "No sensors found within monitoring range."

#### TC06: Kiểm tra tần suất cập nhật
**Steps:**
1. Gọi API status 2 lần trong vòng 30 giây
2. Kiểm tra `Message` trong response
**Expected:**
- Lần 2: Message = "Area status retrieved from cache"

#### TC07: Kiểm tra spam
**Setup:**
1. Tạo alert liên tục cho cùng một station
2. User subscribe area chứa station đó
**Expected:**
- Không nhận quá nhiều notifications trong thời gian ngắn
- Có cooldown giữa các notifications

---

## 6. Response Codes

| Status Code | Mô Tả |
|-------------|-------|
| 200 | Success |
| 400 | Bad Request |
| 401 | Unauthorized |
| 404 | Area Not Found |
| 500 | Internal Server Error |

---

## 7. Lưu Ý Quan Trọng

1. **Caching ở Client**: FE nên cache response trong 30 giây để tránh spam API
2. **Polling Interval**: Khuyến nghị poll mỗi 30-60 giây
3. **Real-time Updates**: Cần implement WebSocket hoặc polling để nhận updates ngay lập tức
4. **Thứ tự ưu tiên**: Warning > Watch > Normal
5. **Station weights**: Status được xác định bởi station có severity cao nhất

---

## 8. Related APIs

- **FE-39**: Subscribe to Alerts (đăng ký nhận thông báo theo area/station)
- **FE-41**: Update Alert Preferences (cập nhật tùy chọn nhận thông báo)
- **FE-43**: Dispatch Notifications (background job xử lý gửi notification)