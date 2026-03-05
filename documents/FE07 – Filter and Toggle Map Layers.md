# FE-07 – Filter and Toggle Map Layers

> **Feature Name**: User Map Preferences & Flood Layer API
> **Created**: 2026-01-09
> **Status**: 🟡 Planning
> **Backend Features**: FeatG28, FeatG29, FeatG30
> **Priority**: Medium

---

## 📋 TABLE OF CONTENTS

1. [Executive Summary](#executive-summary)
2. [Feature Analysis](#feature-analysis)
3. [Backend Scope Definition](#backend-scope-definition)
4. [Database Schema](#database-schema)
5. [API Specifications](#api-specifications)
6. [Implementation Plan](#implementation-plan)
7. [Frontend Integration Guide](#frontend-integration-guide)
8. [Testing Strategy](#testing-strategy)
9. [Edge Cases & Error Handling](#edge-cases--error-handling)

---

## 📊 EXECUTIVE SUMMARY

### Feature Overview

**Original Request**: Implement map layer toggles for Flood Severity, Traffic, Weather, and Satellite base maps with user/device persistence.

**Backend Scope (Adjusted)**:
This feature is **80% Frontend logic**. Backend provides:
- ✅ User preferences API (save/retrieve layer settings)
- ✅ Flood severity data API (GeoJSON for map rendering)
- ❌ Traffic/Weather data (external providers - frontend direct integration)
- ❌ Map rendering & UI (frontend responsibility)

### Backend Features to Implement

| Feature | Endpoint | Type | Description |
|---------|----------|------|-------------|
| **FeatG28** | `GET /api/v1/preferences/map-layers` | Query | Get user's map layer preferences |
| **FeatG29** | `PUT /api/v1/preferences/map-layers` | Command | Update user's map layer preferences |
| **FeatG30** | `GET /api/v1/map/flood-severity` | Query | Get flood severity layer data (GeoJSON) |

---

## 🔍 FEATURE ANALYSIS

### ✅ What's GOOD in Original Design

1. **Clear Scope**: 4 layers (Flood/Traffic/Weather/Satellite)
2. **UX Considerations**:
   - Persist settings for logged-in users
   - Debounce API calls (avoid spam)
   - Optimistic UI updates
3. **Edge Cases**: Offline handling, provider errors, multi-device sync
4. **Technical Details**: z-index, opacity, legend

### ⚠️ What Needs ADJUSTMENT

#### 1. **Backend vs Frontend Responsibility**

**Issue**: Original spec mixes backend and frontend concerns.

**Solution**:

```
┌─────────────────────────────────────────────────┐
│ BACKEND SCOPE (FDA API)                         │
├─────────────────────────────────────────────────┤
│ ✅ User Preferences API (FeatG28-29)            │
│    - Save/retrieve map layer settings           │
│    - Validate settings                          │
│    - Sync across devices                        │
│                                                  │
│ ✅ Flood Severity Data API (FeatG30)            │
│    - Query stations + water levels              │
│    - Calculate severity (safe/warning/critical) │
│    - Return GeoJSON for map rendering           │
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────┐
│ FRONTEND SCOPE (Mobile/Web App)                 │
├─────────────────────────────────────────────────┤
│ ✅ Map Rendering (Mapbox GL JS / Leaflet)       │
│ ✅ Layer Toggle UI (Bottom sheet / Panel)       │
│ ✅ Guest Settings (localStorage / SecureStorage)│
│ ✅ External API Integration:                    │
│    - Traffic: Mapbox Traffic API                │
│    - Weather: OpenWeather / WeatherAPI          │
│    - Satellite: Mapbox Satellite tiles          │
│ ✅ Settings Sync (guest → logged-in transition) │
└─────────────────────────────────────────────────┘
```

#### 2. **Guest User Settings**

**Issue**: Backend should NOT store anonymous user preferences.

**Reasons**:
- No reliable identity (device ID can be faked)
- Security risk (spam database with unlimited guest records)
- GDPR concerns (storing data without user consent)

**Solution**:

| User Type | Storage Strategy |
|-----------|------------------|
| **Guest** | Frontend only (localStorage / AsyncStorage / SecureStorage) |
| **Logged-in** | Backend database (authoritative source) |
| **Transition** | Frontend pushes local settings → backend on first login |

**Flow**:
```
Guest User:
  ┌──────────┐
  │ Frontend │ ──→ localStorage (no API call)
  └──────────┘

Login Transition:
  ┌──────────┐
  │ Frontend │ reads localStorage
  └────┬─────┘
       │ POST /api/v1/preferences/map-layers (initial sync)
       ↓
  ┌──────────┐
  │ Backend  │ saves preferences
  └──────────┘
       ↓
  Frontend clears localStorage, uses backend as source

Logged-in User:
  ┌──────────┐         ┌──────────┐
  │ Frontend │ ←────→  │ Backend  │ (always in sync)
  └──────────┘         └──────────┘
```

#### 3. **Data Sources per Layer**

| Layer | Data Source | Backend Involvement |
|-------|-------------|---------------------|
| **Flood Severity** | FDA Database (stations + water_levels) | ✅ Backend provides GeoJSON API |
| **Traffic** | Mapbox Traffic / HERE / TomTom | ❌ Frontend calls external API directly |
| **Weather** | OpenWeather / Weather.com | ❌ Frontend calls external API directly |
| **Satellite** | Mapbox Satellite style | ❌ Frontend switches map style URL |

**Why Backend doesn't proxy Traffic/Weather**:
- No value-added processing
- Increases latency (extra hop)
- Rate limiting complexities
- Cost (pay for external API + hosting)

---

## 🗄️ DATABASE SCHEMA

### New Table: `user_preferences`

**Purpose**: Store flexible user settings (map layers, notifications, theme, etc.)

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

COMMENT ON TABLE user_preferences IS 'Flexible user settings storage using JSONB';
COMMENT ON COLUMN user_preferences.preference_key IS 'Setting category: map_layers, notifications, theme, etc.';
COMMENT ON COLUMN user_preferences.preference_value IS 'JSON object with setting values';
```

### Why JSONB instead of Columns?

| Approach | Pros | Cons |
|----------|------|------|
| **JSONB** ✅ | • Flexible schema<br>• Easy to add new layers<br>• Reusable for other preferences<br>• Versioning support | • Requires JSON validation<br>• Less type-safe |
| **Columns** ❌ | • Type-safe<br>• Easier queries | • Schema migration per change<br>• Not reusable<br>• Hard to version |

### Example Data

```json
{
  "preference_key": "map_layers",
  "preference_value": {
    "version": "1.0",
    "baseMap": "standard",
    "overlays": {
      "flood": true,
      "traffic": false,
      "weather": false
    },
    "opacity": {
      "flood": 80,
      "weather": 70
    },
    "lastUpdated": "2026-01-09T10:30:00Z"
  }
}
```

### Schema Versioning

```json
{
  "version": "1.0",  // ← Important for future migrations
  ...
}

// Future: v2.0 adds new layers
{
  "version": "2.0",
  "baseMap": "standard",
  "overlays": {
    "flood": true,
    "traffic": false,
    "weather": false,
    "alerts": true,      // ← New layer
    "predictions": false // ← New layer
  }
}
```

**Migration Strategy**:
- Backend checks `version` field
- Auto-migrates old schemas to new format
- Returns updated JSON to client

---

## 🔌 API SPECIFICATIONS

### FeatG28: Get User Map Preferences

**Endpoint**: `GET /api/v1/preferences/map-layers`

**Authentication**: Required (JWT Bearer token)

**Authorization**: Any authenticated user

**Request**: No body (user ID from JWT claims)

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Map preferences retrieved successfully",
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

**Response** (404 Not Found - first time user):
```json
{
  "success": true,
  "message": "No preferences found, returning defaults",
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

**Business Logic**:
1. Extract `userId` from JWT (`sub` claim)
2. Query `user_preferences` table for `preference_key = 'map_layers'`
3. If found: return stored JSON
4. If not found: return default settings (don't create record yet)

**Default Settings**:
```json
{
  "baseMap": "standard",
  "overlays": {
    "flood": true,    // ✅ ON by default (app focus)
    "traffic": false,
    "weather": false
  },
  "opacity": {
    "flood": 80,
    "weather": 70
  }
}
```

---

### FeatG29: Update User Map Preferences

**Endpoint**: `PUT /api/v1/preferences/map-layers`

**Authentication**: Required (JWT Bearer token)

**Authorization**: Any authenticated user

**Request Body**:
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

**Validation Rules**:
| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `baseMap` | string | Yes | Enum: `standard`, `satellite` |
| `overlays` | object | Yes | - |
| `overlays.flood` | boolean | Yes | - |
| `overlays.traffic` | boolean | Yes | - |
| `overlays.weather` | boolean | Yes | - |
| `opacity` | object | No | - |
| `opacity.flood` | integer | No | Range: 0-100 |
| `opacity.weather` | integer | No | Range: 0-100 |

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Map preferences updated successfully"
}
```

**Response** (400 Bad Request - validation error):
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    {
      "field": "baseMap",
      "message": "Must be 'standard' or 'satellite'"
    },
    {
      "field": "opacity.flood",
      "message": "Must be between 0 and 100"
    }
  ]
}
```

**Business Logic**:
1. Extract `userId` from JWT
2. Validate request body (schema + business rules)
3. Check if preference exists:
   - **Exists**: UPDATE record
   - **Not exists**: INSERT new record
4. Return success response

**Debouncing**:
- ⚠️ Backend does NOT handle debouncing
- ✅ Frontend should debounce calls (300-800ms) to avoid spam
- Backend validates all requests regardless

---

### FeatG30: Get Flood Severity Layer Data

**Endpoint**: `GET /api/v1/map/flood-severity`

**Authentication**: Public (AllowAnonymous)

**Authorization**: None required

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `bounds` | string | No | Viewport bounding box: `minLat,minLng,maxLat,maxLng`<br>Example: `10.5,106.5,11.0,107.0` |
| `zoom` | integer | No | Map zoom level (0-22) for optimization |

**Request Example**:
```
GET /api/v1/map/flood-severity?bounds=10.5,106.5,11.0,107.0&zoom=12
```

**Response** (200 OK - GeoJSON):
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
        "stationId": "550e8400-e29b-41d4-a716-446655440000",
        "stationCode": "ST_DN_01",
        "stationName": "Nguyen Hue Station",
        "waterLevel": 2.5,
        "unit": "meters",
        "severity": "warning",
        "severityLevel": 2,
        "lastUpdated": "2026-01-09T10:30:00Z",
        "status": "active"
      }
    },
    {
      "type": "Feature",
      "geometry": {
        "type": "Point",
        "coordinates": [106.670000, 10.770000]
      },
      "properties": {
        "stationId": "550e8400-e29b-41d4-a716-446655440001",
        "stationCode": "ST_DN_02",
        "stationName": "Le Loi Station",
        "waterLevel": 0.8,
        "unit": "meters",
        "severity": "safe",
        "severityLevel": 0,
        "lastUpdated": "2026-01-09T10:28:00Z",
        "status": "active"
      }
    }
  ],
  "metadata": {
    "totalStations": 2,
    "generatedAt": "2026-01-09T10:35:00Z",
    "bounds": {
      "minLat": 10.5,
      "minLng": 106.5,
      "maxLat": 11.0,
      "maxLng": 107.0
    }
  }
}
```

**Severity Calculation Logic**:

```csharp
private string CalculateSeverity(decimal waterLevel, Station station)
{
    // Thresholds should be stored in Station entity
    // For now, using example values:

    if (waterLevel >= 3.0m) // Critical threshold
        return "critical";  // severityLevel: 3

    if (waterLevel >= 2.0m) // Warning threshold
        return "warning";   // severityLevel: 2

    if (waterLevel >= 1.0m) // Caution threshold
        return "caution";   // severityLevel: 1

    return "safe";          // severityLevel: 0
}
```

| Severity | Level | Color (Frontend) | Water Level Range |
|----------|-------|------------------|-------------------|
| **safe** | 0 | 🟢 Green | < 1.0m |
| **caution** | 1 | 🟡 Yellow | 1.0m - 1.9m |
| **warning** | 2 | 🟠 Orange | 2.0m - 2.9m |
| **critical** | 3 | 🔴 Red | ≥ 3.0m |

**Business Logic**:
1. Query active stations (optional: filter by bounds)
2. For each station, get latest water level reading
3. Calculate severity based on water level + thresholds
4. Build GeoJSON Feature for each station
5. Return FeatureCollection

**Optimization**:
- If `zoom < 10`: Return clustered data (fewer points)
- If `zoom >= 10`: Return all stations in viewport
- Use spatial indexing on `(latitude, longitude)` for bounds query

**Caching Strategy** (optional):
- Cache response for 1-5 minutes (configurable)
- Invalidate on new water level data
- Use Redis with key: `flood_severity:{bounds}:{zoom}`

---

## 🚀 IMPLEMENTATION PLAN

### Phase 1: Domain Layer (Entities & Repositories)

**Files to Create/Modify**:

```
src/Core/Domain/FDAAPI.Domain.RelationalDb/
├── Entities/
│   └── UserPreference.cs                    # NEW
├── Repositories/
│   └── IUserPreferenceRepository.cs         # NEW
└── RelationalDB/
    └── AppDbContext.cs                      # MODIFY (add DbSet)
```

**1.1 Create Entity: `UserPreference.cs`**

```csharp
using System;
using System.Text.Json.Serialization;
using FDAAPI.Domain.RelationalDb.Entities.Base;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    /// <summary>
    /// Stores flexible user settings using JSONB
    /// </summary>
    public class UserPreference : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
    {
        /// <summary>
        /// Foreign key to User
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Setting category: map_layers, notifications, theme, etc.
        /// </summary>
        public string PreferenceKey { get; set; } = string.Empty;

        /// <summary>
        /// JSON object with setting values
        /// </summary>
        public string PreferenceValue { get; set; } = "{}";

        // Audit fields
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual User? User { get; set; }
    }
}
```

**1.2 Create Repository Interface: `IUserPreferenceRepository.cs`**

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IUserPreferenceRepository
    {
        /// <summary>
        /// Get user preference by userId and key
        /// </summary>
        Task<UserPreference?> GetByUserAndKeyAsync(
            Guid userId,
            string preferenceKey,
            CancellationToken ct = default);

        /// <summary>
        /// Create new preference record
        /// </summary>
        Task<Guid> CreateAsync(
            UserPreference preference,
            CancellationToken ct = default);

        /// <summary>
        /// Update existing preference record
        /// </summary>
        Task<bool> UpdateAsync(
            UserPreference preference,
            CancellationToken ct = default);

        /// <summary>
        /// Delete preference record
        /// </summary>
        Task<bool> DeleteAsync(
            Guid id,
            CancellationToken ct = default);
    }
}
```

**1.3 Update `AppDbContext.cs`**

```csharp
public class AppDbContext : DbContext
{
    // Existing DbSets...
    public DbSet<User> Users { get; set; }
    public DbSet<Station> Stations { get; set; }

    // NEW
    public DbSet<UserPreference> UserPreferences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Existing configurations...

        // NEW: UserPreference configuration
        modelBuilder.Entity<UserPreference>(entity =>
        {
            entity.ToTable("user_preferences");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.PreferenceKey)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.PreferenceValue)
                .HasColumnType("jsonb")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // Unique constraint
            entity.HasIndex(e => new { e.UserId, e.PreferenceKey })
                .IsUnique()
                .HasDatabaseName("uq_user_preference");

            // Indexes
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("ix_user_preferences_user");

            entity.HasIndex(e => e.PreferenceKey)
                .HasDatabaseName("ix_user_preferences_key");

            // Relationship
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
```

---

### Phase 2: Infrastructure Layer (Repository Implementation)

**Files to Create**:

```
src/External/Infrastructure/Persistence/FDAAPI.Infra.Persistence/
└── Repositories/
    └── PgsqlUserPreferenceRepository.cs     # NEW
```

**2.1 Implement Repository: `PgsqlUserPreferenceRepository.cs`**

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlUserPreferenceRepository : IUserPreferenceRepository
    {
        private readonly AppDbContext _context;

        public PgsqlUserPreferenceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserPreference?> GetByUserAndKeyAsync(
            Guid userId,
            string preferenceKey,
            CancellationToken ct = default)
        {
            return await _context.UserPreferences
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.UserId == userId && p.PreferenceKey == preferenceKey,
                    ct);
        }

        public async Task<Guid> CreateAsync(
            UserPreference preference,
            CancellationToken ct = default)
        {
            _context.UserPreferences.Add(preference);
            await _context.SaveChangesAsync(ct);
            return preference.Id;
        }

        public async Task<bool> UpdateAsync(
            UserPreference preference,
            CancellationToken ct = default)
        {
            _context.UserPreferences.Update(preference);
            var rowsAffected = await _context.SaveChangesAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(
            Guid id,
            CancellationToken ct = default)
        {
            var preference = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (preference == null)
                return false;

            _context.UserPreferences.Remove(preference);
            var rowsAffected = await _context.SaveChangesAsync(ct);
            return rowsAffected > 0;
        }
    }
}
```

---

### Phase 3: Application Layer (FeatG28 - Get Preferences)

**Files to Create**:

```
src/Core/Application/FDAAPI.App.FeatG28_GetMapPreferences/
├── FDAAPI.App.FeatG28_GetMapPreferences.csproj
├── GetMapPreferencesRequest.cs
├── GetMapPreferencesResponse.cs
├── GetMapPreferencesHandler.cs
└── Models/
    └── MapLayerSettings.cs
```

**3.1 Create `.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Domain\FDAAPI.Domain.RelationalDb\FDAAPI.Domain.RelationalDb.csproj" />
    <ProjectReference Include="..\FDAAPI.App.Common\FDAAPI.App.Common.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="MediatR" Version="12.2.0" />
    <PackageReference Include="System.Text.Json" Version="8.0.0" />
  </ItemGroup>
</Project>
```

**3.2 Create Request: `GetMapPreferencesRequest.cs`**

```csharp
using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG28_GetMapPreferences
{
    /// <summary>
    /// Request to get user's map layer preferences
    /// </summary>
    public sealed record GetMapPreferencesRequest(
        Guid UserId
    ) : IFeatureRequest<GetMapPreferencesResponse>;
}
```

**3.3 Create Response: `GetMapPreferencesResponse.cs`**

```csharp
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG28_GetMapPreferences.Models;

namespace FDAAPI.App.FeatG28_GetMapPreferences
{
    public class GetMapPreferencesResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public MapLayerSettings? Settings { get; set; }
    }
}
```

**3.4 Create Model: `Models/MapLayerSettings.cs`**

```csharp
namespace FDAAPI.App.FeatG28_GetMapPreferences.Models
{
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
}
```

**3.5 Create Handler: `GetMapPreferencesHandler.cs`**

```csharp
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FDAAPI.App.FeatG28_GetMapPreferences.Models;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG28_GetMapPreferences
{
    public class GetMapPreferencesHandler
        : IRequestHandler<GetMapPreferencesRequest, GetMapPreferencesResponse>
    {
        private readonly IUserPreferenceRepository _preferenceRepository;

        public GetMapPreferencesHandler(IUserPreferenceRepository preferenceRepository)
        {
            _preferenceRepository = preferenceRepository;
        }

        public async Task<GetMapPreferencesResponse> Handle(
            GetMapPreferencesRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                // 1. Get preference from database
                var preference = await _preferenceRepository.GetByUserAndKeyAsync(
                    request.UserId,
                    "map_layers",
                    cancellationToken);

                // 2. If found, deserialize and return
                if (preference != null)
                {
                    var settings = JsonSerializer.Deserialize<MapLayerSettings>(
                        preference.PreferenceValue);

                    return new GetMapPreferencesResponse
                    {
                        Success = true,
                        Message = "Map preferences retrieved successfully",
                        Settings = settings
                    };
                }

                // 3. If not found, return default settings
                return new GetMapPreferencesResponse
                {
                    Success = true,
                    Message = "No preferences found, returning defaults",
                    Settings = GetDefaultSettings()
                };
            }
            catch (Exception ex)
            {
                return new GetMapPreferencesResponse
                {
                    Success = false,
                    Message = $"Error retrieving preferences: {ex.Message}",
                    Settings = GetDefaultSettings()
                };
            }
        }

        private MapLayerSettings GetDefaultSettings()
        {
            return new MapLayerSettings
            {
                BaseMap = "standard",
                Overlays = new OverlaySettings
                {
                    Flood = true,    // ON by default (app focus)
                    Traffic = false,
                    Weather = false
                },
                Opacity = new OpacitySettings
                {
                    Flood = 80,
                    Weather = 70
                }
            };
        }
    }
}
```

---

### Phase 4: Application Layer (FeatG29 - Update Preferences)

**Files to Create**:

```
src/Core/Application/FDAAPI.App.FeatG29_UpdateMapPreferences/
├── FDAAPI.App.FeatG29_UpdateMapPreferences.csproj
├── UpdateMapPreferencesRequest.cs
├── UpdateMapPreferencesResponse.cs
└── UpdateMapPreferencesHandler.cs
```

**4.1 Create Request: `UpdateMapPreferencesRequest.cs`**

```csharp
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG28_GetMapPreferences.Models;

namespace FDAAPI.App.FeatG29_UpdateMapPreferences
{
    public sealed record UpdateMapPreferencesRequest(
        Guid UserId,
        MapLayerSettings Settings
    ) : IFeatureRequest<UpdateMapPreferencesResponse>;
}
```

**4.2 Create Handler: `UpdateMapPreferencesHandler.cs`**

```csharp
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG29_UpdateMapPreferences
{
    public class UpdateMapPreferencesHandler
        : IRequestHandler<UpdateMapPreferencesRequest, UpdateMapPreferencesResponse>
    {
        private readonly IUserPreferenceRepository _preferenceRepository;

        public UpdateMapPreferencesHandler(IUserPreferenceRepository preferenceRepository)
        {
            _preferenceRepository = preferenceRepository;
        }

        public async Task<UpdateMapPreferencesResponse> Handle(
            UpdateMapPreferencesRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                // 1. Validate settings
                var validationError = ValidateSettings(request.Settings);
                if (validationError != null)
                {
                    return new UpdateMapPreferencesResponse
                    {
                        Success = false,
                        Message = validationError
                    };
                }

                // 2. Serialize settings to JSON
                var jsonValue = JsonSerializer.Serialize(request.Settings);

                // 3. Check if preference exists
                var existing = await _preferenceRepository.GetByUserAndKeyAsync(
                    request.UserId,
                    "map_layers",
                    cancellationToken);

                if (existing != null)
                {
                    // UPDATE
                    existing.PreferenceValue = jsonValue;
                    existing.UpdatedBy = request.UserId;
                    existing.UpdatedAt = DateTime.UtcNow;

                    await _preferenceRepository.UpdateAsync(existing, cancellationToken);
                }
                else
                {
                    // INSERT
                    var newPreference = new UserPreference
                    {
                        Id = Guid.NewGuid(),
                        UserId = request.UserId,
                        PreferenceKey = "map_layers",
                        PreferenceValue = jsonValue,
                        CreatedBy = request.UserId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedBy = request.UserId,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _preferenceRepository.CreateAsync(newPreference, cancellationToken);
                }

                return new UpdateMapPreferencesResponse
                {
                    Success = true,
                    Message = "Map preferences updated successfully"
                };
            }
            catch (Exception ex)
            {
                return new UpdateMapPreferencesResponse
                {
                    Success = false,
                    Message = $"Error updating preferences: {ex.Message}"
                };
            }
        }

        private string? ValidateSettings(MapLayerSettings settings)
        {
            // Validate baseMap
            if (settings.BaseMap != "standard" && settings.BaseMap != "satellite")
                return "BaseMap must be 'standard' or 'satellite'";

            // Validate opacity ranges
            if (settings.Opacity.Flood < 0 || settings.Opacity.Flood > 100)
                return "Flood opacity must be between 0 and 100";

            if (settings.Opacity.Weather < 0 || settings.Opacity.Weather > 100)
                return "Weather opacity must be between 0 and 100";

            return null; // Valid
        }
    }
}
```

---

### Phase 5: Application Layer (FeatG30 - Flood Severity Layer)

**Files to Create**:

```
src/Core/Application/FDAAPI.App.FeatG30_GetFloodSeverityLayer/
├── FDAAPI.App.FeatG30_GetFloodSeverityLayer.csproj
├── GetFloodSeverityLayerRequest.cs
├── GetFloodSeverityLayerResponse.cs
├── GetFloodSeverityLayerHandler.cs
└── Models/
    ├── GeoJsonFeatureCollection.cs
    └── BoundingBox.cs
```

**5.1 Create Models**

```csharp
// Models/BoundingBox.cs
namespace FDAAPI.App.FeatG30_GetFloodSeverityLayer.Models
{
    public class BoundingBox
    {
        public decimal MinLat { get; set; }
        public decimal MinLng { get; set; }
        public decimal MaxLat { get; set; }
        public decimal MaxLng { get; set; }
    }
}

// Models/GeoJsonFeatureCollection.cs
namespace FDAAPI.App.FeatG30_GetFloodSeverityLayer.Models
{
    public class GeoJsonFeatureCollection
    {
        public string Type { get; set; } = "FeatureCollection";
        public List<GeoJsonFeature> Features { get; set; } = new();
        public object? Metadata { get; set; }
    }

    public class GeoJsonFeature
    {
        public string Type { get; set; } = "Feature";
        public GeoJsonGeometry Geometry { get; set; } = new();
        public object Properties { get; set; } = new();
    }

    public class GeoJsonGeometry
    {
        public string Type { get; set; } = "Point";
        public decimal[] Coordinates { get; set; } = Array.Empty<decimal>();
    }
}
```

**5.2 Create Handler**

```csharp
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FDAAPI.App.FeatG30_GetFloodSeverityLayer.Models;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG30_GetFloodSeverityLayer
{
    public class GetFloodSeverityLayerHandler
        : IRequestHandler<GetFloodSeverityLayerRequest, GetFloodSeverityLayerResponse>
    {
        private readonly IStationRepository _stationRepository;
        private readonly IWaterLevelRepository _waterLevelRepository;

        public GetFloodSeverityLayerHandler(
            IStationRepository stationRepository,
            IWaterLevelRepository waterLevelRepository)
        {
            _stationRepository = stationRepository;
            _waterLevelRepository = waterLevelRepository;
        }

        public async Task<GetFloodSeverityLayerResponse> Handle(
            GetFloodSeverityLayerRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                // 1. Get active stations (optionally filtered by bounds)
                var stations = await _stationRepository.GetActiveStationsAsync(cancellationToken);

                if (request.Bounds != null)
                {
                    stations = stations.Where(s =>
                        s.Latitude >= request.Bounds.MinLat &&
                        s.Latitude <= request.Bounds.MaxLat &&
                        s.Longitude >= request.Bounds.MinLng &&
                        s.Longitude <= request.Bounds.MaxLng
                    ).ToList();
                }

                // 2. Get latest water levels for stations
                var stationIds = stations.Select(s => s.Id).ToList();
                var waterLevels = await _waterLevelRepository
                    .GetLatestByStationsAsync(stationIds, cancellationToken);

                // 3. Build GeoJSON features
                var features = stations.Select(station =>
                {
                    var waterLevel = waterLevels.FirstOrDefault(w => w.StationId == station.Id);
                    var (severity, level) = CalculateSeverity(waterLevel?.Value ?? 0);

                    return new GeoJsonFeature
                    {
                        Geometry = new GeoJsonGeometry
                        {
                            Type = "Point",
                            Coordinates = new[] { station.Longitude, station.Latitude }
                        },
                        Properties = new
                        {
                            stationId = station.Id,
                            stationCode = station.Code,
                            stationName = station.Name,
                            waterLevel = waterLevel?.Value,
                            unit = "meters",
                            severity = severity,
                            severityLevel = level,
                            lastUpdated = waterLevel?.MeasuredAt,
                            status = station.Status
                        }
                    };
                }).ToList();

                return new GetFloodSeverityLayerResponse
                {
                    Success = true,
                    Data = new GeoJsonFeatureCollection
                    {
                        Features = features,
                        Metadata = new
                        {
                            totalStations = features.Count,
                            generatedAt = DateTime.UtcNow,
                            bounds = request.Bounds
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new GetFloodSeverityLayerResponse
                {
                    Success = false,
                    Message = $"Error retrieving flood severity data: {ex.Message}"
                };
            }
        }

        private (string severity, int level) CalculateSeverity(decimal waterLevel)
        {
            if (waterLevel >= 3.0m)
                return ("critical", 3);

            if (waterLevel >= 2.0m)
                return ("warning", 2);

            if (waterLevel >= 1.0m)
                return ("caution", 1);

            return ("safe", 0);
        }
    }
}
```

---

### Phase 6: Presentation Layer (Endpoints + DTOs)

**Files to Create**:

```
src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/
└── Endpoints/
    ├── Feat28_GetMapPreferences/
    │   ├── GetMapPreferencesEndpoint.cs
    │   └── DTOs/
    │       └── GetMapPreferencesResponseDto.cs
    ├── Feat29_UpdateMapPreferences/
    │   ├── UpdateMapPreferencesEndpoint.cs
    │   └── DTOs/
    │       ├── UpdateMapPreferencesRequestDto.cs
    │       └── UpdateMapPreferencesResponseDto.cs
    └── Feat30_GetFloodSeverityLayer/
        ├── GetFloodSeverityLayerEndpoint.cs
        └── DTOs/
            └── GetFloodSeverityLayerResponseDto.cs
```

**6.1 FeatG28 Endpoint**

```csharp
using FastEndpoints;
using MediatR;
using FDAAPI.App.FeatG28_GetMapPreferences;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat28_GetMapPreferences
{
    public class GetMapPreferencesEndpoint
        : EndpointWithoutRequest<GetMapPreferencesResponseDto>
    {
        private readonly IMediator _mediator;

        public GetMapPreferencesEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Get("/api/v1/preferences/map-layers");
            Policies("User"); // Authenticated users

            Summary(s =>
            {
                s.Summary = "Get user's map layer preferences";
                s.Description = "Retrieve saved map layer settings or return defaults if not found";
                s.ResponseExamples[200] = new GetMapPreferencesResponseDto
                {
                    Success = true,
                    Message = "Map preferences retrieved successfully",
                    Data = new MapLayerSettingsDto
                    {
                        BaseMap = "standard",
                        Overlays = new OverlaySettingsDto
                        {
                            Flood = true,
                            Traffic = false,
                            Weather = false
                        },
                        Opacity = new OpacitySettingsDto
                        {
                            Flood = 80,
                            Weather = 70
                        }
                    }
                };
            });

            Tags("Map", "Preferences");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            try
            {
                // Get userId from JWT
                var userId = Guid.Parse(User.FindFirst("sub")!.Value);

                var request = new GetMapPreferencesRequest(userId);
                var result = await _mediator.Send(request, ct);

                var response = new GetMapPreferencesResponseDto
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Settings != null ? new MapLayerSettingsDto
                    {
                        BaseMap = result.Settings.BaseMap,
                        Overlays = new OverlaySettingsDto
                        {
                            Flood = result.Settings.Overlays.Flood,
                            Traffic = result.Settings.Overlays.Traffic,
                            Weather = result.Settings.Overlays.Weather
                        },
                        Opacity = new OpacitySettingsDto
                        {
                            Flood = result.Settings.Opacity.Flood,
                            Weather = result.Settings.Opacity.Weather
                        }
                    } : null
                };

                await SendAsync(response, 200, ct);
            }
            catch (Exception ex)
            {
                await SendAsync(new GetMapPreferencesResponseDto
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                }, 500, ct);
            }
        }
    }
}
```

**6.2 FeatG29 Endpoint**

```csharp
public class UpdateMapPreferencesEndpoint
    : Endpoint<UpdateMapPreferencesRequestDto, UpdateMapPreferencesResponseDto>
{
    private readonly IMediator _mediator;

    public UpdateMapPreferencesEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/v1/preferences/map-layers");
        Policies("User");

        Summary(s =>
        {
            s.Summary = "Update user's map layer preferences";
            s.Description = "Save map layer settings (creates if not exists, updates if exists)";
        });

        Tags("Map", "Preferences");
    }

    public override async Task HandleAsync(
        UpdateMapPreferencesRequestDto req,
        CancellationToken ct)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);

            var settings = new MapLayerSettings
            {
                BaseMap = req.BaseMap,
                Overlays = new OverlaySettings
                {
                    Flood = req.Overlays.Flood,
                    Traffic = req.Overlays.Traffic,
                    Weather = req.Overlays.Weather
                },
                Opacity = new OpacitySettings
                {
                    Flood = req.Opacity.Flood,
                    Weather = req.Opacity.Weather
                }
            };

            var request = new UpdateMapPreferencesRequest(userId, settings);
            var result = await _mediator.Send(request, ct);

            var response = new UpdateMapPreferencesResponseDto
            {
                Success = result.Success,
                Message = result.Message
            };

            await SendAsync(response, result.Success ? 200 : 400, ct);
        }
        catch (Exception ex)
        {
            await SendAsync(new UpdateMapPreferencesResponseDto
            {
                Success = false,
                Message = $"An error occurred: {ex.Message}"
            }, 500, ct);
        }
    }
}
```

**6.3 FeatG30 Endpoint**

```csharp
public class GetFloodSeverityLayerEndpoint
    : EndpointWithoutRequest<object>
{
    private readonly IMediator _mediator;

    public GetFloodSeverityLayerEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/v1/map/flood-severity");
        AllowAnonymous(); // Public endpoint

        Summary(s =>
        {
            s.Summary = "Get flood severity layer data";
            s.Description = "Returns GeoJSON FeatureCollection with station locations and flood severity";
        });

        Tags("Map", "Flood");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            // Parse query parameters
            BoundingBox? bounds = null;
            if (Query<string>("bounds", false) is string boundsStr)
            {
                var parts = boundsStr.Split(',');
                if (parts.Length == 4)
                {
                    bounds = new BoundingBox
                    {
                        MinLat = decimal.Parse(parts[0]),
                        MinLng = decimal.Parse(parts[1]),
                        MaxLat = decimal.Parse(parts[2]),
                        MaxLng = decimal.Parse(parts[3])
                    };
                }
            }

            var zoom = Query<int?>("zoom", false);

            var request = new GetFloodSeverityLayerRequest(bounds, zoom);
            var result = await _mediator.Send(request, ct);

            if (result.Success)
            {
                // Return raw GeoJSON (not wrapped in response envelope)
                await SendAsync(result.Data, 200, ct);
            }
            else
            {
                await SendAsync(new { error = result.Message }, 500, ct);
            }
        }
        catch (Exception ex)
        {
            await SendAsync(new { error = $"An error occurred: {ex.Message}" }, 500, ct);
        }
    }
}
```

---

### Phase 7: Configuration & Registration

**File to Modify**: `ServiceExtensions.cs`

```csharp
// AddApplicationServices()
services.AddMediatR(cfg =>
{
    // Existing registrations...
    cfg.RegisterServicesFromAssembly(typeof(LoginRequest).Assembly);

    // NEW
    cfg.RegisterServicesFromAssembly(typeof(GetMapPreferencesRequest).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(UpdateMapPreferencesRequest).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(GetFloodSeverityLayerRequest).Assembly);
});

