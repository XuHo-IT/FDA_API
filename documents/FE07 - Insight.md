# FE-07: Filter and Toggle Map Layers - Technical Analysis

## 📋 Table of Contents

- [Overview](#overview)
- [Strengths](#strengths)
- [Areas for Adjustment](#areas-for-adjustment)
  - [Backend vs Frontend Scope](#1-backend-scope-vs-frontend-scope)
  - [Database Schema](#2-database-schema-requirements)
  - [Guest User Settings](#3-guest-user-settings-device-based)
  - [API Endpoints](#4-api-endpoints-requirements)
  - [Feature Breakdown](#5-feature-breakdown-for-backend)
- [Implementation Plan](#implementation-plan)
- [Database Changes](#database-changes-required)
- [Final Recommendations](#final-recommendations)

---

## Overview

**Status:** ✅ Well-designed and aligned with FDA API architecture

This feature has been thoroughly designed and fits well with the FDA API architecture. However, some adjustments are needed to align with:

- Backend API pattern (FastEndpoints + CQRS)
- Current database schema
- Domain-Centric architecture

---

## Strengths

**Clear Scope:**
- 4 layers: Flood / Traffic / Weather / Satellite

**Excellent User Experience:**
- ✅ Persist settings (logged-in vs guest)
- ✅ Debounce to prevent API spam
- ✅ Optimistic UI updates
- ✅ Comprehensive edge cases: Offline, provider errors, multi-device sync
- ✅ Technical details: z-index, opacity, legend

---

## Areas for Adjustment

### 1. Backend Scope vs Frontend Scope

**Issue:** This feature is 80% frontend logic. Backend only needs to provide:
- API to save/retrieve user preferences
- API to fetch data for layers (flood severity, traffic, weather)

**Backend Implementation Requirements:**

| Component | Status | Description |
|-----------|--------|-------------|
| User preferences API | ✅ Required | Save layer settings |
| Flood severity data API | ✅ Required | Already have stations/water_levels |
| Traffic API | ❌ Not required | External provider - not FDA backend scope |
| Weather API | ❌ Not required | External provider - not FDA backend scope |
| Map rendering logic | ❌ Not required | Completely frontend |

**Recommendation:**
- **Backend feature:** FE-07a - User Map Preferences API
- **Backend feature:** FE-07b - Flood Severity Layer Data API
- **Frontend feature:** Implement full layer toggle UI + integration with external providers

---

### 2. Database Schema Requirements

Current database lacks a table for user preferences. Need to add:

#### Option 1: Generic User Preferences Table (Recommended ✅)

```sql
CREATE TABLE user_preferences (
    id UUID PRIMARY KEY,
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    preference_key VARCHAR(100) NOT NULL, -- 'map_layers', 'notifications', etc.
    preference_value JSONB NOT NULL, -- Flexible JSON storage
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    CONSTRAINT uq_user_pref UNIQUE (user_id, preference_key)
);
```

**Example data:**
```json
{
  "baseMap": "standard",
  "overlays": {
    "flood": true,
    "traffic": false,
    "weather": false
  },
  "opacity": {
    "flood": 80,
    "weather": 70
  }
}
```

#### Option 2: Specific Map Settings Table

```sql
CREATE TABLE user_map_settings (
    id UUID PRIMARY KEY,
    user_id UUID UNIQUE REFERENCES users(id) ON DELETE CASCADE,
    base_map VARCHAR(50) DEFAULT 'standard',
    layer_flood BOOLEAN DEFAULT true,
    layer_traffic BOOLEAN DEFAULT false,
    layer_weather BOOLEAN DEFAULT false,
    opacity_flood INT DEFAULT 80,
    opacity_weather INT DEFAULT 70,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL
);
```

**Why Option 1 (JSONB) is recommended:**
- ✅ **Flexible:** Easy to add new layers (precipitation, alerts, etc.)
- ✅ **Reusable:** Can store other preferences (notifications, theme, etc.)
- ✅ **Versioning:** Easy to migrate schema by adding new fields in JSON

---

### 3. Guest User Settings (Device-based)

**Issue:** Backend should not save guest settings (anonymous users).

**Reasons:**
- Guest users don't have `user_id` → Cannot identify
- Device ID from frontend can be faked/duplicated
- Security risk: Database spam with countless guest records

**Recommendation:**
- ✅ Guest settings: Completely frontend (localStorage web / SecureStorage mobile)
- ✅ Backend only handles authenticated users
- ✅ On guest → login transition: Frontend pushes settings to server

**Flow:**

**Guest users:**
```
Frontend → localStorage (no API call)
```

**Login transition:**
```
1. Frontend reads localStorage settings
2. POST /api/v1/preferences/map-layers (initial settings)
3. Clear localStorage, use server as source of truth
```

**Logged-in users:**
```
Frontend ↔ Backend API (sync settings)
```

---

### 4. API Endpoints Requirements

Following FDA API pattern (FastEndpoints + CQRS):

#### FeatG28: Get User Map Preferences

**Endpoint:** `GET /api/v1/preferences/map-layers`
**Auth:** Authenticated (any role)

**Response:**
```json
{
  "success": true,
  "data": {
    "baseMap": "standard",
    "overlays": {
      "flood": true,
      "traffic": false,
      "weather": false
    },
    "opacity": {
      "flood": 80,
      "weather": 70
    }
  }
}
```

#### FeatG29: Update User Map Preferences

**Endpoint:** `PUT /api/v1/preferences/map-layers`
**Auth:** Authenticated (any role)

**Request:**
```json
{
  "baseMap": "satellite",
  "overlays": {
    "flood": true,
    "traffic": true,
    "weather": true
  },
  "opacity": {
    "flood": 90,
    "weather": 60
  }
}
```

**Response:**
```json
{
  "success": true,
  "message": "Map preferences updated successfully"
}
```

#### FeatG30: Get Flood Severity Layer Data

**Endpoint:** `GET /api/v1/map/flood-severity`
**Auth:** Public (AllowAnonymous)

**Query Parameters:**
- `bounds`: "lat1,lng1,lat2,lng2" (viewport bounds)
- `zoom`: number (optional, for optimization)

**Response:** GeoJSON format
```json
{
  "type": "FeatureCollection",
  "features": [
    {
      "type": "Feature",
      "geometry": {
        "type": "Point",
        "coordinates": [106.660172, 10.762622]
      },
      "properties": {
        "stationId": "uuid",
        "stationCode": "ST_DN_01",
        "waterLevel": 2.5,
        "severity": "warning",
        "lastUpdated": "2026-01-09T10:30:00Z"
      }
    }
  ]
}
```

**Severity levels:** `safe` | `caution` | `warning` | `critical`

**Notes:**
- **Traffic/Weather data:** Frontend calls external APIs directly (Mapbox, OpenWeather, etc.) - not through FDA backend
- **Flood data:** FDA backend provides (from `stations` + `water_levels` tables)

---

### 5. Feature Breakdown for Backend

Split into 2-3 separate features:

#### FeatG28: Get Map Preferences (Query)
- **Layer:** Application
- **Handler:** `GetMapPreferencesHandler`
- **Endpoint:** `GET /api/v1/preferences/map-layers`
- **Auth:** Authenticated users
- **Repository:** `IUserPreferenceRepository`

#### FeatG29: Update Map Preferences (Command)
- **Layer:** Application
- **Handler:** `UpdateMapPreferencesHandler`
- **Endpoint:** `PUT /api/v1/preferences/map-layers`
- **Auth:** Authenticated users
- **Validation:** Valid baseMap enum, boolean overlays, opacity 0-100
- **Repository:** `IUserPreferenceRepository`

#### FeatG30: Get Flood Severity Layer (Query)
- **Layer:** Application
- **Handler:** `GetFloodSeverityLayerHandler`
- **Endpoint:** `GET /api/v1/map/flood-severity`
- **Auth:** Public (AllowAnonymous)
- **Repository:** `IStationRepository`, `IWaterLevelRepository`
- **Output:** GeoJSON format

---

## Implementation Plan

### Phase 1: Backend - User Preferences (FeatG28-29)

#### Domain Layer

**Entities/UserPreference.cs**
```csharp
public class UserPreference : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
{
    public Guid UserId { get; set; }
    public string PreferenceKey { get; set; } // "map_layers"
    public string PreferenceValue { get; set; } // JSON string

    [JsonIgnore]
    public virtual User User { get; set; }
}
```

**Repositories/IUserPreferenceRepository.cs**
```csharp
public interface IUserPreferenceRepository
{
    Task<UserPreference?> GetByUserAndKeyAsync(Guid userId, string key, CancellationToken ct);
    Task<Guid> CreateAsync(UserPreference preference, CancellationToken ct);
    Task<bool> UpdateAsync(UserPreference preference, CancellationToken ct);
}
```

#### Application Layer

**FeatG28_GetMapPreferences/**
```csharp
public sealed record GetMapPreferencesRequest(
    Guid UserId
) : IFeatureRequest<GetMapPreferencesResponse>;

public class GetMapPreferencesResponse : IFeatureResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public MapLayerSettings? Settings { get; set; }
}

public class MapLayerSettings
{
    public string BaseMap { get; set; } = "standard";
    public OverlaySettings Overlays { get; set; } = new();
    public OpacitySettings Opacity { get; set; } = new();
}

public class OverlaySettings
{
    public bool Flood { get; set; } = true;
    public bool Traffic { get; set; } = false;
    public bool Weather { get; set; } = false;
}

public class OpacitySettings
{
    public int Flood { get; set; } = 80;
    public int Weather { get; set; } = 70;
}
```

#### Presentation Layer

```csharp
public class GetMapPreferencesEndpoint : Endpoint<EmptyRequest, GetMapPreferencesResponseDto>
{
    public override void Configure()
    {
        Get("/api/v1/preferences/map-layers");
        Policies("User"); // Authenticated
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirst("sub").Value);
        var command = new GetMapPreferencesRequest(userId);
        var result = await _mediator.Send(command, ct);

        // Map to DTO and send
    }
}
```

---

### Phase 2: Backend - Flood Severity Layer Data (FeatG30)

#### Application Layer

**FeatG30_GetFloodSeverityLayer/**
```csharp
public sealed record GetFloodSeverityLayerRequest(
    BoundingBox? Bounds,
    int? ZoomLevel
) : IFeatureRequest<GetFloodSeverityLayerResponse>;

public class GetFloodSeverityLayerResponse : IFeatureResponse
{
    public bool Success { get; set; }
    public GeoJsonFeatureCollection Data { get; set; }
}

public class BoundingBox
{
    public decimal MinLat { get; set; }
    public decimal MinLng { get; set; }
    public decimal MaxLat { get; set; }
    public decimal MaxLng { get; set; }
}
```

#### Handler Logic

```csharp
public class GetFloodSeverityLayerHandler : IRequestHandler<...>
{
    public async Task<...> Handle(...)
    {
        // 1. Get all stations (or within bounds if provided)
        var stations = await _stationRepository.GetActiveStationsAsync(ct);

        // 2. Get latest water level for each station
        var waterLevels = await _waterLevelRepository.GetLatestByStationsAsync(
            stations.Select(s => s.Id), ct);

        // 3. Calculate severity for each station
        var features = stations.Select(station =>
        {
            var wl = waterLevels.FirstOrDefault(w => w.StationId == station.Id);
            var severity = CalculateSeverity(wl?.Value ?? 0,
                station.ThresholdWarning, station.ThresholdCritical);

            return new GeoJsonFeature
            {
                Geometry = new Point(station.Longitude, station.Latitude),
                Properties = new
                {
                    stationId = station.Id,
                    stationCode = station.Code,
                    waterLevel = wl?.Value,
                    severity = severity,
                    lastUpdated = wl?.MeasuredAt
                }
            };
        });

        return new GetFloodSeverityLayerResponse
        {
            Success = true,
            Data = new GeoJsonFeatureCollection { Features = features }
        };
    }

    private string CalculateSeverity(decimal waterLevel,
        decimal? warningThreshold, decimal? criticalThreshold)
    {
        if (criticalThreshold.HasValue && waterLevel >= criticalThreshold)
            return "critical";
        if (warningThreshold.HasValue && waterLevel >= warningThreshold)
            return "warning";
        if (waterLevel > 0)
            return "caution";
        return "safe";
    }
}
```

---

### Phase 3: Frontend Integration (Out of Backend Scope)

Frontend needs to implement:
- Map rendering (Mapbox GL JS / Leaflet / Google Maps)
- Layer toggle UI
- External API integration (Traffic: Mapbox, Weather: OpenWeather)
- Guest settings (localStorage)
- Sync logic (guest → logged-in)

---

## Database Changes Required

### Migration: Add UserPreferences Table

```sql
CREATE TABLE user_preferences (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    preference_key VARCHAR(100) NOT NULL,
    preference_value JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_user_preference UNIQUE (user_id, preference_key)
);

CREATE INDEX ix_user_preferences_user ON user_preferences(user_id);
CREATE INDEX ix_user_preferences_key ON user_preferences(preference_key);
```

### Seed Default Preferences (Optional)

```sql
-- When user first accesses map, create default settings
INSERT INTO user_preferences (user_id, preference_key, preference_value)
VALUES (
    '<user_id>',
    'map_layers',
    '{"baseMap":"standard","overlays":{"flood":true,"traffic":false,"weather":false},"opacity":{"flood":80,"weather":70}}'
);
```

---

## Final Recommendations

### ✅ What to Implement on Backend

#### FeatG28: Get User Map Preferences
- **Endpoint:** `GET /api/v1/preferences/map-layers`
- Return user's saved settings or defaults

#### FeatG29: Update User Map Preferences
- **Endpoint:** `PUT /api/v1/preferences/map-layers`
- Validate and save settings (JSONB)

#### FeatG30: Get Flood Severity Layer Data
- **Endpoint:** `GET /api/v1/map/flood-severity`
- Return GeoJSON for map rendering
- Calculate severity based on water levels

#### Database Migration
- Add `user_preferences` table
- Add indexes

---

### ❌ What NOT to Implement on Backend

| Item | Reason |
|------|--------|
| ❌ Guest/device-based settings | Frontend only |
| ❌ Traffic layer data | External provider |
| ❌ Weather layer data | External provider |
| ❌ Map rendering logic | Frontend |
| ❌ Layer toggle UI | Frontend |

---

## Adjusted Feature Definition

**Backend Feature Name:** FE-07: User Map Preferences & Flood Layer API

**Sub-features:**
- **FeatG28:** Get Map Preferences (Query)
- **FeatG29:** Update Map Preferences (Command)
- **FeatG30:** Get Flood Severity Layer (Query)

**Scope:** Backend API only

**Frontend:** Separate implementation ticket

---

## Summary

This feature requires careful separation of concerns between backend and frontend. The backend should focus on providing robust APIs for user preferences and flood data, while leaving map rendering, external API integration, and guest user management to the frontend. This approach ensures a clean architecture and optimal performance.
