# FE-19 – Compare AI Predictions with Actual Data

> **Feature Name**: Compare AI Predictions with Actual Data  
> **Goal**: Cho phép hệ thống AI (Python) gửi dự đoán ngập lụt về Backend C#, sau đó tự động so sánh với dữ liệu thực tế từ sensors để đánh giá độ chính xác  
> **Scope**: Backend API (FastEndpoints + CQRS) + Background Job (Hangfire) + Internal Integration API  
> **Related Domain**: `prediction_logs`, `sensor_readings`, `areas`, `stations`  
> **Backend Features**: FeatG76 (Log Prediction), FeatG77 (Get Prediction Comparisons), FeatG78 (Get Prediction Accuracy Statistics), FeatG79 (Verify Predictions Background Job)

---

## 1. Bối cảnh & Vấn đề cần giải quyết

Hiện tại hệ thống đã có:

- **Hệ thống AI (Python)**: Chạy mô hình dự đoán ngập lụt dựa trên dữ liệu lịch sử và thời tiết
- **Backend C#**: Quản lý dữ liệu sensors (`sensor_readings`), stations, areas
- **Background Jobs**: Hangfire đã được setup cho analytics aggregation

**Thiếu**: Cơ chế để:
1. **Nhận dự đoán từ AI**: Python cần gửi prediction về C# backend
2. **Lưu trữ predictions**: Lưu predictions với metadata (area, probability, time range)
3. **Tự động verify**: Khi thời gian dự đoán kết thúc, so sánh với dữ liệu thực tế từ sensors
4. **Đánh giá accuracy**: Tính toán độ chính xác của predictions để cải thiện model

FE-19 sẽ cung cấp:
- **Internal API endpoint** để Python gửi predictions
- **Background job** (Hangfire) tự động verify predictions khi `EndTime` đến
- **Query endpoints** để xem comparison results và accuracy metrics

---

## 2. Mục tiêu nghiệp vụ

- **Mục tiêu chính**: 
  - Cho phép AI system gửi predictions về backend
  - Tự động so sánh predictions với actual data từ sensors
  - Tính toán accuracy metrics để đánh giá chất lượng model

- **Use cases chính**:
  1. **AI System gửi prediction**:
     - Python gọi API `/api/v1/internal/log-prediction`
     - Backend lưu prediction với status `IsVerified = false`
  
  2. **Background Job tự động verify**:
     - Hangfire job chạy định kỳ (mỗi 10 phút)
     - Tìm các predictions đã hết hạn (`EndTime <= Now`) và chưa verify
     - Lấy actual water level từ `sensor_readings` trong khoảng thời gian dự đoán
     - So sánh predicted vs actual, tính accuracy
     - Cập nhật `IsVerified = true`, `ActualWaterLevel`, `IsCorrect`
  
  3. **User/Admin xem comparison results**:
     - Query predictions đã được verify
     - Xem accuracy metrics (tỷ lệ đúng, sai)
     - Filter theo area, time range, accuracy threshold

---

## 3. Nguồn dữ liệu & Quy tắc nghiệp vụ

### 3.1 Bảng dữ liệu liên quan

- `areas`
  - Thông tin khu vực (ward/district) để map predictions
- `sensor_readings`
  - Dữ liệu thực tế từ sensors (water level, measured_at)
- `stations`
  - Thông tin trạm đo, liên kết với areas

### 3.2 Bảng mới cần tạo: `prediction_logs`

