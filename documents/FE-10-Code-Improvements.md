# FE-10 – Code Improvements & Missing Implementation

> **Document Purpose**: Chi tiết code cần bổ sung/sửa cho FE-10 Manage Monitored Areas  
> **Created**: 2026-01-15  
> **Based On**: Prompt-Template-For-New-Features.md

---

## 📋 SUMMARY OF ISSUES

### ✅ Already Implemented (Good)
- ✅ Repository methods (IAreaRepository + PgsqlAreaRepository)
- ✅ AreaStatusCode enum with all needed codes
- ✅ FeatG33_AreaListByUser - Works correctly
- ✅ Mappers (IAreaMapper, AreaMapper)

### ❌ Missing/Needs Fix

| Issue | File | Priority | Description |
|-------|------|----------|-------------|
| 🔴 Duplicate name check | `UpdateAreaHandler.cs` | HIGH | Không check duplicate name khi update |
| 🔴 Admin role check | `UpdateAreaHandler.cs`, `DeleteAreaHandler.cs` | HIGH | Admin phải update/delete được mọi area |
| 🔴 Security - 404 not 403 | `UpdateAreaHandler.cs`, `DeleteAreaHandler.cs` | HIGH | Return 404 thay vì 403 để tránh enumeration |
| 🔴 Return updated data | `UpdateAreaResponse.cs`, `UpdateAreaHandler.cs` | MEDIUM | Response phải return updated area data |
| 🔴 Wrong routes | All Endpoints | HIGH | Routes không đúng theo spec |

---

## 🔧 DETAILED CODE FIXES

### FIX 1: Update `UpdateAreaRequest.cs` - Add Admin Role

**File**: `src/Core/Application/FDAAPI.App.FeatG36_AreaUpdate/UpdateAreaRequest.cs`

**Current Code** (cần đọc để xem):
```csharp
public sealed record UpdateAreaRequest(
    Guid Id,
    Guid UserId,
    string Name,
    decimal Latitude,
    decimal Longitude,
    int RadiusMeters,
    string AddressText
) : IFeatureRequest<UpdateAreaResponse>;
```

**✅ NEW CODE - Add UserRole parameter**:

```csharp
using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG36_AreaUpdate
{
    public sealed record UpdateAreaRequest(
        Guid Id,
        Guid UserId,
        string UserRole,  // ← ADD THIS: "ADMIN", "USER", etc.
        string Name,
        decimal Latitude,
        decimal Longitude,
        int RadiusMeters,
        string AddressText
    ) : IFeatureRequest<UpdateAreaResponse>;
}
```

**Explanation**: Cần biết user role để kiểm tra xem có phải Admin không.

---

### FIX 2: Update `UpdateAreaResponse.cs` - Add Data field

**File**: `src/Core/Application/FDAAPI.App.FeatG36_AreaUpdate/UpdateAreaResponse.cs`

**Current Code**:
```csharp
public class UpdateAreaResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public AreaStatusCode StatusCode { get; set; }
}
```

**✅ NEW CODE - Add Data field**:

```csharp
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Areas;

namespace FDAAPI.App.FeatG36_AreaUpdate
{
    public class UpdateAreaResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AreaStatusCode StatusCode { get; set; }
        public AreaDto? Data { get; set; }  // ← ADD THIS: Return updated area
    }
}
```

---

### FIX 3: REWRITE `UpdateAreaHandler.cs` - Complete Implementation

**File**: `src/Core/Application/FDAAPI.App.FeatG36_AreaUpdate/UpdateAreaHandler.cs`

