# FE-28 – Moderate Community Flood Reports

> **Feature Name**: Moderator Review for Escalated Reports  
> **Created**: 2026-02-05  
> **Status**: 🟡 Planning  
> **Backend Feature**: FeatG84  
> **Priority**: Medium

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

**User Request**: Allow moderators to review and moderate escalated flood reports.

**Backend Scope (Adjusted for Reality)**:
This feature handles **< 5% of reports** (only escalated cases):
- ✅ List escalated reports
- ✅ Approve/reject/archive reports
- ✅ Add moderation notes
- ❌ Review every report (automation handles 95%)
- ❌ Manual approval workflow (not scalable)

### Backend Feature to Implement

| Feature | Endpoint | Type | Description |
|---------|----------|------|-------------|
| **FeatG84** | `GET /api/v1/flood-reports/escalated` | Query | Get escalated reports for moderation |
| **FeatG84** | `PUT /api/v1/flood-reports/{id}/moderate` | Command | Moderate report (approve/reject/archive) |

### Key Principle: Rare Human Intervention

**Traditional Approach** (❌ Not scalable):
```
Every Report → Moderator → Approve/Reject
```

**Our Approach** (✅ Scalable):
```
95% Reports → Automation → Published/Hidden
5% Reports → Escalated → Moderator → Decision
```

**Escalation Triggers**:
- 3+ flags from community
- Conflicting votes (many upvotes + many downvotes)
- High-impact location (school, hospital nearby)
- Trust Score conflicts (high score but many flags)

---

## 🔍 FEATURE ANALYSIS

### ✅ What's GOOD in Original Design

1. **Human Oversight**: Moderators can handle edge cases
2. **Quality Control**: Final decision on controversial reports
3. **Audit Trail**: Moderation notes for transparency

### ⚠️ What Needs ADJUSTMENT

#### 1. **Scope Reduction**

**Issue**: Original spec suggests moderating every report.

**Reality**: 
- 1000 reports/day → 1000 manual reviews (impossible)
- 10,000 reports/day → system breaks

**Solution**: Only moderate escalated reports (< 5% of total).

#### 2. **Escalation Criteria**

**Issue**: When should a report be escalated?

**Solution**: Clear escalation triggers:
- Flags ≥ 3
- Vote conflict (upvotes > 10 AND downvotes > 5)
- High-impact location detected
- Trust Score anomaly (high score but many flags)

#### 3. **Moderation Actions**

**Issue**: What actions can moderators take?

**Solution**: Simple actions:
- `approve` → Set status = `published`, increase Trust Score
- `reject` → Set status = `hidden`, decrease Trust Score
- `archive` → Set status = `archived` (removed from public view)

---

## 🗄️ DATABASE SCHEMA

### New Table: `flood_report_moderations`

```sql
CREATE TABLE flood_report_moderations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    flood_report_id UUID NOT NULL REFERENCES flood_reports(id) ON DELETE CASCADE,
    moderator_user_id UUID NOT NULL REFERENCES users(id),
    action VARCHAR(20) NOT NULL, -- approve | reject | archive
    note TEXT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT chk_moderation_action CHECK (action IN ('approve', 'reject', 'archive'))
);

CREATE INDEX ix_flood_report_moderations_report ON flood_report_moderations(flood_report_id);
CREATE INDEX ix_flood_report_moderations_moderator ON flood_report_moderations(moderator_user_id);
CREATE INDEX ix_flood_report_moderations_created ON flood_report_moderations(created_at DESC);
```

**Note**: This table is for audit trail. The actual status change happens in `flood_reports` table.

---

## 🔌 API SPECIFICATIONS

### FeatG84: Get Escalated Reports

**Endpoint**: `GET /api/v1/flood-reports/escalated`

**Authentication**: Required (JWT Bearer token)

**Authorization**: Moderator role only

**Query Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `page` | integer | No | Page number (default: 1) |
| `pageSize` | integer | No | Items per page (default: 20, max: 50) |
| `sortBy` | string | No | Sort field: `createdAt`, `flagCount`, `voteConflict` (default: `createdAt`) |

