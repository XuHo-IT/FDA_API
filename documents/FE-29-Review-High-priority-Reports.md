# FE-29 – Review High-priority Reports

> **Feature Name**: Authority Review for High-Priority Flood Reports  
> **Created**: 2026-02-05  
> **Status**: 🟡 Planning  
> **Backend Feature**: FeatG85  
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

**User Request**: Allow authority users (government officers) to review high-priority flood reports for emergency response.

**Backend Scope**:
- ✅ List high-priority reports (priority = high/critical)
- ✅ Filter by severity, location, time
- ✅ Provide decision endpoint (acknowledge/respond)
- ✅ Track authority actions
- ❌ Emergency dispatch system (out of scope)
- ❌ Notification system (covered in other features)

### Backend Feature to Implement

| Feature | Endpoint | Type | Description |
|---------|----------|------|-------------|
| **FeatG85** | `GET /api/v1/flood-reports/high-priority` | Query | Get high-priority reports for authority review |
| **FeatG85** | `PUT /api/v1/flood-reports/{id}/acknowledge` | Command | Authority acknowledges report |
| **FeatG85** | `PUT /api/v1/flood-reports/{id}/respond` | Command | Authority marks report as responded to |

### Key Principle: Authority Focus on High-Impact

**Traditional Approach** (❌ Overwhelming):
```
All Reports → Authority → Review Everything
```

**Our Approach** (✅ Focused):
```
High-Priority Reports Only → Authority → Emergency Response
```

**Priority Criteria**:
- Trust Score ≥ 70 AND Severity = high
- Upvotes ≥ 50 (community consensus)
- Near critical infrastructure (schools, hospitals)
- Multiple reports at same location
- Priority = critical (auto-set by system)

---

## 🔍 FEATURE ANALYSIS

### ✅ What's GOOD in Original Design

1. **Emergency Response**: Enables quick action on critical reports
2. **Authority Oversight**: Government can coordinate response
3. **Priority Filtering**: Focuses on high-impact cases

### ⚠️ What Needs ADJUSTMENT

#### 1. **Priority Definition**

**Issue**: What makes a report "high-priority"?

**Solution**: Clear, automated criteria:
- Trust Score ≥ 70 AND Severity = high → Priority = high
- Upvotes ≥ 50 → Priority = high
- Upvotes ≥ 100 → Priority = critical
- Multiple reports at same location → Priority = high
- Near school/hospital → Priority = high

#### 2. **Authority Actions**

**Issue**: What can authority do with reports?

**Solution**: Simple actions:
- `acknowledge` → Authority has seen the report
- `respond` → Authority has taken action (dispatch team, etc.)
- Track response time (acknowledge → respond)

#### 3. **Real-Time Updates**

**Issue**: Authority needs to see new high-priority reports immediately.

**Solution**: 
- Sort by `createdAt DESC` (newest first)
- WebSocket push for new critical reports (future)
- Email/SMS notification (future)

---

## 🗄️ DATABASE SCHEMA

### New Table: `flood_report_authority_actions`

```sql
CREATE TABLE flood_report_authority_actions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    flood_report_id UUID NOT NULL REFERENCES flood_reports(id) ON DELETE CASCADE,
    authority_user_id UUID NOT NULL REFERENCES users(id),
    action_type VARCHAR(20) NOT NULL, -- acknowledge | respond
    note TEXT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT chk_authority_action CHECK (action_type IN ('acknowledge', 'respond'))
);

CREATE INDEX ix_flood_report_authority_actions_report ON flood_report_authority_actions(flood_report_id);
CREATE INDEX ix_flood_report_authority_actions_authority ON flood_report_authority_actions(authority_user_id);
CREATE INDEX ix_flood_report_authority_actions_created ON flood_report_authority_actions(created_at DESC);
```

**Note**: This table tracks authority actions for audit and response time metrics.

---

## 🔌 API SPECIFICATIONS

### FeatG85: Get High-Priority Reports

