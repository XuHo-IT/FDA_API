# FE-14: Customize Alert Thresholds (Mobile)

> **Status**: NOT IMPLEMENTED (New Feature)  
> **Priority**: MEDIUM-HIGH (User personalization feature)  
> **Estimated Effort**: 4-5 days (Full implementation + testing)

---

## 📋 **1. BUSINESS REQUIREMENTS**

### **Goal**
Allow users to set custom water level thresholds for their monitored areas, overriding global AlertRules with personalized alert triggers.

### **Key Features**
1. **Per-Area Customization**: Each user can set different thresholds for each of their areas
2. **Threshold Hierarchy**: Custom threshold > Global AlertRule
3. **Multiple Severity Levels**: User can set thresholds for info, caution, warning, critical
4. **Validation**: Min/max limits, must be increasing (info < caution < warning < critical)
5. **Reset to Default**: Option to remove custom threshold and use global settings
6. **Mobile-First UI**: Easy slider/input for threshold adjustment

### **Use Cases**

**Scenario 1: Homeowner near river**
- Global warning threshold: 2.5m
- User's house floods at 2.0m
- User sets custom threshold: 2.0m (warning)
- Result: User gets alert earlier than other users

**Scenario 2: Business owner**
- Global critical threshold: 3.5m
- User's shop is elevated, safe until 4.0m
- User sets custom threshold: 4.0m (critical)
- Result: User avoids false alarms

**Scenario 3: Different areas, different risks**
- User has 2 areas:
  - Home (low elevation): Custom threshold 1.8m
  - Office (high elevation): Custom threshold 3.0m
- Result: Personalized alerts per location

---

## 🗄️ **2. DATABASE ANALYSIS & CHANGES**

### **Problem: No Custom Threshold Storage**

**Current State**: Only global `AlertRules` exist per stationsharp
public class AlertRule // Global for all users
{
    public Guid StationId { get; set; }
    public decimal ThresholdValue { get; set; } // One threshold for everyone
}**Issue**: All users get alerts at same threshold!

---

### **Solution: Add UserAreaThreshold Entity**

/// <summary>
/// User's custom alert thresholds per area
/// Overrides global AlertRule if set
/// </summary>
public class UserAreaThreshold : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
{
    public Guid UserId { get; set; }
    public Guid AreaId { get; set; }
    public Guid StationId { get; set; } // Redundant but useful for querying
    
    // Custom thresholds (nullable = use global default)
    public decimal? InfoThreshold { get; set; }      // e.g., 1.5m
    public decimal? CautionThreshold { get; set; }   // e.g., 2.0m
    public decimal? WarningThreshold { get; set; }   // e.g., 2.5m
    public decimal? CriticalThreshold { get; set; }  // e.g., 3.5m
    
    // Metadata
    public string Unit { get; set; } = "m"; // meters, cm, ft
    public bool IsActive { get; set; } = true;
    public DateTime? LastAlertAt { get; set; } // Track last alert for this custom threshold
    
    // Audit
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    [ForeignKey(nameof(UserId))]
    [JsonIgnore]
    public virtual User? User { get; set; }
    
    [ForeignKey(nameof(AreaId))]
    [JsonIgnore]
    public virtual Area? Area { get; set; }
    
    [ForeignKey(nameof(StationId))]
    [JsonIgnore]
    public virtual Station? Station { get; set; }
}**Key Design Decisions**:
- ✅ **Per-Area**: Different thresholds for different monitored areas
- ✅ **Nullable thresholds**: If null, use global default
- ✅ **Multiple severity levels**: User can customize each level
- ✅ **Station reference**: Fast lookup when processing alerts

---

### **Entity Configuration**

public class UserAreaThresholdConfiguration : IEntityTypeConfiguration<UserAreaThreshold>
{
    public void Configure(EntityTypeBuilder<UserAreaThreshold> builder)
    {
        builder.HasKey(e => e.Id);
        
        // Properties
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.AreaId).IsRequired();
        builder.Property(e => e.StationId).IsRequired();
        
        builder.Property(e => e.InfoThreshold)
            .HasPrecision(10, 2)
            .IsRequired(false);
        
        builder.Property(e => e.CautionThreshold)
            .HasPrecision(10, 2)
            .IsRequired(false);
        
        builder.Property(e => e.WarningThreshold)
            .HasPrecision(10, 2)
            .IsRequired(false);
        
        builder.Property(e => e.CriticalThreshold)
            .HasPrecision(10, 2)
            .IsRequired(false);
        