**Request Example**:
```
GET /api/v1/flood-reports/escalated?page=1&pageSize=20&sortBy=flagCount
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Escalated reports retrieved successfully",
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "latitude": 10.762622,
        "longitude": 106.660172,
        "description": "Ngập nặng trước cổng trường",
        "severity": "high",
        "trustScore": 45,
        "status": "escalated",
        "escalationReason": "flag_threshold",
        "escalationDetails": {
          "flagCount": 4,
          "upvotes": 15,
          "downvotes": 8,
          "voteConflict": true,
          "highImpactLocation": false
        },
        "votes": {
          "up": 15,
          "down": 8
        },
        "flags": [
          {
            "reason": "fake",
            "count": 2
          },
          {
            "reason": "spam",
            "count": 2
          }
        ],
        "mediaPreview": "https://ik.imagekit.io/...",
        "createdAt": "2026-02-05T10:30:00Z",
        "reporter": {
          "id": "user-id",
          "name": "Nguyen Van A"
        }
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "totalItems": 12,
      "totalPages": 1
    }
  }
}
```

### FeatG84: Moderate Report

**Endpoint**: `PUT /api/v1/flood-reports/{id}/moderate`

**Authentication**: Required (JWT Bearer token)

**Authorization**: Moderator role only

**Path Parameters**:
- `id` (UUID): Flood report ID

**Request Body**:
```json
{
  "action": "approve",  // or "reject", "archive"
  "note": "Verified by camera footage. Report is accurate."
}
```

**Validation Rules**:

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `action` | string | Yes | Enum: `approve`, `reject`, `archive` |
| `note` | string | No | Max 500 chars |

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Report moderated successfully",
  "data": {
    "reportId": "550e8400-e29b-41d4-a716-446655440000",
    "action": "approve",
    "newStatus": "published",
    "newTrustScore": 85,
    "moderatedAt": "2026-02-05T11:00:00Z"
  }
}
```

**Business Logic**:

1. **Validate Report is Escalated**:
   ```csharp
   var report = await _reportRepository.GetByIdAsync(reportId, ct);
   if (report == null)
       return NotFound("Report not found");
   
   if (report.Status != "escalated")
       return BadRequest("Report is not escalated. Only escalated reports can be moderated.");
   ```

2. **Apply Moderation Action**:
   ```csharp
   switch (request.Action)
   {
       case "approve":
           report.Status = "published";
           report.TrustScore = Math.Min(100, report.TrustScore + 20); // Boost
           break;
       
       case "reject":
           report.Status = "hidden";
           report.TrustScore = Math.Max(0, report.TrustScore - 30); // Penalty
           break;
       
       case "archive":
           report.Status = "archived";
           // Don't change Trust Score
           break;
   }
   ```

3. **Create Moderation Record**:
   ```csharp
   var moderation = new FloodReportModeration
   {
       Id = Guid.NewGuid(),
       FloodReportId = reportId,
       ModeratorUserId = moderatorId,
       Action = request.Action,
       Note = request.Note,
       CreatedAt = DateTime.UtcNow
   };
   
   await _moderationRepository.CreateAsync(moderation, ct);
   ```

4. **Update Report**:
   ```csharp
   report.UpdatedAt = DateTime.UtcNow;
   await _reportRepository.UpdateAsync(report, ct);
   ```

5. **Return Response**

---

## 🚀 IMPLEMENTATION PLAN

### Phase 1: Domain Layer

**Files to Create**:

```
src/Core/Domain/FDAAPI.Domain.RelationalDb/
├── Entities/
│   └── FloodReportModeration.cs
└── Repositories/
    └── IFloodReportModerationRepository.cs
```

### Phase 2: Application Layer

**Files to Create**:

```
src/Core/Application/
├── FDAAPI.App.FeatG84_GetEscalatedReports/
│   ├── GetEscalatedReportsRequest.cs
│   ├── GetEscalatedReportsResponse.cs
│   └── GetEscalatedReportsHandler.cs
└── FDAAPI.App.FeatG84_ModerateFloodReport/
    ├── ModerateFloodReportRequest.cs
    ├── ModerateFloodReportResponse.cs
    └── ModerateFloodReportHandler.cs
```

### Phase 3: Escalation Detection Service

**Files to Create**:

```
src/Core/Application/FDAAPI.App.Common/
└── Services/
    └── IEscalationDetector.cs
