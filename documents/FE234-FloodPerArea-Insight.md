# FE234: Flood Evaluation Per Monitored Area - Technical Analysis

## 📋 Table of Contents

- [Overview](#overview)
- [Strengths](#strengths)
- [Areas for Adjustment](#areas-for-adjustment)
  - [1. Evaluation Logic & Precision](#1-evaluation-logic--precision)
  - [2. Spatial Query Performance](#2-spatial-query-performance)
  - [3. Notification Integration](#3-notification-integration)
- [Implementation Plan](#implementation-plan)
  - [Phase 1: Domain Layer](#phase-1-domain-layer)
  - [Phase 2: Application Layer (CQRS)](#phase-2-application-layer-cqrs)
  - [Phase 3: Presentation Layer (FastEndpoints)](#phase-3-presentation-layer-fastendpoints)
- [Database Schema Requirements](#database-schema-requirements)
- [API Endpoints Requirements](#api-endpoints-requirements)
- [Feature Breakdown for Backend](#feature-breakdown-for-backend)
- [Final Recommendations](#final-recommendations)

---

## Overview

**Status:** ✅ Well-designed and aligned with FDA API architecture

This feature enables users to define personalized "Monitored Areas" (center point + radius) and receive real-time flood status evaluations. It effectively bridges the gap between raw sensor data and user-centric situational awareness.

---

## Strengths

**Personalized Monitoring:**
- ✅ **User-Defined Radii**: Flexibility in monitoring range (e.g., 500m for home, 2km for neighborhood).
- ✅ **Real-time Aggregation**: Status reflects the absolute latest sensor readings.

**Robust Evaluation Logic:**
- ✅ **Haversine Formula**: Precise distance calculations between areas and stations.
- ✅ **Numeric Weighting**: 0-3 scale for clear severity categorization.
- ✅ **Distributed Caching**: 30-second TTL to balance freshness and performance.

---

## Areas for Adjustment

### 1. Evaluation Logic & Precision

**Issue:** The current logic uses a simple `Max(Weight)` aggregation. While effective, it doesn't account for "Inverse Distance Weighting" (IDW), where a station 100m away should perhaps influence the status more than one 2km away.

**Backend Implementation Requirements:**

| Component | Status | Description |
|-----------|--------|-------------|
| Weighting Logic | ✅ Required | Already implemented with basic weights |
| Polygon Intersection | ❌ Future | Integrate flood risk polygons |
| Distance Decay | ❌ Optional | Weigh closer sensors higher |

**Recommendation:**
- Keep the current `Max(Weight)` for simplicity in V1, but plan for polygon intersection in V2 to handle "blind spots" where no sensors exist.

---

### 2. Spatial Query Performance

**Issue:** The system currently fetches all active stations (up to 1000) and filters them in memory using C#. While fine for small datasets, it won't scale to thousands of stations.

**Recommendation:**
- **Optimization:** Use a "Bounding Box" query in SQL to narrow down stations before the Haversine calculation in memory.
- **Advanced:** Enable PostGIS for `ST_DWithin` spatial indexing.

---

### 3. Notification Integration

**Issue:** Evaluation is currently "on-demand" (pull model). Users only know the status when they open the app.

**Recommendation:**
- Implement a **Background Worker** that periodically evaluates areas for all active users and triggers push notifications if the status changes (e.g., Normal → Warning).

---

## Implementation Plan

### Phase 1: Domain Layer

**Entities/Area.cs**
```csharp
public class Area : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int RadiusMeters { get; set; }
    public string AddressText { get; set; }
    
    public virtual User User { get; set; }
}
```

**Repositories/IAreaRepository.cs**
```csharp
public interface IAreaRepository
{
    Task<Area?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<(List<Area> Areas, int TotalCount)> GetByUserIdAsync(Guid userId, string? searchTerm, int page, int size, CancellationToken ct);
    Task<Guid> CreateAsync(Area area, CancellationToken ct);
    Task<bool> UpdateAsync(Area area, CancellationToken ct);
}
```

### Phase 2: Application Layer (CQRS)

**FeatG34_AreaStatusEvaluate/AreaStatusEvaluateHandler.cs**
```csharp
public async Task<AreaStatusEvaluateResponse> Handle(AreaStatusEvaluateRequest request, CancellationToken ct)
{
    // 1. Get Area & Active Stations
    // 2. Filter stations within radius (Haversine)
    // 3. Get latest sensor readings
    // 4. Calculate status & weights
    // 5. Cache result for 30s
    return new AreaStatusEvaluateResponse { Data = statusDto };
}
```

### Phase 3: Presentation Layer (FastEndpoints)

**Endpoints/Feat34_AreaStatusEvaluate/AreaStatusEvaluateEndpoint.cs**
```csharp
public override void Configure()
{
    Get("/api/v1/areas/{id}/status");
    Policies("User");
}
```

---

## Database Schema Requirements

### Recommended Approach: Relational + Geo Index

```sql
CREATE TABLE areas (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    latitude NUMERIC(10,6) NOT NULL,
    longitude NUMERIC(10,6) NOT NULL,
    radius_meters INT DEFAULT 1000,
    address_text TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Optimization Indexes
CREATE INDEX ix_areas_user ON areas(user_id);
CREATE INDEX ix_areas_location ON areas(latitude, longitude);
```

---

## API Endpoints Requirements

### FeatG32: Create Monitored Area
**Endpoint:** `POST /api/v1/areas`
**Auth:** Authenticated (`Authority` policy)

**Request:**
```json
{
  "name": "My Workspace",
  "latitude": 10.762622,
  "longitude": 106.660172,
  "radiusMeters": 1000,
  "addressText": "123 Dist 1, HCMC"
}
```

### FeatG34: Evaluate Area Status
**Endpoint:** `GET /api/v1/areas/{id}/status`
**Auth:** Authenticated (`User` policy)

**Response:**
```json
{
  "success": true,
  "data": {
    "areaId": "uuid",
    "status": "Warning",
    "severityLevel": 3,
    "summary": "Warning: Critical water level detected at Station ST_01 (200m away).",
    "contributingStations": [
      {
        "stationCode": "ST_01",
        "distance": 200,
        "waterLevel": 2.5,
        "severity": "critical",
        "weight": 3
      }
    ]
  }
}
```

---

## Feature Breakdown for Backend

| Feature | Handler | Endpoint | Auth |
|---------|---------|----------|------|
| **FeatG32** | `CreateAreaHandler` | `POST /api/v1/areas` | Authority |
| **FeatG33** | `AreaListByUserHandler` | `GET /api/v1/areas-created` | Authority |
| **FeatG34** | `AreaStatusEvaluateHandler` | `GET /api/v1/areas/{id}/status` | User |
| **FeatG35** | `AreaGetHandler` | `GET /api/v1/areas/{id}` | User |
| **FeatG36** | `UpdateAreaHandler` | `PUT /api/v1/areas/{id}` | User |
| **FeatG37** | `DeleteAreaHandler` | `DELETE /api/v1/areas/{id}` | User |

---

## Final Recommendations

1. **Polygon Support**: In Phase 2, integrate flood hazard polygons into the `AreaStatusEvaluateHandler`.
2. **PostGIS**: Migrate to PostGIS for station lookups once the station count exceeds 500.
3. **Optimistic UI**: Frontend should show cached status immediately while re-evaluating in the background.

---

## Adjusted Feature Definition

**Feature Name:** FE234 - Flood Evaluation Per Monitored Area

**Scope:**
- Area management for authenticated users.
- Real-time status evaluation using nearest-neighbor sensor logic.
- Distributed caching for performance.

---

## Summary

FE234 provides a vital user-facing capability. By focusing on precision and performance now, the system remains scalable as the IoT network and user base grow. The clean separation of evaluation logic into its own handler ensures it can be easily refined in the future.