        builder.Property(e => e.Unit)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("m");
        
        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        // Indexes
        builder.HasIndex(e => new { e.UserId, e.AreaId })
            .IsUnique() // One custom threshold per user per area
            .HasDatabaseName("IX_UserAreaThreshold_UserId_AreaId");
        
        builder.HasIndex(e => e.StationId)
            .HasDatabaseName("IX_UserAreaThreshold_StationId");
        
        builder.HasIndex(e => new { e.UserId, e.IsActive })
            .HasDatabaseName("IX_UserAreaThreshold_UserId_Active");
        
        // Relationships
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Area)
            .WithMany()
            .HasForeignKey(e => e.AreaId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Station)
            .WithMany()
            .HasForeignKey(e => e.StationId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Audit fields
        builder.Property(e => e.CreatedBy).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        
        // Check constraints (validation at DB level)
        builder.HasCheckConstraint(
            "CK_UserAreaThreshold_ValidRange",
            @"""InfoThreshold"" IS NULL OR 
              ""CautionThreshold"" IS NULL OR 
              ""InfoThreshold"" < ""CautionThreshold"""
        );
        
        builder.HasCheckConstraint(
            "CK_UserAreaThreshold_ValidWarning",
            @"""WarningThreshold"" IS NULL OR 
              ""CautionThreshold"" IS NULL OR 
              ""CautionThreshold"" < ""WarningThreshold"""
        );
        
        builder.HasCheckConstraint(
            "CK_UserAreaThreshold_ValidCritical",
            @"""CriticalThreshold"" IS NULL OR 
              ""WarningThreshold"" IS NULL OR 
              ""WarningThreshold"" < ""CriticalThreshold"""
        );
    }
}---

### **AppDbContext Update**

public class AppDbContext : DbContext
{
    // ... existing DbSets ...
    public DbSet<UserAreaThreshold> UserAreaThresholds { get; set; } // ✅ NEW
}---

### **Migration**

dotnet ef migrations add AddUserAreaThresholdsTable \
  --project "src/Core/Domain/FDAAPI.Domain.RelationalDb/FDAAPI.Domain.RelationalDb.csproj" \
  --startup-project "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj"**Expected SQL**:
CREATE TABLE "UserAreaThresholds" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "AreaId" uuid NOT NULL,
    "StationId" uuid NOT NULL,
    "InfoThreshold" numeric(10,2) NULL,
    "CautionThreshold" numeric(10,2) NULL,
    "WarningThreshold" numeric(10,2) NULL,
    "CriticalThreshold" numeric(10,2) NULL,
    "Unit" varchar(10) NOT NULL DEFAULT 'm',
    "IsActive" boolean NOT NULL DEFAULT true,
    "LastAlertAt" timestamp with time zone NULL,
    "CreatedBy" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedBy" uuid NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_UserAreaThresholds" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_UserAreaThresholds_Users" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserAreaThresholds_Areas" FOREIGN KEY ("AreaId") REFERENCES "Areas" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserAreaThresholds_Stations" FOREIGN KEY ("StationId") REFERENCES "Stations" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_UserAreaThreshold_ValidRange" CHECK ("InfoThreshold" IS NULL OR "CautionThreshold" IS NULL OR "InfoThreshold" < "CautionThreshold"),
    CONSTRAINT "CK_UserAreaThreshold_ValidWarning" CHECK ("WarningThreshold" IS NULL OR "CautionThreshold" IS NULL OR "CautionThreshold" < "WarningThreshold"),
    CONSTRAINT "CK_UserAreaThreshold_ValidCritical" CHECK ("CriticalThreshold" IS NULL OR "WarningThreshold" IS NULL OR "WarningThreshold" < "CriticalThreshold")
);

CREATE UNIQUE INDEX "IX_UserAreaThreshold_UserId_AreaId" ON "UserAreaThresholds" ("UserId", "AreaId");
CREATE INDEX "IX_UserAreaThreshold_StationId" ON "UserAreaThresholds" ("StationId");
CREATE INDEX "IX_UserAreaThreshold_UserId_Active" ON "UserAreaThresholds" ("UserId", "IsActive");---

## 🏗️ **3. ARCHITECTURE DESIGN**

### **Threshold Resolution Logic**
csharp
public class AlertRule // Global for all users
{
public Guid StationId { get; set; }
public decimal ThresholdValue { get; set; } // One threshold for everyone
}

**Issue**: All users get alerts at same threshold!

---

### **Solution: Add UserAreaThreshold Entity**
user-specific (not global)
3. Need to track which threshold triggered the alert (custom vs global)

---

### **Proposed Alert Entity Enhancement**

