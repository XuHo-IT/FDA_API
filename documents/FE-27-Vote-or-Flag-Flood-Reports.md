# FE-27 – Vote or Flag Flood Reports

> **Feature Name**: Community Feedback on Flood Reports  
> **Created**: 2026-02-05  
> **Status**: 🟡 Planning  
> **Backend Features**: FeatG82, FeatG83  
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

**User Request**: Allow community to vote (up/down) or flag (spam/fake/inappropriate) flood reports to improve quality.

**Backend Scope**:
- ✅ Record votes and flags
- ✅ Update Trust Score based on votes/flags
- ✅ Auto-update priority when thresholds reached
- ✅ One vote/flag per user per report
- ❌ UI for voting/flagging (frontend responsibility)

### Backend Features to Implement

| Feature | Endpoint | Type | Description |
|---------|----------|------|-------------|
| **FeatG82** | `POST /api/v1/flood-reports/{id}/vote` | Command | Vote up or down on report |
| **FeatG83** | `POST /api/v1/flood-reports/{id}/flag` | Command | Flag report as spam/fake/inappropriate |

### Key Innovation: Real-Time Trust Score Updates

**Traditional Approach** (❌ Static):
```
Vote → Store in database → Manual review later
```

**Our Approach** (✅ Dynamic):
```
Vote/Flag → Update Trust Score immediately → Auto-hide if score < 30 → Auto-escalate if conflicts
```

**Impact on Trust Score**:
- Upvote: +5 points
- Downvote: -5 points
- Flag spam: -15 points
- Flag fake: -25 points
- Flag inappropriate: -10 points

**Auto-Actions**:
- Trust Score < 30 → Auto-hide report
- Flags ≥ 3 → Auto-escalate to moderation
- Upvotes ≥ 10 → Priority = high
- Upvotes ≥ 50 → Priority = critical

---

## 🔍 FEATURE ANALYSIS

### ✅ What's GOOD in Original Design

1. **Community Moderation**: Crowdsourced quality control
2. **Multiple Feedback Types**: Votes (quality) + Flags (abuse)
3. **Prevents Abuse**: One vote/flag per user
4. **Real Impact**: Votes/flags affect report visibility

### ⚠️ What Needs ADJUSTMENT

#### 1. **Real-Time Trust Score Updates**

**Issue**: Votes/flags should immediately affect report status.

**Solution**: Recalculate Trust Score after each vote/flag:
```csharp
// After vote/flag
var newTrustScore = await RecalculateTrustScore(reportId);
var report = await GetReportAsync(reportId);

if (newTrustScore < 30 && report.Status == "published")
{
    report.Status = "hidden";
    report.TrustScore = newTrustScore;
    await UpdateReportAsync(report);
}
```

#### 2. **Prevent Vote Manipulation**

**Issue**: Users could create multiple accounts to vote.

**Mitigation**:
- One vote per user (enforced by unique constraint)
- Rate limiting: max 10 votes per user per hour
- Monitor for suspicious patterns (future: ML detection)

#### 3. **Flag Thresholds**

**Issue**: How many flags trigger escalation?

**Solution**: Configurable thresholds:
- 3 flags → Escalate
- 5 flags → Auto-hide (even if Trust Score > 30)

---

## 🗄️ DATABASE SCHEMA

**Uses existing tables** (from FE-25):
- `flood_report_votes`
- `flood_report_flags`

**No new tables required**.

**Indexes** (already created in FE-25):
- `uq_vote` UNIQUE (flood_report_id, user_id)
- `uq_flag` UNIQUE (flood_report_id, user_id)

---

## 🔌 API SPECIFICATIONS

### FeatG82: Vote on Flood Report

**Endpoint**: `POST /api/v1/flood-reports/{id}/vote`

**Authentication**: Required (JWT Bearer token)

**Authorization**: Any authenticated user (User policy)

**Path Parameters**:
- `id` (UUID): Flood report ID

**Request Body**:
```json
{
  "type": "up"  // or "down"
}
```