```sql
CREATE TABLE prediction_logs (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    area_id             UUID NOT NULL REFERENCES areas(id),
    predicted_prob      NUMERIC(5,4) NOT NULL,  -- 0.0000 - 1.0000
    ai_prob             NUMERIC(5,4),           -- Probability từ AI model
    physics_prob        NUMERIC(5,4),           -- Probability từ physics model
    risk_level          VARCHAR(20),            -- low, medium, high, critical
    start_time          TIMESTAMPTZ NOT NULL,   -- Thời điểm bắt đầu dự đoán
    end_time            TIMESTAMPTZ NOT NULL,   -- Thời điểm kết thúc dự đoán
    actual_water_level  NUMERIC(14,4),          -- Mực nước thực tế (sau khi verify)
    is_verified         BOOLEAN DEFAULT false,  -- Đã verify chưa
    is_correct          BOOLEAN,                -- Dự đoán đúng hay sai
    accuracy_score      NUMERIC(5,4),           -- Điểm accuracy (0-1)
    verified_at         TIMESTAMPTZ,            -- Thời điểm verify
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT chk_prob_range CHECK (predicted_prob >= 0 AND predicted_prob <= 1),
    CONSTRAINT chk_time_range CHECK (end_time > start_time)
);

CREATE INDEX ix_prediction_logs_area_time ON prediction_logs(area_id, start_time DESC);
CREATE INDEX ix_prediction_logs_verified ON prediction_logs(is_verified, end_time);
CREATE INDEX ix_prediction_logs_end_time ON prediction_logs(end_time) WHERE is_verified = false;
```

### 3.3 Định nghĩa "Prediction Log"

Đối với mỗi prediction từ AI, backend lưu:

- **Thông tin dự đoán**:
  - `areaId` – Khu vực được dự đoán
  - `predictedProb` – Xác suất ngập (0.0 - 1.0)
  - `aiProb` – Probability từ AI model
  - `physicsProb` – Probability từ physics model
  - `riskLevel` – Mức độ rủi ro (low/medium/high/critical)
  - `startTime` – Thời điểm bắt đầu dự đoán
  - `endTime` – Thời điểm kết thúc dự đoán

- **Kết quả verify** (sau khi background job chạy):
  - `actualWaterLevel` – Mực nước thực tế từ sensors
  - `isVerified` – Đã verify chưa
  - `isCorrect` – Dự đoán đúng hay sai
  - `accuracyScore` – Điểm accuracy (0-1)
  - `verifiedAt` – Thời điểm verify

### 3.4 Quy tắc verify prediction

**Logic verify (phiên bản V1)**:

1. **Lấy actual water level**:
   - Query `sensor_readings` trong khoảng `[startTime, endTime]`
   - Lọc theo `area_id` (stations thuộc area đó)
   - Lấy `MAX(Value)` làm `actualWaterLevel`

2. **Xác định dự đoán đúng/sai**:
   - Nếu `actualWaterLevel < 0.2m` (safe) và `predictedProb < 0.3` (low risk) → **Đúng**
   - Nếu `actualWaterLevel >= 0.2m` (flood) và `predictedProb >= 0.3` (risk) → **Đúng**
   - Ngược lại → **Sai**

3. **Tính accuracy score**:
   - Nếu đúng: `accuracyScore = 1.0`
   - Nếu sai: `accuracyScore = 1.0 - |predictedProb - actualNormalizedProb|`
   - `actualNormalizedProb = actualWaterLevel / maxWaterLevel` (normalize về 0-1)

**Có thể refine sau**:
- Dùng threshold động theo từng area
- Xét cả duration (thời gian ngập)
- Weighted accuracy dựa trên severity

---

## 4. API Specification

### 4.1 Endpoint 1 – Log Prediction (Internal API)

- **Method**: `POST`
- **Route**: `/api/v1/internal/log-prediction`
- **Auth**: 
  - `AllowAnonymous()` HOẶC API Key authentication (để bảo mật)
  - Khuyến nghị: Dùng API Key trong header `X-API-Key`

#### Request Body

```json
{
  "areaId": "550e8400-e29b-41d4-a716-446655440000",
  "predictedProb": 0.75,
  "aiProb": 0.80,
  "physicsProb": 0.70,
  "riskLevel": "high",
  "startTime": "2026-01-29T10:00:00Z",
  "endTime": "2026-01-29T16:00:00Z"
}
```