```

```csharp
public interface IEscalationDetector
{
    Task<bool> ShouldEscalateAsync(Guid reportId, CancellationToken ct);
    Task<string> GetEscalationReasonAsync(Guid reportId, CancellationToken ct);
}
```

**Implementation**:
```csharp
public class EscalationDetector : IEscalationDetector
{
    public async Task<bool> ShouldEscalateAsync(Guid reportId, CancellationToken ct)
    {
        var report = await _context.FloodReports
            .Include(r => r.Votes)
            .Include(r => r.Flags)
            .FirstOrDefaultAsync(r => r.Id == reportId, ct);
        
        // Check flag threshold
        if (report.Flags.Count >= 3)
            return true;
        
        // Check vote conflict
        var upvotes = report.Votes.Count(v => v.VoteType == "up");
        var downvotes = report.Votes.Count(v => v.VoteType == "down");
        if (upvotes > 10 && downvotes > 5)
            return true;
        
        // Check high-impact location (future: check nearby schools/hospitals)
        // if (IsNearHighImpactLocation(report.Latitude, report.Longitude))
        //     return true;
        
        return false;
    }
}
```

---

## 🧪 TESTING STRATEGY

### Unit Tests

1. **GetEscalatedReportsHandler Tests**:
   - ✅ Returns only escalated reports
   - ✅ Filters correctly
   - ✅ Includes escalation details

2. **ModerateFloodReportHandler Tests**:
   - ✅ Approve action → status = published, Trust Score increased
   - ✅ Reject action → status = hidden, Trust Score decreased
   - ✅ Archive action → status = archived, Trust Score unchanged
   - ✅ Rejects moderation of non-escalated reports

### Integration Tests

```bash
# Test 1: Get escalated reports
GET /api/v1/flood-reports/escalated
Authorization: Bearer {moderator_token}

# Expected: 200 OK, only escalated reports

# Test 2: Moderate (approve)
PUT /api/v1/flood-reports/{id}/moderate
Authorization: Bearer {moderator_token}
{"action": "approve", "note": "Verified"}

# Expected: 200 OK, status = published

# Test 3: Moderate non-escalated (should fail)
PUT /api/v1/flood-reports/{id}/moderate
Authorization: Bearer {moderator_token}
{"action": "approve"}

# Expected: 400 Bad Request
```

---

## 🚨 EDGE CASES & ERROR HANDLING

### 1. **Non-Moderator Access**

**Scenario**: Regular user tries to access escalated reports.

**Solution**: Return 403 Forbidden.

### 2. **Moderate Non-Escalated Report**

**Scenario**: Moderator tries to moderate published report.

**Solution**: Return 400 Bad Request with clear message.

### 3. **Concurrent Moderation**

**Scenario**: Two moderators moderate same report simultaneously.

**Solution**: Last write wins (simple approach) or optimistic locking.

---

## ✅ FEASIBILITY ASSESSMENT

### Technical Feasibility: ⭐⭐⭐⭐⭐ (5/5)

**Strengths**:
- ✅ Simple CRUD operations
- ✅ Clear escalation criteria
- ✅ Straightforward moderation actions

**Challenges**:
- ⚠️ Need to implement escalation detection logic
- ⚠️ Need to track high-impact locations (future)

### Business Feasibility: ⭐⭐⭐⭐⭐ (5/5)

**Strengths**:
- ✅ Handles edge cases automation can't
- ✅ Low volume (< 5% of reports)
- ✅ Scalable approach

### Implementation Effort: ⭐⭐⭐ (3/5)

**Estimated Time**: 1 week

**Breakdown**:
- Domain Layer: 1 day
- Application Layer: 2 days
- Escalation Detection: 2 days
- Presentation Layer: 1 day
- Testing: 1 day

---

## ⚠️ POTENTIAL ISSUES & MITIGATIONS

### Issue 1: Escalation Detection Accuracy

**Problem**: May miss some cases that need moderation.

**Mitigation**:
- Start with conservative thresholds (escalate more)
- Monitor false positive/negative rates
- Adjust thresholds based on data

### Issue 2: Moderator Availability

**Problem**: No moderators available to review escalated reports.

**Mitigation**:
- Escalated reports remain visible (not hidden)
- Auto-approve after 24 hours if no action (future)
- Notify moderators via email/push (future)

### Issue 3: Moderation Bias

**Problem**: Different moderators may have different standards.

**Mitigation**:
- Clear moderation guidelines
- Audit trail (all actions logged)
- Regular review of moderation decisions

---

## 🎯 ACCEPTANCE CRITERIA

### Backend Features

- [x] **FeatG84**: GET /api/v1/flood-reports/escalated
  - ✅ Returns only escalated reports
  - ✅ Requires Moderator role
  - ✅ Includes escalation details

- [x] **FeatG84**: PUT /api/v1/flood-reports/{id}/moderate
  - ✅ Approve action → published, Trust Score boost
  - ✅ Reject action → hidden, Trust Score penalty
  - ✅ Archive action → archived
  - ✅ Creates moderation audit record
  - ✅ Requires Moderator role

---

**Document Version**: 1.0  
**Last Updated**: 2026-02-05  
**Author**: Development Team  
**Status**: ✅ Ready for Implementation

