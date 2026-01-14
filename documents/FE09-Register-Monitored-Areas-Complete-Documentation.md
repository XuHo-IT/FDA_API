# FE-09 – Register Monitored Areas (My Areas)

> **Feature Name**: Register and Manage User's Monitored Areas
> **Created**: 2026-01-14
> **Status**: 🟡 Planning
> **Backend Features**: FeatG32 (CreateArea), FeatG33 (AreaListByUser), FeatG35 (AreaGet), FeatG36 (AreaUpdate), FeatG37 (AreaDelete)
> **Priority**: High
> **Related**: FE-234 (Flood Evaluation Per Area)

---

## 📋 TABLE OF CONTENTS

1. [Executive Summary](#executive-summary)
2. [Feature Analysis](#feature-analysis)
3. [Domain Model](#domain-model)
4. [API Specifications](#api-specifications)
5. [Implementation Plan](#implementation-plan)
6. [Testing Strategy](#testing-strategy)
7. [Security & Validation](#security--validation)
8. [Performance Considerations](#performance-considerations)

---

## 📊 EXECUTIVE SUMMARY

### Feature Overview

**Problem**: Users need to monitor specific locations that are important to them (home, office, school, family members' locations) for flood risks.

**Solution**: Provide UI + API to allow users to:
- Select locations on a map (click or search)
- Define monitoring areas with custom radius
- Save multiple areas to their account
- View, update, and delete their monitored areas
- Prevent duplicate areas (same location with overlapping radius)

### Backend Features to Implement

| Feature | Endpoint | Type | Description | Status |
|---------|----------|------|-------------|--------|
| **FeatG32** | `POST /api/v1/areas/area` | Command | Create a new monitored area | ✅ Implemented |
| **FeatG33** | `GET /api/v1/areas/me` | Query | List user's monitored areas | ✅ Implemented |
| **FeatG35** | `GET /api/v1/areas/{id}` | Query | Get area details by ID | ✅ Implemented |
| **FeatG36** | `PUT /api/v1/areas/{id}` | Command | Update area details | ✅ Implemented |
| **FeatG37** | `DELETE /api/v1/areas/{id}` | Command | Delete an area | ✅ Implemented |

### Key Improvements Needed

This feature document focuses on **enhancements** to existing implementations:
1. ✅ **Geometry Validation**: Validate latitude/longitude ranges
2. ✅ **Radius Constraints**: Min/max radius validation
3. 🟡 **Duplicate Prevention**: Check for overlapping areas (same user + similar location)
4. 🟡 **Area Limit**: Free users max 5 areas, paid users unlimited
5. 🟡 **Name Uniqueness**: User cannot create areas with duplicate names
6. ✅ **Authorization**: Users can only manage their own areas

---

## 🔍 FEATURE ANALYSIS

### Key Requirements

#### 1. UI Requirements
- **Map Selection**: User clicks on map to select location
- **Search Integration**: User can search for address/place name
- **Radius Adjustment**: Slider/input to adjust monitoring radius (100m - 5km)
- **Visual Feedback**: Show circle overlay on map with selected radius
- **Area List**: Display all user's areas with status indicators

#### 2. API Requirements
- **Create Area**: Validate geometry, check duplicates, enforce limits
- **List Areas**: Return user's areas sorted by creation date
- **Update Area**: Allow changing name, radius, address text
- **Delete Area**: Soft or hard delete (currently hard delete)
- **Get Area**: Fetch single area details with validation

#### 3. Business Rules

| Rule | Description | Status |
|------|-------------|--------|
| **Geometry Validation** | Latitude: -90 to 90, Longitude: -180 to 180 | ✅ Implemented |
| **Radius Constraints** | Min: 100m, Max: 5,000m (5km) | ✅ Implemented |
| **Area Limit (Free)** | Max 5 areas per user | 🟡 Need to implement |
| **Duplicate Prevention** | No two areas with same center ±50m and overlapping radius | 🟡 Need to implement |
| **Name Uniqueness** | User cannot create areas with duplicate names | 🟡 Need to implement |
| **Ownership** | Users can only access/modify their own areas | ✅ Implemented |

---

## 🗄️ DOMAIN MODEL

### Existing Table: `areas`

```sql
CREATE TABLE areas (
  id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id       UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  name          VARCHAR(255) NOT NULL,
  latitude      NUMERIC(10,6) NOT NULL,
  longitude     NUMERIC(10,6) NOT NULL,
  radius_meters INT NOT NULL,
  address_text  TEXT,

  created_by    UUID NOT NULL,
  created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_by    UUID NOT NULL,
  updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),

  CONSTRAINT chk_latitude CHECK (latitude >= -90 AND latitude <= 90),
  CONSTRAINT chk_longitude CHECK (longitude >= -180 AND longitude <= 180),
  CONSTRAINT chk_radius CHECK (radius_meters >= 100 AND radius_meters <= 5000)
);

-- Existing Indexes
CREATE INDEX ix_areas_user ON areas(user_id, created_at DESC);
CREATE INDEX ix_areas_geo ON areas(latitude, longitude);

-- Additional Index for Duplicate Detection (Recommended)
CREATE INDEX ix_areas_user_name ON areas(user_id, LOWER(name));
```

### Entity Model

**Location**: `src/Core/Domain/FDAAPI.Domain.RelationalDb/Entities/Area.cs`

```csharp
public class Area : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int RadiusMeters { get; set; }
    public string AddressText { get; set; } = string.Empty;

    // Audit fields
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    [JsonIgnore]
    public virtual User User { get; set; } = null!;
}
```

---

## 🔌 API SPECIFICATIONS

### FeatG32: Create Monitored Area

**Endpoint**: `POST /api/v1/areas/area`
**Authorization**: `Policies("User")` - Authenticated users (USER, AUTHORITY, ADMIN, SUPERADMIN)
**Pattern**: MediatR + FluentValidation + Mapper

#### Request DTO

```json
{
  "name": "My Home",
  "latitude": 10.762622,
  "longitude": 106.660172,
  "radiusMeters": 500,
  "addressText": "123 Nguyen Hue, District 1, Ho Chi Minh City"
}
```

#### Request Validation (FluentValidation)

```csharp
public class CreateAreaRequestValidator : AbstractValidator<CreateAreaRequest>
{
    public CreateAreaRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Area name is required.")
            .MaximumLength(255).WithMessage("Area name must not exceed 255 characters.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90 degrees.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180 degrees.");

        RuleFor(x => x.RadiusMeters)
            .GreaterThanOrEqualTo(100).WithMessage("Radius must be at least 100 meters.")
            .LessThanOrEqualTo(5000).WithMessage("Radius must not exceed 5000 meters (5km).");

        RuleFor(x => x.AddressText)
            .MaximumLength(500).WithMessage("Address text must not exceed 500 characters.");
    }
}
```

#### Success Response (201 Created)

```json
{
  "success": true,
  "message": "Area created successfully",
  "statusCode": 201,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "My Home",
    "latitude": 10.762622,
    "longitude": 106.660172,
    "radiusMeters": 500,
    "addressText": "123 Nguyen Hue, District 1, Ho Chi Minh City",
    "createdAt": "2026-01-14T10:30:00Z"
  }
}
```

#### Error Responses

**400 Bad Request - Validation Error**
```json
{
  "success": false,
  "message": "Validation failed",
  "statusCode": 400,
  "errors": {
    "Latitude": ["Latitude must be between -90 and 90 degrees."],
    "RadiusMeters": ["Radius must be at least 100 meters."]
  }
}
```

**409 Conflict - Duplicate Area**
```json
{
  "success": false,
  "message": "An area with similar location already exists within 50 meters",
  "statusCode": 409
}
```

**429 Too Many Requests - Limit Exceeded**
```json
{
  "success": false,
  "message": "You have reached the maximum limit of 5 areas. Upgrade to premium for unlimited areas.",
  "statusCode": 429
}
```

---

### FeatG33: List User's Monitored Areas

**Endpoint**: `GET /api/v1/areas/me`
**Authorization**: `Policies("User")` - Authenticated users
**Pattern**: MediatR + Mapper

#### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Areas retrieved successfully",
  "statusCode": 200,
  "areas": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "My Home",
      "latitude": 10.762622,
      "longitude": 106.660172,
      "radiusMeters": 500,
      "addressText": "123 Nguyen Hue, District 1, HCMC",
      "createdAt": "2026-01-14T10:30:00Z"
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "name": "My Office",
      "latitude": 10.772622,
      "longitude": 106.670172,
      "radiusMeters": 300,
      "addressText": "456 Le Loi, District 1, HCMC",
      "createdAt": "2026-01-14T11:00:00Z"
    }
  ]
}
```

---

### FeatG35: Get Area by ID

**Endpoint**: `GET /api/v1/areas/{id}`
**Authorization**: `Policies("User")` - Owner or Admin
**Pattern**: MediatR + Mapper

#### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Area retrieved successfully",
  "statusCode": 200,
  "area": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "My Home",
    "latitude": 10.762622,
    "longitude": 106.660172,
    "radiusMeters": 500,
    "addressText": "123 Nguyen Hue, District 1, HCMC",
    "createdAt": "2026-01-14T10:30:00Z",
    "updatedAt": "2026-01-14T10:30:00Z"
  }
}
```

#### Error Response (404 Not Found)

```json
{
  "success": false,
  "message": "Area not found",
  "statusCode": 404
}
```

---

### FeatG36: Update Area

**Endpoint**: `PUT /api/v1/areas/{id}`
**Authorization**: `Policies("User")` - Owner or Admin
**Pattern**: MediatR + FluentValidation + Mapper

#### Request DTO

```json
{
  "name": "My Home (Updated)",
  "latitude": 10.762622,
  "longitude": 106.660172,
  "radiusMeters": 800,
  "addressText": "123 Nguyen Hue, District 1, HCMC (Updated)"
}
```

#### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Area updated successfully",
  "statusCode": 200,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "My Home (Updated)",
    "latitude": 10.762622,
    "longitude": 106.660172,
    "radiusMeters": 800,
    "addressText": "123 Nguyen Hue, District 1, HCMC (Updated)",
    "updatedAt": "2026-01-14T12:00:00Z"
  }
}
```

---

### FeatG37: Delete Area

**Endpoint**: `DELETE /api/v1/areas/{id}`
**Authorization**: `Policies("User")` - Owner or Admin
**Pattern**: MediatR

#### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Area deleted successfully",
  "statusCode": 200
}
```

---

## 🚀 IMPLEMENTATION PLAN

### ✅ Phase 1: Already Implemented (FeatG32-37)

- [x] Domain Layer: `Area` entity with proper constraints
- [x] Infrastructure: `IAreaRepository` + `PgsqlAreaRepository`
- [x] Application Layer: MediatR handlers for CRUD operations
- [x] Presentation Layer: FastEndpoints with DTO mapping
- [x] Validation: FluentValidation for geometry and radius
- [x] Mapper: `IAreaMapper` + `AreaMapper` for Entity → DTO
- [x] Authorization: Policy-based access control

### 🟡 Phase 2: Enhancements Needed

#### 2.1 Duplicate Prevention Logic

**Location**: `FDAAPI.App.FeatG32_AreaCreate/CreateAreaHandler.cs`

**Implementation**:
```csharp
// Before creating, check for duplicates
var existingAreas = await _areaRepository.GetByUserIdAsync(request.UserId, ct);

// Calculate distance using Haversine formula
foreach (var area in existingAreas)
{
    var distance = CalculateDistance(
        request.Latitude, request.Longitude,
        area.Latitude, area.Longitude);

    // If within 50m and radius overlaps, reject
    if (distance <= 50 &&
        (distance + request.RadiusMeters) >= area.RadiusMeters)
    {
        return new CreateAreaResponse
        {
            Success = false,
            Message = $"An area '{area.Name}' already exists within 50 meters",
            StatusCode = AreaStatusCode.Conflict
        };
    }
}
```

**Haversine Formula**:
```csharp
private double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
{
    const double R = 6371000; // Earth radius in meters
    var dLat = ToRadians((double)(lat2 - lat1));
    var dLon = ToRadians((double)(lon2 - lon1));

    var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    return R * c; // Distance in meters
}

private double ToRadians(double degrees) => degrees * Math.PI / 180.0;
```

#### 2.2 Area Limit Enforcement

**Implementation**:
```csharp
// Check area count limit
var areaCount = await _areaRepository.CountByUserIdAsync(request.UserId, ct);

// TODO: Get user's subscription plan (Free vs Premium)
const int FREE_TIER_LIMIT = 5;

if (areaCount >= FREE_TIER_LIMIT)
{
    return new CreateAreaResponse
    {
        Success = false,
        Message = $"You have reached the maximum limit of {FREE_TIER_LIMIT} areas. Upgrade to premium for unlimited areas.",
        StatusCode = AreaStatusCode.LimitExceeded // 429
    };
}
```

#### 2.3 Name Uniqueness Check

**Implementation**:
```csharp
// Check for duplicate name (case-insensitive)
var existingAreaWithSameName = await _areaRepository
    .GetByUserIdAndNameAsync(request.UserId, request.Name, ct);

if (existingAreaWithSameName != null)
{
    return new CreateAreaResponse
    {
        Success = false,
        Message = $"You already have an area named '{request.Name}'. Please choose a different name.",
        StatusCode = AreaStatusCode.Conflict
    };
}
```

#### 2.4 New Repository Methods Needed

**Interface**: `IAreaRepository`
```csharp
Task<int> CountByUserIdAsync(Guid userId, CancellationToken ct = default);
Task<Area?> GetByUserIdAndNameAsync(Guid userId, string name, CancellationToken ct = default);
Task<List<Area>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
```

**Implementation**: `PgsqlAreaRepository`
```csharp
public async Task<int> CountByUserIdAsync(Guid userId, CancellationToken ct)
{
    return await _context.Areas
        .Where(a => a.UserId == userId)
        .CountAsync(ct);
}

public async Task<Area?> GetByUserIdAndNameAsync(Guid userId, string name, CancellationToken ct)
{
    return await _context.Areas
        .Where(a => a.UserId == userId && a.Name.ToLower() == name.ToLower())
        .FirstOrDefaultAsync(ct);
}

public async Task<List<Area>> GetByUserIdAsync(Guid userId, CancellationToken ct)
{
    return await _context.Areas
        .Where(a => a.UserId == userId)
        .OrderByDescending(a => a.CreatedAt)
        .ToListAsync(ct);
}
```

---

## 🧪 TESTING STRATEGY

### Test Cases

#### TEST CASE 1: Create Area - Happy Path
**Scenario**: User creates their first monitored area

**cURL**:
```bash
curl -X POST http://localhost:5000/api/v1/areas/area \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {access_token}" \
  -d '{
    "name": "My Home",
    "latitude": 10.762622,
    "longitude": 106.660172,
    "radiusMeters": 500,
    "addressText": "123 Nguyen Hue, District 1, HCMC"
  }'
```

**Expected Response (201 Created)**:
```json
{
  "success": true,
  "message": "Area created successfully",
  "statusCode": 201,
  "data": {
    "id": "uuid",
    "name": "My Home",
    "latitude": 10.762622,
    "longitude": 106.660172,
    "radiusMeters": 500,
    "addressText": "123 Nguyen Hue, District 1, HCMC",
    "createdAt": "2026-01-14T10:30:00Z"
  }
}
```

**Validation**:
- ✅ Status code is 201
- ✅ Area ID is returned
- ✅ All fields match request
- ✅ CreatedAt is set to current time

**Database Check**:
```sql
SELECT * FROM "Areas" WHERE "UserId" = 'user_uuid' ORDER BY "CreatedAt" DESC;
```

---

#### TEST CASE 2: Create Area - Invalid Geometry
**Scenario**: User submits invalid latitude

**cURL**:
```bash
curl -X POST http://localhost:5000/api/v1/areas/area \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {access_token}" \
  -d '{
    "name": "Invalid Area",
    "latitude": 95.0,
    "longitude": 106.660172,
    "radiusMeters": 500,
    "addressText": "Test"
  }'
```

**Expected Response (400 Bad Request)**:
```json
{
  "success": false,
  "message": "Validation failed",
  "statusCode": 400,
  "errors": {
    "Latitude": ["Latitude must be between -90 and 90 degrees."]
  }
}
```

**Validation**:
- ✅ Status code is 400
- ✅ Error message explains the issue
- ✅ No area is created in database

---

#### TEST CASE 3: Create Area - Radius Too Small
**Scenario**: User submits radius < 100m

**cURL**:
```bash
curl -X POST http://localhost:5000/api/v1/areas/area \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {access_token}" \
  -d '{
    "name": "Small Area",
    "latitude": 10.762622,
    "longitude": 106.660172,
    "radiusMeters": 50,
    "addressText": "Test"
  }'
```

**Expected Response (400 Bad Request)**:
```json
{
  "success": false,
  "message": "Validation failed",
  "statusCode": 400,
  "errors": {
    "RadiusMeters": ["Radius must be at least 100 meters."]
  }
}
```

---

#### TEST CASE 4: Create Area - Duplicate Prevention
**Scenario**: User tries to create area at same location

**Setup**: Create first area at (10.762622, 106.660172) with 500m radius

**cURL**:
```bash
curl -X POST http://localhost:5000/api/v1/areas/area \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {access_token}" \
  -d '{
    "name": "Duplicate Home",
    "latitude": 10.762650,
    "longitude": 106.660180,
    "radiusMeters": 500,
    "addressText": "123 Nguyen Hue"
  }'
```

**Expected Response (409 Conflict)**:
```json
{
  "success": false,
  "message": "An area 'My Home' already exists within 50 meters",
  "statusCode": 409
}
```

**Validation**:
- ✅ Status code is 409
- ✅ Error mentions existing area name
- ✅ No duplicate area is created

---

#### TEST CASE 5: Create Area - Limit Exceeded
**Scenario**: Free user tries to create 6th area

**Setup**: Create 5 areas for user first

**cURL**:
```bash
curl -X POST http://localhost:5000/api/v1/areas/area \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {access_token}" \
  -d '{
    "name": "6th Area",
    "latitude": 10.800000,
    "longitude": 106.700000,
    "radiusMeters": 500,
    "addressText": "Far away location"
  }'
```

**Expected Response (429 Too Many Requests)**:
```json
{
  "success": false,
  "message": "You have reached the maximum limit of 5 areas. Upgrade to premium for unlimited areas.",
  "statusCode": 429
}
```

---

#### TEST CASE 6: List User's Areas
**Scenario**: User retrieves all their monitored areas

**Setup**: Create 3 areas for user

**cURL**:
```bash
curl -X GET http://localhost:5000/api/v1/areas/me \
  -H "Authorization: Bearer {access_token}"
```

**Expected Response (200 OK)**:
```json
{
  "success": true,
  "message": "Areas retrieved successfully",
  "statusCode": 200,
  "areas": [
    {
      "id": "uuid-3",
      "name": "My School",
      "latitude": 10.782622,
      "longitude": 106.680172,
      "radiusMeters": 300,
      "createdAt": "2026-01-14T12:00:00Z"
    },
    {
      "id": "uuid-2",
      "name": "My Office",
      "latitude": 10.772622,
      "longitude": 106.670172,
      "radiusMeters": 500,
      "createdAt": "2026-01-14T11:00:00Z"
    },
    {
      "id": "uuid-1",
      "name": "My Home",
      "latitude": 10.762622,
      "longitude": 106.660172,
      "radiusMeters": 500,
      "createdAt": "2026-01-14T10:30:00Z"
    }
  ]
}
```

**Validation**:
- ✅ Status code is 200
- ✅ Areas are sorted by createdAt DESC (newest first)
- ✅ User only sees their own areas

---

#### TEST CASE 7: Update Area
**Scenario**: User updates area radius and name

**cURL**:
```bash
curl -X PUT http://localhost:5000/api/v1/areas/{area_id} \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {access_token}" \
  -d '{
    "name": "My Home (Updated)",
    "latitude": 10.762622,
    "longitude": 106.660172,
    "radiusMeters": 800,
    "addressText": "123 Nguyen Hue, District 1, HCMC"
  }'
```

**Expected Response (200 OK)**:
```json
{
  "success": true,
  "message": "Area updated successfully",
  "statusCode": 200,
  "data": {
    "id": "uuid",
    "name": "My Home (Updated)",
    "radiusMeters": 800,
    "updatedAt": "2026-01-14T13:00:00Z"
  }
}
```

---

#### TEST CASE 8: Delete Area
**Scenario**: User deletes a monitored area

**cURL**:
```bash
curl -X DELETE http://localhost:5000/api/v1/areas/{area_id} \
  -H "Authorization: Bearer {access_token}"
```

**Expected Response (200 OK)**:
```json
{
  "success": true,
  "message": "Area deleted successfully",
  "statusCode": 200
}
```

**Validation**:
- ✅ Status code is 200
- ✅ Area is removed from database
- ✅ User's area count is decreased

**Database Check**:
```sql
SELECT * FROM "Areas" WHERE "Id" = 'area_uuid';
-- Should return 0 rows
```

---

#### TEST CASE 9: Authorization - Access Other User's Area
**Scenario**: User A tries to access User B's area

**cURL**:
```bash
curl -X GET http://localhost:5000/api/v1/areas/{user_b_area_id} \
  -H "Authorization: Bearer {user_a_token}"
```

**Expected Response (403 Forbidden or 404 Not Found)**:
```json
{
  "success": false,
  "message": "Area not found or access denied",
  "statusCode": 404
}
```

---

#### TEST CASE 10: Duplicate Name Prevention
**Scenario**: User tries to create area with duplicate name

**Setup**: Create area named "My Home"

**cURL**:
```bash
curl -X POST http://localhost:5000/api/v1/areas/area \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {access_token}" \
  -d '{
    "name": "my home",
    "latitude": 10.800000,
    "longitude": 106.700000,
    "radiusMeters": 500,
    "addressText": "Different location"
  }'
```

**Expected Response (409 Conflict)**:
```json
{
  "success": false,
  "message": "You already have an area named 'My Home'. Please choose a different name.",
  "statusCode": 409
}
```

**Validation**:
- ✅ Name comparison is case-insensitive
- ✅ Error message is clear

---

## 🔒 SECURITY & VALIDATION

### Input Validation

| Field | Validation Rule | Error Message |
|-------|----------------|---------------|
| **Name** | Required, Max 255 chars | "Area name is required" / "Area name must not exceed 255 characters" |
| **Latitude** | Required, -90 to 90 | "Latitude must be between -90 and 90 degrees" |
| **Longitude** | Required, -180 to 180 | "Longitude must be between -180 and 180 degrees" |
| **RadiusMeters** | Required, 100 to 5000 | "Radius must be at least 100 meters" / "Radius must not exceed 5000 meters" |
| **AddressText** | Optional, Max 500 chars | "Address text must not exceed 500 characters" |

### Authorization

- ✅ **Create/List**: Require authentication via `Policies("Authority")`
- ✅ **Get/Update/Delete**: Verify ownership (UserId matches or Admin role)
- ✅ **Cross-User Access**: Return 404 instead of 403 to prevent enumeration

### Business Rules

- ✅ **Geometry Validation**: Database constraints + FluentValidation
- ✅ **Duplicate Prevention**: Haversine distance check (50m threshold)
- ✅ **Area Limit**: Free tier = 5 areas (enforced in handler)
- ✅ **Name Uniqueness**: Case-insensitive check per user

---

## ⚡ PERFORMANCE CONSIDERATIONS

### Database Optimization

1. **Indexes**:
   - ✅ `ix_areas_user`: (user_id, created_at DESC) - Fast user area listing
   - ✅ `ix_areas_geo`: (latitude, longitude) - Spatial queries
   - 🟡 `ix_areas_user_name`: (user_id, LOWER(name)) - Name uniqueness check

2. **Query Optimization**:
   - Use `AsNoTracking()` for read-only queries (List, Get)
   - Limit result set size (pagination for >100 areas)

### Caching Strategy

- **User Area List**: Cache for 5 minutes (invalidate on create/update/delete)
- **Area Count**: Cache per user for rate limiting checks

### Future: PostGIS Integration

For high-scale deployments (>10k areas):

```sql
-- Add geography column
ALTER TABLE areas ADD COLUMN location GEOGRAPHY(Point, 4326);

-- Create spatial index
CREATE INDEX ix_areas_location ON areas USING GIST(location);

-- Update existing rows
UPDATE areas SET location = ST_SetSRID(ST_MakePoint(longitude, latitude), 4326);

-- Query within radius (PostGIS)
SELECT * FROM areas
WHERE ST_DWithin(
  location::geography,
  ST_SetSRID(ST_MakePoint(106.660172, 10.762622), 4326)::geography,
  50
);
```

---

## 📝 IMPLEMENTATION CHECKLIST

### ✅ Already Completed
- [x] Domain: `Area` entity with constraints
- [x] Infrastructure: `IAreaRepository` + `PgsqlAreaRepository` (basic methods)
- [x] Application: MediatR handlers (FeatG32, 33, 35, 36, 37)
- [x] Presentation: FastEndpoints with DTOs
- [x] Validation: FluentValidation for geometry and radius
- [x] Mapper: `IAreaMapper` for Entity → DTO conversion
- [x] Authorization: Policy-based access control

### 🟡 Phase 2: Enhancements (To Do)
- [ ] Add `CountByUserIdAsync()` to `IAreaRepository`
- [ ] Add `GetByUserIdAndNameAsync()` to `IAreaRepository`
- [ ] Add `GetByUserIdAsync()` to `IAreaRepository`
- [ ] Implement Haversine distance calculation in `CreateAreaHandler`
- [ ] Add duplicate location prevention (50m threshold)
- [ ] Add area limit enforcement (5 areas for free tier)
- [ ] Add name uniqueness check (case-insensitive)
- [ ] Add `AreaStatusCode.Conflict` enum value (409)
- [ ] Add `AreaStatusCode.LimitExceeded` enum value (429)
- [ ] Update `CreateAreaHandler` with all business rules
- [ ] Add unit tests for Haversine formula
- [ ] Add integration tests for duplicate prevention
- [ ] Add integration tests for area limit
- [ ] Update API documentation with new error codes

---

## 🎯 SUCCESS CRITERIA

### Functional Requirements
- ✅ Users can create areas with map selection
- ✅ Users can list their monitored areas
- ✅ Users can update area details
- ✅ Users can delete areas
- ✅ Geometry validation prevents invalid coordinates
- ✅ Radius constraints enforced (100m - 5km)
- 🟡 Duplicate prevention works (50m threshold)
- 🟡 Area limit enforced (5 areas for free users)
- 🟡 Name uniqueness per user

### Non-Functional Requirements
- ✅ API response time < 500ms (without duplicate check)
- ✅ Authorization prevents cross-user access
- ✅ Input validation prevents invalid data
- 🟡 Graceful error messages for business rule violations

---

**Document Version**: 1.0
**Last Updated**: 2026-01-14
**Author**: FDA Development Team
**Status**: 🟡 Planning - Enhancements Needed
**Next Steps**: Implement Phase 2 enhancements (duplicate prevention, limits, name uniqueness)
