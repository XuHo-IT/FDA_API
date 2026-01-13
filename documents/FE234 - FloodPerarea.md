# FE-234 – Flood Evaluation Per Monitored Area

> **Feature Name**: Flood Status Evaluation for User-Defined Areas
> **Created**: 2026-01-12
> **Status**: 🟡 Planning
> **Backend Features**: FeatG32, FeatG33, FeatG34
> **Priority**: High

---

## 📋 TABLE OF CONTENTS

1. [Executive Summary](#executive-summary)
2. [Feature Analysis](#feature-analysis)
3. [Domain Model](#domain-model)
4. [Status Logic & Aggregation](#status-logic--aggregation)
5. [API Specifications](#api-specifications)
6. [Implementation Plan](#implementation-plan)
7. [Testing Strategy](#testing-strategy)
8. [Performance & Optimization](#performance--optimization)

---

## 📊 EXECUTIVE SUMMARY

### Feature Overview

**Problem**: Users need to know the flood status of specific locations important to them (home, office, school) rather than just general station statuses.

**Solution**: Allow users to define "Monitored Areas" (center point + radius). The system will aggregate data from nearby IoT sensors and flood risk polygons to provide a real-time status for each area.

### Backend Features to Implement

| Feature | Endpoint | Type | Description |
|---------|----------|------|-------------|
| **FeatG32** | `POST /api/v1/areas` | Command | Create a new monitored area |
| **FeatG33** | `GET /api/v1/areas` | Query | List user's monitored areas |
| **FeatG34** | `GET /api/v1/areas/{id}/status` | Query | Evaluate and return flood status for an area |

---

## 🔍 FEATURE ANALYSIS

### Key Requirements
1. **Dynamic Areas**: Users can define multiple areas with custom radii.
2. **Real-time Aggregation**: Status must reflect the latest sensor readings.
3. **Proximity Logic**: Only sensors within the defined radius should contribute to an area's status.
4. **Defined Statuses**:
   - ✅ **Normal**: No significant water level rise.
   - 🟡 **Watch**: Approaching warning thresholds or potential risk detected.
   - 🔴 **Warning**: Thresholds exceeded or active flooding in proximity.
   - ⚪ **Unknown**: No sensors available within radius or data is offline.

---

## 🗄️ DOMAIN MODEL

### New Table: `areas`

```sql
CREATE TABLE areas (
  id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id       UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  name          VARCHAR(255),
  latitude      NUMERIC(10,6) NOT NULL,
  longitude     NUMERIC(10,6) NOT NULL,
  radius_m      INT DEFAULT 1000, -- Monitoring radius in meters
  address_text  TEXT,

  created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at    TIMESTAMPTZ DEFAULT NOW()
);

-- Optimization for Spatial Queries (Choose one)

-- Option A: PostGIS (Recommended for high performance)
-- ALTER TABLE areas ADD COLUMN location GEOGRAPHY(Point, 4326);
-- CREATE INDEX ix_areas_location ON areas USING GIST(location);

-- Option B: Basic Index (Standard for smaller datasets)
CREATE INDEX ix_areas_user ON areas(user_id);
CREATE INDEX ix_areas_geo ON areas(latitude, longitude); 
```

---

## 🧠 STATUS LOGIC & AGGREGATION

### 1. Proximity Search
To evaluate an area at $(Lat_A, Lng_A)$ with $Radius_R$:
1. Fetch all active **Stations** within the bounding box of the circle.
2. Calculate the distance for each: $Distance(Station, Area) \leq Radius_R$.

### 2. Severity Mapping & Weighting
Each station contributes a severity score based on its latest reading.

| Condition | Severity | Weight | Area Status Mapping |
|-----------|----------|--------|---------------------|
| No data / No station | **Unknown** | -1 | **Unknown** |
| Reading < Warning | **Safe** | 0 | **Normal** |
| Reading >= Warning | **Warning** | 2 | **Watch** |
| Reading >= Critical | **Critical** | 3 | **Warning** |

**Note on Weighting**: Using numeric weights (0-3) future-proofs the system for ML-based predictions, threshold tuning, and weighted aggregations where some stations may have higher reliability than others.

**Final Area Status Calculation**: 
1. If no sensors found within range → **Unknown**.
2. Otherwise, `Area Status = Max(Station Severity Weights)`.

### 3. Polygon Data Integration (Future)
If the area center or radius intersects with a "High Risk" flood polygon, the status is automatically escalated to **Watch** or **Warning** regardless of sensor data.

---

## 🔌 API SPECIFICATIONS

### FeatG32: Create Monitored Area
**Endpoint**: `POST /api/v1/areas`
**Request**:
```json
{
  "name": "My Home",
  "latitude": 10.762622,
  "longitude": 106.660172,
  "radiusMeters": 500,
  "addressText": "123 District 1, HCMC"
}
```

### FeatG34: Get Area Flood Status
**Endpoint**: `GET /api/v1/areas/{id}/status`
**Response**:
```json
{
  "success": true,
  "data": {
    "areaId": "uuid",
    "status": "warning",
    "severityLevel": 3,
    "summary": "Critical water level detected at Station ST_01 (200m away)",
    "contributingStations": [
      {
        "stationCode": "ST_01",
        "distance": 200,
        "waterLevel": 3.2,
        "severity": "critical",
        "weight": 3
      }
    ],
    "evaluatedAt": "2026-01-12T10:00:00Z"
  }
}
```

---

## 🚀 IMPLEMENTATION PLAN

### Phase 1: Domain & Infrastructure
1. Create `Area` entity in `Core.Domain`.
2. Implement `IAreaRepository` and `PgsqlAreaRepository`.
3. Add `Areas` table migration (Consider PostGIS for Geo Index).

### Phase 2: Application Logic
1. **FeatG32-33**: Standard CRUD for Areas.
2. **FeatG34**: Implement the evaluation engine.
   - Use Haversine formula for distance calculation (or PostGIS `ST_DWithin`).
   - Aggregate latest readings and apply numeric weights.
   - Handle **Unknown** status when no sensors are in range.

---

## ⚡ PERFORMANCE & OPTIMIZATION
1. **Caching**: Result of FeatG34 should be cached per Area for 30-60s to reduce DB load.
2. **Background Jobs**: For users with many areas, precompute statuses periodically using background tasks (e.g., Quartz.NET).
3. **Database**: Use PostGIS if the number of areas/stations exceeds 10k for optimal spatial lookups.

---

## 🧪 TESTING STRATEGY
1. **Unit Test**: Haversine distance calculation and numeric weighting.
2. **Integration Test**: Create area -> No station nearby -> Expect **Unknown**.
3. **Integration Test**: Create area -> Critical reading nearby -> Expect **Warning** (Weight 3).

---

**Document Version**: 1.1
**Last Updated**: 2026-01-12
**Author**: FDA Development Team
**Status**: 🟡 Planning / Discussion
