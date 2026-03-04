# FE-26 – View Community Flood Reports

> **Feature Name**: Community Flood Reports Viewing API  
> **Created**: 2026-02-05  
> **Status**: 🟡 Planning  
> **Backend Feature**: FeatG81  
> **Priority**: High

---

## 📋 TABLE OF CONTENTS

1. [Executive Summary](#executive-summary)
2. [Feature Analysis](#feature-analysis)
3. [Backend Scope Definition](#backend-scope-definition)
4. [Database Schema](#database-schema)
5. [API Specifications](#api-specifications)
6. [Implementation Plan](#implementation-plan)
7. [Testing Strategy](#testing-strategy)
8. [Edge Cases & Error Handling](#edge-cases--error-handling)
9. [Feasibility Assessment](#feasibility-assessment)
10. [Potential Issues & Mitigations](#potential-issues--mitigations)

---

## 📊 EXECUTIVE SUMMARY

### Feature Overview

**User Request**: Display community flood reports on map/list view for public awareness.

**Backend Scope**:
- ✅ Query published flood reports
- ✅ Filter by bounds, severity, time range
- ✅ Aggregate votes and flags
- ✅ Sort by trust score and recency
- ❌ Map rendering (frontend responsibility)
- ❌ UI components (frontend responsibility)

### Backend Feature to Implement

| Feature | Endpoint | Type | Description |
|---------|----------|------|-------------|
| **FeatG81** | `GET /api/v1/flood-reports/community` | Query | Get published flood reports |

### Key Requirements

1. **Only Published Reports**: Never return `hidden` or `escalated` reports
2. **Confidence Indicators**: Include confidence level for frontend display
3. **Aggregated Metrics**: Include vote counts, flag counts
4. **Performance**: Support large datasets with pagination

---

## 🔍 FEATURE ANALYSIS

### ✅ What's GOOD in Original Design

1. **Public Access**: Anyone can view reports (no auth required)
2. **Map Integration**: Bounds-based filtering for map viewport
3. **Filtering**: By severity, time range
4. **Real-time**: Shows latest reports

### ⚠️ What Needs ADJUSTMENT

#### 1. **Status Filtering**

**Issue**: Must only show `published` reports, never `hidden` or `escalated`.

**Solution**: Hard filter in query:
```sql
WHERE status = 'published'
```

#### 2. **Confidence Level Display**

**Issue**: Frontend needs to show confidence badges.

**Solution**: Include `confidenceLevel` in response:
- `high` → Green badge "Confirmed"
- `medium` → Yellow badge "Community-reported"
- `low` → Gray badge "Low confidence"

#### 3. **Performance Optimization**

**Issue**: Could return thousands of reports.

**Solution**:
- Pagination (default: 50 per page)
- Bounds filtering (only show reports in viewport)
- Index on `(status, latitude, longitude, created_at)`

---

## 🗄️ DATABASE SCHEMA

**Uses existing tables**:
- `flood_reports` (from FE-25)
- `flood_report_media` (from FE-25)
- `flood_report_votes` (from FE-27)
- `flood_report_flags` (from FE-27)

**No new tables required**.

---

## 🔌 API SPECIFICATIONS

### FeatG81: Get Community Flood Reports

**Endpoint**: `GET /api/v1/flood-reports/community`

**Authentication**: Public (AllowAnonymous)

**Authorization**: None required

**Query Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `bounds` | string | No | Viewport bounds: `minLat,minLng,maxLat,maxLng` |
| `severity` | string | No | Filter by severity: `low`, `medium`, `high` |
| `timeRange` | string | No | Filter by time: `1h`, `6h`, `24h`, `7d`, `30d` |
| `page` | integer | No | Page number (default: 1) |
| `pageSize` | integer | No | Items per page (default: 50, max: 100) |
| `sortBy` | string | No | Sort field: `trustScore`, `createdAt` (default: `createdAt`) |
| `sortOrder` | string | No | Sort order: `asc`, `desc` (default: `desc`) |

**Request Example**:
```
GET /api/v1/flood-reports/community?bounds=10.5,106.5,11.0,107.0&severity=high&timeRange=24h&page=1&pageSize=50
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Flood reports retrieved successfully",
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "latitude": 10.762622,
        "longitude": 106.660172,
        "address": "123 Nguyen Hue Street",
        "description": "Ngập nặng trước cổng trường",
        "severity": "high",
        "trustScore": 75,
        "confidenceLevel": "high",
        "priority": "high",
        "votes": {
          "up": 12,
          "down": 2
        },
        "flags": 0,
        "mediaPreview": "https://ik.imagekit.io/fda/flood-reports/photo1.jpg",
        "mediaCount": 2,
        "createdAt": "2026-02-05T10:30:00Z",
        "reporter": {
          "id": "user-id",
          "name": "Nguyen Van A"
        }
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 50,
      "totalItems": 127,
      "totalPages": 3
    },
    "filters": {
      "bounds": {
        "minLat": 10.5,
        "minLng": 106.5,
        "maxLat": 11.0,
        "maxLng": 107.0
      },
      "severity": "high",
      "timeRange": "24h"
    }
  }
}
```

**Business Logic**:

1. **Parse Query Parameters**:
   - Parse bounds string to BoundingBox object
   - Parse timeRange to DateTime range
   - Validate page and pageSize

2. **Build Query**:
   ```csharp
   var query = _context.FloodReports
       .Where(r => r.Status == "published") // Hard filter
       .AsQueryable();
   
   // Apply filters
   if (bounds != null)
   {
       query = query.Where(r =>
           r.Latitude >= bounds.MinLat &&
           r.Latitude <= bounds.MaxLat &&
           r.Longitude >= bounds.MinLng &&
           r.Longitude <= bounds.MaxLng);
   }
   
   if (severity != null)
       query = query.Where(r => r.Severity == severity);
   
   if (timeRange != null)
   {
       var startTime = CalculateStartTime(timeRange);
       query = query.Where(r => r.CreatedAt >= startTime);
   }
   ```

3. **Aggregate Votes and Flags**:
   ```csharp
   var reports = await query
       .Select(r => new FloodReportDto
       {
           Id = r.Id,
           // ... other fields
           Votes = new VoteSummaryDto
           {
               Up = r.Votes.Count(v => v.VoteType == "up"),
               Down = r.Votes.Count(v => v.VoteType == "down")
           },
           Flags = r.Flags.Count
       })
       .ToListAsync(ct);
   ```

4. **Sort and Paginate**:
   ```csharp
   var sorted = sortBy switch
   {
       "trustScore" => sortOrder == "asc" 
           ? reports.OrderBy(r => r.TrustScore)
           : reports.OrderByDescending(r => r.TrustScore),
       _ => sortOrder == "asc"
           ? reports.OrderBy(r => r.CreatedAt)
           : reports.OrderByDescending(r => r.CreatedAt)
   };
   
   var paginated = sorted
       .Skip((page - 1) * pageSize)
       .Take(pageSize)
       .ToList();
   ```

---

## 🚀 IMPLEMENTATION PLAN

### Phase 1: Application Layer

**Files to Create**:

```
src/Core/Application/FDAAPI.App.FeatG81_GetCommunityFloodReports/
├── FDAAPI.App.FeatG81_GetCommunityFloodReports.csproj
├── GetCommunityFloodReportsRequest.cs
├── GetCommunityFloodReportsResponse.cs
├── GetCommunityFloodReportsHandler.cs
└── Models/
    ├── FloodReportListItemDto.cs
    └── BoundingBox.cs
```

**1.1 Create Handler: `GetCommunityFloodReportsHandler.cs`**

```csharp
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.App.FeatG81_GetCommunityFloodReports
{
    public class GetCommunityFloodReportsHandler
        : IRequestHandler<GetCommunityFloodReportsRequest, GetCommunityFloodReportsResponse>
    {
        private readonly AppDbContext _context;
        
        public GetCommunityFloodReportsHandler(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<GetCommunityFloodReportsResponse> Handle(
            GetCommunityFloodReportsRequest request,
            CancellationToken ct)
        {
            // 1. Build base query (only published reports)
            var query = _context.FloodReports
                .Where(r => r.Status == "published")
                .Include(r => r.Media)
                .Include(r => r.Votes)
                .Include(r => r.Flags)
                .Include(r => r.Reporter)
                .AsQueryable();
            
            // 2. Apply filters
            if (request.Bounds != null)
            {
                query = query.Where(r =>
                    r.Latitude >= request.Bounds.MinLat &&
                    r.Latitude <= request.Bounds.MaxLat &&
                    r.Longitude >= request.Bounds.MinLng &&
                    r.Longitude <= request.Bounds.MaxLng);
            }
            
            if (!string.IsNullOrEmpty(request.Severity))
                query = query.Where(r => r.Severity == request.Severity);
            
            if (request.TimeRangeStart.HasValue)
                query = query.Where(r => r.CreatedAt >= request.TimeRangeStart.Value);
            
            // 3. Get total count (before pagination)
            var totalCount = await query.CountAsync(ct);
            
            // 4. Sort
            query = request.SortBy switch
            {
                "trustScore" => request.SortOrder == "asc"
                    ? query.OrderBy(r => r.TrustScore)
                    : query.OrderByDescending(r => r.TrustScore),
                _ => request.SortOrder == "asc"
                    ? query.OrderBy(r => r.CreatedAt)
                    : query.OrderByDescending(r => r.CreatedAt)
            };
            
            // 5. Paginate
            var reports = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);
            
            // 6. Map to DTOs
            var items = reports.Select(r => new FloodReportListItemDto
            {
                Id = r.Id,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                Address = r.Address,
                Description = r.Description,
                Severity = r.Severity,
                TrustScore = r.TrustScore,
                ConfidenceLevel = r.ConfidenceLevel,
                Priority = r.Priority,
                Votes = new VoteSummaryDto
                {
                    Up = r.Votes.Count(v => v.VoteType == "up"),
                    Down = r.Votes.Count(v => v.VoteType == "down")
                },
                Flags = r.Flags.Count,
                MediaPreview = r.Media.FirstOrDefault()?.MediaUrl,
                MediaCount = r.Media.Count,
                CreatedAt = r.CreatedAt,
                Reporter = r.Reporter != null ? new ReporterDto
                {
                    Id = r.Reporter.Id,
                    Name = r.Reporter.FullName ?? "Anonymous"
                } : null
            }).ToList();
            
            return new GetCommunityFloodReportsResponse
            {
                Success = true,
                Message = "Flood reports retrieved successfully",
                Data = new GetCommunityFloodReportsDataDto
                {
                    Items = items,
                    Pagination = new PaginationDto
                    {
                        Page = request.Page,
                        PageSize = request.PageSize,
                        TotalItems = totalCount,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
                    },
                    Filters = new FiltersDto
                    {
                        Bounds = request.Bounds,
                        Severity = request.Severity,
                        TimeRange = request.TimeRange
                    }
                }
            };
        }
    }
}
```

### Phase 2: Presentation Layer

**Files to Create**:

```
src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/
└── Endpoints/
    └── FeatG81_GetCommunityFloodReports/
        ├── GetCommunityFloodReportsEndpoint.cs
        └── DTOs/
            └── GetCommunityFloodReportsResponseDto.cs
```

---

## 🧪 TESTING STRATEGY

### Unit Tests

1. **Query Building Tests**:
   - ✅ Filters by bounds correctly
   - ✅ Filters by severity correctly
   - ✅ Filters by time range correctly
   - ✅ Only returns published reports
   - ✅ Excludes hidden and escalated reports

2. **Aggregation Tests**:
   - ✅ Counts votes correctly
   - ✅ Counts flags correctly
   - ✅ Gets media preview correctly

### Integration Tests

```bash
# Test 1: Get all published reports
GET /api/v1/flood-reports/community

# Expected: 200 OK, only published reports

# Test 2: Filter by bounds
GET /api/v1/flood-reports/community?bounds=10.5,106.5,11.0,107.0

# Expected: 200 OK, only reports in bounds

# Test 3: Filter by severity
GET /api/v1/flood-reports/community?severity=high

# Expected: 200 OK, only high severity reports

# Test 4: Pagination
GET /api/v1/flood-reports/community?page=2&pageSize=20

# Expected: 200 OK, page 2 with 20 items
```

---

## 🚨 EDGE CASES & ERROR HANDLING

### 1. **No Reports Found**

**Scenario**: No published reports match filters.

**Solution**: Return empty array with pagination info:
```json
{
  "items": [],
  "pagination": {
    "page": 1,
    "pageSize": 50,
    "totalItems": 0,
    "totalPages": 0
  }
}
```

### 2. **Invalid Bounds**

**Scenario**: Bounds string malformed.

**Solution**: Return 400 Bad Request with error message.

### 3. **Large Result Set**

**Scenario**: 10,000 reports match filters.

**Solution**: 
- Pagination limits to 50 per page (default)
- Max pageSize = 100 (enforced)
- Use efficient query with indexes

### 4. **Performance with Many Reports**

**Scenario**: Database has 100,000 published reports.

**Solution**:
- Index on `(status, latitude, longitude, created_at)`
- Use bounding box query (spatial index if PostGIS)
- Cache recent reports (Redis, 5 minutes)

---

## ✅ FEASIBILITY ASSESSMENT

### Technical Feasibility: ⭐⭐⭐⭐⭐ (5/5)

**Strengths**:
- ✅ Simple query operation
- ✅ Uses existing database schema
- ✅ No complex business logic
- ✅ Standard pagination pattern

**Challenges**:
- ⚠️ Need efficient indexes for performance
- ⚠️ Aggregation queries could be slow with many votes/flags

### Business Feasibility: ⭐⭐⭐⭐⭐ (5/5)

**Strengths**:
- ✅ Essential feature for community engagement
- ✅ Public access increases adoption
- ✅ Map integration enables visualization

### Implementation Effort: ⭐⭐⭐ (3/5)

**Estimated Time**: 1 week

**Breakdown**:
- Application Layer: 2 days
- Presentation Layer: 1 day
- Testing: 2 days

---

## ⚠️ POTENTIAL ISSUES & MITIGATIONS

### Issue 1: Performance with Large Datasets

**Problem**: Query could be slow with 100,000+ reports.

**Mitigation**:
- Add composite index: `(status, latitude, longitude, created_at)`
- Use bounding box filtering (reduces dataset)
- Cache results (Redis, 5 minutes TTL)
- Consider materialized view for aggregated metrics

### Issue 2: N+1 Query Problem

**Problem**: Loading votes/flags for each report separately.

**Mitigation**:
- Use `.Include()` for eager loading
- Or use separate aggregation query (GROUP BY)

### Issue 3: Real-time Updates

**Problem**: New reports don't appear immediately.

**Mitigation**:
- Short cache TTL (5 minutes)
- Frontend polling (every 30 seconds)
- Or WebSocket push (future enhancement)

---

## 🎯 ACCEPTANCE CRITERIA

### Backend Features

- [x] **FeatG81**: GET /api/v1/flood-reports/community
  - ✅ Returns only published reports
  - ✅ Filters by bounds, severity, time range
  - ✅ Pagination support
  - ✅ Aggregates votes and flags
  - ✅ Includes confidence level
  - ✅ Public access (no auth required)

### Performance

- [x] Query completes in < 500ms (p95) for 1000 reports
- [x] Supports pagination up to 100 items per page
- [x] Efficient bounding box queries

---

**Document Version**: 1.0  
**Last Updated**: 2026-02-05  
**Author**: Development Team  
**Status**: ✅ Ready for Implementation