**Option 1: Add fields to existing Alert**
public class Alert
{
    // ... existing fields ...
    public Guid? TriggeredByUserThresholdId { get; set; } // NULL = global, Guid = custom
    public Guid? AffectedUserId { get; set; } // NEW: Which user this alert is for
    
    [ForeignKey(nameof(TriggeredByUserThresholdId))]
    [JsonIgnore]
    public virtual UserAreaThreshold? UserThreshold { get; set; }
}**Option 2: Create UserAlert entity (1:1 with Alert)**
public class UserAlert : EntityWithId<Guid>
{
    public Guid AlertId { get; set; }
    public Guid UserId { get; set; }
    public Guid? CustomThresholdId { get; set; } // NULL = used global threshold
    public string ThresholdSource { get; set; } = "global"; // global, custom
    public decimal ThresholdUsed { get; set; }
    
    [ForeignKey(nameof(AlertId))]
    public virtual Alert? Alert { get; set; }
    
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}**Recommendation**: **Option 2** (UserAlert entity)
- ✅ Cleaner separation: Alert = event, UserAlert = user-specific reaction
- ✅ One alert can trigger multiple UserAlerts (different thresholds)
- ✅ Better for history queries

---

## 📝 **4. STEP-BY-STEP CODING PLAN**

### **STEP 1: Database Migration**

**Create entities**:
csharp
/// <summary>
/// User's custom alert thresholds per area
/// Overrides global AlertRule if set
/// </summary>
public class UserAreaThreshold : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
{
public Guid UserId { get; set; }
public Guid AreaId { get; set; }
public Guid StationId { get; set; } // Redundant but useful for querying
// Custom thresholds (nullable = use global default)
public decimal? InfoThreshold { get; set; } // e.g., 1.5m
public decimal? CautionThreshold { get; set; } // e.g., 2.0m
public decimal? WarningThreshold { get; set; } // e.g., 2.5m
public decimal? CriticalThreshold { get; set; } // e.g., 3.5m
// Metadata
public string Unit { get; set; } = "m"; // meters, cm, ft
public bool IsActive { get; set; } = true;
public DateTime? LastAlertAt { get; set; } // Track last alert for this custom threshold
// Audit
public Guid CreatedBy { get; set; }
public DateTime CreatedAt { get; set; }
public Guid UpdatedBy { get; set; }
public DateTime UpdatedAt { get; set; }
// Navigation properties
[ForeignKey(nameof(UserId))]
[JsonIgnore]
public virtual User? User { get; set; }
[ForeignKey(nameof(AreaId))]
[JsonIgnore]
public virtual Area? Area { get; set; }
[ForeignKey(nameof(StationId))]
[JsonIgnore]
public virtual Station? Station { get; set; }
}


**Key Design Decisions**:
- ✅ **Per-Area**: Different thresholds for different monitored areas
- ✅ **Nullable thresholds**: If null, use global default
- ✅ **Multiple severity levels**: User can customize each level
- ✅ **Station reference**: Fast lookup when processing alerts

---

### **Entity Configuration**

public class UserAreaThresholdConfiguration : IEntityTypeConfiguration<UserAreaThreshold>
{
    public void Configure(EntityTypeBuilder<UserAreaThreshold> builder)
    {
        builder.HasKey(e => e.Id);
        
        // Properties
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.AreaId).IsRequired();
        builder.Property(e => e.StationId).IsRequired();
        
        builder.Property(e => e.InfoThreshold)
            .HasPrecision(10, 2)
            .IsRequired(false);
        
        builder.Property(e => e.CautionThreshold)
            .HasPrecision(10, 2)
            .IsRequired(false);
        
        builder.Property(e => e.WarningThreshold)
            .HasPrecision(10, 2)
            .IsRequired(false);
        
        builder.Property(e => e.CriticalThreshold)
            .HasPrecision(10, 2)
            .IsRequired(false);
        
        builder.Property(e => e.Unit)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("m");
        
        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        // Indexes
        builder.HasIndex(e => new { e.UserId, e.AreaId })
            .IsUnique() // One custom threshold per user per area
            .HasDatabaseName("IX_UserAreaThreshold_UserId_AreaId");
        
        builder.HasIndex(e => e.StationId)
            .HasDatabaseName("IX_UserAreaThreshold_StationId");
        
        builder.HasIndex(e => new { e.UserId, e.IsActive })
            .HasDatabaseName("IX_UserAreaThreshold_UserId_Active");
        