**Endpoint**: `GET /api/v1/flood-reports/high-priority`

**Authentication**: Required (JWT Bearer token)

**Authorization**: Authority role only

**Query Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `priority` | string | No | Filter by priority: `high`, `critical` (default: both) |
| `severity` | string | No | Filter by severity: `low`, `medium`, `high` |
| `bounds` | string | No | Viewport bounds: `minLat,minLng,maxLat,maxLng` |
| `timeRange` | string | No | Filter by time: `1h`, `6h`, `24h` (default: `24h`) |
| `page` | integer | No | Page number (default: 1) |
| `pageSize` | integer | No | Items per page (default: 20, max: 50) |
| `sortBy` | string | No | Sort field: `priority`, `severity`, `createdAt`, `trustScore` (default: `priority`) |

**Request Example**:
```
GET /api/v1/flood-reports/high-priority?priority=critical&severity=high&timeRange=6h&sortBy=priority
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "High-priority reports retrieved successfully",
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "latitude": 10.762622,
        "longitude": 106.660172,
        "address": "123 Nguyen Hue Street, District 1",
        "description": "Ngập nặng trước cổng trường, nước cao 50cm",
        "severity": "high",
        "trustScore": 85,
        "priority": "critical",
        "status": "published",
        "confidenceLevel": "high",
        "votes": {
          "up": 52,
          "down": 3
        },
        "flags": 0,
        "mediaCount": 3,
        "mediaPreview": "https://ik.imagekit.io/...",
        "nearbyInfrastructure": [
          {
            "type": "school",
            "name": "Trường Tiểu học ABC",
            "distance": 200
          }
        ],
        "duplicateReports": 3,
        "createdAt": "2026-02-05T10:30:00Z",
        "reporter": {
          "id": "user-id",
          "name": "Nguyen Van A"
        },
        "authorityActions": {
          "acknowledged": false,
          "responded": false,
          "acknowledgedAt": null,
          "respondedAt": null,
          "responseTimeMinutes": null
        }
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "totalItems": 8,
      "totalPages": 1
    },
    "summary": {
      "totalCritical": 3,
      "totalHigh": 5,
      "unacknowledged": 6,
      "unresponded": 8
    }
  }
}
```

**Business Logic**:

1. **Build Query** (only high-priority):
   ```csharp
   var query = _context.FloodReports
       .Where(r => r.Priority == "high" || r.Priority == "critical")
       .Where(r => r.Status == "published") // Only published reports
       .AsQueryable();
   ```

2. **Apply Filters** (priority, severity, bounds, timeRange)

3. **Include Authority Actions**:
   ```csharp
   .Select(r => new HighPriorityReportDto
   {
       // ... report fields
       AuthorityActions = new AuthorityActionsDto
       {
           Acknowledged = r.AuthorityActions.Any(a => a.ActionType == "acknowledge"),
           Responded = r.AuthorityActions.Any(a => a.ActionType == "respond"),
           AcknowledgedAt = r.AuthorityActions
               .Where(a => a.ActionType == "acknowledge")
               .OrderByDescending(a => a.CreatedAt)
               .FirstOrDefault()?.CreatedAt,
           RespondedAt = r.AuthorityActions
               .Where(a => a.ActionType == "respond")
               .OrderByDescending(a => a.CreatedAt)
               .FirstOrDefault()?.CreatedAt
       }
   })
   ```

4. **Calculate Response Time**:
   ```csharp
   if (acknowledgedAt.HasValue && respondedAt.HasValue)
   {
       responseTimeMinutes = (respondedAt.Value - acknowledgedAt.Value).TotalMinutes;
   }
   ```

### FeatG85: Acknowledge Report

**Endpoint**: `PUT /api/v1/flood-reports/{id}/acknowledge`

**Authentication**: Required (JWT Bearer token)

**Authorization**: Authority role only

**Path Parameters**:
- `id` (UUID): Flood report ID