**Validation Rules**:

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `type` | string | Yes | Enum: `up`, `down` |

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Vote recorded successfully",
  "data": {
    "reportId": "550e8400-e29b-41d4-a716-446655440000",
    "voteType": "up",
    "newTrustScore": 80,
    "status": "published",
    "priority": "high",
    "votes": {
      "up": 13,
      "down": 2
    }
  }
}
```

**Response** (400 Bad Request - already voted):
```json
{
  "success": false,
  "message": "You have already voted on this report",
  "errors": [
    {
      "field": "vote",
      "message": "Each user can only vote once per report"
    }
  ]
}
```

**Business Logic**:

1. **Validate Report Exists**:
   ```csharp
   var report = await _reportRepository.GetByIdAsync(reportId, ct);
   if (report == null)
       return NotFound("Flood report not found");
   ```

2. **Check Existing Vote**:
   ```csharp
   var existingVote = await _voteRepository.GetByUserAndReportAsync(
       userId, reportId, ct);
   
   if (existingVote != null)
   {
       // Update existing vote
       existingVote.VoteType = request.Type;
       existingVote.CreatedAt = DateTime.UtcNow;
       await _voteRepository.UpdateAsync(existingVote, ct);
   }
   else
   {
       // Create new vote
       var vote = new FloodReportVote
       {
           Id = Guid.NewGuid(),
           FloodReportId = reportId,
           UserId = userId,
           VoteType = request.Type,
           CreatedAt = DateTime.UtcNow
       };
       await _voteRepository.CreateAsync(vote, ct);
   }
   ```

3. **Recalculate Trust Score**:
   ```csharp
   var newTrustScore = await _trustScoreCalculator.RecalculateAsync(
       reportId, ct);
   
   report.TrustScore = newTrustScore;
   ```

4. **Update Priority**:
   ```csharp
   var upvoteCount = await _voteRepository.CountUpvotesAsync(reportId, ct);
   
   if (upvoteCount >= 50)
       report.Priority = "critical";
   else if (upvoteCount >= 10)
       report.Priority = "high";
   ```

5. **Auto-Hide if Low Score**:
   ```csharp
   if (newTrustScore < 30 && report.Status == "published")
   {
       report.Status = "hidden";
   }
   ```

6. **Save and Return**:
   ```csharp
   await _reportRepository.UpdateAsync(report, ct);
   
   return new VoteResponse
   {
       Success = true,
       Data = new VoteDataDto
       {
           ReportId = reportId,
           VoteType = request.Type,
           NewTrustScore = newTrustScore,
           Status = report.Status,
           Priority = report.Priority,
           Votes = new VoteSummaryDto
           {
               Up = await _voteRepository.CountUpvotesAsync(reportId, ct),
               Down = await _voteRepository.CountDownvotesAsync(reportId, ct)
           }
       }
   };
   ```

### FeatG83: Flag Flood Report

**Endpoint**: `POST /api/v1/flood-reports/{id}/flag`

**Authentication**: Required (JWT Bearer token)

**Authorization**: Any authenticated user (User policy)

**Path Parameters**:
- `id` (UUID): Flood report ID

**Request Body**:
```json
{
  "reason": "fake"  // or "spam", "inappropriate"
}
```

**Validation Rules**:

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `reason` | string | Yes | Enum: `spam`, `fake`, `inappropriate` |

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Flag recorded successfully",
  "data": {
    "reportId": "550e8400-e29b-41d4-a716-446655440000",
    "reason": "fake",
    "newTrustScore": 25,
    "status": "hidden",
    "flagCount": 3,
    "escalated": true
  }
}
```

**Business Logic**:

1. **Validate and Create Flag** (similar to vote)

2. **Update Trust Score** (with flag penalty):
   ```csharp
   var flagPenalty = request.Reason switch
   {
       "fake" => -25,
       "spam" => -15,
       "inappropriate" => -10,
       _ => 0
   };
   
   var newTrustScore = report.TrustScore + flagPenalty;
   report.TrustScore = Math.Max(0, Math.Min(100, newTrustScore));
   ```