        // Relationships
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Area)
            .WithMany()
            .HasForeignKey(e => e.AreaId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Station)
            .WithMany()
            .HasForeignKey(e => e.StationId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Audit fields
        builder.Property(e => e.CreatedBy).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        
        // Check constraints (validation at DB level)
        builder.HasCheckConstraint(
            "CK_UserAreaThreshold_ValidRange",
            @"""InfoThreshold"" IS NULL OR 
              ""CautionThreshold"" IS NULL OR 
              ""InfoThreshold"" < ""CautionThreshold"""
        );
        
        builder.HasCheckConstraint(
            "CK_UserAreaThreshold_ValidWarning",
            @"""WarningThreshold"" IS NULL OR 
              ""CautionThreshold"" IS NULL OR 
              ""CautionThreshold"" < ""WarningThreshold"""
        );
        
        builder.HasCheckConstraint(
            "CK_UserAreaThreshold_ValidCritical",
            @"""CriticalThreshold"" IS NULL OR 
              ""WarningThreshold"" IS NULL OR 
              ""WarningThreshold"" < ""CriticalThreshold"""
        );
    }
}
HasValue || 
                      x.WarningThreshold.Value < x.CriticalThreshold.Value)
            .WithMessage("Warning threshold must be less than Critical threshold");
        
        // Range validation
        When(x => x.InfoThreshold.HasValue, () =>
        {
            RuleFor(x => x.InfoThreshold)
                .GreaterThan(0).WithMessage("Threshold must be positive")
                .LessThan(100).WithMessage("Threshold seems unrealistic (max 100m)");
        });
    }
}**Handler**:
public class SaveCustomThresholdHandler : IRequestHandler<SaveCustomThresholdRequest, SaveCustomThresholdResponse>
{
    private readonly IUserAreaThresholdRepository _thresholdRepo;
    private readonly IAreaRepository _areaRepo;
    
    public SaveCustomThresholdHandler(
        IUserAreaThresholdRepository thresholdRepo,
        IAreaRepository areaRepo)
    {
        _thresholdRepo = thresholdRepo;
        _areaRepo = areaRepo;
    }
    
    public async Task<SaveCustomThresholdResponse> Handle(
        SaveCustomThresholdRequest request, 
        CancellationToken ct)
    {
        // 1. Verify area belongs to user
        var area = await _areaRepo.GetByIdAsync(request.AreaId, ct);
        if (area == null)
        {
            return new SaveCustomThresholdResponse
            {
                Success = false,
                Message = "Area not found"
            };
        }
        
        if (area.UserId != request.UserId)
        {
            return new SaveCustomThresholdResponse
            {
                Success = false,
                Message = "Area does not belong to user"
            };
        }
        
        // 2. Check if custom threshold already exists
        var existing = await _thresholdRepo.GetByUserAndAreaAsync(
            request.UserId, 
            request.AreaId, 
            ct);
        
        if (existing != null)
        {
            // Update existing
            existing.InfoThreshold = request.InfoThreshold;
            existing.CautionThreshold = request.CautionThreshold;
            existing.WarningThreshold = request.WarningThreshold;
            existing.CriticalThreshold = request.CriticalThreshold;
            existing.Unit = request.Unit;
            existing.UpdatedBy = request.UserId;
            existing.UpdatedAt = DateTime.UtcNow;
            
            await _thresholdRepo.UpdateAsync(existing, ct);
            
            return new SaveCustomThresholdResponse
            {
                Success = true,
                Message = "Custom threshold updated",
                ThresholdId = existing.Id
            };
        }
        else
        {
            // Create new
            var threshold = new UserAreaThreshold
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                AreaId = request.AreaId,
                StationId = area.StationId, // Assuming Area has StationId
                InfoThreshold = request.InfoThreshold,
                CautionThreshold = request.CautionThreshold,
                WarningThreshold = request.WarningThreshold,
                CriticalThreshold = request.CriticalThreshold,
                Unit = request.Unit,
                IsActive = true,
                CreatedBy = request.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = request.UserId,
                UpdatedAt = DateTime.UtcNow
            };
            
            var id = await _thresholdRepo.CreateAsync(threshold, ct);
            
            return new SaveCustomThresholdResponse
            {
                Success = true,
                Message = "Custom threshold created",
                ThresholdId = id
            };
        }
    }
}---

### **STEP 4: Create Feature - Get Custom Threshold**

#### **FeatG48_GetCustomThreshold**

**Request**:
public sealed record GetCustomThresholdRequest(
    Guid UserId,
    Guid AreaId
) : IFeatureRequest<GetCustomThresholdResponse>;**Handler**:sharp
public class GetCustomThresholdHandler : IRequestHandler<GetCustomThresholdRequest, GetCustomThresholdResponse>
{
    private readonly IUserAreaThresholdRepository _thresholdRepo;
    private readonly IAlertRuleRepository _alertRuleRepo;
    