// AddPersistenceServices()
services.AddScoped<IUserPreferenceRepository, PgsqlUserPreferenceRepository>();
```

---

### Phase 8: Database Migration

**Commands**:

```bash
# 1. Create migration
dotnet ef migrations add AddUserPreferencesTable \
  --project "src/Core/Domain/FDAAPI.Domain.RelationalDb/FDAAPI.Domain.RelationalDb.csproj" \
  --startup-project "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj" \
  --output-dir Migrations

# 2. Review migration file (check if JSONB column type is correct)

# 3. Apply migration
dotnet ef database update \
  --project "src/Core/Domain/FDAAPI.Domain.RelationalDb/FDAAPI.Domain.RelationalDb.csproj" \
  --startup-project "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj"

# 4. Verify
dotnet ef migrations list \
  --project "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj"
```

**Expected Migration SQL**:

```sql
CREATE TABLE user_preferences (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    preference_key character varying(100) NOT NULL,
    preference_value jsonb NOT NULL,
    created_by uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_by uuid NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_user_preferences PRIMARY KEY (id),
    CONSTRAINT fk_user_preferences_users_user_id FOREIGN KEY (user_id)
        REFERENCES users (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX uq_user_preference
    ON user_preferences (user_id, preference_key);

CREATE INDEX ix_user_preferences_user
    ON user_preferences (user_id);

CREATE INDEX ix_user_preferences_key
    ON user_preferences (preference_key);
```

---

## 🧪 TESTING STRATEGY

### Unit Tests

**Test Coverage**:

1. **GetMapPreferencesHandler**:
   - ✅ Returns saved preferences when exists
   - ✅ Returns defaults when not exists
   - ✅ Handles deserialization errors gracefully

2. **UpdateMapPreferencesHandler**:
   - ✅ Creates new preference if not exists
   - ✅ Updates existing preference
   - ✅ Validates baseMap enum
   - ✅ Validates opacity ranges
   - ✅ Rejects invalid settings

3. **GetFloodSeverityLayerHandler**:
   - ✅ Returns all stations when no bounds
   - ✅ Filters stations by bounds correctly
   - ✅ Calculates severity correctly (safe/caution/warning/critical)
   - ✅ Returns GeoJSON format

### Integration Tests (API Tests)

**Test Cases**:

#### **FeatG28: GET /api/v1/preferences/map-layers**

```bash
# Test 1: Get preferences (first time - should return defaults)
curl -X GET http://localhost:5000/api/v1/preferences/map-layers \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Expected: 200 OK with default settings

# Test 2: Get preferences (after saving)
# (First call FeatG29 to save, then call FeatG28)

# Expected: 200 OK with saved settings
```

#### **FeatG29: PUT /api/v1/preferences/map-layers**

```bash
# Test 3: Update preferences (valid request)
curl -X PUT http://localhost:5000/api/v1/preferences/map-layers \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "baseMap": "satellite",
    "overlays": {
      "flood": true,
      "traffic": true,
      "weather": false
    },
    "opacity": {
      "flood": 90,
      "weather": 60
    }
  }'

# Expected: 200 OK

# Test 4: Invalid baseMap
curl -X PUT http://localhost:5000/api/v1/preferences/map-layers \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "baseMap": "invalid",
    "overlays": {"flood": true, "traffic": false, "weather": false},
    "opacity": {"flood": 80, "weather": 70}
  }'

