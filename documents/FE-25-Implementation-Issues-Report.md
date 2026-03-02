# FE-25 Implementation - Issues & Non-Optimized Parts Report

**Date**: February 26, 2026  
**Feature**: FE-25 – Community Flood Reporting (Photo/Video)  
**Reviewed**: FeatG79 (Create), FeatG81 (Get), FeatG82 (Update created), FeatG83 (List), FeatG84 (GetNearby), FeatG85 (Delete created)

---

## 📋 CRITICAL ISSUES

### 1. ❌ **Missing FloodReportMediaRepository Interface**
**Severity**: 🔴 CRITICAL  
**Location**: FeatG82_FloodReportUpdate, FeatG85_FloodReportDelete  

**Issue**:
Both handler implementations reference `IFloodReportMediaRepository.GetByReportIdAsync()` and `DeleteAsync()`, but this interface needs to be:
- Defined in `FDAAPI.Domain.RelationalDb.Repositories`
- Implemented in persistence layer

**Example from FeatG82**:
```csharp
var media = await _mediaRepository.GetByReportIdAsync(report.Id, ct);
```

**Fix Required**:
1. Create `IFloodReportMediaRepository` interface in Domain layer
2. Implement in Persistence layer with EF Core
3. Register in dependency injection

---

### 2. ❌ **Missing IFloodReportRepository Interface Methods**
**Severity**: 🔴 CRITICAL  
**Methods Missing**:
- `GetByIdAsync(Guid id, CancellationToken ct)` - needed by Get, Update, Delete handlers
- `UpdateAsync(FloodReport report, CancellationToken ct)` - needed by Update handler
- `DeleteAsync(Guid id, CancellationToken ct)` - needed by Delete handler

**Current Status**: Only likely has `CreateAsync`, `GetNearbyAsync`, `ListAsync`

---

### 3. ⚠️ **Update Handler Cannot Modify Location**
**Severity**: 🔴 CRITICAL  
**Location**: FeatG82_FloodReportUpdate (line ~50-60)  

**Issue**:
Current implementation prevents updating `Latitude` and `Longitude`:
```csharp
// Note: location, reporter, trust score cannot be changed
```

**Problem**: 
- Users cannot correct the location if they reported in the wrong area
- This is inflexible and violates basic CRUD principles

**Recommendation**:
- Allow location updates BUT recalculate Trust Score
- Add validation to prevent moving reports >500m
- Log location changes for audit purposes

**Updated Logic**:
```csharp
if (request.Latitude != 0 && request.Longitude != 0)
{
    // Validate location hasn't moved too far
    double distance = CalculateHaversineDistance(
        report.Latitude, report.Longitude,
        request.Latitude, request.Longitude);
    
    if (distance > 500)  // 500m threshold
    {
        return new UpdateFloodReportResponse 
        { 
            Success = false,
            Message = "Location change too large. Max 500m allowed."
        };
    }
    
    report.Latitude = request.Latitude;
    report.Longitude = request.Longitude;
    
    // Recalculate trust score after location change
    report.TrustScore = await _trustScoreCalculator.RecalculateAsync(...);
}
```

---

### 4. ⚠️ **Delete Handler - Media Files Not Cleaned Up**
**Severity**: 🟠 HIGH  
**Location**: FeatG85_FloodReportDelete (lines ~60-75)  

**Issue**:
```csharp
// TODO: Delete from ImageKit if photo
// TODO: Delete from Cloudinary if video
await _mediaRepository.DeleteAsync(media.Id, ct);
```

**Problems**:
1. Media files remain in cloud storage (ImageKit/Cloudinary) after deletion
2. Orphaned files waste storage and money
3. No cleanup mechanism implemented

**Fix Required**:
```csharp
// Inject these services
private readonly IImageStorageService _imageKit;
private readonly IVideoStorageService _cloudinary;

// In delete handler:
foreach (var media in mediaRecords)
{
    if (media.MediaType == "photo")
    {
        await _imageKit.DeleteImageAsync(
            ExtractFileIdFromUrl(media.MediaUrl), ct);
    }
    else if (media.MediaType == "video")
    {
        await _cloudinary.DeleteVideoAsync(
            ExtractFileIdFromUrl(media.MediaUrl), ct);
    }
    
    await _mediaRepository.DeleteAsync(media.Id, ct);
}
```

---

## ⚠️ ARCHITECTURAL ISSUES

### 5. ⚠️ **Authorization Model Not Aligned**
**Severity**: 🟠 MEDIUM  
**Location**: All Update/Delete handlers  

**Current Pattern**:
```csharp
bool isAdmin = request.UserRole == "ADMIN" || request.UserRole == "SUPERADMIN";
if (!isAdmin && report.ReporterUserId != request.UserId)
    return 404; // Enumeration prevention
```