    public async Task<GetCustomThresholdResponse> Handle(
        GetCustomThresholdRequest request, 
        CancellationToken ct)
    {
        // 1. Get custom threshold if exists
        var customThreshold = await _thresholdRepo.GetByUserAndAreaAsync(
            request.UserId, 
            request.AreaId, 
            ct);
        
        // 2. Get global default thresholds for comparison
        var globalRules = await _alertRuleRepo.GetByStationIdAsync(
            customThreshold?.StationId ?? Guid.Empty, 
            ct);
        
        var response = new GetCustomThresholdResponse
        {
            Success = true,
            HasCustomThreshold = customThreshold != null,
            CustomThreshold = customThreshold != null ? new ThresholdDto
            {
                InfoThreshold = customThreshold.InfoThreshold,
                CautionThreshold = customThreshold.CautionThreshold,
                WarningThreshold = customThreshold.WarningThreshold,
                CriticalThreshold = customThreshold.CriticalThreshold,
                Unit = customThreshold.Unit
            } : null,
            GlobalThreshold = new ThresholdDto
            {
                // Map from AlertRules
                WarningThreshold = globalRules.FirstOrDefault(r => r.Severity == "warning")?.ThresholdValue,
                CriticalThreshold = globalRules.FirstOrDefault(r => r.Severity == "critical")?.ThresholdValue,
                Unit = "m"
            }
        };
        
        return response;
    }
}---

### **STEP 5: Modify ProcessAlertsHandler (Custom Threshold Support)**

**Current logic**: Check global AlertRule only
**New logic**: Check custom threshold per user first

public class ProcessAlertsHandler : IRequestHandler<ProcessAlertsRequest, ProcessAlertsResponse>
{
    private readonly IUserAreaThresholdRepository _customThresholdRepo; // ✅ NEW
    private readonly IUserAlertSubscriptionRepository _subscriptionRepo; // ✅ NEW
    // ... existing repos ...
    
    public async Task<ProcessAlertsResponse> Handle(ProcessAlertsRequest request, CancellationToken ct)
    {
        // 1. Get latest sensor readings
        var readings = await _sensorRepo.GetLatestReadingsByStationsAsync(allStations, ct);
        
        foreach (var reading in readings)
        {
            // 2. Get all users subscribed to this station
            var subscriptions = await _subscriptionRepo.GetByStationIdAsync(reading.StationId, ct);
            
            foreach (var subscription in subscriptions)
            {
                // ===== CHECK CUSTOM THRESHOLD FIRST ===== ✅
                var customThreshold = await _customThresholdRepo.GetByUserAndAreaAsync(
                    subscription.UserId, 
                    subscription.AreaId ?? Guid.Empty, 
                    ct);
                
                string? exceededSeverity = null;
                decimal? thresholdUsed = null;
                
                if (customThreshold != null)
                {
                    // Use custom threshold
                    if (customThreshold.CriticalThreshold.HasValue && 
                        reading.Value >= (double)customThreshold.CriticalThreshold.Value)
                    {
                        exceededSeverity = "critical";
                        thresholdUsed = customThreshold.CriticalThreshold.Value;
                    }
                    else if (customThreshold.WarningThreshold.HasValue && 
                             reading.Value >= (double)customThreshold.WarningThreshold.Value)
                    {
                        exceededSeverity = "warning";
                        thresholdUsed = customThreshold.WarningThreshold.Value;
                    }
                    // ... check caution, info ...
                }
                else
                {
                    // Fallback to global AlertRule
                    var globalRule = await _alertRuleRepo.GetActiveRuleByStationAndSeverityAsync(
                        reading.StationId, 
                        "warning", // or loop through severities
                        ct);
                    
                    if (globalRule != null && reading.Value >= (double)globalRule.ThresholdValue)
                    {
                        exceededSeverity = globalRule.Severity;
                        thresholdUsed = globalRule.ThresholdValue;
                    }
                }
                
                // 3. Create user-specific alert if threshold exceeded
                if (exceededSeverity != null)
                {
                    var userAlert = new Alert
                    {
                        // ... standard alert fields ...
                        Severity = exceededSeverity,
                        CurrentValue = (decimal)reading.Value,
                        Message = $"Water level {reading.Value}{reading.Unit} exceeded your custom threshold of {thresholdUsed}m"
                    };
                    
                    await _alertRepo.CreateAsync(userAlert, ct);
                }
            }
        }
        
        return new ProcessAlertsResponse { Success = true };
    }
}---

### **STEP 6: Create Endpoints**

