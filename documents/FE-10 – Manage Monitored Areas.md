# FE-10 – Manage Monitored Areas

> **Feature Name**: Manage User's Monitored Areas (List, Update, Delete)  
> **Created**: 2026-01-15  
> **Status**: 🟢 Ready for Implementation  
> **Backend Features**: FeatG33 (AreaListByUser), FeatG36 (AreaUpdate), FeatG37 (AreaDelete)  
> **Priority**: High  
> **Dependencies**: FE-09 (Register Monitored Areas - Already Implemented)  
> **Related**: FE-234 (Flood Evaluation Per Area)

---

## 📋 TABLE OF CONTENTS

1. [Executive Summary](#executive-summary)
2. [Feature Analysis](#feature-analysis)
3. [API Specifications](#api-specifications)
4. [Implementation Plan](#implementation-plan)
5. [Testing Strategy](#testing-strategy)
6. [UI/UX Requirements](#uiux-requirements)
7. [Security & Authorization](#security--authorization)
8. [Success Criteria](#success-criteria)

---

## 📊 EXECUTIVE SUMMARY

### Feature Overview

**Problem**: After users create monitored areas (FE-09), they need a way to manage, update, and remove those areas. Currently, the APIs exist but lack comprehensive validation, business rule enforcement, and a clear UI/UX flow.

**Solution**: Provide a complete area management interface allowing users to:
- **View all their monitored areas** in a list format
- **Edit area details** (name, radius, address) with validation
- **Delete areas** they no longer need to monitor
- **Prevent unauthorized access** (users can only manage their own areas)
- **Handle edge cases** (duplicate names, area limits, ownership validation)

### Backend Features Status

| Feature | Endpoint | Type | Description | Status |
|---------|----------|------|-------------|--------|
| **FeatG33** | `GET /api/v1/areas/me` | Query | List user's monitored areas | ✅ Implemented |
| **FeatG36** | `PUT /api/v1/areas/{id}` | Command | Update area details | ✅ Implemented |
| **FeatG37** | `DELETE /api/v1/areas/{id}` | Command | Delete an area | ✅ Implemented |

### Key Enhancements Needed

This feature focuses on **enhancements and testing** of existing implementations:

1. ✅ **Ownership Validation**: Verify user owns the area before update/delete
2. 🟡 **Duplicate Name Check on Update**: Prevent updating to a name that already exists
3. 🟡 **Area Limit Tracking**: After delete, user can create new areas if under limit
4. 🟡 **Comprehensive Error Messages**: Clear feedback for all failure scenarios
5. 🟡 **Multi-Area Testing**: Test scenarios with multiple areas per user
6. 🟡 **Permission Testing**: Cross-user access prevention
7. 🟡 **UI Components**: Design and implement area management interface

---

## 🔍 FEATURE ANALYSIS

### Key Requirements

#### 1. UI/UX Requirements

**Area List View**:
- Display all user's monitored areas in a card/list format
- Show area name, address, radius, creation date
- Provide quick actions: Edit, Delete, View on Map
- Sort by creation date (newest first) or alphabetically
- Visual indicators for area status (active, max limit reached)

**Edit Area Interface**:
- Pre-populate form with existing area data
- Allow editing: Name, Radius, Address Text
- Prevent editing: Location (latitude/longitude) - requires delete & recreate
- Show validation errors inline
- Confirm before saving changes

**Delete Area Interface**:
- Confirm before deletion with warning message
- Show area name in confirmation dialog
- Provide "Undo" option (soft delete) or permanent delete
- Update area count after deletion

#### 2. API Requirements

**List Areas (GET /api/v1/areas/me)**:
- Return only areas belonging to authenticated user
- Sort by `created_at DESC` (newest first)
- Include all area details (id, name, lat/lng, radius, address, timestamps)
- Return empty array if user has no areas
- Handle pagination (future enhancement for >20 areas)

**Update Area (PUT /api/v1/areas/{id})**:
- Validate ownership (user must own the area or be admin)
- Validate all fields (name, radius, lat/lng ranges)
- Check duplicate name (case-insensitive, exclude current area)
- Update `updated_by` and `updated_at` fields
- Return updated area data

**Delete Area (DELETE /api/v1/areas/{id})**:
- Validate ownership (user must own the area or be admin)
- Perform hard delete (remove from database)
- Return success confirmation
- Optional: Soft delete (set `deleted_at` field) for audit trail

#### 3. Business Rules

| Rule | Description | Priority | Status |
|------|-------------|----------|--------|
| **Ownership Enforcement** | Users can only update/delete their own areas | High | ✅ Implemented |
| **Admin Override** | Admins can manage any area | High | ✅ Implemented |
| **Duplicate Name on Update** | Cannot rename area to a name that already exists (case-insensitive) | Medium | 🟡 Need to implement |
| **Geometry Validation** | Latitude/Longitude/Radius must be valid on update | High | ✅ Implemented |
| **Area Not Found** | Return 404 if area doesn't exist | High | ✅ Implemented |
| **Concurrent Update Protection** | Handle race conditions (optimistic locking) | Low | 🔴 Future enhancement |
| **Audit Trail** | Track who updated/deleted and when | Medium | ✅ Implemented (UpdatedBy/UpdatedAt) |

---

## 🔌 API SPECIFICATIONS

### FeatG33: List User's Monitored Areas

**Endpoint**: `GET /api/v1/areas/me`  
**Authorization**: `Policies("User")` - Authenticated users (USER, AUTHORITY, ADMIN, SUPERADMIN)  
**Pattern**: MediatR + Mapper  
**HTTP Method**: GET

#### Request

No request body. User ID extracted from JWT token claims.

```http
GET /api/v1/areas/me HTTP/1.1
Host: localhost:5000
Authorization: Bearer {access_token}
```

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
      "addressText": "123 Nguyen Hue, District 1, Ho Chi Minh City",
      "createdAt": "2026-01-14T10:30:00Z",
      "updatedAt": "2026-01-14T10:30:00Z"
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "name": "My Office",
      "latitude": 10.772622,
      "longitude": 106.670172,
      "radiusMeters": 300,
      "addressText": "456 Le Loi, District 1, HCMC",
      "createdAt": "2026-01-14T11:00:00Z",
      "updatedAt": "2026-01-14T11:00:00Z"
    }
  ]
}
```

#### Success Response - Empty List (200 OK)

```json
{
  "success": true,
  "message": "No areas found",
  "statusCode": 200,
  "areas": []
}
```

#### Error Response (401 Unauthorized)

```json
{
  "success": false,
  "message": "Unauthorized. Please login.",
  "statusCode": 401
}
```

---

### FeatG36: Update Monitored Area

**Endpoint**: `PUT /api/v1/areas/{id}`  
**Authorization**: `Policies("User")` - Owner or Admin  
**Pattern**: MediatR + FluentValidation + Mapper  
**HTTP Method**: PUT

#### Request DTO

```json
{
  "name": "My Home (Updated)",
  "latitude": 10.762622,
  "longitude": 106.660172,
  "radiusMeters": 800,
  "addressText": "123 Nguyen Hue Street, District 1, HCMC (Main Entrance)"
}
```

#### Request Validation (FluentValidation)

**File**: `FDAAPI.App.FeatG36_AreaUpdate/UpdateAreaRequestValidator.cs`

```csharp
public class UpdateAreaRequestValidator : AbstractValidator<UpdateAreaRequest>
{
    public UpdateAreaRequestValidator()
    {
        RuleFor(x => x.AreaId)
            .NotEmpty().WithMessage("Area ID is required.");

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

#### Business Logic in Handler

**File**: `FDAAPI.App.FeatG36_AreaUpdate/UpdateAreaHandler.cs`

**Key Steps**:
1. Fetch existing area by ID
2. **Verify ownership** (area.UserId == request.UserId OR user is Admin)
3. **Check duplicate name** (if name changed, check if new name already exists)
4. Update area fields
5. Set `UpdatedBy` and `UpdatedAt`
6. Save changes via repository
7. Return updated area DTO

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
    "addressText": "123 Nguyen Hue Street, District 1, HCMC (Main Entrance)",
    "createdAt": "2026-01-14T10:30:00Z",
    "updatedAt": "2026-01-15T14:00:00Z"
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
    "Name": ["Area name is required."],
    "RadiusMeters": ["Radius must be at least 100 meters."]
  }
}
```

**404 Not Found - Area Not Found**
```json
{
  "success": false,
  "message": "Area not found",
  "statusCode": 404
}
```

**403 Forbidden - Not Owner**
```json
{
  "success": false,
  "message": "You do not have permission to update this area",
  "statusCode": 403
}
```

**409 Conflict - Duplicate Name**
```json
{
  "success": false,
  "message": "You already have an area named 'My Office'. Please choose a different name.",
  "statusCode": 409
}
```

---

### FeatG37: Delete Monitored Area

**Endpoint**: `DELETE /api/v1/areas/{id}`  
**Authorization**: `Policies("User")` - Owner or Admin  
**Pattern**: MediatR  
**HTTP Method**: DELETE

#### Request

No request body. Area ID in URL path, User ID from JWT token.

```http
DELETE /api/v1/areas/550e8400-e29b-41d4-a716-446655440000 HTTP/1.1
Host: localhost:5000
Authorization: Bearer {access_token}
```

#### Business Logic in Handler

**File**: `FDAAPI.App.FeatG37_AreaDelete/DeleteAreaHandler.cs`

**Key Steps**:
1. Fetch area by ID
2. **Verify ownership** (area.UserId == request.UserId OR user is Admin)
3. Delete area from repository (hard delete)
4. Return success response

**Optional Enhancement**: Soft delete
```csharp
// Instead of hard delete, set deleted_at timestamp
area.DeletedAt = DateTime.UtcNow;
area.DeletedBy = request.UserId;
await _areaRepository.UpdateAsync(area, ct);
```

#### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Area deleted successfully",
  "statusCode": 200
}
```

#### Error Responses

**404 Not Found - Area Not Found**
```json
{
  "success": false,
  "message": "Area not found",
  "statusCode": 404
}
```

**403 Forbidden - Not Owner**
```json
{
  "success": false,
  "message": "You do not have permission to delete this area",
  "statusCode": 403
}
```

---

## 🚀 IMPLEMENTATION PLAN

### ✅ Phase 1: Already Implemented (FeatG33, 36, 37)

- [x] Domain Layer: `Area` entity with proper constraints
- [x] Infrastructure: `IAreaRepository` + `PgsqlAreaRepository`
- [x] Application Layer: MediatR handlers for List, Update, Delete
- [x] Presentation Layer: FastEndpoints with DTOs
- [x] Validation: FluentValidation for geometry and radius
- [x] Mapper: `IAreaMapper` for Entity → DTO conversion
- [x] Authorization: Policy-based access control

### 🟡 Phase 2: Enhancements Needed

#### 2.1 Duplicate Name Check on Update

**Location**: `FDAAPI.App.FeatG36_AreaUpdate/UpdateAreaHandler.cs`

**Implementation**:
```csharp
// After verifying ownership, before updating
if (existingArea.Name != request.Name)
{
    // Name is being changed, check for duplicates
    var duplicateArea = await _areaRepository
        .GetByUserIdAndNameAsync(request.UserId, request.Name, ct);
    
    if (duplicateArea != null && duplicateArea.Id != request.AreaId)
    {
        return new UpdateAreaResponse
        {
            Success = false,
            Message = $"You already have an area named '{request.Name}'. Please choose a different name.",
            StatusCode = AreaStatusCode.Conflict
        };
    }
}
```

#### 2.2 Enhanced Error Messages

**Update handlers to return more descriptive error messages**:
- Distinguish between "Area not found" and "Access denied"
- Provide clear validation error messages
- Include helpful hints (e.g., "Max 5 areas allowed. Delete an area to add a new one.")

#### 2.3 Repository Methods Verification

**Verify these methods exist in `IAreaRepository`**:
```csharp
Task<List<Area>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
Task<Area?> GetByIdAsync(Guid id, CancellationToken ct = default);
Task<Area?> GetByUserIdAndNameAsync(Guid userId, string name, CancellationToken ct = default);
Task<bool> UpdateAsync(Area area, CancellationToken ct = default);
Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
```

#### 2.4 Add Missing StatusCode Enum Values

**File**: `FDAAPI.App.Common/Models/Areas/AreaStatusCode.cs`

Ensure these values exist:
```csharp
public enum AreaStatusCode
{
    Success = 200,
    Created = 201,
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    Conflict = 409,
    InternalServerError = 500
}
```

---

### 🟢 Phase 3: UI/UX Implementation (Frontend)

#### 3.1 Area List Page

**Component**: `AreaListPage.tsx` or `MyAreasPage.tsx`

**Features**:
- Display all user's areas in a grid or list
- Show area details: name, address, radius, created date
- Quick actions: Edit, Delete, View on Map
- Empty state: "No areas yet. Add your first area to start monitoring."
- Loading state: Skeleton cards while fetching data

**API Call**:
```typescript
const { data, isLoading, error } = useQuery({
  queryKey: ['areas', 'me'],
  queryFn: () => api.get('/api/v1/areas/me')
});
```

#### 3.2 Edit Area Dialog/Modal

**Component**: `EditAreaDialog.tsx`

**Features**:
- Form with pre-populated data
- Fields: Name (text), Radius (slider 100-5000m), Address (text)
- Location display (read-only, show on map)
- Validation feedback
- Save/Cancel buttons

**API Call**:
```typescript
const updateMutation = useMutation({
  mutationFn: (data) => api.put(`/api/v1/areas/${areaId}`, data),
  onSuccess: () => {
    queryClient.invalidateQueries(['areas', 'me']);
    toast.success('Area updated successfully');
  },
  onError: (error) => {
    toast.error(error.response.data.message);
  }
});
```

#### 3.3 Delete Confirmation Dialog

**Component**: `DeleteAreaDialog.tsx`

**Features**:
- Warning message: "Are you sure you want to delete '{areaName}'?"
- Explanation: "This action cannot be undone. You will stop receiving flood alerts for this location."
- Delete/Cancel buttons
- Loading state during deletion

**API Call**:
```typescript
const deleteMutation = useMutation({
  mutationFn: (areaId) => api.delete(`/api/v1/areas/${areaId}`),
  onSuccess: () => {
    queryClient.invalidateQueries(['areas', 'me']);
    toast.success('Area deleted successfully');
  }
});
```

---

## 🧪 TESTING STRATEGY

### Test Data Setup

**Create test users and areas**:

```sql
-- Test User 1: Normal user with 3 areas
INSERT INTO "Users" ("Id", "Email", "PasswordHash", "FullName", "Provider", "Status", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt")
VALUES 
('11111111-1111-1111-1111-111111111111', 'user1@test.com', '$2a$11$hashedpassword1', 'Test User 1', 'local', 'ACTIVE', '11111111-1111-1111-1111-111111111111', NOW(), '11111111-1111-1111-1111-111111111111', NOW());

-- Test User 2: Another user with 2 areas
INSERT INTO "Users" ("Id", "Email", "PasswordHash", "FullName", "Provider", "Status", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt")
VALUES 
('22222222-2222-2222-2222-222222222222', 'user2@test.com', '$2a$11$hashedpassword2', 'Test User 2', 'local', 'ACTIVE', '22222222-2222-2222-2222-222222222222', NOW(), '22222222-2222-2222-2222-222222222222', NOW());

-- User 1's areas
INSERT INTO "Areas" ("Id", "UserId", "Name", "Latitude", "Longitude", "RadiusMeters", "AddressText", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt")
VALUES 
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '11111111-1111-1111-1111-111111111111', 'My Home', 10.762622, 106.660172, 500, '123 Nguyen Hue, D1, HCMC', '11111111-1111-1111-1111-111111111111', NOW(), '11111111-1111-1111-1111-111111111111', NOW()),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '11111111-1111-1111-1111-111111111111', 'My Office', 10.772622, 106.670172, 300, '456 Le Loi, D1, HCMC', '11111111-1111-1111-1111-111111111111', NOW(), '11111111-1111-1111-1111-111111111111', NOW()),
('cccccccc-cccc-cccc-cccc-cccccccccccc', '11111111-1111-1111-1111-111111111111', 'Kids School', 10.782622, 106.680172, 400, '789 Tran Hung Dao, D5, HCMC', '11111111-1111-1111-1111-111111111111', NOW(), '11111111-1111-1111-1111-111111111111', NOW());

-- User 2's areas
INSERT INTO "Areas" ("Id", "UserId", "Name", "Latitude", "Longitude", "RadiusMeters", "AddressText", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt")
VALUES 
('dddddddd-dddd-dddd-dddd-dddddddddddd', '22222222-2222-2222-2222-222222222222', 'User2 Home', 10.800000, 106.700000, 600, 'Binh Thanh District', '22222222-2222-2222-2222-222222222222', NOW(), '22222222-2222-2222-2222-222222222222', NOW()),
('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', '22222222-2222-2222-2222-222222222222', 'User2 Office', 10.810000, 106.710000, 700, 'Thu Duc City', '22222222-2222-2222-2222-222222222222', NOW(), '22222222-2222-2222-2222-222222222222', NOW());
```

---

### TEST CASE 1: List All Areas - User with Multiple Areas

**Scenario**: User 1 retrieves all their monitored areas (3 areas)

**Pre-requisites**: 
- User 1 logged in with valid access token
- User 1 has 3 areas in database

**cURL**:
```bash
curl -X GET http://localhost:5000/api/v1/areas/me \
  -H "Authorization: Bearer {user1_access_token}"
```

**Expected Response (200 OK)**:
```json
{
  "success": true,
  "message": "Areas retrieved successfully",
  "statusCode": 200,
  "areas": [
    {
      "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "name": "My Home",
      "latitude": 10.762622,
      "longitude": 106.660172,
      "radiusMeters": 500,
      "addressText": "123 Nguyen Hue, D1, HCMC",
      "createdAt": "2026-01-15T10:00:00Z",
      "updatedAt": "2026-01-15T10:00:00Z"
    },
    {
      "id": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
      "name": "My Office",
      "latitude": 10.772622,
      "longitude": 106.670172,
      "radiusMeters": 300,
      "addressText": "456 Le Loi, D1, HCMC",
      "createdAt": "2026-01-15T10:05:00Z",
      "updatedAt": "2026-01-15T10:05:00Z"
    },
    {
      "id": "cccccccc-cccc-cccc-cccc-cccccccccccc",
      "name": "Kids School",
      "latitude": 10.782622,
      "longitude": 106.680172,
      "radiusMeters": 400,
      "addressText": "789 Tran Hung Dao, D5, HCMC",
      "createdAt": "2026-01-15T10:10:00Z",
      "updatedAt": "2026-01-15T10:10:00Z"
    }
  ]
}
```

**Validation**:
- ✅ Status code is 200
- ✅ Returns exactly 3 areas
- ✅ All areas belong to User 1
- ✅ Areas sorted by creation date (can be DESC or ASC based on implementation)
- ✅ All required fields present (id, name, lat, lng, radius, address, timestamps)

**Database Verification**:
```sql
SELECT * FROM "Areas" WHERE "UserId" = '11111111-1111-1111-1111-111111111111';
-- Should return 3 rows
```

---

### TEST CASE 2: List Areas - User with No Areas

**Scenario**: New user with no areas retrieves empty list

**Pre-requisites**: 
- User 3 logged in with valid access token
- User 3 has 0 areas in database

**cURL**:
```bash
curl -X GET http://localhost:5000/api/v1/areas/me \
  -H "Authorization: Bearer {user3_access_token}"
```

**Expected Response (200 OK)**:
```json
{
  "success": true,
  "message": "No areas found",
  "statusCode": 200,
  "areas": []
}
```

**Validation**:
- ✅ Status code is 200
- ✅ Returns empty array
- ✅ Clear message indicating no areas

---

### TEST CASE 3: Update Area - Happy Path

**Scenario**: User 1 updates the radius and address of "My Home"

**Pre-requisites**: 
- User 1 owns area "My Home" (ID: aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa)

**cURL**:
```bash
curl -X PUT http://localhost:5000/api/v1/areas/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {user1_access_token}" \
  -d '{
    "name": "My Home",
    "latitude": 10.762622,
    "longitude": 106.660172,
    "radiusMeters": 800,
    "addressText": "123 Nguyen Hue Street, District 1, HCMC (Updated Address)"
  }'
```

**Expected Response (200 OK)**:
```json
{
  "success": true,
  "message": "Area updated successfully",
  "statusCode": 200,
  "data": {
    "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "name": "My Home",
    "latitude": 10.762622,
    "longitude": 106.660172,
    "radiusMeters": 800,
    "addressText": "123 Nguyen Hue Street, District 1, HCMC (Updated Address)",
    "createdAt": "2026-01-15T10:00:00Z",
    "updatedAt": "2026-01-15T14:30:00Z"
  }
}
```

**Validation**:
- ✅ Status code is 200
- ✅ Radius updated to 800m
- ✅ Address text updated
- ✅ UpdatedAt timestamp changed
- ✅ CreatedAt timestamp unchanged

**Database Verification**:
```sql
SELECT "RadiusMeters", "AddressText", "UpdatedAt" 
FROM "Areas" 
WHERE "Id" = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
-- RadiusMeters should be 800
-- AddressText should be updated
-- UpdatedAt should be recent
```

---

### TEST CASE 4: Update Area - Duplicate Name

**Scenario**: User 1 tries to rename "My Home" to "My Office" (which already exists)

**Pre-requisites**: 
- User 1 has areas "My Home" and "My Office"

**cURL**:
```bash
curl -X PUT http://localhost:5000/api/v1/areas/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {user1_access_token}" \
  -d '{
    "name": "My Office",
    "latitude": 10.762622,
    "longitude": 106.660172,
    "radiusMeters": 500,
    "addressText": "123 Nguyen Hue, D1, HCMC"
  }'
```

**Expected Response (409 Conflict)**:
```json
{
  "success": false,
  "message": "You already have an area named 'My Office'. Please choose a different name.",
  "statusCode": 409
}
```

**Validation**:
- ✅ Status code is 409
- ✅ Error message mentions the duplicate name
- ✅ Area name is NOT updated in database

**Database Verification**:
```sql
SELECT "Name" FROM "Areas" WHERE "Id" = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
-- Name should still be "My Home", not "My Office"
```

---

### TEST CASE 5: Update Area - Invalid Radius

**Scenario**: User 1 tries to update radius to 50m (below minimum of 100m)

**cURL**:
```bash
curl -X PUT http://localhost:5000/api/v1/areas/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {user1_access_token}" \
  -d '{
    "name": "My Home",
    "latitude": 10.762622,
    "longitude": 106.660172,
    "radiusMeters": 50,
    "addressText": "123 Nguyen Hue, D1, HCMC"
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

**Validation**:
- ✅ Status code is 400
- ✅ Validation error for RadiusMeters field
- ✅ Area is NOT updated

---

### TEST CASE 6: Update Area - Unauthorized Access (Cross-User)

**Scenario**: User 2 tries to update User 1's area

**Pre-requisites**: 
- User 2 logged in
- Area belongs to User 1

**cURL**:
```bash
curl -X PUT http://localhost:5000/api/v1/areas/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {user2_access_token}" \
  -d '{
    "name": "Hacked Area",
    "latitude": 10.762622,
    "longitude": 106.660172,
    "radiusMeters": 1000,
    "addressText": "Malicious Update"
  }'
```

**Expected Response (403 Forbidden OR 404 Not Found)**:
```json
{
  "success": false,
  "message": "Area not found or access denied",
  "statusCode": 404
}
```

**Note**: Return 404 instead of 403 to prevent area ID enumeration attack.

**Validation**:
- ✅ Status code is 404 (or 403)
- ✅ Area is NOT updated
- ✅ Error message does not reveal area existence

**Database Verification**:
```sql
SELECT "Name", "UpdatedBy" FROM "Areas" WHERE "Id" = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
-- Name should still be "My Home"
-- UpdatedBy should still be User 1's ID, not User 2's
```

---

### TEST CASE 7: Update Area - Non-existent Area

**Scenario**: User 1 tries to update an area that doesn't exist

**cURL**:
```bash
curl -X PUT http://localhost:5000/api/v1/areas/99999999-9999-9999-9999-999999999999 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {user1_access_token}" \
  -d '{
    "name": "Ghost Area",
    "latitude": 10.762622,
    "longitude": 106.660172,
    "radiusMeters": 500,
    "addressText": "Non-existent"
  }'
```

**Expected Response (404 Not Found)**:
```json
{
  "success": false,
  "message": "Area not found",
  "statusCode": 404
}
```

**Validation**:
- ✅ Status code is 404
- ✅ Clear error message

---

### TEST CASE 8: Delete Area - Happy Path

**Scenario**: User 1 deletes "Kids School" area

**Pre-requisites**: 
- User 1 owns area "Kids School"

**cURL**:
```bash
curl -X DELETE http://localhost:5000/api/v1/areas/cccccccc-cccc-cccc-cccc-cccccccccccc \
  -H "Authorization: Bearer {user1_access_token}"
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
- ✅ User's area count decreased from 3 to 2

**Database Verification**:
```sql
SELECT * FROM "Areas" WHERE "Id" = 'cccccccc-cccc-cccc-cccc-cccccccccccc';
-- Should return 0 rows (hard delete)

SELECT COUNT(*) FROM "Areas" WHERE "UserId" = '11111111-1111-1111-1111-111111111111';
-- Should return 2 (was 3, now 2)
```

---

### TEST CASE 9: Delete Area - Unauthorized Access (Cross-User)

**Scenario**: User 2 tries to delete User 1's area

**cURL**:
```bash
curl -X DELETE http://localhost:5000/api/v1/areas/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa \
  -H "Authorization: Bearer {user2_access_token}"
```

**Expected Response (403 Forbidden OR 404 Not Found)**:
```json
{
  "success": false,
  "message": "Area not found or access denied",
  "statusCode": 404
}
```

**Validation**:
- ✅ Status code is 404 (or 403)
- ✅ Area is NOT deleted from database
- ✅ User 1's area count unchanged

**Database Verification**:
```sql
SELECT * FROM "Areas" WHERE "Id" = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
-- Should still exist (1 row)
```

---

### TEST CASE 10: Delete Area - Non-existent Area

**Scenario**: User 1 tries to delete an area that doesn't exist

**cURL**:
```bash
curl -X DELETE http://localhost:5000/api/v1/areas/99999999-9999-9999-9999-999999999999 \
  -H "Authorization: Bearer {user1_access_token}"
```

**Expected Response (404 Not Found)**:
```json
{
  "success": false,
  "message": "Area not found",
  "statusCode": 404
}
```

**Validation**:
- ✅ Status code is 404
- ✅ Clear error message

---

### TEST CASE 11: Admin Override - Update Any User's Area

**Scenario**: Admin updates User 1's area

**Pre-requisites**: 
- Admin user logged in
- Area belongs to User 1

**cURL**:
```bash
curl -X PUT http://localhost:5000/api/v1/areas/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {admin_access_token}" \
  -d '{
    "name": "My Home (Admin Updated)",
    "latitude": 10.762622,
    "longitude": 106.660172,
    "radiusMeters": 1000,
    "addressText": "Updated by Admin"
  }'
```

**Expected Response (200 OK)**:
```json
{
  "success": true,
  "message": "Area updated successfully",
  "statusCode": 200,
  "data": {
    "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "name": "My Home (Admin Updated)",
    "radiusMeters": 1000,
    "addressText": "Updated by Admin",
    "updatedAt": "2026-01-15T15:00:00Z"
  }
}
```

**Validation**:
- ✅ Admin can update any user's area
- ✅ UpdatedBy field set to Admin's ID
- ✅ Area is successfully updated

---

### TEST CASE 12: Admin Override - Delete Any User's Area

**Scenario**: Admin deletes User 2's area

**cURL**:
```bash
curl -X DELETE http://localhost:5000/api/v1/areas/dddddddd-dddd-dddd-dddd-dddddddddddd \
  -H "Authorization: Bearer {admin_access_token}"
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
- ✅ Admin can delete any user's area
- ✅ Area is removed from database

---

### TEST CASE 13: Multi-Area Management - Update Multiple Areas

**Scenario**: User 1 updates 2 different areas in sequence

**Step 1**: Update "My Home"
```bash
curl -X PUT http://localhost:5000/api/v1/areas/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {user1_access_token}" \
  -d '{"name": "My Home", "latitude": 10.762622, "longitude": 106.660172, "radiusMeters": 700, "addressText": "Updated Home"}'
```

**Step 2**: Update "My Office"
```bash
curl -X PUT http://localhost:5000/api/v1/areas/bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {user1_access_token}" \
  -d '{"name": "My Office", "latitude": 10.772622, "longitude": 106.670172, "radiusMeters": 400, "addressText": "Updated Office"}'
```

**Validation**:
- ✅ Both updates succeed
- ✅ Each area updated independently
- ✅ No interference between updates

**Database Verification**:
```sql
SELECT "Id", "Name", "RadiusMeters", "UpdatedAt" 
FROM "Areas" 
WHERE "UserId" = '11111111-1111-1111-1111-111111111111';
-- Both areas should show updated RadiusMeters and recent UpdatedAt
```

---

### TEST CASE 14: List Areas After Deletion

**Scenario**: User 1 lists areas after deleting one

**Step 1**: User 1 has 3 areas initially
**Step 2**: Delete "Kids School"
**Step 3**: List areas

**cURL**:
```bash
curl -X GET http://localhost:5000/api/v1/areas/me \
  -H "Authorization: Bearer {user1_access_token}"
```

**Expected Response (200 OK)**:
```json
{
  "success": true,
  "message": "Areas retrieved successfully",
  "statusCode": 200,
  "areas": [
    {
      "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "name": "My Home",
      "radiusMeters": 500
    },
    {
      "id": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
      "name": "My Office",
      "radiusMeters": 300
    }
  ]
}
```

**Validation**:
- ✅ Only 2 areas returned (instead of 3)
- ✅ Deleted area not in list
- ✅ Remaining areas intact

---

### TEST CASE 15: Concurrent Update Protection (Future Enhancement)

**Scenario**: Two users (User A and User B via shared account) try to update the same area simultaneously

**Expected Behavior**:
- First update succeeds
- Second update detects concurrent modification (via optimistic locking)
- Returns 409 Conflict with message "Area was updated by another user. Please refresh and try again."

**Implementation Note**: Requires adding `RowVersion` or `ETag` field to `Area` entity.

---

## 🎨 UI/UX REQUIREMENTS

### 1. Area List Page Layout

**Desktop View** (>1024px):
```
┌─────────────────────────────────────────────────────┐
│ Header: "My Monitored Areas" [+ Add Area Button]   │
├─────────────────────────────────────────────────────┤
│ ┌────────────┐  ┌────────────┐  ┌────────────┐    │
│ │ My Home    │  │ My Office  │  │ Kids School│    │
│ │ 500m radius│  │ 300m radius│  │ 400m radius│    │
│ │ D1, HCMC   │  │ D1, HCMC   │  │ D5, HCMC   │    │
│ │ [Edit][Del]│  │ [Edit][Del]│  │ [Edit][Del]│    │
│ └────────────┘  └────────────┘  └────────────┘    │
│                                                     │
│ Showing 3 of 5 areas max (Free Plan)              │
└─────────────────────────────────────────────────────┘
```

**Mobile View** (<768px):
```
┌──────────────────────────┐
│ My Monitored Areas       │
│ [+ Add Area]             │
├──────────────────────────┤
│ ┌──────────────────────┐ │
│ │ My Home              │ │
│ │ 500m • D1, HCMC      │ │
│ │ [Edit] [Delete]      │ │
│ └──────────────────────┘ │
│                          │
│ ┌──────────────────────┐ │
│ │ My Office            │ │
│ │ 300m • D1, HCMC      │ │
│ │ [Edit] [Delete]      │ │
│ └──────────────────────┘ │
│                          │
│ 3/5 areas (Free)        │
└──────────────────────────┘
```

### 2. Area Card Component

**Component**: `AreaCard.tsx`

**Props**:
```typescript
interface AreaCardProps {
  area: {
    id: string;
    name: string;
    latitude: number;
    longitude: number;
    radiusMeters: number;
    addressText: string;
    createdAt: string;
  };
  onEdit: (areaId: string) => void;
  onDelete: (areaId: string) => void;
  onViewMap: (areaId: string) => void;
}
```

**Visual Elements**:
- Area name (bold, large text)
- Radius with icon (e.g., "📍 500 meters")
- Address (truncated if too long)
- Created date (relative time: "2 days ago")
- Action buttons: Edit (✏️), Delete (🗑️), View on Map (🗺️)

**States**:
- Default state
- Hover state (elevate card, show full address tooltip)
- Loading state (during delete operation)

### 3. Edit Area Modal

**Component**: `EditAreaModal.tsx`

**Layout**:
```
┌────────────────────────────────────────┐
│ Edit Area: "My Home"               [X] │
├────────────────────────────────────────┤
│                                        │
│ Area Name *                            │
│ ┌────────────────────────────────────┐ │
│ │ My Home                            │ │
│ └────────────────────────────────────┘ │
│                                        │
│ Monitoring Radius (meters) *           │
│ ┌────────────────────────────────────┐ │
│ │ ◀──────●────────────────────────▶  │ │
│ │        800m                         │ │
│ └────────────────────────────────────┘ │
│ (Min: 100m, Max: 5000m)                │
│                                        │
│ Location (Read-only)                   │
│ ┌────────────────────────────────────┐ │
│ │ 10.762622, 106.660172              │ │
│ └────────────────────────────────────┘ │
│ [View on Map]                          │
│                                        │
│ Address                                │
│ ┌────────────────────────────────────┐ │
│ │ 123 Nguyen Hue St, D1, HCMC        │ │
│ └────────────────────────────────────┘ │
│                                        │
│        [Cancel]        [Save Changes]  │
└────────────────────────────────────────┘
```

**Validation Feedback**:
- Show error messages below fields
- Disable "Save Changes" button if form invalid
- Show loading spinner on button during API call

### 4. Delete Confirmation Dialog

**Component**: `DeleteAreaDialog.tsx`

**Layout**:
```
┌────────────────────────────────────────┐
│ ⚠️  Delete Area?                   [X] │
├────────────────────────────────────────┤
│                                        │
│ Are you sure you want to delete        │
│ "My Home"?                             │
│                                        │
│ This action cannot be undone. You will │
│ stop receiving flood alerts for this   │
│ location.                              │
│                                        │
│                                        │
│        [Cancel]        [Delete Area]   │
│                           (Red button) │
└────────────────────────────────────────┘
```

**Behavior**:
- "Delete Area" button is red/destructive style
- Show loading spinner during deletion
- Auto-close dialog on success
- Show error toast on failure (don't close dialog)

### 5. Empty State

**Scenario**: User has no areas yet

**Layout**:
```
┌─────────────────────────────────────┐
│                                     │
│          🗺️                         │
│   No monitored areas yet            │
│                                     │
│   Add your first area to start      │
│   receiving flood alerts for        │
│   important locations.              │
│                                     │
│       [+ Add Your First Area]       │
│                                     │
└─────────────────────────────────────┘
```

### 6. Loading State

**Scenario**: Fetching areas from API

**Layout**:
```
┌────────────────────────────────────────┐
│ My Monitored Areas                     │
├────────────────────────────────────────┤
│ ┌──────────────┐  ┌──────────────┐    │
│ │ ████████     │  │ ████████     │    │
│ │ ████         │  │ ████         │    │
│ │ ██████       │  │ ██████       │    │
│ └──────────────┘  └──────────────┘    │
│                                        │
│ (Skeleton loading cards)               │
└────────────────────────────────────────┘
```

---

## 🔒 SECURITY & AUTHORIZATION

### 1. Authentication Requirements

**All Endpoints Require Authentication**:
- `GET /api/v1/areas/me` - ✅ Require valid JWT token
- `PUT /api/v1/areas/{id}` - ✅ Require valid JWT token
- `DELETE /api/v1/areas/{id}` - ✅ Require valid JWT token

**Policy**: `Policies("User")` - Allows USER, AUTHORITY, ADMIN, SUPERADMIN roles

### 2. Authorization Matrix

| Action | Owner | Other User | Admin | Logic |
|--------|-------|------------|-------|-------|
| List My Areas | ✅ Yes | ❌ No | ✅ Yes (own areas) | UserId from JWT |
| Update Area | ✅ Yes | ❌ No | ✅ Yes (any area) | `area.UserId == userId OR user.IsAdmin` |
| Delete Area | ✅ Yes | ❌ No | ✅ Yes (any area) | `area.UserId == userId OR user.IsAdmin` |

### 3. Ownership Validation Logic

**Implementation in Handlers**:

```csharp
// UpdateAreaHandler.cs & DeleteAreaHandler.cs

// 1. Fetch area by ID
var area = await _areaRepository.GetByIdAsync(request.AreaId, ct);
if (area == null)
{
    return new Response { StatusCode = 404, Message = "Area not found" };
}

// 2. Check if user is Admin
var isAdmin = User.IsInRole("ADMIN") || User.IsInRole("SUPERADMIN");

// 3. Verify ownership
if (!isAdmin && area.UserId != request.UserId)
{
    // Return 404 instead of 403 to prevent area ID enumeration
    return new Response { StatusCode = 404, Message = "Area not found" };
}

// 4. Proceed with update/delete
```

### 4. Prevent Area ID Enumeration

**Strategy**: Return `404 Not Found` instead of `403 Forbidden` when user tries to access area they don't own.

**Rationale**: Prevents attackers from discovering valid area IDs by probing with different GUIDs.

**Implementation**:
```csharp
// ❌ BAD: Reveals area exists
if (area.UserId != userId)
    return Forbidden("You don't have permission"); // 403

// ✅ GOOD: Hides area existence
if (area.UserId != userId)
    return NotFound("Area not found"); // 404
```

### 5. Input Validation Summary

| Field | Validation | Error Code |
|-------|-----------|------------|
| Area ID | Valid GUID format | 400 |
| User ID | Valid GUID from JWT | 401 |
| Name | Required, max 255 chars | 400 |
| Latitude | -90 to 90 | 400 |
| Longitude | -180 to 180 | 400 |
| Radius | 100 to 5000 meters | 400 |
| Address | Optional, max 500 chars | 400 |

### 6. Rate Limiting (Future Enhancement)

**Recommendation**: Implement rate limiting to prevent abuse

```csharp
// Example: Max 30 update requests per minute per user
[RateLimit(30, 60)] // 30 requests per 60 seconds
public class UpdateAreaEndpoint : Endpoint<UpdateAreaRequestDto>
{
    // ...
}
```

---

## 🎯 SUCCESS CRITERIA

### Functional Requirements

- ✅ **List Areas**: Users can view all their monitored areas
- ✅ **Update Area**: Users can edit area name, radius, address
- ✅ **Delete Area**: Users can remove areas they no longer need
- ✅ **Ownership Enforcement**: Users can only manage their own areas
- ✅ **Admin Override**: Admins can manage any user's areas
- 🟡 **Duplicate Name Prevention**: Cannot rename to existing area name
- ✅ **Validation**: All inputs validated with clear error messages
- ✅ **Empty State**: Graceful handling when user has no areas

### Non-Functional Requirements

- ✅ **Performance**: API response time < 500ms for list/update/delete
- ✅ **Security**: Authorization checks prevent unauthorized access
- ✅ **Usability**: Clear error messages and feedback
- ✅ **Reliability**: Operations are atomic (succeed or fail completely)
- 🟡 **Audit Trail**: Track who updated/deleted and when (UpdatedBy/UpdatedAt)

### Testing Coverage

- ✅ **Happy Path**: All operations work with valid data
- ✅ **Validation**: All invalid inputs rejected with proper errors
- ✅ **Authorization**: Cross-user access prevented
- ✅ **Multi-Area**: Multiple areas managed independently
- ✅ **Edge Cases**: Non-existent areas, empty lists, duplicate names
- ✅ **Admin Tests**: Admin override functionality verified

---

## 📝 IMPLEMENTATION CHECKLIST

### Backend (API)

- [ ] **Verify FeatG33 (List Areas)**:
  - [ ] Returns only user's areas (UserId filter)
  - [ ] Returns empty array if no areas
  - [ ] Sorted by created_at DESC
  
- [ ] **Enhance FeatG36 (Update Area)**:
  - [ ] Ownership validation (user or admin)
  - [ ] Duplicate name check (exclude current area)
  - [ ] Update UpdatedBy and UpdatedAt fields
  - [ ] Return 404 for unauthorized access (not 403)
  
- [ ] **Enhance FeatG37 (Delete Area)**:
  - [ ] Ownership validation (user or admin)
  - [ ] Hard delete implementation
  - [ ] Return 404 for unauthorized access (not 403)

- [ ] **Repository Methods**:
  - [ ] `GetByUserIdAsync()` - returns all user's areas
  - [ ] `GetByIdAsync()` - returns single area
  - [ ] `GetByUserIdAndNameAsync()` - for duplicate check
  - [ ] `UpdateAsync()` - updates area
  - [ ] `DeleteAsync()` - deletes area

- [ ] **StatusCode Enum**:
  - [ ] Add `AreaStatusCode.Conflict` (409) for duplicate name
  - [ ] Add `AreaStatusCode.Forbidden` (403) if needed

- [ ] **Testing**:
  - [ ] Write integration tests for all 15 test cases
  - [ ] Test multi-area scenarios
  - [ ] Test cross-user access prevention
  - [ ] Test admin override functionality

### Frontend (UI)

- [ ] **Area List Page**:
  - [ ] Fetch areas from `GET /api/v1/areas/me`
  - [ ] Display area cards in grid/list
  - [ ] Show empty state if no areas
  - [ ] Show loading skeleton during fetch
  - [ ] Sort areas by creation date

- [ ] **Area Card Component**:
  - [ ] Display area name, radius, address, date
  - [ ] Edit button → opens EditAreaModal
  - [ ] Delete button → opens DeleteConfirmDialog
  - [ ] View on Map button → centers map on area

- [ ] **Edit Area Modal**:
  - [ ] Pre-populate form with area data
  - [ ] Radius slider (100-5000m)
  - [ ] Form validation
  - [ ] Call `PUT /api/v1/areas/{id}`
  - [ ] Handle success/error responses
  - [ ] Refresh area list on success

- [ ] **Delete Confirmation Dialog**:
  - [ ] Show warning message with area name
  - [ ] Destructive "Delete" button (red)
  - [ ] Call `DELETE /api/v1/areas/{id}`
  - [ ] Handle success/error responses
  - [ ] Refresh area list on success

- [ ] **Error Handling**:
  - [ ] Show toast notifications for errors
  - [ ] Display inline validation errors
  - [ ] Handle network errors gracefully

### Documentation

- [x] **Feature Documentation**: This document (FE-10)
- [ ] **API Documentation**: Update Swagger/OpenAPI specs
- [ ] **User Guide**: Create end-user documentation
- [ ] **Developer Guide**: Document testing procedures

---

## 🔄 RELATED FEATURES

### Dependencies

- **FE-09 (Register Monitored Areas)**: Must be implemented first (Create Area functionality)
- **FE-01 (Authentication)**: JWT authentication required for all endpoints

### Future Enhancements

- **FE-234 (Flood Evaluation Per Area)**: Display flood risk status on area cards
- **Soft Delete**: Add `DeletedAt` field for audit trail (can restore deleted areas)
- **Optimistic Locking**: Add `RowVersion` field to prevent concurrent update conflicts
- **Pagination**: For users with >20 areas
- **Bulk Operations**: Delete multiple areas at once
- **Area Templates**: Save area configurations for quick creation
- **Area Sharing**: Share monitored areas with family members

---

## 📚 REFERENCES

### Related Documents

1. **FE-09 (Register Monitored Areas)**: `documents/FE09-Register-Monitored-Areas-Complete-Documentation.md`
2. **Database Schema**: `documents/db.md`
3. **Architecture Principles**: `documents/general.md`
4. **Workflow**: `documents/workflow.md`
5. **Prompt Template**: `documents/Prompt-Template-For-New-Features.md`

### Code References

1. **Area Entity**: `src/Core/Domain/FDAAPI.Domain.RelationalDb/Entities/Area.cs`
2. **Area Repository Interface**: `src/Core/Domain/FDAAPI.Domain.RelationalDb/Repositories/IAreaRepository.cs`
3. **Area Repository Implementation**: `src/External/Infrastructure/Persistence/FDAAPI.Infra.Persistence/Repositories/PgsqlAreaRepository.cs`
4. **FeatG33 (List)**: `src/Core/Application/FDAAPI.App.FeatG33_AreaListByUser/`
5. **FeatG36 (Update)**: `src/Core/Application/FDAAPI.App.FeatG36_AreaUpdate/`
6. **FeatG37 (Delete)**: `src/Core/Application/FDAAPI.App.FeatG37_AreaDelete/`

---

**Document Version**: 1.0  
**Created**: 2026-01-15  
**Status**: 🟢 Ready for Implementation  
**Next Steps**: 
1. Review and validate all 15 test cases
2. Implement duplicate name check in UpdateAreaHandler
3. Verify ownership validation in all handlers
4. Implement frontend UI components
5. Run comprehensive testing (multi-area, cross-user, admin scenarios)

---

**Author**: FDA Development Team  
**Reviewed By**: (To be reviewed)  
**Approved By**: (To be approved)
