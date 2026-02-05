# FE-20 Interpretability Metadata - Complete Documentation

## 📋 Table of Contents

1. [Implementation Summary](#implementation-summary)
2. [Architecture Details](#architecture-details)
3. [Database Schema](#database-schema)
4. [API Endpoints](#api-endpoints)
5. [Metadata Structure](#metadata-structure)
6. [Implementation Guide](#implementation-guide)
7. [Query & Analytics](#query--analytics)
8. [Test Data & Test Cases](#test-data--test-cases)
9. [Deployment Guide](#deployment-guide)

---

# Implementation Summary

## Overview

**Feature Code**: FE-20  
**Feature Name**: Interpretability Metadata for Prediction Logs  
**Implementation Date**: 2026-02-05  
**Status**: ✅ Completed - Ready for Testing  
**Architecture**: Domain-Centric Architecture with CQRS Pattern (MediatR)

---

## Requirements Implemented

### Core Features

1. **Metadata Reception**:
   - Python backend gửi metadata (JSON string) cùng với prediction data
   - C# backend nhận và validate JSON format
   - Lưu trữ metadata trong PostgreSQL dạng JSONB

2. **Metadata Structure**:
   - `dominant_factor`: Nhân tố chính ảnh hưởng dự đoán
   - `geographical_context`: Mô tả bối cảnh địa lý
   - `historical_similarity`: Sự kiện lịch sử tương tự nhất
   - `explanation`: Giải thích bằng tiếng Việt
   - `confidence_level`: Mức độ tự tin (LOW/MEDIUM/HIGH)
   - `area_id`: ID khu vực dự đoán
   - `generated_at`: Thời gian tạo metadata

3. **Database Storage**:
   - Column `Metadata` kiểu `jsonb` trong bảng `prediction_logs`
   - Index GIN cho query nhanh trên metadata
   - Default value: `"{}"` (empty JSON object)

4. **Query & Analytics**:
   - Query metadata bằng PostgreSQL JSONB operators (`->`, `->>`)
   - Phân tích dominant factors, confidence levels
   - Tìm predictions với historical similarity cao

---

# Architecture Details

## Layer Breakdown

### 1. Presentation Layer (`src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi`)

#### Endpoint Created/Modified

**LogPredictionEndpoint.cs** - Internal API endpoint

- **Route**: `POST /api/v1/internal/log-prediction`
- **Request DTO**: `LogPredictionRequestDto` (đã thêm `Metadata` property)
- **Response DTO**: `LogPredictionResponseDto`

**Changes**:
- Thêm property `Metadata` (string, nullable) vào `LogPredictionRequestDto`
- Map `Metadata` từ DTO sang `LogPredictionRequest`

---

### 2. Application Layer (`src/Core/Application/FDAAPI.App.FeatG76_LogPrediction`)

#### Request/Response Modified

**LogPredictionRequest.cs** - MediatR request record

```csharp
public sealed record LogPredictionRequest(
    Guid AdministrativeAreaId,
    decimal PredictedProb,
    decimal? AiProb,
    decimal? PhysicsProb,
    string RiskLevel,
    DateTime StartTime,
    DateTime EndTime,
    string? Metadata  // FE-20: Interpretability metadata
) : IFeatureRequest<LogPredictionResponse>;
```

**LogPredictionHandler.cs** - Handler logic

**Changes**:
- Nhận `Metadata` từ request
- Set `predictionLog.Metadata = request.Metadata ?? "{}";` khi tạo entity

---

### 3. Domain Layer (`src/Core/Domain/FDAAPI.Domain.RelationalDb`)

#### Entity Modified

**PredictionLog.cs** - Core prediction log entity

**Added Property**:
```csharp
public string Metadata { get; set; } = "{}";  // FE-20: Interpretability metadata (JSONB)
```

#### Configuration Modified

**PredictionLogConfiguration.cs** - EF Core configuration

**Added Configuration**:
```csharp
builder.Property(e => e.Metadata)
    .HasColumnType("jsonb")
    .IsRequired()
    .HasDefaultValue("{}");
```

**Added Index**:
```csharp
// Index for JSONB queries
migrationBuilder.Sql(
    "CREATE INDEX IF NOT EXISTS ix_prediction_logs_metadata_gin " +
    "ON prediction_logs USING GIN ((\"Metadata\"));");
```

---

# Database Schema

## Table: `prediction_logs`

### New Column Added

| Column Name | Data Type | Nullable | Default | Description |
|------------|-----------|----------|---------|-------------|
| `Metadata` | `jsonb` | NOT NULL | `"{}"` | FE-20: Interpretability metadata (JSON) |

### Index Added

```sql
CREATE INDEX ix_prediction_logs_metadata_gin 
ON prediction_logs USING GIN ((Metadata));
```

### Migration

**Migration Name**: `AddMetadataToPredictionLogs`

**SQL Generated**:
```sql
ALTER TABLE prediction_logs 
ADD COLUMN "Metadata" jsonb NOT NULL DEFAULT '{}';

CREATE INDEX ix_prediction_logs_metadata_gin 
ON prediction_logs USING GIN ((Metadata));
```

---

# API Endpoints

## POST /api/v1/internal/log-prediction

### Request

**Endpoint**: `POST /api/v1/internal/log-prediction`  
**Authentication**: Anonymous (Internal API)  
**Content-Type**: `application/json`

**Request Body**:
```json
{
  "administrativeAreaId": "550e8400-e29b-41d4-a716-446655440000",
  "predictedProb": 0.675,
  "aiProb": 0.68,
  "physicsProb": 0.65,
  "riskLevel": "HIGH",
  "startTime": "2026-02-05T14:30:00Z",
  "endTime": "2026-02-05T15:30:00Z",
  "metadata": "{\"dominant_factor\":{\"factor_name\":\"rainfall\",\"factor_name_vietnamese\":\"Lượng mưa\",\"contribution_percent\":52.3,\"impact_description\":\"Mưa lớn là nguyên nhân chính gây ra nguy cơ lũ quét\"},\"geographical_context\":\"Khu vực miền núi - Nguy cơ lũ quét cao\",\"historical_similarity\":{\"event_name\":\"Bão Sơn Ca 2022\",\"similarity_score\":0.87,\"event_year\":2022,\"casualties\":65,\"economic_damage_million_usd\":125.5,\"rainfall_mm\":891.5,\"key_characteristics\":[\"Mưa lớn\",\"Gió mạnh\",\"Ngập lụt diện rộng\"]},\"explanation\":\"Nguy cơ cao do lượng mưa (52.3%) tương tự Bão Sơn Ca (2022), độ tương tự 87%\",\"confidence_level\":\"HIGH\",\"area_id\":\"district-hoa-vang\",\"generated_at\":\"2026-02-05T14:30:45.123456\"}"
}
```

**Request Fields**:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `administrativeAreaId` | `Guid` | Yes | ID của administrative area |
| `predictedProb` | `decimal` | Yes | Xác suất dự đoán (0-1) |
| `aiProb` | `decimal?` | No | Xác suất từ AI model |
| `physicsProb` | `decimal?` | No | Xác suất từ physics model |
| `riskLevel` | `string` | Yes | Mức độ rủi ro (low/medium/high/critical) |
| `startTime` | `DateTime` | Yes | Thời gian bắt đầu |
| `endTime` | `DateTime` | Yes | Thời gian kết thúc |
| `metadata` | `string?` | No | FE-20: JSON string chứa interpretability metadata |

### Response

**Success Response (201 Created)**:
```json
{
  "success": true,
  "message": "Prediction logged successfully for Administrative Area Hòa Vang (district).",
  "statusCode": 201,
  "data": {
    "predictionLogId": "660e8400-e29b-41d4-a716-446655440000",
    "areaId": null,
    "administrativeAreaId": "550e8400-e29b-41d4-a716-446655440000",
    "isVerified": false,
    "createdAt": "2026-02-05T14:30:45Z"
  }
}
```

**Error Response (400 Bad Request)**:
```json
{
  "success": false,
  "message": "Invalid metadata JSON format: Unexpected character encountered while parsing value.",
  "statusCode": 400
}
```

**Error Response (404 Not Found)**:
```json
{
  "success": false,
  "message": "Administrative Area with ID 550e8400-e29b-41d4-a716-446655440000 not found.",
  "statusCode": 404
}
```

---

# Metadata Structure

## Complete JSON Schema

```json
{
  "dominant_factor": {
    "factor_name": "rainfall",
    "factor_name_vietnamese": "Lượng mưa",
    "contribution_percent": 52.3,
    "impact_description": "Mưa lớn là nguyên nhân chính gây ra nguy cơ lũ quét"
  },
  "geographical_context": "Khu vực miền núi - Nguy cơ lũ quét cao",
  "historical_similarity": {
    "event_name": "Bão Sơn Ca 2022",
    "similarity_score": 0.87,
    "event_year": 2022,
    "casualties": 65,
    "economic_damage_million_usd": 125.5,
    "rainfall_mm": 891.5,
    "key_characteristics": [
      "Mưa lớn",
      "Gió mạnh",
      "Ngập lụt diện rộng"
    ]
  },
  "explanation": "Nguy cơ cao do lượng mưa (52.3%) tương tự Bão Sơn Ca (2022), độ tương tự 87%",
  "confidence_level": "HIGH",
  "area_id": "district-hoa-vang",
  "generated_at": "2026-02-05T14:30:45.123456"
}
```

## Field Descriptions

| Field | Type | Required | Description | Example |
|-------|------|----------|-------------|---------|
| `dominant_factor` | Object | Yes | Nhân tố chính ảnh hưởng dự đoán | `{factor_name: "rainfall", contribution_percent: 52.3}` |
| `dominant_factor.factor_name` | String | Yes | Tên nhân tố (tiếng Anh) | `"rainfall"` |
| `dominant_factor.factor_name_vietnamese` | String | Yes | Tên nhân tố (tiếng Việt) | `"Lượng mưa"` |
| `dominant_factor.contribution_percent` | Number | Yes | Phần trăm đóng góp (0-100) | `52.3` |
| `dominant_factor.impact_description` | String | Yes | Mô tả tác động | `"Mưa lớn là..."` |
| `geographical_context` | String | Yes | Mô tả bối cảnh địa lý | `"Khu vực miền núi"` |
| `historical_similarity` | Object | Yes | Sự kiện lịch sử tương tự | `{event_name: "Bão Sơn Ca", similarity_score: 0.87}` |
| `historical_similarity.event_name` | String | Yes | Tên sự kiện | `"Bão Sơn Ca 2022"` |
| `historical_similarity.similarity_score` | Number | Yes | Độ tương tự (0-1) | `0.87` |
| `historical_similarity.event_year` | Number | Yes | Năm xảy ra | `2022` |
| `historical_similarity.casualties` | Number | No | Số người thiệt mạng | `65` |
| `historical_similarity.economic_damage_million_usd` | Number | No | Thiệt hại kinh tế (triệu USD) | `125.5` |
| `historical_similarity.rainfall_mm` | Number | No | Lượng mưa (mm) | `891.5` |
| `historical_similarity.key_characteristics` | Array | No | Đặc điểm chính | `["Mưa lớn", "Gió mạnh"]` |
| `explanation` | String | Yes | Giải thích bằng tiếng Việt | `"Nguy cơ cao do..."` |
| `confidence_level` | String | Yes | Mức độ tự tin | `"HIGH"`, `"MEDIUM"`, `"LOW"` |
| `area_id` | String | Yes | ID khu vực (Python format) | `"district-hoa-vang"` |
| `generated_at` | String (ISO 8601) | Yes | Thời gian tạo metadata | `"2026-02-05T14:30:45.123456"` |

---

# Implementation Guide

## Step-by-Step Implementation

### Step 1: Update Request DTO

**File**: `src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/Endpoints/FeatG76_LogPrediction/DTOs/LogPredictionRequestDto.cs`

```csharp
public class LogPredictionRequestDto
{
    public Guid AdministrativeAreaId { get; set; }
    public decimal PredictedProb { get; set; }
    public decimal? AiProb { get; set; }
    public decimal? PhysicsProb { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    // FE-20: Interpretability metadata (JSON string)
    public string? Metadata { get; set; }
}
```

### Step 2: Update Application Request

**File**: `src/Core/Application/FDAAPI.App.FeatG76_LogPrediction/LogPredictionRequest.cs`

```csharp
public sealed record LogPredictionRequest(
    Guid AdministrativeAreaId,
    decimal PredictedProb,
    decimal? AiProb,
    decimal? PhysicsProb,
    string RiskLevel,
    DateTime StartTime,
    DateTime EndTime,
    string? Metadata  // FE-20: Interpretability metadata
) : IFeatureRequest<LogPredictionResponse>;
```

### Step 3: Update Endpoint Mapping

**File**: `src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/Endpoints/FeatG76_LogPrediction/LogPredictionEndpoint.cs`

```csharp
var request = new LogPredictionRequest(
    AdministrativeAreaId: req.AdministrativeAreaId,
    PredictedProb: req.PredictedProb,
    AiProb: req.AiProb,
    PhysicsProb: req.PhysicsProb,
    RiskLevel: req.RiskLevel,
    StartTime: req.StartTime,
    EndTime: req.EndTime,
    Metadata: req.Metadata  // FE-20: Map metadata
);
```

### Step 4: Update Entity

**File**: `src/Core/Domain/FDAAPI.Domain.RelationalDb/Entities/PredictionLog.cs`

```csharp
public class PredictionLog : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
{
    // ... existing properties ...
    
    // FE-20: Interpretability metadata for this prediction
    public string Metadata { get; set; } = "{}";
    
    // ... rest of properties ...
}
```

### Step 5: Update EF Configuration

**File**: `src/Core/Domain/FDAAPI.Domain.RelationalDb/RealationalDB/Configurations/PredictionLogConfiguration.cs`

```csharp
builder.Property(e => e.Metadata)
    .HasColumnType("jsonb")
    .IsRequired()
    .HasDefaultValue("{}");
```

### Step 6: Update Handler

**File**: `src/Core/Application/FDAAPI.App.FeatG76_LogPrediction/LogPredictionHandler.cs`

```csharp
var predictionLog = new PredictionLog
{
    // ... existing properties ...
    Metadata = string.IsNullOrWhiteSpace(request.Metadata) ? "{}" : request.Metadata,
    // ... rest of properties ...
};
```

### Step 7: Create Migration

```bash
dotnet ef migrations add AddMetadataToPredictionLogs \
  --project "src/Core/Domain/FDAAPI.Domain.RelationalDb/FDAAPI.Domain.RelationalDb.csproj" \
  --startup-project "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj" \
  --output-dir Migrations
```

### Step 8: Apply Migration

```bash
dotnet ef database update \
  --project "src/Core/Domain/FDAAPI.Domain.RelationalDb/FDAAPI.Domain.RelationalDb.csproj" \
  --startup-project "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj"
```

---

# Query & Analytics

## PostgreSQL JSONB Queries

### Query 1: Tìm predictions với dominant factor là "rainfall"

```sql
SELECT 
    id,
    administrative_area_id,
    predicted_prob,
    risk_level,
    "Metadata"->'dominant_factor'->>'factor_name_vietnamese' as DominantFactor,
    "Metadata"->>'explanation' as Explanation
FROM prediction_logs
WHERE "Metadata"->'dominant_factor'->>'factor_name' = 'rainfall'
ORDER BY created_at DESC;
```

### Query 2: Tìm predictions với confidence level HIGH

```sql
SELECT *
FROM prediction_logs
WHERE "Metadata"->>'confidence_level' = 'HIGH'
ORDER BY created_at DESC;
```

### Query 3: Tìm predictions với similarity_score > 0.85

```sql
SELECT 
    id,
    administrative_area_id,
    "Metadata"->'historical_similarity'->>'event_name' as HistoricalEvent,
    ("Metadata"->'historical_similarity'->>'similarity_score')::float as SimilarityScore
FROM prediction_logs
WHERE ("Metadata"->'historical_similarity'->>'similarity_score')::float > 0.85
ORDER BY ("Metadata"->'historical_similarity'->>'similarity_score')::float DESC;
```

### Query 4: Thống kê dominant factors

```sql
SELECT 
    "Metadata"->'dominant_factor'->>'factor_name_vietnamese' as FactorName,
    COUNT(*) as Count,
    AVG(("Metadata"->'dominant_factor'->>'contribution_percent')::float) as AvgContribution
FROM prediction_logs
WHERE "Metadata" IS NOT NULL AND "Metadata" != '{}'::jsonb
GROUP BY "Metadata"->'dominant_factor'->>'factor_name_vietnamese'
ORDER BY Count DESC;
```

### Query 5: Tìm predictions với historical event cụ thể

```sql
SELECT *
FROM prediction_logs
WHERE "Metadata"->'historical_similarity'->>'event_name' LIKE '%Bão Sơn Ca%'
ORDER BY created_at DESC;
```

## C# Service Example

### PredictionAnalyticsService.cs

```csharp
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using FDAAPI.Domain.RelationalDb.RealationalDB;

public class PredictionAnalyticsService
{
    private readonly AppDbContext _context;

    public PredictionAnalyticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<string, int>> GetDominantFactorStatsAsync(
        DateTime from, DateTime to)
    {
        var predictions = await _context.PredictionLogs
            .Where(p => p.CreatedAt >= from && p.CreatedAt <= to)
            .Select(p => p.Metadata)
            .ToListAsync();

        var stats = new Dictionary<string, int>();

        foreach (var json in predictions)
        {
            if (string.IsNullOrWhiteSpace(json) || json == "{}") continue;

            try
            {
                using var doc = JsonDocument.Parse(json);
                var factor = doc.RootElement
                    .GetProperty("dominant_factor")
                    .GetProperty("factor_name_vietnamese")
                    .GetString();

                if (string.IsNullOrWhiteSpace(factor)) continue;

                stats[factor] = stats.TryGetValue(factor, out var cnt) ? cnt + 1 : 1;
            }
            catch
            {
                // Log parse error if needed
            }
        }

        return stats;
    }

    public async Task<List<PredictionWithMetadata>> GetHighConfidencePredictionsAsync()
    {
        var predictions = await _context.PredictionLogs
            .Where(p => p.CreatedAt >= DateTime.UtcNow.AddDays(-7))
            .ToListAsync();

        var results = new List<PredictionWithMetadata>();

        foreach (var pred in predictions)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pred.Metadata) || pred.Metadata == "{}")
                    continue;

                using var doc = JsonDocument.Parse(pred.Metadata);
                var root = doc.RootElement;

                var confidence = root.GetProperty("confidence_level").GetString();

                if (confidence == "HIGH")
                {
                    results.Add(new PredictionWithMetadata
                    {
                        Id = pred.Id,
                        Probability = pred.PredictedProb,
                        Explanation = root.GetProperty("explanation").GetString(),
                        HistoricalEvent = root
                            .GetProperty("historical_similarity")
                            .GetProperty("event_name")
                            .GetString(),
                        ConfidenceLevel = confidence
                    });
                }
            }
            catch
            {
                // Log parse error if needed
            }
        }

        return results;
    }
}

public class PredictionWithMetadata
{
    public Guid Id { get; set; }
    public decimal Probability { get; set; }
    public string? Explanation { get; set; }
    public string? HistoricalEvent { get; set; }
    public string? ConfidenceLevel { get; set; }
}
```

---

# Test Data & Test Cases

## Test Case 1: Log Prediction với Metadata hợp lệ

**Request**:
```bash
curl -X POST http://localhost:5000/api/v1/internal/log-prediction \
  -H "Content-Type: application/json" \
  -d '{
    "administrativeAreaId": "550e8400-e29b-41d4-a716-446655440000",
    "predictedProb": 0.675,
    "aiProb": 0.68,
    "physicsProb": 0.65,
    "riskLevel": "HIGH",
    "startTime": "2026-02-05T14:30:00Z",
    "endTime": "2026-02-05T15:30:00Z",
    "metadata": "{\"dominant_factor\":{\"factor_name\":\"rainfall\",\"factor_name_vietnamese\":\"Lượng mưa\",\"contribution_percent\":52.3,\"impact_description\":\"Mưa lớn\"},\"geographical_context\":\"Khu vực miền núi\",\"historical_similarity\":{\"event_name\":\"Bão Sơn Ca 2022\",\"similarity_score\":0.87,\"event_year\":2022},\"explanation\":\"Nguy cơ cao\",\"confidence_level\":\"HIGH\",\"area_id\":\"district-hoa-vang\",\"generated_at\":\"2026-02-05T14:30:45.123456\"}"
  }'
```

**Expected Response**: `201 Created` với `predictionLogId`

**Database Check**:
```sql
SELECT id, metadata FROM prediction_logs WHERE id = '<predictionLogId>';
-- Should return JSONB with all metadata fields
```

---

## Test Case 2: Log Prediction không có Metadata

**Request**:
```bash
curl -X POST http://localhost:5000/api/v1/internal/log-prediction \
  -H "Content-Type: application/json" \
  -d '{
    "administrativeAreaId": "550e8400-e29b-41d4-a716-446655440000",
    "predictedProb": 0.675,
    "riskLevel": "HIGH",
    "startTime": "2026-02-05T14:30:00Z",
    "endTime": "2026-02-05T15:30:00Z"
  }'
```

**Expected Response**: `201 Created` với `predictionLogId`

**Database Check**:
```sql
SELECT metadata FROM prediction_logs WHERE id = '<predictionLogId>';
-- Should return: {}
```

---

## Test Case 3: Log Prediction với Metadata JSON không hợp lệ

**Request**:
```bash
curl -X POST http://localhost:5000/api/v1/internal/log-prediction \
  -H "Content-Type: application/json" \
  -d '{
    "administrativeAreaId": "550e8400-e29b-41d4-a716-446655440000",
    "predictedProb": 0.675,
    "riskLevel": "HIGH",
    "startTime": "2026-02-05T14:30:00Z",
    "endTime": "2026-02-05T15:30:00Z",
    "metadata": "{ invalid json }"
  }'
```

**Expected Response**: `400 Bad Request` với message "Invalid metadata JSON format"

---

## Test Case 4: Query Metadata từ Database

**SQL Query**:
```sql
-- Tìm predictions với confidence HIGH
SELECT 
    id,
    predicted_prob,
    "Metadata"->>'confidence_level' as Confidence,
    "Metadata"->>'explanation' as Explanation
FROM prediction_logs
WHERE "Metadata"->>'confidence_level' = 'HIGH'
LIMIT 10;
```

**Expected Result**: List predictions với confidence HIGH và explanation

---

## Test Case 5: Analytics - Thống kê Dominant Factors

**SQL Query**:
```sql
SELECT 
    "Metadata"->'dominant_factor'->>'factor_name_vietnamese' as Factor,
    COUNT(*) as Count
FROM prediction_logs
WHERE "Metadata" IS NOT NULL AND "Metadata" != '{}'::jsonb
GROUP BY "Metadata"->'dominant_factor'->>'factor_name_vietnamese'
ORDER BY Count DESC;
```

**Expected Result**: Bảng thống kê các dominant factors và số lần xuất hiện

---

# Deployment Guide

## Pre-Deployment Checklist

- [ ] Code changes reviewed và tested
- [ ] Migration file created và reviewed
- [ ] Database backup created
- [ ] API endpoint tested với Postman/cURL
- [ ] JSON validation tested
- [ ] Error handling tested

## Deployment Steps

### Step 1: Backup Database

```bash
pg_dump -U postgres -d fda_api > backup_before_fe20_$(date +%Y%m%d_%H%M%S).sql
```

### Step 2: Build Project

```bash
dotnet build
```

### Step 3: Review Migration

```bash
dotnet ef migrations list \
  --project "src/Core/Domain/FDAAPI.Domain.RelationalDb/FDAAPI.Domain.RelationalDb.csproj" \
  --startup-project "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj"
```

### Step 4: Apply Migration

```bash
dotnet ef database update \
  --project "src/Core/Domain/FDAAPI.Domain.RelationalDb/FDAAPI.Domain.RelationalDb.csproj" \
  --startup-project "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj"
```

### Step 5: Verify Migration

```sql
-- Check column exists
SELECT column_name, data_type, column_default
FROM information_schema.columns
WHERE table_name = 'prediction_logs' AND column_name = 'Metadata';

-- Check index exists
SELECT indexname, indexdef
FROM pg_indexes
WHERE tablename = 'prediction_logs' AND indexname LIKE '%metadata%';
```

### Step 6: Test Endpoint

```bash
# Test với metadata hợp lệ
curl -X POST http://localhost:5000/api/v1/internal/log-prediction \
  -H "Content-Type: application/json" \
  -d @test_request_with_metadata.json

# Test không có metadata
curl -X POST http://localhost:5000/api/v1/internal/log-prediction \
  -H "Content-Type: application/json" \
  -d @test_request_without_metadata.json
```

### Step 7: Monitor

- Monitor logs cho JSON parse errors
- Monitor database performance với JSONB queries
- Verify metadata được lưu đúng format trong DB

## Rollback Plan

Nếu cần rollback:

```sql
-- Remove index
DROP INDEX IF EXISTS ix_prediction_logs_metadata_gin;

-- Remove column (WARNING: Data loss!)
ALTER TABLE prediction_logs DROP COLUMN IF EXISTS "Metadata";

-- Or revert migration
dotnet ef database update <previous_migration_name> \
  --project "src/Core/Domain/FDAAPI.Domain.RelationalDb/FDAAPI.Domain.RelationalDb.csproj" \
  --startup-project "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj"
```

---

## Post-Deployment

### Monitoring

1. **Log Monitoring**:
   - Monitor JSON parse errors
   - Monitor endpoint response times
   - Monitor database query performance

2. **Database Monitoring**:
   - Check JSONB index usage
   - Monitor table size growth
   - Check query performance với metadata queries

3. **API Monitoring**:
   - Monitor endpoint success rate
   - Monitor request/response sizes
   - Monitor error rates

### Performance Optimization

1. **Index Optimization**:
   - Monitor GIN index usage
   - Consider additional indexes nếu cần query specific fields thường xuyên

2. **Query Optimization**:
   - Use JSONB operators (`->`, `->>`) efficiently
   - Avoid parsing JSON trong C# nếu có thể query trực tiếp trong SQL

---

## References

- [PostgreSQL JSONB Documentation](https://www.postgresql.org/docs/current/datatype-json.html)
- [EF Core JSONB Mapping](https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions)
- [System.Text.Json Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.text.json)

---

**Last Updated**: 2026-02-05  
**Version**: 1.0.0  
**Status**: ✅ Completed - Ready for Production