**Request Body**:
```json
{
  "note": "Received. Dispatching response team."
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Report acknowledged successfully",
  "data": {
    "reportId": "550e8400-e29b-41d4-a716-446655440000",
    "acknowledgedAt": "2026-02-05T11:00:00Z",
    "acknowledgedBy": {
      "id": "authority-user-id",
      "name": "Tran Van B"
    }
  }
}
```

### FeatG85: Respond to Report

**Endpoint**: `PUT /api/v1/flood-reports/{id}/respond`

**Authentication**: Required (JWT Bearer token)

**Authorization**: Authority role only

**Path Parameters**:
- `id` (UUID): Flood report ID

**Request Body**:
```json
{
  "note": "Response team dispatched. Estimated arrival: 15 minutes.",
  "responseDetails": {
    "teamDispatched": true,
    "estimatedArrivalMinutes": 15,
    "responseType": "emergency"
  }
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Report marked as responded",
  "data": {
    "reportId": "550e8400-e29b-41d4-a716-446655440000",
    "respondedAt": "2026-02-05T11:05:00Z",
    "responseTimeMinutes": 5,
    "respondedBy": {
      "id": "authority-user-id",
      "name": "Tran Van B"
    }
  }
}
```

---

## 🚀 IMPLEMENTATION PLAN

### Phase 1: Domain Layer

**Files to Create**:

```
src/Core/Domain/FDAAPI.Domain.RelationalDb/
├── Entities/
│   └── FloodReportAuthorityAction.cs
└── Repositories/
    └── IFloodReportAuthorityActionRepository.cs
```

### Phase 2: Application Layer

**Files to Create**:

```
src/Core/Application/
├── FDAAPI.App.FeatG85_GetHighPriorityReports/
│   ├── GetHighPriorityReportsRequest.cs
│   ├── GetHighPriorityReportsResponse.cs
│   └── GetHighPriorityReportsHandler.cs
├── FDAAPI.App.FeatG85_AcknowledgeFloodReport/
│   ├── AcknowledgeFloodReportRequest.cs
│   ├── AcknowledgeFloodReportResponse.cs
│   └── AcknowledgeFloodReportHandler.cs
└── FDAAPI.App.FeatG85_RespondToFloodReport/
    ├── RespondToFloodReportRequest.cs
    ├── RespondToFloodReportResponse.cs
    └── RespondToFloodReportHandler.cs
```

### Phase 3: Priority Calculation Service

**Files to Create**:

```
src/Core/Application/FDAAPI.App.Common/
└── Services/
    └── IPriorityCalculator.cs
```

```csharp
public interface IPriorityCalculator
{
    Task<string> CalculatePriorityAsync(Guid reportId, CancellationToken ct);
}

public class PriorityCalculator : IPriorityCalculator
{
    public async Task<string> CalculatePriorityAsync(Guid reportId, CancellationToken ct)
    {
        var report = await _context.FloodReports
            .Include(r => r.Votes)
            .FirstOrDefaultAsync(r => r.Id == reportId, ct);
        
        var upvotes = report.Votes.Count(v => v.VoteType == "up");
        
        // Critical: 100+ upvotes OR (Trust Score ≥ 80 AND Severity = high)
        if (upvotes >= 100 || (report.TrustScore >= 80 && report.Severity == "high"))
            return "critical";
        
        // High: 50+ upvotes OR (Trust Score ≥ 70 AND Severity = high)
        if (upvotes >= 50 || (report.TrustScore >= 70 && report.Severity == "high"))
            return "high";
        
        return "normal";
    }
}
```

**Note**: This service should be called:
- After report creation (if conditions met)
- After each vote (recalculate)
- Periodically via background job (every 5 minutes)

---

## 🧪 TESTING STRATEGY

### Unit Tests

1. **GetHighPriorityReportsHandler Tests**:
   - ✅ Returns only high/critical priority reports
   - ✅ Filters correctly
   - ✅ Includes authority action status
   - ✅ Calculates response time correctly

2. **AcknowledgeFloodReportHandler Tests**:
   - ✅ Creates acknowledge action
   - ✅ Tracks acknowledge time
   - ✅ Requires Authority role