#### **Endpoint 1: Save Custom Threshold**sharp
// POST /api/v1/areas/{areaId}/threshold
public class SaveCustomThresholdEndpoint : Endpoint<SaveCustomThresholdRequestDto, SaveCustomThresholdResponseDto>
{
    private readonly IMediator _mediator;
    
    public SaveCustomThresholdEndpoint(IMediator mediator) => _mediator = mediator;
    
    public override void Configure()
    {
        Post("/api/v1/areas/{areaId}/threshold");
        Policies("User");
        Summary(s =>
        {
            s.Summary = "Set custom alert threshold for area";
            s.Description = "Allows users to override global thresholds with personalized values";
            s.ExampleRequest = new SaveCustomThresholdRequestDto
            {
                InfoThreshold = 1.5m,
                CautionThreshold = 2.0m,
                WarningThreshold = 2.5m,
                CriticalThreshold = 3.5m,
                Unit = "m"
            };
        });
        Tags("Alert Customization");
    }
    
    public override async Task HandleAsync(SaveCustomThresholdRequestDto req, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        var areaId = Route<Guid>("areaId");
        
        var command = new SaveCustomThresholdRequest(
            userId,
            areaId,
            req.InfoThreshold,
            req.CautionThreshold,
            req.WarningThreshold,
            req.CriticalThreshold,
            req.Unit ?? "m"
        );
        
        var result = await _mediator.Send(command, ct);
        
        var response = new SaveCustomThresholdResponseDto
        {
            Success = result.Success,
            Message = result.Message,
            ThresholdId = result.ThresholdId
        };
        
        await SendAsync(response, result.Success ? 200 : 400, ct);
    }
}#### **Endpoint 2: Get Custom Threshold**
// GET /api/v1/areas/{areaId}/threshold
public class GetCustomThresholdEndpoint : EndpointWithoutRequest<GetCustomThresholdResponseDto>
{
    private readonly IMediator _mediator;
    
    public GetCustomThresholdEndpoint(IMediator mediator) => _mediator = mediator;
    
    public override void Configure()
    {
        Get("/api/v1/areas/{areaId}/threshold");
        Policies("User");
        Summary(s =>
        {
            s.Summary = "Get custom threshold for area";
            s.Description = "Returns user's custom threshold and global defaults";
        });
        Tags("Alert Customization");
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        var areaId = Route<Guid>("areaId");
        
        var query = new GetCustomThresholdRequest(userId, areaId);
        var result = await _mediator.Send(query, ct);
        
        var response = new GetCustomThresholdResponseDto
        {
            Success = result.Success,
            HasCustomThreshold = result.HasCustomThreshold,
            CustomThreshold = result.CustomThreshold,
            GlobalThreshold = result.GlobalThreshold
        };
        
        await SendAsync(response, 200, ct);
    }
}#### **Endpoint 3: Delete Custom Threshold (Reset to Default)**
// DELETE /api/v1/areas/{areaId}/threshold
public class DeleteCustomThresholdEndpoint : EndpointWithoutRequest
{
    private readonly IUserAreaThresholdRepository _thresholdRepo;
    
    public override void Configure()
    {
        Delete("/api/v1/areas/{areaId}/threshold");
        Policies("User");
        Summary(s =>
        {
            s.Summary = "Reset to global threshold";
            s.Description = "Removes custom threshold, user will receive alerts based on global settings";
        });
        Tags("Alert Customization");
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        var areaId = Route<Guid>("areaId");
        
        var threshold = await _thresholdRepo.GetByUserAndAreaAsync(userId, areaId, ct);
        
        if (threshold != null)
        {
            await _thresholdRepo.DeleteAsync(threshold.Id, ct);
            await SendAsync(new { success = true, message = "Custom threshold removed" }, 200, ct);
        }
        else
        {
            await SendAsync(new { success = false, message = "No custom threshold found" }, 404, ct);
        }
    }
}---

## 🧪 **5. TEST CASES**

### **TEST CASE 1: Save Custom Threshold (New)**

**Request**:
curl -X POST http://localhost:5000/api/v1/areas/area-123/threshold \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "infoThreshold": 1.5,
    "cautionThreshold": 2.0,
    "warningThreshold": 2.5,
    "criticalThreshold": 3.5,
    "unit": "m"
  }'**Expected Response (200 OK)**:
{
  "success": true,
  "message": "Custom threshold created",
  "thresholdId": "threshold-uuid-123"
}**DB Verification**:
SELECT * FROM "UserAreaThresholds" 
WHERE "UserId" = 'user-456' AND "AreaId" = 'area-123';