#### Response 200 OK

```json
{
  "success": true,
  "message": "Prediction logged successfully",
  "statusCode": 200,
  "data": {
    "predictionLogId": "660e8400-e29b-41d4-a716-446655440000",
    "areaId": "550e8400-e29b-41d4-a716-446655440000",
    "isVerified": false,
    "createdAt": "2026-01-29T10:05:00Z"
  }
}
```

### 4.2 Endpoint 2 – Get Prediction Comparisons

- **Method**: `GET`
- **Route**: `/api/v1/predictions/comparisons`
- **Auth**: `Policies("User")` – Authenticated users

#### Query Params

- `areaId` *(optional)*: Filter theo area
- `startDate` *(optional)*: Start date filter
- `endDate` *(optional)*: End date filter
- `isVerified` *(optional)*: `true`/`false` (default: `true`)
- `minAccuracy` *(optional)*: Minimum accuracy score (0-1)
- `page` *(optional)*: Số trang (default: 1)
- `size` *(optional)*: Số record/trang (default: 50)

#### Response 200 OK

```json
{
  "success": true,
  "message": "Prediction comparisons retrieved successfully",
  "statusCode": 200,
  "data": {
    "total": 100,
    "items": [
      {
        "predictionLogId": "660e8400-e29b-41d4-a716-446655440000",
        "areaId": "550e8400-e29b-41d4-a716-446655440000",
        "areaName": "District 1",
        "predictedProb": 0.75,
        "aiProb": 0.80,
        "physicsProb": 0.70,
        "riskLevel": "high",
        "startTime": "2026-01-29T10:00:00Z",
        "endTime": "2026-01-29T16:00:00Z",
        "actualWaterLevel": 0.85,
        "isVerified": true,
        "isCorrect": true,
        "accuracyScore": 1.0,
        "verifiedAt": "2026-01-29T16:10:00Z",
        "createdAt": "2026-01-29T10:05:00Z"
      }
    ],
    "summary": {
      "totalPredictions": 100,
      "verifiedCount": 95,
      "correctCount": 78,
      "accuracyRate": 0.82,
      "avgAccuracyScore": 0.85
    }
  }
}
```

### 4.3 Endpoint 3 – Get Prediction Accuracy Statistics

- **Method**: `GET`
- **Route**: `/api/v1/predictions/accuracy-stats`
- **Auth**: `Policies("User")`

#### Query Params

- `areaId` *(optional)*: Filter theo area
- `startDate` *(optional)*: Start date
- `endDate` *(optional)*: End date
- `groupBy` *(optional)*: `day`/`week`/`month` (default: `day`)

#### Response 200 OK

```json
{
  "success": true,
  "data": {
    "period": {
      "startDate": "2026-01-01T00:00:00Z",
      "endDate": "2026-01-31T23:59:59Z"
    },
    "overall": {
      "totalPredictions": 500,
      "verifiedCount": 480,
      "correctCount": 390,
      "accuracyRate": 0.8125,
      "avgAccuracyScore": 0.85
    },
    "byPeriod": [
      {
        "period": "2026-01-15",
        "total": 20,
        "correct": 16,
        "accuracyRate": 0.80,
        "avgAccuracyScore": 0.82
      }
    ],
    "byArea": [
      {
        "areaId": "550e8400-e29b-41d4-a716-446655440000",
        "areaName": "District 1",
        "total": 100,
        "correct": 85,
        "accuracyRate": 0.85
      }
    ]
  }
}
```

---

## 5. Thiết kế backend (Clean Architecture)

### 5.1 Domain Layer

#### Entity: `PredictionLog`