**❌ CURRENT CODE** (có nhiều vấn đề):
```csharp
public async Task<UpdateAreaResponse> Handle(UpdateAreaRequest request, CancellationToken ct)
{
    var area = await _areaRepository.GetByIdAsync(request.Id, ct);

    if (area == null)
    {
        return new UpdateAreaResponse
        {
            Success = false,
            Message = "Area not found",
            StatusCode = AreaStatusCode.NotFound
        };
    }

    // ❌ PROBLEM 1: Không check Admin role
    if (area.UserId != request.UserId)
    {
        return new UpdateAreaResponse
        {
            Success = false,
            Message = "Unauthorized to update this area",
            StatusCode = AreaStatusCode.Forbidden  // ❌ PROBLEM 2: Nên return 404, không phải 403
        };
    }

    // ❌ PROBLEM 3: Không check duplicate name

    area.Name = request.Name;
    area.Latitude = request.Latitude;
    area.Longitude = request.Longitude;
    area.RadiusMeters = request.RadiusMeters;
    area.AddressText = request.AddressText;
    area.UpdatedAt = DateTime.UtcNow;
    area.UpdatedBy = request.UserId;

    var result = await _areaRepository.UpdateAsync(area, ct);

    if (!result)
    {
        return new UpdateAreaResponse
        {
            Success = false,
            Message = "Failed to update area",
            StatusCode = AreaStatusCode.InternalServerError
        };
    }

    return new UpdateAreaResponse
    {
        Success = true,
        Message = "Area updated successfully",
        StatusCode = AreaStatusCode.Success
        // ❌ PROBLEM 4: Không return updated data
    };
}
```

**✅ COMPLETE REWRITE - Đúng theo template**:

```csharp
using FDAAPI.App.Common.Models.Areas;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG36_AreaUpdate
{
    public class UpdateAreaHandler : IRequestHandler<UpdateAreaRequest, UpdateAreaResponse>
    {
        private readonly IAreaRepository _areaRepository;
        private readonly IAreaMapper _areaMapper;

        public UpdateAreaHandler(
            IAreaRepository areaRepository, 
            IAreaMapper areaMapper)
        {
            _areaRepository = areaRepository;
            _areaMapper = areaMapper;
        }

        public async Task<UpdateAreaResponse> Handle(UpdateAreaRequest request, CancellationToken ct)
        {
            // 1. Fetch existing area by ID
            var area = await _areaRepository.GetByIdAsync(request.Id, ct);

            if (area == null)
            {
                return new UpdateAreaResponse
                {
                    Success = false,
                    Message = "Area not found",
                    StatusCode = AreaStatusCode.NotFound
                };
            }

            // 2. ✅ FIX: Check if user is Admin or SuperAdmin
            bool isAdmin = request.UserRole == "ADMIN" || request.UserRole == "SUPERADMIN";

            // 3. ✅ FIX: Authorization check with Admin override
            if (!isAdmin && area.UserId != request.UserId)
            {
                // ✅ FIX: Return 404 instead of 403 to prevent area ID enumeration
                return new UpdateAreaResponse
                {
                    Success = false,
                    Message = "Area not found",  // ← Changed from "Unauthorized..."
                    StatusCode = AreaStatusCode.NotFound  // ← Changed from Forbidden
                };
            }

            // 4. ✅ NEW: Check duplicate name (if name is being changed)
            if (area.Name != request.Name)
            {
                var duplicateArea = await _areaRepository
                    .GetByUserIdAndNameAsync(area.UserId, request.Name, ct);
                
                if (duplicateArea != null && duplicateArea.Id != request.Id)
                {
                    return new UpdateAreaResponse
                    {
                        Success = false,
                        Message = $"You already have an area named '{request.Name}'. Please choose a different name.",
                        StatusCode = AreaStatusCode.Conflict
                    };
                }
            }

            // 5. Update area fields
            area.Name = request.Name;
            area.Latitude = request.Latitude;
            area.Longitude = request.Longitude;
            area.RadiusMeters = request.RadiusMeters;
            area.AddressText = request.AddressText ?? string.Empty;
            area.UpdatedAt = DateTime.UtcNow;
            area.UpdatedBy = request.UserId;

            // 6. Save changes
            var result = await _areaRepository.UpdateAsync(area, ct);

            if (!result)
            {
                return new UpdateAreaResponse
                {
                    Success = false,
                    Message = "Failed to update area",
                    StatusCode = AreaStatusCode.InternalServerError
                };
            }

            // 7. ✅ FIX: Return updated area data
            return new UpdateAreaResponse
            {
                Success = true,
                Message = "Area updated successfully",
                StatusCode = AreaStatusCode.Success,
                Data = _areaMapper.MapToDto(area)  // ← ADD THIS
            };
        }
    }
}
```

**Key Improvements**:
1. ✅ Added Admin role check (line 42-43)
2. ✅ Return 404 instead of 403 for security (line 47-52)
3. ✅ Added duplicate name check (line 55-69)
4. ✅ Return updated area data in response (line 94)