**Issues**:
1. **UserRole passed as string** - should use enum or policy-based authorization
2. **Authorization should use Policies** - FastEndpoints/ASP.NET Core pattern
3. **Leaking to Application Layer** - should be in Presentation layer

**Better Approach**:
```csharp
// Use FastEndpoints Policy-based authorization at endpoint level:
[Authorize(Policy = "CanEditFloodReport")]
public class UpdateFloodReportEndpoint : Endpoint<UpdateFloodReportRequest, UpdateFloodReportResponse>
{
    // Handler doesn't need to check authorization
    // FastEndpoints handles it automatically
}

// In Program.cs:
services.AddAuthorization(options =>
{
    options.AddPolicy("CanEditFloodReport", policy =>
        policy.Requirements.Add(new FloodReportEditRequirement()));
});
```

---

### 6. ⚠️ **Missing Transactional Support**
**Severity**: 🟠 MEDIUM  
**Location**: FeatG79_FloodReportCreate  

**Issue**:
When uploading multiple files (photos + videos), if insertion fails midway:
- Some files uploaded to cloud ✅
- Database transaction rolledback ❌
- Orphaned files left in cloud storage

**Current Code Risk**:
```csharp
// Upload photos (succeeds)
foreach (var photo in request.Photos)
{
    uploadedMedia.Add(new MediaUploadResult { ... });
}

// Upload videos (succeeds)
foreach (var video in request.Videos)
{
    uploadedMedia.Add(new MediaUploadResult { ... });
}

// DB insert (fails) → orphaned files in cloud!
await _reportRepository.CreateAsync(report, ct);
```

**Fix**: Use compensating transactions:
```csharp
var uploadedFiles = new List<string>();
try
{
    // Upload all files
    foreach (var photo in request.Photos)
    {
        var url = await _imageKit.UploadImageAsync(...);
        uploadedFiles.Add(url);
    }
    
    // Try DB insert
    var reportId = await _reportRepository.CreateAsync(report, ct);
    
    // If successful, create media records
    foreach (var url in uploadedFiles)
    {
        await _mediaRepository.CreateAsync(
            new FloodReportMedia { Url = url, ... }, ct);
    }
}
catch (Exception ex)
{
    // Cleanup uploaded files
    foreach (var url in uploadedFiles)
    {
        try { await DeleteCloudFile(url); }
        catch { /* log but don't throw */ }
    }
    throw;
}
```

---

### 7. ⚠️ **No Soft-Delete Implementation**
**Severity**: 🟡 MEDIUM/LOW  
**Location**: FeatG85_FloodReportDelete  

**Issue**:
Hard delete removes all report data permanently. Better to:
- Keep data for analytics/audits
- Show "deleted by user" in UI
- Allow moderator review of deletions

**Recommended**:
```csharp
// Add to FloodReport entity:
public bool IsDeleted { get; set; }
public DateTime? DeletedAt { get; set; }

// In Delete handler:
report.IsDeleted = true;
report.DeletedAt = DateTime.UtcNow;
await _reportRepository.UpdateAsync(report, ct);

// In all read operations, filter:
var reports = dbContext.FloodReports
    .Where(r => !r.IsDeleted)
    .ToListAsync();
```

---

## 🔴 DATA VALIDATION ISSUES

### 8. ⚠️ **Update Handler - XSS Vulnerability**
**Severity**: 🔴 HIGH  
**Location**: FeatG82_FloodReportUpdate  

**Issue**:
Description and Address fields updated without HTML sanitization:
```csharp
if (!string.IsNullOrEmpty(request.Description))
    report.Description = request.Description;  // ❌ Not sanitized!
```

**Risk**: Stored XSS attacks when data displayed in UI

**Fix**:
```csharp
// Inject HTML sanitizer
private readonly IHtmlSanitizer _sanitizer;

// In update:
if (!string.IsNullOrEmpty(request.Description))
    report.Description = _sanitizer.Sanitize(request.Description);
```

---

### 9. ⚠️ **Update Handler - Incomplete Validation**
**Severity**: 🟠 MEDIUM  
**Location**: FeatG82_FloodReportUpdateRequestValidator  

**Issue**:
Current validator is minimal. Missing validations for:
1. **Severity enum values** - doesn't validate strictly
2. **Latitude/Longitude when provided** - no range check if updated
3. **Address length** - only max, no min