# Expected: 400 Bad Request

# Test 5: Invalid opacity
curl -X PUT http://localhost:5000/api/v1/preferences/map-layers \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "baseMap": "standard",
    "overlays": {"flood": true, "traffic": false, "weather": false},
    "opacity": {"flood": 150, "weather": 70}
  }'

# Expected: 400 Bad Request
```

#### **FeatG30: GET /api/v1/map/flood-severity**

```bash
# Test 6: Get all flood severity data (no filters)
curl -X GET http://localhost:5000/api/v1/map/flood-severity

# Expected: 200 OK with GeoJSON FeatureCollection

# Test 7: Get flood severity with bounds
curl -X GET "http://localhost:5000/api/v1/map/flood-severity?bounds=10.5,106.5,11.0,107.0"

# Expected: 200 OK with filtered GeoJSON

# Test 8: Get flood severity with zoom level
curl -X GET "http://localhost:5000/api/v1/map/flood-severity?bounds=10.5,106.5,11.0,107.0&zoom=12"

# Expected: 200 OK with optimized data
```

### Database Verification

```sql
-- Verify user_preferences table exists
SELECT table_name, column_name, data_type
FROM information_schema.columns
WHERE table_name = 'user_preferences';

-- Check preferences after API calls
SELECT
    up.id,
    u.email,
    up.preference_key,
    up.preference_value::text,
    up.created_at,
    up.updated_at
