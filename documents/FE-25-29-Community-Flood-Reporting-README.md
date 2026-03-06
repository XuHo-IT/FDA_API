# FE-25 → FE-29 – Community Flood Reporting & Moderation APIs

> **Feature Group**: Community Flood Reporting System  
> **Created**: 2026-02-05  
> **Status**: 🟡 Planning  
> **Priority**: High

---

## 📋 TABLE OF CONTENTS

1. [Overview](#overview)
2. [Architecture Principles](#architecture-principles)
3. [Feature Breakdown](#feature-breakdown)
4. [Domain Model](#domain-model)
5. [Database Design](#database-design)
6. [API Endpoints Summary](#api-endpoints-summary)
7. [Authorization Matrix](#authorization-matrix)
8. [Implementation Phases](#implementation-phases)
9. [Key Innovations](#key-innovations)
10. [Out of Scope](#out-of-scope)

---

## 📊 OVERVIEW

### Feature Group Description

Nhóm feature FE-25 → FE-29 xây dựng **Community Flood Reporting System**, cho phép người dân:

- 📸 **Báo cáo điểm ngập** (ảnh/video) - FE-25
- 👁️ **Xem báo cáo cộng đồng** - FE-26
- 👍 **Vote / Flag báo cáo** - FE-27
- 🔍 **Moderator kiểm duyệt** (hiếm khi) - FE-28
- 🚨 **Authority xử lý báo cáo ưu tiên cao** - FE-29

### Key Innovation: Automation-First Approach

**Traditional Approach** (❌ Not scalable):
```
Report → Pending → Moderator reviews → Approved/Rejected
```

**Our Approach** (✅ Scalable):
```
Report → Trust Score → Auto-publish (≥60) OR Auto-hide (<30) OR Escalate (conflicts)
```

**Reality Check**:
- 99% reports không được duyệt tay
- Hệ thống tự đánh giá độ tin cậy
- Con người chỉ xử lý edge cases & high-impact

---

## 🎯 ARCHITECTURE PRINCIPLES

### Core Principles

1. **FastEndpoints + CQRS**: Standard FDA API pattern
2. **Domain-Centric**: FloodReport là aggregate root
3. **Role-Based Authorization**: Clear permission matrix
4. **Automation-First**: Trust Score system handles 95% of cases
5. **Media Storage**: Backend uploads photos to ImageKit, videos to Cloudinary

### Layer Responsibilities

```
┌─────────────────────────────────────────────────┐
│ BACKEND SCOPE (FDA API)                         │
├─────────────────────────────────────────────────┤
│ ✅ Report CRUD operations                        │
│ ✅ Trust Score calculation                       │
│ ✅ Vote/Flag recording                           │
│ ✅ Priority calculation                          │
│ ✅ Moderation (escalated cases only)             │
│ ✅ Authority review (high-priority only)        │
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────┐
│ FRONTEND SCOPE (Mobile/Web App)                 │
├─────────────────────────────────────────────────┤
│ ✅ Media upload UI                               │
│ ✅ Map rendering                                 │
│ ✅ Report submission form                        │
│ ✅ Vote/Flag UI                                  │
│ ✅ Media preview                                 │
└─────────────────────────────────────────────────┘
```

---

## 🧩 FEATURE BREAKDOWN

### 🟢 FE-25 – Report Flood Points (Photo / Video)

**Backend Feature**: FeatG80  
**Endpoint**: `POST /api/v1/flood-reports`  
**Auth**: Optional (AllowAnonymous)

**Responsibilities**:
- ✅ Accept flood report with location, description, severity
- ✅ Accept media files via multipart/form-data
- ✅ Upload photos to ImageKit
- ✅ Upload videos to Cloudinary (with thumbnail generation)
- ✅ Calculate Trust Score automatically
- ✅ Determine status (published/hidden/escalated)
- ✅ Validate file types, sizes, and formats

**Key Innovation**: Trust Score automation

**Documentation**: [FE-25-Report-Flood-Points.md](./FE-25-Report-Flood-Points.md)

---

### 🟡 FE-26 – View Community Flood Reports

**Backend Feature**: FeatG81  
**Endpoint**: `GET /api/v1/flood-reports/community`  
**Auth**: Public (AllowAnonymous)

**Responsibilities**:
- ✅ Query published flood reports only
- ✅ Filter by bounds, severity, time range
- ✅ Aggregate votes and flags
- ✅ Sort by trust score and recency
- ✅ Pagination support

**Key Requirement**: Never return `hidden` or `escalated` reports

**Documentation**: [FE-26-View-Community-Flood-Reports.md](./FE-26-View-Community-Flood-Reports.md)

---

### 🟠 FE-27 – Vote or Flag Flood Reports

**Backend Features**: FeatG82, FeatG83  
**Endpoints**: 
- `POST /api/v1/flood-reports/{id}/vote`
- `POST /api/v1/flood-reports/{id}/flag`  
**Auth**: Required (User policy)

**Responsibilities**:
- ✅ Record votes (up/down) and flags (spam/fake/inappropriate)
- ✅ Update Trust Score immediately
- ✅ Auto-update priority when thresholds reached
- ✅ One vote/flag per user per report

**Key Innovation**: Real-time Trust Score updates

**Impact on Trust Score**:
- Upvote: +5 points
- Downvote: -5 points
- Flag spam: -15 points
- Flag fake: -25 points

**Auto-Actions**:
- Trust Score < 30 → Auto-hide
- Flags ≥ 3 → Auto-escalate
- Upvotes ≥ 10 → Priority = high
- Upvotes ≥ 50 → Priority = critical

**Documentation**: [FE-27-Vote-or-Flag-Flood-Reports.md](./FE-27-Vote-or-Flag-Flood-Reports.md)

---

### 🔵 FE-28 – Moderate Community Flood Reports

**Backend Feature**: FeatG84  
**Endpoints**: 
- `GET /api/v1/flood-reports/escalated`
- `PUT /api/v1/flood-reports/{id}/moderate`  
**Auth**: Moderator role only

**Responsibilities**:
- ✅ List escalated reports only (< 5% of total)
- ✅ Approve/reject/archive reports
- ✅ Add moderation notes
- ✅ Track moderation actions

**Key Principle**: Rare human intervention (only escalated cases)

**Escalation Triggers**:
- 3+ flags from community
- Conflicting votes (many upvotes + many downvotes)
- High-impact location
- Trust Score conflicts

**Documentation**: [FE-28-Moderate-Community-Flood-Reports.md](./FE-28-Moderate-Community-Flood-Reports.md)

---

### 🔴 FE-29 – Review High-priority Reports

**Backend Feature**: FeatG85  
**Endpoints**: 
- `GET /api/v1/flood-reports/high-priority`
- `PUT /api/v1/flood-reports/{id}/acknowledge`
- `PUT /api/v1/flood-reports/{id}/respond`  
**Auth**: Authority role only

**Responsibilities**:
- ✅ List high-priority reports (priority = high/critical)
- ✅ Filter by severity, location, time
- ✅ Track authority actions (acknowledge/respond)
- ✅ Calculate response time metrics

**Key Principle**: Authority focus on high-impact cases

**Priority Criteria**:
- Trust Score ≥ 70 AND Severity = high → Priority = high
- Upvotes ≥ 50 → Priority = high
- Upvotes ≥ 100 → Priority = critical
- Multiple reports at same location → Priority = high

**Documentation**: [FE-29-Review-High-priority-Reports.md](./FE-29-Review-High-priority-Reports.md)

---

## 🧱 DOMAIN MODEL

### Aggregate Root: FloodReport

```
FloodReport
 ├─ Id
 ├─ ReporterUserId (nullable for anonymous)
 ├─ Location (lat, lng)
 ├─ Address (optional)
 ├─ Description
 ├─ Media[] (photo/video URLs)
 ├─ Severity (user-reported: low | medium | high)
 ├─ TrustScore (0-100, auto-calculated)
 ├─ Status (published | hidden | escalated | archived)
 ├─ ConfidenceLevel (low | medium | high)
 ├─ Priority (normal | high | critical, auto-calculated)
 ├─ Votes (Up / Down)
 ├─ Flags (Spam / Fake / Inappropriate)
 ├─ CreatedAt
 └─ UpdatedAt
```

### Related Entities

- **FloodReportMedia**: Photo/video URLs
- **FloodReportVote**: User votes (up/down)
- **FloodReportFlag**: User flags (spam/fake/inappropriate)
- **FloodReportModeration**: Moderator actions (audit trail)
- **FloodReportAuthorityAction**: Authority actions (acknowledge/respond)

---

## 🗄️ DATABASE DESIGN

### Core Tables

1. **flood_reports** (Main table)
   - Trust Score (0-100)
   - Status (published | hidden | escalated | archived)
   - Priority (normal | high | critical)

2. **flood_report_media** (Media URLs)
   - Media type (photo | video)
   - Media URL
   - Thumbnail URL (for videos)

3. **flood_report_votes** (Community votes)
   - Vote type (up | down)
   - Unique constraint: (flood_report_id, user_id)

4. **flood_report_flags** (Community flags)
   - Flag reason (spam | fake | inappropriate)
   - Unique constraint: (flood_report_id, user_id)

5. **flood_report_moderations** (Moderator actions)
   - Action (approve | reject | archive)
   - Note

6. **flood_report_authority_actions** (Authority actions)
   - Action type (acknowledge | respond)
   - Response time tracking

**See individual feature docs for complete schema**.

---

## 🔌 API ENDPOINTS SUMMARY

| Feature | Method | Endpoint | Auth | Role |
|---------|--------|----------|------|------|
| **FE-25** | POST | `/api/v1/flood-reports` | Optional | Public |
| **FE-26** | GET | `/api/v1/flood-reports/community` | Public | Public |
| **FE-27** | POST | `/api/v1/flood-reports/{id}/vote` | Required | User |
| **FE-27** | POST | `/api/v1/flood-reports/{id}/flag` | Required | User |
| **FE-28** | GET | `/api/v1/flood-reports/escalated` | Required | Moderator |
| **FE-28** | PUT | `/api/v1/flood-reports/{id}/moderate` | Required | Moderator |
| **FE-29** | GET | `/api/v1/flood-reports/high-priority` | Required | Authority |
| **FE-29** | PUT | `/api/v1/flood-reports/{id}/acknowledge` | Required | Authority |
| **FE-29** | PUT | `/api/v1/flood-reports/{id}/respond` | Required | Authority |

---

## 🔐 AUTHORIZATION MATRIX

| Feature | Citizen | Guest | Moderator | Authority |
|---------|---------|-------|-----------|-----------|
| **FE-25 Report** | ✅ | ✅ | ❌ | ❌ |
| **FE-26 View** | ✅ | ✅ | ✅ | ✅ |
| **FE-27 Vote/Flag** | ✅ | ❌ | ❌ | ❌ |
| **FE-28 Moderate** | ❌ | ❌ | ✅ | ❌ |
| **FE-29 Review Priority** | ❌ | ❌ | ❌ | ✅ |

---

## 🚧 IMPLEMENTATION PHASES

### Phase 1 – Reporting Core (Week 1-2)

**Features**: FE-25

**Tasks**:
- Create FloodReport entity
- Create FloodReportMedia entity
- Implement Trust Score calculator
- Create report endpoint
- Media URL validation

**Deliverables**:
- Users can submit flood reports
- Reports auto-published/hidden based on Trust Score

---

### Phase 2 – Community Interaction (Week 3-4)

**Features**: FE-26, FE-27

**Tasks**:
- Create vote/flag entities
- Implement vote/flag endpoints
- Implement Trust Score recalculation
- Create community view endpoint
- Priority calculation service

**Deliverables**:
- Users can view published reports
- Users can vote/flag reports
- Trust Score updates in real-time

---

### Phase 3 – Governance & Authority (Week 5-6)

**Features**: FE-28, FE-29

**Tasks**:
- Create moderation entities
- Create authority action entities
- Implement escalation detection
- Implement moderation endpoints
- Implement authority review endpoints

**Deliverables**:
- Moderators can review escalated reports
- Authorities can review high-priority reports
- Response time tracking

---

## 💡 KEY INNOVATIONS

### 1. Trust Score Automation

**Problem**: Manual moderation doesn't scale.

**Solution**: Automated Trust Score calculation based on:
- Reporter reputation (20%)
- Media presence (15%)
- Geo consistency (25%)
- Time relevance (20%)
- Initial score (20%)

**Impact**: 95% of reports handled automatically.

---

### 2. Real-Time Trust Score Updates

**Problem**: Votes/flags should immediately affect report visibility.

**Solution**: Recalculate Trust Score after each vote/flag:
- Upvote: +5 points
- Downvote: -5 points
- Flag fake: -25 points

**Impact**: Community feedback has immediate effect.

---

### 3. Escalation-Only Moderation

**Problem**: Moderating every report is impossible.

**Solution**: Only escalate reports that need human review:
- 3+ flags
- Conflicting votes
- High-impact locations

**Impact**: Moderators handle < 5% of reports.

---

### 4. Priority-Based Authority Review

**Problem**: Authorities overwhelmed by all reports.

**Solution**: Focus on high-priority only:
- Trust Score ≥ 70 AND Severity = high
- Upvotes ≥ 50
- Near critical infrastructure

**Impact**: Authorities focus on actionable cases.

---

## ❌ OUT OF BACKEND SCOPE

### Frontend Responsibilities

- ✅ Media selection UI (camera/gallery picker)
- ✅ File preview before upload
- ✅ Upload progress indicator
- ✅ Map rendering (Mapbox/Leaflet)
- ✅ Report submission form (multipart/form-data)
- ✅ Vote/Flag UI
- ✅ Media preview (display URLs from backend)

### Future Enhancements

- ❌ Spam detection AI (future)
- ❌ Image verification AI (future)
- ❌ Duplicate report detection (future)
- ❌ Notification system (covered in other features)
- ❌ Emergency dispatch system (out of scope)

---

## ✅ FINAL ASSESSMENT

### Criteria Ratings

| Criteria | Rating | Notes |
|----------|--------|-------|
| **Domain clarity** | ⭐⭐⭐⭐⭐ | Clear aggregate root, well-defined entities |
| **FDA architecture fit** | ⭐⭐⭐⭐⭐ | Follows Domain-Centric, CQRS patterns |
| **Scalability** | ⭐⭐⭐⭐⭐ | Automation-first approach scales infinitely |
| **Capstone value** | ⭐⭐⭐⭐⭐ | Real-world proven approach, impressive demo |
| **Implementation complexity** | ⭐⭐⭐⭐ | Moderate complexity, well-documented |

### Overall Assessment

**Status**: ✅ **Ready for Implementation**

**Strengths**:
- ✅ Automation-first approach (scalable)
- ✅ Clear separation of concerns
- ✅ Well-defined domain model
- ✅ Real-world proven patterns

**Challenges**:
- ⚠️ Trust Score algorithm needs tuning
- ⚠️ Performance optimization for large datasets
- ⚠️ Spam/fake report handling

---

## 📚 DOCUMENTATION LINKS

- [FE-25: Report Flood Points](./FE-25-Report-Flood-Points.md)
- [FE-26: View Community Flood Reports](./FE-26-View-Community-Flood-Reports.md)
- [FE-27: Vote or Flag Flood Reports](./FE-27-Vote-or-Flag-Flood-Reports.md)
- [FE-28: Moderate Community Flood Reports](./FE-28-Moderate-Community-Flood-Reports.md)
- [FE-29: Review High-priority Reports](./FE-29-Review-High-priority-Reports.md)

---

## 🎯 NEXT STEPS

1. ✅ Review all documentation
2. ✅ Approve architecture approach
3. ✅ Create implementation tasks
4. ✅ Begin Phase 1 (FE-25)

---

**Document Version**: 1.0  
**Last Updated**: 2026-02-05  
**Author**: Development Team  
**Status**: ✅ Ready for Implementation