**Better Validator**:
```csharp
public class UpdateFloodReportRequestValidator 
    : AbstractValidator<UpdateFloodReportRequest>
{
    public UpdateFloodReportRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Report ID is required");

        When(x => !string.IsNullOrEmpty(x.Severity), () =>
        {
            RuleFor(x => x.Severity)
                .Must(s => new[] { "low", "medium", "high" }
                    .Contains(s.ToLower()))
                .WithMessage("Invalid severity value");
        });

        When(x => !string.IsNullOrEmpty(x.Address), () =>
        {
            RuleFor(x => x.Address)
                .Length(3, 500)
                .WithMessage("Address must be 3-500 characters");
        });

        When(x => !string.IsNullOrEmpty(x.Description), () =>
        {
            RuleFor(x => x.Description)
                .Length(5, 1000)
                .Matches(@"[a-zA-Z0-9]")
                .WithMessage("Description must contain alphanumeric characters");
        });
    }
}
```

---

## 🔵 PERFORMANCE ISSUES

### 10. ⚠️ **GetNearby Query Performance**
**Severity**: 🟡 MEDIUM  
**Location**: FeatG84_FloodReportGetNearby  

**Issue**:
The endpoint queries for nearby reports every time. With many reports, this becomes slow:
```csharp
// Performs spatial calculation on EVERY call
var nearbyReports = await FindNearbyPublishedReports(
    latitude, longitude, createdAt);
```

**Optimization**:
1. **Add spatial index** (PostGIS for PostgreSQL):
   ```sql
   CREATE INDEX ix_flood_reports_location 
   ON flood_reports USING GIST(
       ST_SetSRID(ST_MakePoint(longitude, latitude), 4326)
   );
   ```

2. **Cache nearby reports**:
   ```csharp
   private const string CACHE_KEY = "nearby_reports_{0}_{1}_{2}";
   
   var cacheKey = string.Format(CACHE_KEY, lat, lng, radius);
   var cached = await _cache.GetAsync(cacheKey);
   if (cached != null) return cached;
   
   var results = await _repo.GetNearbyAsync(...);
   await _cache.SetAsync(cacheKey, results, TimeSpan.FromMinutes(2));
   ```

3. **Limit result set**:
   ```csharp
   // Only return 10 closest reports, not all
   .OrderBy(r => Distance(r.Latitude, r.Longitude, lat, lng))
   .Take(10)
   ```

---

### 11. ⚠️ **N+1 Query Problem in Handlers**
**Severity**: 🟡 MEDIUM  
**Location**: FeatG82_FloodReportUpdate, FeatG81_FloodReportGet  

**Current Pattern**:
```csharp
var report = await _reportRepository.GetByIdAsync(request.Id, ct);
var media = await _mediaRepository.GetByReportIdAsync(report.Id, ct);
// = 2 queries
```

**Better Approach** - eager load in repo:
```csharp
var report = await dbContext.FloodReports
    .Include(r => r.Media)  // ← Eager load
    .FirstOrDefaultAsync(r => r.Id == id, ct);
    
// = 1 query, includes media
```

---

## 🟡 MISSING FEATURES

### 12. ⚠️ **No Optimistic Locking**
**Severity**: 🟡 MEDIUM  
**Location**: FeatG82_FloodReportUpdate  

**Issue**:
Two users updating simultaneously causes last-write-wins scenario:
```
User A: Updates severity from "low" to "high"
User B: Updates description, doesn't see the severity change
Result: User A's severity change lost
```

**Fix** - Add version column:
```csharp
public class FloodReport
{
    public Guid Id { get; set; }
    [ConcurrencyCheck]
    public int Version { get; set; }
    // ... other properties
}

// In update:
try
{
    report.Version++;
    await _reportRepository.UpdateAsync(report, ct);
}
catch (DbUpdateConcurrencyException)
{
    return new UpdateFloodReportResponse
    {
        Success = false,
        Message = "Report was modified by another user. Please refresh and try again."
    };
}
```

---

### 13. ⚠️ **No Audit Trail for Updates**
**Severity**: 🟡 MEDIUM  
**Location**: FeatG82_FloodReportUpdate  

**Issue**:
No tracking of who changed what and when:
- Can't see edit history
- Can't rollback changes
- No compliance audit trail

**Recommended**:
```csharp
// Create audit table:
public class FloodReportAudit
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public string Operation { get; set; } // "CREATE", "UPDATE", "DELETE"
    public string FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime ChangedAt { get; set; }
}

// In update handler:
await _auditRepository.LogUpdateAsync(
    reportId: report.Id,
    userId: request.UserId,
    changes: new[] 
    { 
        new { Field = "Description", Old = report.Description, New = request.Description }
    }, ct);
```

---

## 📝 DOCUMENTATION ISSUES

### 14. ❌ **Missing Endpoint Routing**
**Severity**: 🔴 CRITICAL  
**Location**: Missing endpoints in FastEndpoints layer  