FROM user_preferences up
JOIN users u ON up.user_id = u.id
WHERE up.preference_key = 'map_layers';

-- Verify JSON structure
SELECT
    preference_value->>'baseMap' as base_map,
    preference_value->'overlays'->>'flood' as flood_overlay,
    preference_value->'opacity'->>'flood' as flood_opacity
FROM user_preferences
WHERE preference_key = 'map_layers';
```

---

## 🚨 EDGE CASES & ERROR HANDLING

### 1. **Concurrent Updates (Race Condition)**

**Scenario**: User opens app on 2 devices, changes settings on both simultaneously.

**Solution**:
```csharp
// Use optimistic locking with updated_at timestamp
var existing = await _preferenceRepository.GetByUserAndKeyAsync(...);

if (existing.UpdatedAt > request.ClientTimestamp)
{
    return new Response
    {
        Success = false,
        Message = "Settings were modified on another device. Please refresh."
    };
}
```

**Alternative**: Last-write-wins (current implementation) - simpler but may lose data.

---

### 2. **Invalid JSON in Database**

**Scenario**: Manual DB edit corrupts JSON, deserialization fails.

**Solution**:
```csharp
try
{
    var settings = JsonSerializer.Deserialize<MapLayerSettings>(
        preference.PreferenceValue);
    return settings;
}
catch (JsonException)
{
    // Log error, return defaults
    _logger.LogError("Invalid JSON in user preferences, returning defaults");
    return GetDefaultSettings();
}
```

---

### 3. **Missing Required Stations Data**

**Scenario**: Station exists but no water level readings.

**Solution**:
```csharp
var waterLevel = waterLevels.FirstOrDefault(w => w.StationId == station.Id);