---

### FIX 4: Update `DeleteAreaRequest.cs` - Add UserRole

**File**: `src/Core/Application/FDAAPI.App.FeatG37_AreaDelete/DeleteAreaRequest.cs`

**Current Code**:
```csharp
public sealed record DeleteAreaRequest(
    Guid Id,
    Guid UserId
) : IFeatureRequest<DeleteAreaResponse>;
```

**✅ NEW CODE**:

```csharp
using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG37_AreaDelete
{
    public sealed record DeleteAreaRequest(
        Guid Id,
        Guid UserId,
        string UserRole  // ← ADD THIS
    ) : IFeatureRequest<DeleteAreaResponse>;
}
```

---

### FIX 5: REWRITE `DeleteAreaHandler.cs` - Add Admin Check

**File**: `src/Core/Application/FDAAPI.App.FeatG37_AreaDelete/DeleteAreaHandler.cs`

**❌ CURRENT CODE**:
```csharp
public async Task<DeleteAreaResponse> Handle(DeleteAreaRequest request, CancellationToken ct)
{
    var area = await _areaRepository.GetByIdAsync(request.Id, ct);

    if (area == null)
    {
        return new DeleteAreaResponse
        {
            Success = false,
            Message = "Area not found",
            StatusCode = AreaStatusCode.NotFound
        };
    }

    // ❌ PROBLEM: Không check Admin role
    if (area.UserId != request.UserId)
    {
        return new DeleteAreaResponse
        {
            Success = false,
            Message = "Unauthorized to delete this area",
            StatusCode = AreaStatusCode.Forbidden  // ❌ PROBLEM: Nên return 404
        };
    }

    var result = await _areaRepository.DeleteAsync(request.Id, ct);

    if (!result)
    {
        return new DeleteAreaResponse
        {
            Success = false,
            Message = "Failed to delete area",
            StatusCode = AreaStatusCode.InternalServerError
        };
    }

    return new DeleteAreaResponse
    {
        Success = true,
        Message = "Area deleted successfully",
        StatusCode = AreaStatusCode.Success
    };
}
```

**✅ COMPLETE REWRITE**:

```csharp
using FDAAPI.App.Common.Models.Areas;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG37_AreaDelete
{
    public class DeleteAreaHandler : IRequestHandler<DeleteAreaRequest, DeleteAreaResponse>
    {
        private readonly IAreaRepository _areaRepository;

        public DeleteAreaHandler(IAreaRepository areaRepository)
        {
            _areaRepository = areaRepository;
        }

        public async Task<DeleteAreaResponse> Handle(DeleteAreaRequest request, CancellationToken ct)
        {
            // 1. Fetch area by ID
            var area = await _areaRepository.GetByIdAsync(request.Id, ct);

            if (area == null)
            {
                return new DeleteAreaResponse
                {
                    Success = false,
                    Message = "Area not found",
                    StatusCode = AreaStatusCode.NotFound
                };
            }

            // 2. ✅ FIX: Check if user is Admin or SuperAdmin
            bool isAdmin = request.UserRole == "ADMIN" || request.UserRole == "SUPERADMIN";

            // 3. ✅ FIX: Authorization check with Admin override
            if (!isAdmin && area.UserId != request.UserId)
            {
                // ✅ FIX: Return 404 instead of 403 to prevent enumeration
                return new DeleteAreaResponse
                {
                    Success = false,
                    Message = "Area not found",  // ← Changed from "Unauthorized..."
                    StatusCode = AreaStatusCode.NotFound  // ← Changed from Forbidden
                };
            }

            // 4. Delete area from database
            var result = await _areaRepository.DeleteAsync(request.Id, ct);

            if (!result)
            {
                return new DeleteAreaResponse
                {
                    Success = false,
                    Message = "Failed to delete area",
                    StatusCode = AreaStatusCode.InternalServerError
                };
            }

            return new DeleteAreaResponse
            {
                Success = true,
                Message = "Area deleted successfully",
                StatusCode = AreaStatusCode.Success
            };
        }
    }
}
```

**Key Improvements**:
1. ✅ Added Admin role check (line 34)
2. ✅ Return 404 instead of 403 for security (line 42-47)
3. ✅ Removed unused IAreaMapper injection

---