**Issue**:
FeatG82 and FeatG85 handlers created but NO corresponding endpoints:
- No `PUT /api/v1/flood-reports/{id}` endpoint
- No `DELETE /api/v1/flood-reports/{id}` endpoint

**Required Files**:
```
src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/
└── Endpoints/
    ├── FeatG82_FloodReportUpdate/
    │   ├── UpdateFloodReportEndpoint.cs
    │   └── UpdateFloodReportRequest.cs (DTO mapping)
    └── FeatG85_FloodReportDelete/
        └── DeleteFloodReportEndpoint.cs
```

---

### 15. ⚠️ **FE-25 Document Incomplete**
**Severity**: 🟡 MEDIUM  
**Location**: `/documents/FE-25-Report-Flood-Points.md`  

**Missing Sections**:
- [ ] PUT endpoint specification for update
- [ ] DELETE endpoint specification
- [ ] Endpoint integration examples
- [ ] Error scenarios for update/delete
- [ ] Auth/authorization requirements

---

## 🔧 CODE QUALITY ISSUES

### 16. ⚠️ **No Logging in Handlers**
**Severity**: 🟡 MEDIUM  
**Location**: All handlers  

**Issue**:
Zero logging makes debugging production issues impossible:
```csharp
public async Task<UpdateFloodReportResponse> Handle(
    UpdateFloodReportRequest request, CancellationToken ct)
{
    // No logging! What happened after 3 minutes of processing?
    var report = await _reportRepository.GetByIdAsync(request.Id, ct);
    // ...
}
```

**Fix**:
```csharp
private readonly ILogger<UpdateFloodReportHandler> _logger;

public async Task<UpdateFloodReportResponse> Handle(
    UpdateFloodReportRequest request, CancellationToken ct)
{
    _logger.LogInformation(
        "Updating report {ReportId} by user {UserId}", 
        request.Id, request.UserId);
    
    try
    {
        var report = await _reportRepository.GetByIdAsync(request.Id, ct);
        if (report == null)
        {
            _logger.LogWarning(
                "Report {ReportId} not found for update", 
                request.Id);
            return NotFound();
        }
        
        // ... update logic ...
        
        _logger.LogInformation(
            "Report {ReportId} updated successfully", 
            request.Id);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex,
            "Error updating report {ReportId}", 
            request.Id);
        throw;
    }
}
```

---

## 📊 SUMMARY TABLE

| # | Issue | Severity | Type | Status |
|---|-------|----------|------|--------|
| 1 | Missing FloodReportMediaRepository | 🔴 CRITICAL | Code | ⏳ TODO |
| 2 | Missing Repository Methods | 🔴 CRITICAL | Code | ⏳ TODO |
| 3 | Cannot Update Location | 🔴 CRITICAL | Design | ⏳ TODO |
| 4 | Media Files Not Cleaned | 🟠 HIGH | Code | ⏳ TODO |
| 5 | Authorization not aligned | 🟠 MEDIUM | Architecture | ⏳ TODO |
| 6 | No transaction support | 🟠 MEDIUM | Code | ⏳ TODO |
| 7 | No soft delete | 🟡 MEDIUM | Design | 💡 Optional |
| 8 | XSS vulnerability | 🔴 HIGH | Security | ⏳ TODO |
| 9 | Incomplete validation | 🟠 MEDIUM | Code | ⏳ TODO |
| 10 | Query performance | 🟡 MEDIUM | Performance | 💡 Optimize |
| 11 | N+1 queries | 🟡 MEDIUM | Performance | 💡 Optimize |
| 12 | No optimistic locking | 🟡 MEDIUM | Design | 💡 Optional |
| 13 | No audit trail | 🟡 MEDIUM | Design | 💡 Optional |
| 14 | Missing endpoints | 🔴 CRITICAL | Code | ⏳ TODO |
| 15 | Incomplete docs | 🟡 MEDIUM | Documentation | ⏳ TODO |
| 16 | No logging | 🟡 MEDIUM | Code | ⏳ TODO |

---

## ✅ NEXT STEPS (Priority Order)

1. **Create missing endpoints** (FeatG82, FeatG85)
2. **Implement repository interfaces** (IFloodReportMediaRepository methods)
3. **Add XSS sanitization** to Description/Address fields
4. **Implement media cleanup** on delete
5. **Add transactional support** to create operation
6. **Improve validator** for UpdateFloodReportRequest
7. **Allow location updates** with validation
8. **Add logging** throughout handlers
9. **Optimize queries** with spatial indexes and caching
10. **Consider audit trail** (nice-to-have)

---

**Report Generated**: 2026-02-26  
**Reviewed By**: Code Analysis System  
**Status**: Ready for Dev Team Action