3. **RespondToFloodReportHandler Tests**:
   - ✅ Creates respond action
   - ✅ Calculates response time
   - ✅ Requires Authority role

### Integration Tests

```bash
# Test 1: Get high-priority reports
GET /api/v1/flood-reports/high-priority
Authorization: Bearer {authority_token}

# Expected: 200 OK, only high/critical reports

# Test 2: Acknowledge
PUT /api/v1/flood-reports/{id}/acknowledge
Authorization: Bearer {authority_token}
{"note": "Received"}

# Expected: 200 OK, acknowledged

# Test 3: Respond
PUT /api/v1/flood-reports/{id}/respond
Authorization: Bearer {authority_token}
{"note": "Team dispatched"}

# Expected: 200 OK, response time calculated
```

---

## 🚨 EDGE CASES & ERROR HANDLING

### 1. **Non-Authority Access**

**Scenario**: Regular user tries to access high-priority reports.

**Solution**: Return 403 Forbidden.

### 2. **Report Priority Changed**

**Scenario**: Report priority changes from high to normal after authority views it.

**Solution**: Still show in list if previously high-priority (or filter by current priority).

### 3. **Multiple Authorities**

**Scenario**: Multiple authority users acknowledge same report.

**Solution**: Allow multiple acknowledgments (track all).

---

## ✅ FEASIBILITY ASSESSMENT

### Technical Feasibility: ⭐⭐⭐⭐⭐ (5/5)

**Strengths**:
- ✅ Simple query and update operations
- ✅ Clear priority criteria
- ✅ Straightforward authority actions

**Challenges**:
- ⚠️ Need to implement priority calculation service
- ⚠️ Need to track nearby infrastructure (future)

### Business Feasibility: ⭐⭐⭐⭐⭐ (5/5)

**Strengths**:
- ✅ Enables emergency response coordination
- ✅ Focuses on high-impact cases
- ✅ Tracks response metrics

### Implementation Effort: ⭐⭐⭐⭐ (4/5)

**Estimated Time**: 1.5 weeks

**Breakdown**:
- Domain Layer: 1 day
- Application Layer: 3 days
- Priority Calculator: 2 days
- Presentation Layer: 2 days
- Testing: 2 days

---

## ⚠️ POTENTIAL ISSUES & MITIGATIONS

### Issue 1: Priority Calculation Accuracy

**Problem**: May miss some high-priority cases or mark normal cases as high.

**Mitigation**:
- Start with conservative thresholds
- Monitor false positive/negative rates
- Adjust based on real data
- Allow manual priority override (future)

### Issue 2: Response Time Tracking

**Problem**: Response time may not reflect actual on-ground response.

**Mitigation**:
- Track acknowledge → respond time (system metric)
- Actual response time tracked separately (future)
- Use for internal metrics, not public reporting

### Issue 3: Authority Availability

**Problem**: No authority available to respond to critical reports.

**Mitigation**:
- Escalate to higher authority (future)
- Auto-notify multiple authorities (future)
- Email/SMS alerts for critical reports (future)

---

## 🎯 ACCEPTANCE CRITERIA

### Backend Features

- [x] **FeatG85**: GET /api/v1/flood-reports/high-priority
  - ✅ Returns only high/critical priority reports
  - ✅ Requires Authority role
  - ✅ Includes authority action status
  - ✅ Calculates response time

- [x] **FeatG85**: PUT /api/v1/flood-reports/{id}/acknowledge
  - ✅ Creates acknowledge action
  - ✅ Tracks acknowledge time
  - ✅ Requires Authority role

- [x] **FeatG85**: PUT /api/v1/flood-reports/{id}/respond
  - ✅ Creates respond action
  - ✅ Calculates response time
  - ✅ Requires Authority role

---

**Document Version**: 1.0  
**Last Updated**: 2026-02-05  
**Author**: Development Team  
**Status**: ✅ Ready for Implementation