### FIX 6: Update `AreaListByUserEndpoint.cs` - Fix Route

**File**: `src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/Endpoints/Feat33_AreaListByUser/AreaListByUserEndpoint.cs`

**Change Line 22**:
```csharp
// ❌ CURRENT:
Get("/api/v1/areas/areas-created");

// ✅ FIX:
Get("/api/v1/areas/me");
```

**Complete Configure() method**:
```csharp
public override void Configure()
{
    Get("/api/v1/areas/me");  // ← CHANGED from /api/v1/areas/areas-created
    Policies("User");
    Summary(s =>
    {
        s.Summary = "List monitored areas for the current user";
        s.Description = "Retrieve a paginated list of geographic areas created by the authenticated user";
    });
    Tags("Area");
}
```

---

### FIX 7: Update `UpdateAreaEndpoint.cs` - Fix Route & Pass UserRole

**File**: `src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/Endpoints/Feat36_AreaUpdate/UpdateAreaEndpoint.cs`

**✅ COMPLETE REWRITE**:

```csharp
using FastEndpoints;
using FDAAPI.App.FeatG36_AreaUpdate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat36_AreaUpdate.DTOs;
using MediatR;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat36_AreaUpdate
{
    public class UpdateAreaEndpoint : Endpoint<UpdateAreaRequestDto, UpdateAreaResponseDto>
    {
        private readonly IMediator _mediator;

        public UpdateAreaEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Put("/api/v1/areas/{id}");  // ← FIX: Changed from /api/v1/areas/area/{id}
            Policies("User");
            Summary(s =>
            {
                s.Summary = "Update an existing monitored area";
                s.Description = "Update details of a geographic area by its unique identifier";
            });
            Tags("Area");
        }

        public override async Task HandleAsync(UpdateAreaRequestDto req, CancellationToken ct)
        {
            var id = Route<Guid>("id");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            
            if (userIdClaim == null)
            {
                await SendAsync(new UpdateAreaResponseDto
                {
                    Success = false,
                    Message = "Unauthorized"
                }, 401, ct);
                return;
            }

            var userId = Guid.Parse(userIdClaim.Value);

            // ✅ NEW: Extract user role from JWT claims
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "USER";

            var command = new UpdateAreaRequest(
                id,
                userId,
                userRole,  // ← ADD THIS: Pass user role to handler
                req.Name,
                req.Latitude,
                req.Longitude,
                req.RadiusMeters,
                req.AddressText ?? string.Empty
            );

            var result = await _mediator.Send(command, ct);

            var response = new UpdateAreaResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Data  // ← ADD THIS: Include updated area data
            };

            // Map StatusCode to HTTP status
            await SendAsync(response, (int)result.StatusCode, ct);
        }
    }
}
```

**Key Changes**:
1. ✅ Route changed to `/api/v1/areas/{id}` (line 23)
2. ✅ Extract user role from JWT claims (line 50)
3. ✅ Pass user role to handler (line 54)
4. ✅ Include updated data in response (line 68)
5. ✅ Use StatusCode enum directly for HTTP status (line 72)

---

### FIX 8: Update `UpdateAreaResponseDto.cs` - Add Data field

**File**: `src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/Endpoints/Feat36_AreaUpdate/DTOs/UpdateAreaResponseDto.cs`

**Current Code** (cần kiểm tra):
```csharp
public class UpdateAreaResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
```

**✅ NEW CODE**:

```csharp
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat33_AreaListByUser.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat36_AreaUpdate.DTOs
{
    public class UpdateAreaResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AreaDto? Data { get; set; }  // ← ADD THIS: Updated area data
    }
}
```

---

### FIX 9: Update `DeleteAreaEndpoint.cs` - Fix Route & Pass UserRole

**File**: `src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/Endpoints/Feat37_AreaDelete/DeleteAreaEndpoint.cs`

**✅ COMPLETE REWRITE**:

```csharp
using FastEndpoints;
using FDAAPI.App.FeatG37_AreaDelete;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat37_AreaDelete.DTOs;
using MediatR;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat37_AreaDelete
{
    public class DeleteAreaEndpoint : EndpointWithoutRequest<DeleteAreaResponseDto>
    {
        private readonly IMediator _mediator;

        public DeleteAreaEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Delete("/api/v1/areas/{id}");  // ← FIX: Changed from /api/v1/areas/area/{id}
            Policies("User");
            Summary(s =>
            {
                s.Summary = "Delete a monitored area";
                s.Description = "Remove a geographic area by its unique identifier";
            });
            Tags("Area");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var id = Route<Guid>("id");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");

            if (userIdClaim == null)
            {
                await SendAsync(new DeleteAreaResponseDto
                {
                    Success = false,
                    Message = "Unauthorized"
                }, 401, ct);
                return;
            }

            var userId = Guid.Parse(userIdClaim.Value);

            // ✅ NEW: Extract user role from JWT claims
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "USER";

            var command = new DeleteAreaRequest(
                id, 
                userId,
                userRole  // ← ADD THIS: Pass user role to handler
            );

            var result = await _mediator.Send(command, ct);

            var response = new DeleteAreaResponseDto
            {
                Success = result.Success,
                Message = result.Message
            };

            // Use StatusCode enum directly for HTTP status
            await SendAsync(response, (int)result.StatusCode, ct);
        }
    }
}
```

**Key Changes**:
1. ✅ Route changed to `/api/v1/areas/{id}` (line 23)
2. ✅ Extract user role from JWT claims (line 50)
3. ✅ Pass user role to handler (line 54)
4. ✅ Use StatusCode enum directly for HTTP status (line 68)

---

## 📋 IMPLEMENTATION CHECKLIST

### Application Layer

- [ ] **UpdateAreaRequest.cs**: Add `UserRole` parameter
- [ ] **UpdateAreaResponse.cs**: Add `Data` field (AreaDto)
- [ ] **UpdateAreaHandler.cs**: 
  - [ ] Add Admin role check
  - [ ] Add duplicate name check
  - [ ] Return 404 instead of 403
  - [ ] Return updated area data
- [ ] **DeleteAreaRequest.cs**: Add `UserRole` parameter
- [ ] **DeleteAreaHandler.cs**:
  - [ ] Add Admin role check
  - [ ] Return 404 instead of 403
  - [ ] Remove unused IAreaMapper injection

### Presentation Layer

- [ ] **AreaListByUserEndpoint.cs**: Change route to `/api/v1/areas/me`
- [ ] **UpdateAreaEndpoint.cs**:
  - [ ] Change route to `/api/v1/areas/{id}`
  - [ ] Extract and pass user role
  - [ ] Include updated data in response
- [ ] **UpdateAreaResponseDto.cs**: Add `Data` field
- [ ] **DeleteAreaEndpoint.cs**:
  - [ ] Change route to `/api/v1/areas/{id}`
  - [ ] Extract and pass user role

### Testing

After implementing all fixes:

- [ ] Build solution without errors
- [ ] Test GET /api/v1/areas/me - List areas
- [ ] Test PUT /api/v1/areas/{id} - Update as owner
- [ ] Test PUT /api/v1/areas/{id} - Update as admin (different user's area)
- [ ] Test PUT /api/v1/areas/{id} - Duplicate name check
- [ ] Test PUT /api/v1/areas/{id} - Unauthorized access returns 404
- [ ] Test DELETE /api/v1/areas/{id} - Delete as owner
- [ ] Test DELETE /api/v1/areas/{id} - Delete as admin
- [ ] Test DELETE /api/v1/areas/{id} - Unauthorized access returns 404

---

## 🎯 PRIORITY ORDER

1. **HIGH - Security Fixes** (Do First):
   - Fix routes to match API spec
   - Return 404 instead of 403 for unauthorized access
   - Add Admin role checks

2. **MEDIUM - Business Logic**:
   - Add duplicate name check in UpdateAreaHandler
   - Return updated area data in UpdateAreaResponse

3. **LOW - Nice to Have**:
   - Clean up unused dependencies (IAreaMapper in DeleteAreaHandler)

---

## 📚 REFERENCES

- **Template**: `documents/Prompt-Template-For-New-Features.md`
- **Spec**: `documents/FE-10 – Manage Monitored Areas.md`
- **Example**: `FDAAPI.App.FeatG32_AreaCreate` (Best practice MediatR pattern)

---

**Document Version**: 1.0  
**Created**: 2026-01-15  
**Status**: Ready for Implementation  
**Estimated Time**: 2-3 hours for all fixes + testing