```csharp
public class PredictionLog : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
{
    public Guid AreaId { get; set; }
    public decimal PredictedProb { get; set; }  // 0.0000 - 1.0000
    public decimal? AiProb { get; set; }
    public decimal? PhysicsProb { get; set; }
    public string RiskLevel { get; set; } = string.Empty;  // low, medium, high, critical
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    // Verification results
    public decimal? ActualWaterLevel { get; set; }
    public bool IsVerified { get; set; } = false;
    public bool? IsCorrect { get; set; }
    public decimal? AccuracyScore { get; set; }
    public DateTime? VerifiedAt { get; set; }
    
    // Navigation
    [JsonIgnore]
    public virtual Area? Area { get; set; }
}
```

#### Repository Interface: `IPredictionLogRepository`

```csharp
public interface IPredictionLogRepository
{
    Task<Guid> CreateAsync(PredictionLog entity, CancellationToken ct = default);
    Task<PredictionLog?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<PredictionLog>> GetPendingVerificationAsync(DateTime beforeTime, int limit, CancellationToken ct = default);
    Task<List<PredictionLog>> GetVerifiedAsync(
        Guid? areaId, 
        DateTime? startDate, 
        DateTime? endDate, 
        decimal? minAccuracy,
        int page, 
        int size, 
        CancellationToken ct = default);
    Task<bool> UpdateAsync(PredictionLog entity, CancellationToken ct = default);
}
```

### 5.2 Application Layer (CQRS)

#### Command: `LogPredictionRequest`

```csharp
public sealed record LogPredictionRequest(
    Guid AreaId,
    decimal PredictedProb,
    decimal? AiProb,
    decimal? PhysicsProb,
    string RiskLevel,
    DateTime StartTime,
    DateTime EndTime
) : IFeatureRequest<LogPredictionResponse>;
```

#### Handler: `LogPredictionHandler`

- Validate input (probability range, time range)
- Create `PredictionLog` entity
- Save to database
- Return prediction log ID

#### Query: `GetPredictionComparisonsRequest`

```csharp
public sealed record GetPredictionComparisonsRequest(
    Guid? AreaId,
    DateTime? StartDate,
    DateTime? EndDate,
    bool? IsVerified,
    decimal? MinAccuracy,
    int Page,
    int Size
) : IFeatureRequest<GetPredictionComparisonsResponse>;
```

#### Handler: `GetPredictionComparisonsHandler`

- Query verified predictions từ repository
- Calculate summary statistics
- Map to DTOs
- Return paginated results

#### Background Job: `VerifyPredictionsJob` (Hangfire RecurringJob)

- Chạy định kỳ (mỗi 10 phút)
- Tìm predictions chưa verify và đã hết hạn (`EndTime <= Now`)
- Với mỗi prediction:
  - Query `sensor_readings` trong khoảng `[StartTime, EndTime]` cho area đó
  - Tính `ActualWaterLevel = MAX(Value)`
  - So sánh với predicted, tính `IsCorrect` và `AccuracyScore`
  - Update `PredictionLog` với kết quả verify

### 5.3 Presentation Layer (FastEndpoints)

#### Endpoint 1: `LogPredictionEndpoint`

- Route: `POST /api/v1/internal/log-prediction`
- Auth: `AllowAnonymous()` hoặc API Key
- DTOs: `LogPredictionRequestDto`, `LogPredictionResponseDto`
- Inject `IMediator`

#### Endpoint 2: `GetPredictionComparisonsEndpoint`

- Route: `GET /api/v1/predictions/comparisons`
- Auth: `Policies("User")`
- DTOs: `GetPredictionComparisonsRequestDto`, `GetPredictionComparisonsResponseDto`

#### Endpoint 3: `GetPredictionAccuracyStatsEndpoint`

- Route: `GET /api/v1/predictions/accuracy-stats`
- Auth: `Policies("User")`
- DTOs: `GetPredictionAccuracyStatsRequestDto`, `GetPredictionAccuracyStatsResponseDto`

### 5.4 Background Job Registration

Trong `Program.cs` hoặc extension method:

```csharp
// Register Hangfire recurring job
RecurringJob.AddOrUpdate<VerifyPredictionsRunner>(
    "verify-predictions-job",
    runner => runner.RunAsync(),
    "*/10 * * * *",  // Every 10 minutes
    new RecurringJobOptions 
    { 
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh") 
    });
```

---

## 6. Yêu cầu phi chức năng

- **Hiệu năng**:
  - Background job xử lý batch (mỗi lần 50-100 predictions)
  - Index trên `end_time` và `is_verified` để query nhanh
  - Cache accuracy statistics nếu cần

- **Độ trễ**:
  - Internal API response < 200ms
  - Background job verify mỗi 10 phút (có thể điều chỉnh)

- **An toàn & quyền truy cập**:
  - Internal API nên dùng API Key (không expose trong Swagger)
  - Query endpoints yêu cầu authentication

- **Tính mở rộng**:
  - Thiết kế để sau này có thể:
    - Thêm nhiều loại predictions (short-term, long-term)
    - So sánh với nhiều nguồn dữ liệu (sensors, satellite, reports)
    - Machine learning để cải thiện accuracy calculation

---

## 7. Workflow tổng quan

```
┌─────────────────────────────────────────────────────────────┐
│  AI System (Python)                                         │
│  - Chạy model dự đoán                                       │
│  - Tính predictedProb, aiProb, physicsProb                  │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       │ POST /api/v1/internal/log-prediction
                       ▼
┌─────────────────────────────────────────────────────────────┐
│  Backend C# (FastEndpoints)                                 │
│  - LogPredictionEndpoint                                     │
│  - LogPredictionHandler                                      │
│  - Lưu vào prediction_logs (is_verified = false)          │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       │ (Chờ đến EndTime)
                       ▼
┌─────────────────────────────────────────────────────────────┐
│  Hangfire RecurringJob (Mỗi 10 phút)                        │
│  - VerifyPredictionsRunner                                   │
│  - Tìm predictions đã hết hạn                                │
│  - Query sensor_readings để lấy actual data                 │
│  - So sánh predicted vs actual                              │
│  - Update is_verified = true, actual_water_level, is_correct │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       │ (User query)
                       ▼
┌─────────────────────────────────────────────────────────────┐
│  User/Admin                                                  │
│  - GET /api/v1/predictions/comparisons                       │
│  - GET /api/v1/predictions/accuracy-stats                   │
│  - Xem kết quả so sánh và accuracy metrics                   │
└─────────────────────────────────────────────────────────────┘
```

---

## 8. Kết luận

FE-19 cung cấp cơ chế hoàn chỉnh để:

- **Tích hợp AI predictions**: Python gửi predictions về C# backend
- **Tự động verify**: Background job so sánh predictions với actual data
- **Đánh giá accuracy**: Tính toán metrics để cải thiện model
- **Query & Analytics**: Endpoints để xem comparison results

Feature này là nền tảng cho:
- Continuous improvement của AI model
- Monitoring prediction quality
- Data-driven decision making

Class Diagram và Sequence Diagram tương ứng cho FE-19 sẽ được lưu tại:

- `documents/Class.Diagram/FE19_CompareAIPredictionsWithActualData.puml`
- `documents/Sequence.Diagram/FE19_CompareAIPredictionsWithActualData.puml`

---

## 9. Feature Numbers

**Sử dụng**: FeatG76-79

- **FeatG76**: Log Prediction (Internal API)
- **FeatG77**: Get Prediction Comparisons
- **FeatG78**: Get Prediction Accuracy Statistics
- **FeatG79**: Verify Predictions Background Job (Hangfire)

**Note**: Feature numbers FeatG76-79 được assign cho FE-19. Previous features: FeatG47-75 (đã được teammate sử dụng), FeatG42-46 (Alerts & Flood History), FeatG32-37 (Area Management), FeatG7-30 (Auth & Map Preferences).