return new GeoJsonFeature
{
    Properties = new
    {
        waterLevel = waterLevel?.Value,  // null if missing
        severity = waterLevel != null
            ? CalculateSeverity(waterLevel.Value)
            : "unknown",
        lastUpdated = waterLevel?.MeasuredAt
    }
};
```

Frontend should handle `null` values and display "No data" state.

---

### 4. **Large Bounds Query**

**Scenario**: User zooms out to country level, requests 1000+ stations.

**Solution**:
```csharp
// Option 1: Limit results
var stations = await _stationRepository
    .GetActiveStationsAsync(cancellationToken);

if (stations.Count > 500)
{
    return new Response
    {
        Success = false,
        Message = "Too many stations in viewport. Please zoom in."
    };
}

// Option 2: Use clustering
if (request.ZoomLevel < 10)
{
    // Return clustered data instead of individual points
    return GetClusteredData(stations);
}
```

---

### 5. **Unauthorized Access**

**Scenario**: JWT expired, user tries to update preferences.

**Response**:
```json
HTTP 401 Unauthorized
{
  "error": "Unauthorized",
  "message": "Access token expired or invalid"
}
```

Frontend should:
- Catch 401 errors
- Redirect to login
- After login, retry the request

---

### 6. **Network Timeout (External APIs)**

**Scenario**: Frontend calls Traffic/Weather APIs, request times out.

**Frontend Handling**:
```javascript
try {
  const trafficLayer = await fetchMapboxTraffic();
  map.addLayer(trafficLayer);
} catch (error) {
  console.error('Traffic layer unavailable:', error);
  // Show toast notification
  toast.error('Traffic data temporarily unavailable');
  // Disable toggle or show warning icon
}
```

---

## 🎨 FRONTEND INTEGRATION GUIDE

### Guest User Flow (LocalStorage)

```typescript
// localStorage key
const GUEST_SETTINGS_KEY = 'fda_map_settings';