-- Should show:
-- InfoThreshold: 1.5
-- CautionThreshold: 2.0
-- WarningThreshold: 2.5
-- CriticalThreshold: 3.5---

### **TEST CASE 2: Update Existing Custom Threshold**

**Request**: (Same as Test Case 1, but threshold already exists)
curl -X POST http://localhost:5000/api/v1/areas/area-123/threshold \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "warningThreshold": 3.0,
    "criticalThreshold": 4.0
  }'**Expected**:
- ✅ Existing record updated (not duplicated)
- ✅ Response: "Custom threshold updated"
- ✅ DB: Only 1 row exists for user+area

---

### **TEST CASE 3: Validation Error - Thresholds Not Increasing**

**Request**:
curl -X POST http://localhost:5000/api/v1/areas/area-123/threshold \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "cautionThreshold": 3.0,
    "warningThreshold": 2.0
  }'**Expected Response (400 Bad Request)**:
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    {
      "field": "request",
      "message": "Caution threshold must be less than Warning threshold"
    }
  ]
}---

### **TEST CASE 4: Alert Triggered by Custom Threshold (Lower than Global)**

**Setup**:
-- Global warning threshold: 3.0m
INSERT INTO "AlertRules" ("StationId", "Severity", "ThresholdValue")
VALUES ('station-abc', 'warning', 3.0);

-- User's custom warning threshold: 2.0m (earlier alert!)
INSERT INTO "UserAreaThresholds" 
VALUES ('user-123', 'area-456', 'station-abc', NULL, NULL, 2.0, NULL);

-- Sensor reading: 2.5m (exceeds custom, not global)
INSERT INTO "SensorReadings" VALUES ('station-abc', 2.5, NOW());**Expected**:
- ✅ Alert created for user-123 at 2.5m (custom threshold)
- ✅ Other users DON'T get alert (global threshold 3.0m not exceeded)
- ✅ Alert message mentions "exceeded your custom threshold of 2.0m"

---

### **TEST CASE 5: Alert Triggered by Global (No Custom Set)**

**Setup**:
-- User has NO custom threshold
-- Global warning: 3.0m
-- Sensor reading: 3.5m**Expected**:
- ✅ Alert created using global threshold
- ✅ Alert message: "exceeded threshold 3.0m" (no mention of "custom")

---

### **TEST CASE 6: Get Custom Threshold (Returns Both Custom & Global)**

**Request**:
curl -X GET http://localhost:5000/api/v1/areas/area-123/threshold \
  -H "Authorization: Bearer $TOKEN"**Expected Response (200 OK)**:
{
  "success": true,
  "hasCustomThreshold": true,
  "customThreshold": {
    "infoThreshold": null,
    "cautionThreshold": 2.0,
    "warningThreshold": 2.5,
    "criticalThreshold": 3.5,
    "unit": "m"
  },
  "globalThreshold": {
    "infoThreshold": null,
    "cautionThreshold": null,
    "warningThreshold": 3.0,
    "criticalThreshold": 4.0,
    "unit": "m"
  }
}---

### **TEST CASE 7: Delete Custom Threshold (Reset to Default)**

**Request**:
curl -X DELETE http://localhost:5000/api/v1/areas/area-123/threshold \
  -H "Authorization: Bearer $TOKEN"**Expected Response (200 OK)**:
{
  "success": true,
  "message": "Custom threshold removed"
}**Verification**:
SELECT COUNT(*) FROM "UserAreaThresholds" 
WHERE "UserId" = 'user-123' AND "AreaId" = 'area-123';
-- Should return 0

-- Next alert should use global threshold---

## 📱 **6. MOBILE UI RECOMMENDATIONS**

### **UI Flow**

AppDbContext Update
public class AppDbContext : DbContext
{
    // ... existing DbSets ...
    public DbSet<UserAreaThreshold> UserAreaThresholds { get; set; } // ✅ NEW
}ate createState() => _CustomThresholdScreenState();
}