3. **Check Flag Thresholds**:
   ```csharp
   var flagCount = await _flagRepository.CountAsync(reportId, ct);
   
   if (flagCount >= 5)
   {
       report.Status = "hidden"; // Auto-hide
   }
   else if (flagCount >= 3)
   {
       report.Status = "escalated"; // Needs moderation
   }
   ```

4. **Save and Return**

---

## 🚀 IMPLEMENTATION PLAN

### Phase 1: Domain Layer

**Files to Create**:

```
src/Core/Domain/FDAAPI.Domain.RelationalDb/
├── Repositories/
│   ├── IFloodReportVoteRepository.cs
│   └── IFloodReportFlagRepository.cs
```

### Phase 2: Application Layer

**Files to Create**:

```
src/Core/Application/
├── FDAAPI.App.FeatG82_VoteFloodReport/
│   ├── VoteFloodReportRequest.cs
│   ├── VoteFloodReportResponse.cs
│   └── VoteFloodReportHandler.cs
└── FDAAPI.App.FeatG83_FlagFloodReport/
    ├── FlagFloodReportRequest.cs
    ├── FlagFloodReportResponse.cs
    └── FlagFloodReportHandler.cs
```

### Phase 3: Trust Score Recalculation Service

**Files to Create**:

```
src/Core/Application/FDAAPI.App.Common/
└── Services/
    └── ITrustScoreRecalculator.cs
```

```csharp
public interface ITrustScoreRecalculator
{
    Task<int> RecalculateAsync(Guid reportId, CancellationToken ct);
}
```

**Implementation**:
```csharp
public class TrustScoreRecalculator : ITrustScoreRecalculator
{
    private readonly AppDbContext _context;
    
    public async Task<int> RecalculateAsync(Guid reportId, CancellationToken ct)
    {
        var report = await _context.FloodReports
            .Include(r => r.Votes)
            .Include(r => r.Flags)
            .FirstOrDefaultAsync(r => r.Id == reportId, ct);
        
        if (report == null)
            throw new NotFoundException("Report not found");
        
        // Start with original base score (from creation)
        int baseScore = 50; // Or retrieve from report creation metadata
        
        // Add vote impact
        var upvotes = report.Votes.Count(v => v.VoteType == "up");
        var downvotes = report.Votes.Count(v => v.VoteType == "down");
        var voteImpact = (upvotes * 5) - (downvotes * 5);
        
        // Subtract flag penalties
        var fakeFlags = report.Flags.Count(f => f.Reason == "fake");
        var spamFlags = report.Flags.Count(f => f.Reason == "spam");
        var inappropriateFlags = report.Flags.Count(f => f.Reason == "inappropriate");
        
        var flagPenalty = (fakeFlags * 25) + (spamFlags * 15) + (inappropriateFlags * 10);
        
        var newScore = baseScore + voteImpact - flagPenalty;
        
        return Math.Max(0, Math.Min(100, newScore));
    }
}
```

---

## 🧪 TESTING STRATEGY

### Unit Tests

1. **Vote Handler Tests**:
   - ✅ Creates new vote
   - ✅ Updates existing vote
   - ✅ Rejects duplicate vote (unique constraint)
   - ✅ Updates Trust Score correctly
   - ✅ Auto-hides when score < 30
   - ✅ Updates priority when upvotes ≥ 10

2. **Flag Handler Tests**:
   - ✅ Creates new flag
   - ✅ Updates Trust Score with penalties
   - ✅ Escalates when flags ≥ 3
   - ✅ Auto-hides when flags ≥ 5

3. **Trust Score Recalculator Tests**:
   - ✅ Calculates score correctly with votes
   - ✅ Applies flag penalties correctly
   - ✅ Clamps score to 0-100 range

### Integration Tests