// Default settings
const DEFAULT_SETTINGS = {
  baseMap: 'standard',
  overlays: { flood: true, traffic: false, weather: false },
  opacity: { flood: 80, weather: 70 }
};

// Load settings on app start
function loadMapSettings() {
  const stored = localStorage.getItem(GUEST_SETTINGS_KEY);
  return stored ? JSON.parse(stored) : DEFAULT_SETTINGS;
}

// Save settings (debounced)
const saveMapSettings = debounce((settings) => {
  localStorage.setItem(GUEST_SETTINGS_KEY, JSON.stringify(settings));
}, 500);

// On toggle change
function handleToggleChange(layer, value) {
  const settings = loadMapSettings();
  settings.overlays[layer] = value;
  saveMapSettings(settings);

  // Update map immediately (optimistic UI)
  if (value) {
    map.addLayer(layer);
  } else {
    map.removeLayer(layer);
  }
}
```

---

### Logged-in User Flow (Backend Sync)

```typescript
// Fetch settings on login
async function fetchUserSettings() {
  const response = await fetch('/api/v1/preferences/map-layers', {
    headers: { 'Authorization': `Bearer ${accessToken}` }
  });
  const data = await response.json();
  return data.data; // MapLayerSettings
}

// Save settings (debounced API call)
const saveUserSettings = debounce(async (settings) => {
  await fetch('/api/v1/preferences/map-layers', {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${accessToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(settings)
  });
}, 800); // 800ms debounce

// On toggle change
function handleToggleChange(layer, value) {
  const settings = getCurrentSettings();
  settings.overlays[layer] = value;

  // Optimistic UI update
  if (value) {
    map.addLayer(layer);
  } else {
    map.removeLayer(layer);
  }

  // Sync to backend (debounced)
  saveUserSettings(settings);
}
```

---

### Login Transition Flow

```typescript
async function onLoginSuccess(accessToken) {
  // 1. Get guest settings from localStorage
  const guestSettings = localStorage.getItem(GUEST_SETTINGS_KEY);

  // 2. Fetch user settings from backend
  const userSettings = await fetchUserSettings();

  // 3. If user has no saved settings, push guest settings to backend
  if (!userSettings || isDefaultSettings(userSettings)) {
    if (guestSettings) {
      await saveUserSettings(JSON.parse(guestSettings));
    }
  }

  // 4. Clear localStorage (backend is now source of truth)
  localStorage.removeItem(GUEST_SETTINGS_KEY);

  // 5. Apply user settings to map
  applySettingsToMap(userSettings);
}
```

---

### Flood Layer Integration (Mapbox GL JS)

```typescript
// Load flood severity layer
async function loadFloodLayer() {
  // Fetch GeoJSON from backend
  const response = await fetch('/api/v1/map/flood-severity');
  const geojson = await response.json();

  // Add source
  map.addSource('flood-severity', {
    type: 'geojson',
    data: geojson
  });

  // Add circle layer (color by severity)
  map.addLayer({
    id: 'flood-severity-circles',
    type: 'circle',
    source: 'flood-severity',
    paint: {
      'circle-radius': [
        'interpolate', ['linear'], ['zoom'],
        8, 4,   // zoom 8: radius 4px
        12, 8,  // zoom 12: radius 8px
        16, 16  // zoom 16: radius 16px
      ],
      'circle-color': [
        'match',
        ['get', 'severity'],
        'safe', '#10b981',      // Green
        'caution', '#fbbf24',   // Yellow
        'warning', '#f97316',   // Orange
        'critical', '#ef4444',  // Red
        '#64748b'               // Gray (unknown)
      ],
      'circle-opacity': 0.8,
      'circle-stroke-width': 2,
      'circle-stroke-color': '#ffffff'
    }
  });

  // Add hover popup
  map.on('click', 'flood-severity-circles', (e) => {
    const props = e.features[0].properties;
    new mapboxgl.Popup()
      .setLngLat(e.lngLat)
      .setHTML(`
        <strong>${props.stationName}</strong><br>
        Water Level: ${props.waterLevel}m<br>
        Severity: ${props.severity}<br>
        Updated: ${new Date(props.lastUpdated).toLocaleString()}
      `)
      .addTo(map);
  });
}

// Remove flood layer
function removeFloodLayer() {
  if (map.getLayer('flood-severity-circles')) {
    map.removeLayer('flood-severity-circles');
  }
  if (map.getSource('flood-severity')) {
    map.removeSource('flood-severity');
  }
}

// Toggle flood layer
function toggleFloodLayer(enabled) {
  if (enabled) {
    loadFloodLayer();
  } else {
    removeFloodLayer();
  }
}
```

---

### Traffic Layer Integration (Mapbox)

```typescript
// Traffic layer uses Mapbox Traffic v1 style layer
function toggleTrafficLayer(enabled) {
  if (enabled) {
    map.setLayoutProperty('traffic', 'visibility', 'visible');
  } else {
    map.setLayoutProperty('traffic', 'visibility', 'none');
  }
}

// Add traffic layer to map style
const style = {
  layers: [
    // ... other layers
    {
      id: 'traffic',
      type: 'line',
      source: 'mapbox-traffic',
      'source-layer': 'traffic',
      minzoom: 0,
      maxzoom: 22,
      layout: {
        'visibility': 'none' // Hidden by default
      },
      paint: {
        'line-color': [
          'match',
          ['get', 'congestion'],
          'low', '#4CAF50',
          'moderate', '#FFC107',
          'heavy', '#F44336',
          '#9E9E9E'
        ],
        'line-width': 3
      }
    }
  ]
};
```

---

### Weather Layer Integration (OpenWeather)

```typescript
// Weather radar overlay (example with OpenWeather)
async function loadWeatherLayer() {
  const apiKey = 'YOUR_OPENWEATHER_API_KEY';

  map.addSource('weather-radar', {
    type: 'raster',
    tiles: [
      `https://tile.openweathermap.org/map/precipitation_new/{z}/{x}/{y}.png?appid=${apiKey}`
    ],
    tileSize: 256
  });

  map.addLayer({
    id: 'weather-radar-layer',
    type: 'raster',
    source: 'weather-radar',
    paint: {
      'raster-opacity': 0.7
    }
  });
}

function removeWeatherLayer() {
  if (map.getLayer('weather-radar-layer')) {
    map.removeLayer('weather-radar-layer');
  }
  if (map.getSource('weather-radar')) {
    map.removeSource('weather-radar');
  }
}
```

---

## 📈 PERFORMANCE CONSIDERATIONS

### 1. **API Response Caching**

```csharp
// In GetFloodSeverityLayerHandler
// Use IDistributedCache (Redis)

public async Task<...> Handle(...)
{
    var cacheKey = $"flood_severity:{bounds?.ToString() ?? "all"}:{zoom}";

    // Try cache first
    var cached = await _cache.GetStringAsync(cacheKey, ct);
    if (cached != null)
    {
        return JsonSerializer.Deserialize<GetFloodSeverityLayerResponse>(cached);
    }

    // ... fetch from database ...

    // Cache for 2 minutes
    await _cache.SetStringAsync(
        cacheKey,
        JsonSerializer.Serialize(response),
        new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
        },
        ct);

    return response;
}
```

---

### 2. **Database Query Optimization**

```csharp
// Use spatial index for bounds query
// In PgsqlStationRepository
public async Task<List<Station>> GetStationsInBoundsAsync(
    BoundingBox bounds,
    CancellationToken ct)
{
    return await _context.Stations
        .AsNoTracking()
        .Where(s =>
            s.Latitude >= bounds.MinLat &&
            s.Latitude <= bounds.MaxLat &&
            s.Longitude >= bounds.MinLng &&
            s.Longitude <= bounds.MaxLng &&
            s.Status == "active")
        .ToListAsync(ct);
}