class _CustomThresholdScreenState extends State<CustomThresholdScreen> {
  double? warningThreshold = 2.5;
  double? criticalThreshold = 3.5;
  
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Alert Thresholds')),
      body: Column(
        children: [
          // Show global default for comparison
          InfoCard(
            title: 'Global Default',
            warning: '3.0m',
            critical: '4.0m'
          ),
          
          Divider(),
          
          // Custom threshold sliders
          Text('Your Custom Thresholds'),
          
          SliderWithLabel(
            label: 'Warning Level',
            value: warningThreshold ?? 2.5,
            min: 0.5,
            max: 10.0,
            divisions: 95,
            color: Colors.orange,
            onChanged: (value) {
              setState(() => warningThreshold = value);
            }
          ),
          
          SliderWithLabel(
            label: 'Critical Level',
            value: criticalThreshold ?? 3.5,
            min: 1.0,
            max: 10.0,
            divisions: 90,
            color: Colors.red,
            onChanged: (value) {
              setState(() => criticalThreshold = value);
            }
          ),
          
          SizedBox(height: 20),
          
          // Validation hint
          if (warningThreshold >= criticalThreshold)
            Text(
              '⚠️ Warning must be less than Critical',
              style: TextStyle(color: Colors.red)
            ),
          
          Spacer(),
          
          // Action buttons
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceEvenly,
            children: [
              OutlinedButton(
                onPressed: resetToDefault,
                child: Text('Reset to Default')
              ),
              ElevatedButton(
                onPressed: saveCustomThreshold,
                child: Text('Save')
              )
            ]
          )
        ]
      )
    );
  }
  
  Future<void> saveCustomThreshold() async {
    // API call to save
    final response = await api.post('/areas/${widget.areaId}/threshold', {
      'warningThreshold': warningThreshold,
      'criticalThreshold': criticalThreshold
    });
    
    if (response.success) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('✅ Custom threshold saved'))
      );
    }
  }
  
  Future<void> resetToDefault() async {
    await api.delete('/areas/${widget.areaId}/threshold');
    Navigator.pop(context);
  }
}---

## ✅ **7. ACCEPTANCE CRITERIA**

### **Must Have:**
- [ ] User can save custom thresholds per area
- [ ] Validation: Thresholds must be increasing (info < caution < warning < critical)
- [ ] Validation: Thresholds must be positive and < 100m
- [ ] ProcessAlertsHandler checks custom threshold before global
- [ ] Alert message indicates if custom or global threshold was used
- [ ] User can view both custom and global thresholds
- [ ] User can delete custom threshold (reset to default)
- [ ] API prevents user from setting threshold for area they don't own

### **Nice to Have:**
- [ ] Admin can see distribution of users using custom thresholds
- [ ] Suggest optimal threshold based on historical data
- [ ] Unit conversion (meters ↔ feet ↔ cm)

---

## 🚨 **8. RISKS & MITIGATION**

| Risk | Impact | Mitigation |
|------|--------|------------|
| Performance: ProcessAlertsHandler slow | High | Add index on (StationId, UserId), cache custom thresholds |
| User sets unrealistic threshold (100m) | Medium | Validation rules, show warning in UI |
| Custom threshold bypass global safety | High | Admin can set minimum threshold (e.g., critical > 5m always triggers) |
| Complex query logic | Medium | Unit tests for threshold resolution logic |

---

## 📊 **9. MONITORING QUERIES**

### **Custom Threshold Adoption Rate**
SELECT 
    COUNT(DISTINCT "UserId") AS "UsersWithCustomThresholds",
    (SELECT COUNT(*) FROM "Users") AS "TotalUsers",
    ROUND(100.0 * COUNT(DISTINCT "UserId") / (SELECT COUNT(*) FROM "Users"), 2) AS "AdoptionRate%"
FROM "UserAreaThresholds"
WHERE "IsActive" = true;### **Average Threshold Difference (Custom vs Global)**
SELECT 
    AVG(uat."WarningThreshold" - ar."ThresholdValue") AS "AvgDifference",
    COUNT(*) AS "TotalCustom"
FROM "UserAreaThresholds" uat
JOIN "AlertRules" ar ON uat."StationId" = ar."StationId" AND ar."Severity" = 'warning'
WHERE uat."WarningThreshold" IS NOT NULL;---

🏗️ 3. ARCHITECTURE DESIGN
Threshold Resolution Logic
┌───────────────────────────────────────────────────────┐
│  ProcessAlertsHandler                                  │
│  ├─ Get AlertRules (global thresholds)                │
│  ├─ Get SensorReading                                 │
│  ├─ For each AlertRule:                               │
│  │   ├─ Get affected users (via UserAlertSubscription)│
│  │   ├─ For each user:                                │
│  │   │   ├─ Check UserAreaThreshold (custom) ✅       │
│  │   │   ├─ If custom exists → use custom            │
│  │   │   ├─ Else → use global AlertRule              │
│  │   │   └─ Create Alert if threshold exceeded       │
│  └─ Continue to DispatchNotifications                 │
└───────────────────────────────────────────────────────┘
Key Changes:
ProcessAlertsHandler must check custom thresholds per user
Alerts become user-specific (not global)
Need to track which threshold triggered the alert (custom vs global)