```bash
# Test 1: Vote up
POST /api/v1/flood-reports/{id}/vote
Authorization: Bearer {token}
{"type": "up"}

# Expected: 200 OK, Trust Score increased

# Test 2: Vote again (update)
POST /api/v1/flood-reports/{id}/vote
Authorization: Bearer {token}
{"type": "down"}

# Expected: 200 OK, Vote updated, Trust Score decreased

# Test 3: Flag fake
POST /api/v1/flood-reports/{id}/flag
Authorization: Bearer {token}
{"reason": "fake"}

# Expected: 200 OK, Trust Score decreased by 25, escalated if flags ≥ 3
```

---

## 🚨 EDGE CASES & ERROR HANDLING

### 1. **Report Not Found**

**Scenario**: User votes on non-existent report.

**Solution**: Return 404 Not Found.

### 2. **Vote on Hidden Report**

**Scenario**: User tries to vote on hidden report.

**Solution**: Allow voting (user might want to restore it).

### 3. **Concurrent Votes**

**Scenario**: User votes from 2 devices simultaneously.

**Solution**: Unique constraint prevents duplicate, one succeeds.

### 4. **Trust Score Recalculation Race Condition**

**Scenario**: Multiple votes happen simultaneously.

**Solution**: Use database transaction, recalculate after all votes.

---

## ✅ FEASIBILITY ASSESSMENT

### Technical Feasibility: ⭐⭐⭐⭐⭐ (5/5)

**Strengths**:
- ✅ Simple CRUD operations
- ✅ Uses existing schema
- ✅ Unique constraints prevent duplicates
- ✅ Trust Score recalculation is deterministic

**Challenges**:
- ⚠️ Need to track base score from creation (store in metadata?)
- ⚠️ Recalculation could be expensive (consider caching)

### Business Feasibility: ⭐⭐⭐⭐⭐ (5/5)

**Strengths**:
- ✅ Community-driven quality control
- ✅ Real-time impact on report visibility
- ✅ Prevents spam/fake reports

### Implementation Effort: ⭐⭐⭐⭐ (4/5)

**Estimated Time**: 1.5 weeks

**Breakdown**:
- Domain Layer: 1 day
- Application Layer: 3 days
- Trust Score Recalculator: 2 days
- Presentation Layer: 2 days
- Testing: 2 days

---

## ⚠️ POTENTIAL ISSUES & MITIGATIONS

### Issue 1: Vote Manipulation

**Problem**: Users create multiple accounts to upvote their own reports.

**Mitigation**:
- One vote per user (enforced by unique constraint)
- Rate limiting: max 10 votes per user per hour
- Monitor for suspicious patterns
- Future: ML-based fraud detection

### Issue 2: Trust Score Recalculation Performance

**Problem**: Recalculating for every vote could be slow.

**Mitigation**:
- Batch recalculation (every N votes or every M minutes)
- Cache Trust Score, invalidate on vote/flag
- Use background job for recalculation (future)

### Issue 3: Flag Abuse

**Problem**: Users flag legitimate reports to hide them.

**Mitigation**:
- Require 3+ flags to escalate (not just 1)
- Track flagger reputation (future)
- Moderator review for escalated reports (FE-28)

### Issue 4: Base Score Tracking

**Problem**: Need original Trust Score from creation to recalculate.

**Solution**: Store base score in report metadata or separate field.

---

## 🎯 ACCEPTANCE CRITERIA

### Backend Features

- [x] **FeatG82**: POST /api/v1/flood-reports/{id}/vote
  - ✅ Records vote (up/down)
  - ✅ One vote per user per report
  - ✅ Updates Trust Score immediately
  - ✅ Auto-hides if score < 30
  - ✅ Updates priority when thresholds reached

- [x] **FeatG83**: POST /api/v1/flood-reports/{id}/flag
  - ✅ Records flag (spam/fake/inappropriate)
  - ✅ One flag per user per report
  - ✅ Applies flag penalties to Trust Score
  - ✅ Escalates when flags ≥ 3
  - ✅ Auto-hides when flags ≥ 5

---

**Document Version**: 1.0  
**Last Updated**: 2026-02-05  
**Author**: Development Team  
**Status**: ✅ Ready for Implementation