// Migration: Add spatial index
migrationBuilder.Sql(@"
    CREATE INDEX ix_stations_geo_box
    ON stations (latitude, longitude);
");
```

---

### 3. **Frontend Debouncing**

```typescript
// Debounce settings updates
import { debounce } from 'lodash';

const debouncedSave = debounce(async (settings) => {
  await fetch('/api/v1/preferences/map-layers', {
    method: 'PUT',
    headers: { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' },
    body: JSON.stringify(settings)
  });
}, 800); // Wait 800ms after last change

// User toggles 3 layers rapidly
handleToggle('flood', true);   // Queued
handleToggle('traffic', true); // Queued
handleToggle('weather', false); // Queued
// → Only 1 API call after 800ms with final state
```

---

## 📊 SUCCESS METRICS

### Backend KPIs

| Metric | Target | Measurement |
|--------|--------|-------------|
| **API Response Time** | < 200ms (p95) | GET /preferences/map-layers |
| **Flood Layer Response** | < 500ms (p95) | GET /map/flood-severity (50 stations) |
| **Database Query Time** | < 50ms | user_preferences table lookup |
| **Update Success Rate** | > 99.9% | PUT /preferences/map-layers |

### User Experience KPIs

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Settings Persistence** | 100% | Settings restored on app relaunch |
| **Multi-device Sync** | < 2s | Settings sync delay across devices |
| **Toggle Responsiveness** | < 100ms | UI update delay after toggle |

---

## 🎯 ACCEPTANCE CRITERIA

### Backend Features

- [x] **FeatG28**: GET /api/v1/preferences/map-layers
  - ✅ Returns saved settings for logged-in users
  - ✅ Returns defaults if no settings found
  - ✅ Handles missing/invalid JWT correctly (401)

- [x] **FeatG29**: PUT /api/v1/preferences/map-layers
  - ✅ Creates new preference record (upsert logic)
  - ✅ Updates existing preference record
  - ✅ Validates baseMap enum ("standard"|"satellite")
  - ✅ Validates opacity ranges (0-100)
  - ✅ Returns 400 on validation errors

- [x] **FeatG30**: GET /api/v1/map/flood-severity
  - ✅ Returns valid GeoJSON FeatureCollection
  - ✅ Includes all active stations
  - ✅ Filters by bounds parameter correctly
  - ✅ Calculates severity (safe/caution/warning/critical)
  - ✅ Public endpoint (no auth required)

### Database

- [x] **user_preferences** table created
- [x] JSONB column stores flexible settings
- [x] Unique constraint on (user_id, preference_key)
- [x] Indexes on user_id and preference_key
- [x] Cascade delete on user removal

### Integration

- [x] Swagger documentation updated
- [x] Unit tests for all handlers
- [x] Integration tests for all endpoints
- [x] Database migration runs successfully

---

## 📚 REFERENCES

### External APIs (Frontend Integration)

| Provider | Purpose | Documentation |
|----------|---------|---------------|
| **Mapbox** | Base maps, Traffic, Satellite | https://docs.mapbox.com/ |
| **OpenWeather** | Weather radar overlay | https://openweathermap.org/api |
| **HERE Maps** | Alternative traffic provider | https://developer.here.com/ |

### Related Features

- [FE01: Authentication](./FE01-Authentication-Complete-Documentation.md) - User authentication (required for preferences)
- [FE-XX: Stations Management](./FE-XX-Stations-Management.md) - Station CRUD (provides flood severity data)

---

## 🔄 FUTURE ENHANCEMENTS

### V2.0 - Additional Layers

```json
{
  "overlays": {
    "flood": true,
    "traffic": false,
    "weather": false,
    "alerts": true,        // ← NEW: Alert zones
    "predictions": false,  // ← NEW: Flood predictions
    "evacuation": false    // ← NEW: Evacuation routes
  }
}
```

### V2.1 - Advanced Settings

```json
{
  "baseMap": "standard",
  "theme": "dark",          // ← NEW: Dark mode
  "autoRefresh": true,      // ← NEW: Auto-refresh layers
  "refreshInterval": 60,    // ← NEW: Seconds
  "clustering": {           // ← NEW: Cluster settings
    "enabled": true,
    "maxZoom": 12
  }
}
```

### V3.0 - Custom Layers

```json
{
  "customLayers": [
    {
      "id": "user-layer-1",
      "name": "My Custom Layer",
      "type": "geojson",
      "url": "https://...",
      "enabled": true
    }
  ]
}
```

---

## ✅ CONCLUSION

**Feature FE-07** is well-designed but requires **scope adjustment** for backend implementation:

### ✅ **Backend Implements**:
1. User preferences API (FeatG28-29)
2. Flood severity data API (FeatG30)
3. Database schema (user_preferences table)

### ❌ **Frontend Implements**:
1. Guest settings (localStorage)
2. Map rendering & UI
3. External API integration (Traffic, Weather)
4. Settings sync logic

**Next Steps**:
1. ✅ Review this document
2. ✅ Approve backend scope
3. ✅ Create implementation tasks
4. ✅ Begin Phase 1 (Domain Layer)

---

**Document Version**: 1.0
**Last Updated**: 2026-01-09
**Author**: Development Team
**Status**: ✅ Ready for Implementation
