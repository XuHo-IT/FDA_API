# FE-13: Receive Priority Flood Notifications

> **Status**: Partially Implemented (G42, G43 with PriorityRoutingService)  
> **Priority**: HIGH (Premium feature differentiation)  
> **Estimated Effort**: 2-3 days (refine + testing)

---

## 📋 **1. BUSINESS REQUIREMENTS**

### **Goal**
Prioritize flood alert delivery based on user subscription tier and alert severity, ensuring critical users (Authority/Premium) receive faster notifications through premium channels.

### **Key Features**
1. **Tier-Based Priority**: Authority > Premium > Free
2. **Priority Channels**:
   - **Authority**: SMS + High-priority Push + Email (all channels)
   - **Premium**: SMS (critical only) + Email + Standard Push
   - **Free**: Standard Push only
3. **Faster Delivery**: Priority alerts dispatched first in queue
4. **Visual Distinction**: High-priority push notifications have distinct styling
5. **Priority Templates**: Different message templates per tier/priority

### **Business Rules**
┌──────────────┬─────────────┬─────────────────────────────────┐
│ User Tier │ Severity │ Channels │
├──────────────┼─────────────┼─────────────────────────────────┤
│ Authority │ Critical │ SMS + High-Priority Push + Email│
│ │ Warning │ SMS + High-Priority Push + Email│
│ │ Caution │ High-Priority Push + Email │
│ │ Info │ Push + Email │
├──────────────┼─────────────┼─────────────────────────────────┤
│ Premium │ Critical │ SMS + Push + Email │
│ │ Warning │ Push + Email │
│ │ Caution │ Push + Email │
│ │ Info │ Push │
├──────────────┼─────────────┼─────────────────────────────────┤
│ Free │ Critical │ Push │
│ │ Warning │ Push │
│ │ Caution │ Push (delayed 5 min) │
│ │ Info │ In-App only (no push) │
└──────────────┴─────────────┴─────────────────────────────────┘


---

## 🗄️ **2. DATABASE ANALYSIS & CHANGES**

### **Existing Enums (Already Perfect!)** ✅

#### **SubscriptionTier Enum**
public enum SubscriptionTier
{
    Free = 0,       // Basic users
    Premium = 1,    // Paid subscribers
    Authority = 2   // Government officials
}**Location**: `FDAAPI.Domain.RelationalDb.Enums.SubscriptionTier`
**Status**: ✅ Already exists

---

#### **NotificationPriority Enum**
public enum NotificationPriority
{
    Low = 0,        // Normal updates, push only
    Medium = 1,     // Caution level, push + email
    High = 2,       // Warning level, push + email + SMS (premium)
    Critical = 3    // Critical flooding, all channels + retry
}**Location**: `FDAAPI.Domain.RelationalDb.Enums.NotificationPriority`
**Status**: ✅ Already exists

---

#### **NotificationChannel Enum**
public enum NotificationChannel
{
    Push = 1,       // Mobile push notification
    Email = 2,      // Email notification
    SMS = 3,        // SMS (premium feature)
    InApp = 4       // In-app notification bell
}**Status**: ✅ Already exists

---

### **Existing Entities**

#### **Alert Entity** ✅
public class Alert
{
    // ... other fields ...
    public string Severity { get; set; } = "info"; // info, caution, warning, critical
    public NotificationPriority Priority { get; set; } = NotificationPriority.Low; // ✅ Priority field exists!
}**Status**: ✅ NO CHANGES NEEDED

---

#### **NotificationLog Entity** ✅
public class NotificationLog
{
    // ... other fields ...
    public NotificationChannel Channel { get; set; }
    public NotificationPriority Priority { get; set; }  // ✅ Priority field exists!
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
}**Status**: ✅ NO CHANGES NEEDED

---

### ⚠️ **Missing: User Subscription Tier Tracking**

**Problem**: Currently, there's no way to determine user's subscription tier!

**Current State**:
// In DispatchNotificationsHandler.cs:68
var userTier = SubscriptionTier.Free; // ❌ TODO: Get from user_subscriptions table**Solution**: Add `user_subscriptions` table (from db.md schema):
sharp
public class UserSubscription : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
{
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "active"; // active, expired, cancelled
    public string RenewMode { get; set; } = "manual"; // manual, auto
    public string? CancelReason { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    [ForeignKey(nameof(UserId))]
    [JsonIgnore]
    public virtual User? User { get; set; }
    
    [ForeignKey(nameof(PlanId))]
    [JsonIgnore]
    public virtual PricingPlan? Plan { get; set; }
}

public class PricingPlan : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
{
    public string Code { get; set; } = string.Empty; // FREE, PREMIUM, AUTHORITY
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PriceMonth { get; set; }
    public decimal PriceYear { get; set; }
    public SubscriptionTier Tier { get; set; } = SubscriptionTier.Free; // ✅ Link to tier
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    [JsonIgnore]
    public virtual ICollection<UserSubscription>? UserSubscriptions { get; set; }
}**Migration**:
dotnet ef migrations add AddUserSubscriptionsAndPricingPlans**Configuration**:
// AppDbContext
public DbSet<UserSubscription> UserSubscriptions { get; set; }
public DbSet<PricingPlan> PricingPlans { get; set; }

// Configuration
modelBuilder.Entity<PricingPlan>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasIndex(e => e.Code).IsUnique();
    
    // Seed default plans
    entity.HasData(
        new PricingPlan
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Code = "FREE",
            Name = "Free Plan",
            Description = "Basic flood alerts",
            PriceMonth = 0,
            PriceYear = 0,
            Tier = SubscriptionTier.Free,
            IsActive = true,
            SortOrder = 1,
            CreatedBy = Guid.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = Guid.Empty,
            UpdatedAt = DateTime.UtcNow
        },
        new PricingPlan
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Code = "PREMIUM",
            Name = "Premium Plan",
            Description = "Priority alerts + SMS",
            PriceMonth = 9.99m,
            PriceYear = 99.99m,
            Tier = SubscriptionTier.Premium,
            IsActive = true,
            SortOrder = 2,
            CreatedBy = Guid.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = Guid.Empty,
            UpdatedAt = DateTime.UtcNow
        },
        new PricingPlan
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Code = "AUTHORITY",
            Name = "Authority Access",
            Description = "Government officials - all channels",
            PriceMonth = 0,
            PriceYear = 0,
            Tier = SubscriptionTier.Authority,
            IsActive = true,
            SortOrder = 3,
            CreatedBy = Guid.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = Guid.Empty,
            UpdatedAt = DateTime.UtcNow
        }
    );
});

modelBuilder.Entity<UserSubscription>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasIndex(e => new { e.UserId, e.Status });
    
    entity.HasOne(e => e.User)
        .WithMany()
        .HasForeignKey(e => e.UserId)
        .OnDelete(DeleteBehavior.Cascade);
    
    entity.HasOne(e => e.Plan)
        .WithMany(p => p.UserSubscriptions)
        .HasForeignKey(e => e.PlanId)
        .OnDelete(DeleteBehavior.Restrict);
});**Status**: 🔴 **REQUIRED - Must implement**

---

### **Recommended: Add Priority Dispatch Delay Table**

For implementing delayed notifications for Free users:

public class PriorityDispatchConfig : EntityWithId<Guid>
{
    public SubscriptionTier Tier { get; set; }
    public NotificationPriority Priority { get; set; }
    public int DelaySeconds { get; set; } = 0; // 0 = immediate, >0 = delayed
    public int MaxRetriesOverride { get; set; } = 3; // Override default retries
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}**Seed Data**:
entity.HasData(
    // Authority - immediate delivery
    new { Id = Guid.NewGuid(), Tier = SubscriptionTier.Authority, Priority = NotificationPriority.Critical, DelaySeconds = 0, MaxRetriesOverride = 5 },
    new { Id = Guid.NewGuid(), Tier = SubscriptionTier.Authority, Priority = NotificationPriority.High, DelaySeconds = 0, MaxRetriesOverride = 5 },
    
    // Premium - immediate for high priority
    new { Id = Guid.NewGuid(), Tier = SubscriptionTier.Premium, Priority = NotificationPriority.Critical, DelaySeconds = 0, MaxRetriesOverride = 3 },
    new { Id = Guid.NewGuid(), Tier = SubscriptionTier.Premium, Priority = NotificationPriority.High, DelaySeconds = 30, MaxRetriesOverride = 3 },
    
    // Free - delayed delivery
    new { Id = Guid.NewGuid(), Tier = SubscriptionTier.Free, Priority = NotificationPriority.Critical, DelaySeconds = 60, MaxRetriesOverride = 1 },
    new { Id = Guid.NewGuid(), Tier = SubscriptionTier.Free, Priority = NotificationPriority.Medium, DelaySeconds = 300, MaxRetriesOverride = 1 },
    new { Id = Guid.NewGuid(), Tier = SubscriptionTier.Free, Priority = NotificationPriority.Low, DelaySeconds = 600, MaxRetriesOverride = 0 }
);**Status**: 🟡 **OPTIONAL but RECOMMENDED**

---

## 🏗️ **3. ARCHITECTURE DESIGN**

### **Existing Implementation Analysis**

#### ✅ **PriorityRoutingService (Already Good!)**

**File**: `FDAAPI.Infra.Services/Notifications/PriorityRoutingService.cs`

public class PriorityRoutingService : IPriorityRoutingService
{
    // ✅ Maps severity + tier → priority
    public NotificationPriority DeterminePriority(string severity, SubscriptionTier userTier)
    {
        if (userTier == SubscriptionTier.Authority)
        {
            return severity.ToLower() switch
            {
                "critical" => NotificationPriority.Critical,
                "warning" => NotificationPriority.High,
                _ => NotificationPriority.Medium
            };
        }
        
        if (userTier == SubscriptionTier.Premium)
        {
            return severity.ToLower() switch
            {
                "critical" => NotificationPriority.High,
                "warning" => NotificationPriority.Medium,
                _ => NotificationPriority.Low
            };
        }
        
        // Free users
        return severity.ToLower() switch
        {
            "critical" => NotificationPriority.Medium,
            "warning" => NotificationPriority.Low,
            _ => NotificationPriority.Low
        };
    }
    
    // ✅ Returns available channels per tier
    public List<NotificationChannel> GetChannelsForPriority(
        NotificationPriority priority, 
        SubscriptionTier userTier)
    {
        var channels = new List<NotificationChannel>
        {
            NotificationChannel.Push,
            NotificationChannel.InApp
        };
        
        // Premium and Authority get Email
        if (userTier >= SubscriptionTier.Premium)
            channels.Add(NotificationChannel.Email);
        
        // High priority + Premium/Authority get SMS
        if (priority >= NotificationPriority.High && userTier >= SubscriptionTier.Premium)
            channels.Add(NotificationChannel.SMS);
        
        return channels;
    }
}**Status**: ✅ **Logic is sound, but needs refinement**

---

### ⚠️ **Issues to Fix**

#### **Problem 1: User Tier Not Fetched from Database**

**Current Code** (DispatchNotificationsHandler.cs:68):
var userTier = SubscriptionTier.Free; // ❌ Hardcoded!**Solution**: Fetch from UserSubscriptions table:arp
// Add method to IUserRepository
Task<SubscriptionTier> GetUserTierAsync(Guid userId, CancellationToken ct);

// Implementation
public async Task<SubscriptionTier> GetUserTierAsync(Guid userId, CancellationToken ct)
{
    var activeSubscription = await _context.UserSubscriptions
        .Where(s => s.UserId == userId && 
                    s.Status == "active" && 
                    s.EndDate > DateTime.UtcNow)
        .OrderByDescending(s => s.EndDate)
        .Include(s => s.Plan)
        .FirstOrDefaultAsync(ct);
    
    return activeSubscription?.Plan?.Tier ?? SubscriptionTier.Free;
}---

#### **Problem 2: Priority Queue Not Implemented**

**Current Code**: NotificationLogs processed in FIFO order (CreatedAt ASC)

**Issue**: Authority alerts sent same time as Free user alerts!

**Solution**: Order by Priority DESC, then CreatedAt ASC:sharp
// In PgsqlNotificationLogRepository.GetPendingAndRetryNotificationsAsync()
public async Task<List<NotificationLog>> GetPendingAndRetryNotificationsAsync(int limit, CancellationToken ct)
{
    return await _context.NotificationLogs
        .Where(n => 
            n.Status == "pending" || 
            (n.Status == "pending_retry" && n.UpdatedAt <= DateTime.UtcNow)
        )
        .OrderByDescending(n => n.Priority) // ✅ Priority first!
        .ThenBy(n => n.CreatedAt)           // ✅ Then FIFO
        .Take(limit)
        .Include(n => n.User)
        .Include(n => n.Alert)
        .ToListAsync(ct);
}---

#### **Problem 3: No Delayed Dispatch for Free Users**

**Requirement**: Free users with "Caution" severity should be delayed 5 minutes

**Solution**: Add delay logic when creating NotificationLog:
// In DispatchNotificationsHandler - when creating NotificationLog
var dispatchDelay = await GetDispatchDelayAsync(userTier, priority, ct);

var notificationLog = new NotificationLog
{
    // ... other fields ...
    Status = dispatchDelay > 0 ? "pending_delayed" : "pending",
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow.AddSeconds(dispatchDelay) // ✅ Delayed dispatch time
};Then modify query to respect UpdatedAt:
.Where(n => 
    (n.Status == "pending" && n.UpdatedAt <= DateTime.UtcNow) || // ✅ Check delay
    (n.Status == "pending_retry" && n.UpdatedAt <= DateTime.UtcNow)
)---

#### **Problem 4: Push Notification Priority Not Sent to FCM**

**Current Code** (PushNotificationService.cs):
public async Task<bool> SendPushNotificationAsync(
    string deviceToken,
    string title,
    string body,
    Dictionary<string, string>? data = null,
    CancellationToken ct = default)
{
    // ❌ No priority parameter!
}**Solution**: Add priority parameter and map to FCM priority:
public async Task<bool> SendPushNotificationAsync(
    string deviceToken,
    string title,
    string body,
    NotificationPriority priority, // ✅ Add priority
    Dictionary<string, string>? data = null,
    CancellationToken ct = default)
{
    // Map to FCM priority
    string fcmPriority = priority >= NotificationPriority.High ? "high" : "normal";
    
    // TODO: Call FCM API with priority
    // fcmMessage.Android.Priority = fcmPriority;
    // fcmMessage.APNS.Headers["apns-priority"] = priority >= NotificationPriority.High ? "10" : "5";
}---

### **Recommended Architecture Flow**
Location: FDAAPI.Domain.RelationalDb.Enums.SubscriptionTier
Status: ✅ Already exists
NotificationPriority Enum
---## 📝 **4. STEP-BY-STEP CODING PLAN**### **STEP 1: Database Migration****Create entities & repositories**:
Location: FDAAPI.Domain.RelationalDb.Enums.NotificationPriority
Status: ✅ Already exists

NotificationChannel Enum

**Migration**:
dotnet ef migrations add AddPriorityNotificationTables \
  --project "src/Core/Domain/FDAAPI.Domain.RelationalDb/FDAAPI.Domain.RelationalDb.csproj" \
  --startup-project "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj"---

### **STEP 2: Implement Repository Methods**

**File**: `FDAAPI.Infra.Persistence/Repositories/PgsqlUserSubscriptionRepository.cs` (NEW)

public class PgsqlUserSubscriptionRepository : IUserSubscriptionRepository
{
    private readonly AppDbContext _context;
    
    public PgsqlUserSubscriptionRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<SubscriptionTier> GetUserTierAsync(Guid userId, CancellationToken ct)
    {
        var activeSubscription = await _context.UserSubscriptions
            .Where(s => s.UserId == userId && 
                        s.Status == "active" && 
                        s.EndDate > DateTime.UtcNow)
            .OrderByDescending(s => s.EndDate)
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(ct);
        
        return activeSubscription?.Plan?.Tier ?? SubscriptionTier.Free;
    }
    
    public async Task<UserSubscription?> GetActiveSubscriptionAsync(Guid userId, CancellationToken ct)
    {
        return await _context.UserSubscriptions
            .Where(s => s.UserId == userId && 
                        s.Status == "active" && 
                        s.EndDate > DateTime.UtcNow)
            .Include(s => s.Plan)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(ct);
    }
}**Register in ServiceExtensions.cs**:
services.AddScoped<IUserSubscriptionRepository, PgsqlUserSubscriptionRepository>();---

### **STEP 3: Refine PriorityRoutingService**

**File**: `FDAAPI.Infra.Services/Notifications/PriorityRoutingService.cs`

**Add new method** for dispatch delay:
public int GetDispatchDelaySeconds(SubscriptionTier userTier, NotificationPriority priority)
{
    // Authority - immediate
    if (userTier == SubscriptionTier.Authority)
        return 0;
    
    // Premium - immediate for high priority
    if (userTier == SubscriptionTier.Premium)
    {
        return priority >= NotificationPriority.High ? 0 : 30; // 30sec delay for low priority
    }
    
    // Free - delayed based on priority
    return priority switch
    {
        NotificationPriority.Critical => 60,    // 1 min
        NotificationPriority.High => 180,       // 3 min
        NotificationPriority.Medium => 300,     // 5 min
        NotificationPriority.Low => 600,        // 10 min
        _ => 0
    };
}

public int GetMaxRetriesForTier(SubscriptionTier userTier, NotificationPriority priority)
{
    if (userTier == SubscriptionTier.Authority && priority >= NotificationPriority.High)
        return 5; // More retries for critical government alerts
    
    if (userTier == SubscriptionTier.Premium)
        return 3;
    
    // Free users - limited retries
    return priority >= NotificationPriority.High ? 1 : 0;
}---

### **STEP 4: Update DispatchNotificationsHandler**

**File**: `FDAAPI.App.FeatG43_DispatchNotifications/DispatchNotificationsHandler.cs`

**Inject new dependencies**:
public class DispatchNotificationsHandler : IRequestHandler<DispatchNotificationsRequest, DispatchNotificationsResponse>
{
    private readonly IUserSubscriptionRepository _subscriptionRepo; // ✅ NEW
    // ... existing repos ...
}**Update logic**:
foreach (var subscription in subscriptions)
{
    var user = await _userRepo.GetByIdAsync(subscription.UserId, ct);
    if (user == null) continue;
    
    // ===== FETCH USER TIER FROM DATABASE ===== ✅
    var userTier = await _subscriptionRepo.GetUserTierAsync(user.Id, ct);
    
    // Determine priority based on severity + tier
    var priority = _routingService.DeterminePriority(alert.Severity, userTier);
    
    // Get channels for this tier
    var availableChannels = _routingService.GetChannelsForPriority(priority, userTier);
    var userChannels = FilterChannelsByPreferences(availableChannels, subscription);
    
    // ===== CALCULATE DISPATCH DELAY ===== ✅
    var dispatchDelay = _routingService.GetDispatchDelaySeconds(userTier, priority);
    var maxRetries = _routingService.GetMaxRetriesForTier(userTier, priority);
    
    foreach (var channel in userChannels)
    {
        var notificationLog = new NotificationLog
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AlertId = alert.Id,
            Channel = channel,
            Priority = priority, // ✅ Store priority
            Destination = GetDestination(user, channel),
            Content = await _templateService.GenerateContentAsync(alert, userTier, ct),
            Status = dispatchDelay > 0 ? "pending_delayed" : "pending",
            RetryCount = 0,
            MaxRetries = maxRetries, // ✅ Tier-specific retries
            CreatedBy = Guid.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = Guid.Empty,
            UpdatedAt = DateTime.UtcNow.AddSeconds(dispatchDelay) // ✅ Delayed dispatch
        };
        
        await _notificationLogRepo.CreateAsync(notificationLog, ct);
        created++;
    }
}---

### **STEP 5: Update NotificationDispatchService**

**File**: `FDAAPI.Infra.Services/Notifications/NotificationDispatchService.cs`

**Pass priority to Push service**:
case NotificationChannel.Push:
    success = await _pushService.SendPushNotificationAsync(
        notificationLog.Destination,
        "Flood Alert",
        notificationLog.Content,
        notificationLog.Priority, // ✅ Pass priority
        null,
        ct);
    break;---

### **STEP 6: Update PushNotificationService**

**File**: `FDAAPI.Infra.Services/Notifications/PushNotificationService.cs`

**Add priority parameter**:arp
public async Task<bool> SendPushNotificationAsync(
    string deviceToken,
    string title,
    string body,
    NotificationPriority priority, // ✅ NEW
    Dictionary<string, string>? data = null,
    CancellationToken ct = default)
{
    try
    {
        // Map to FCM priority
        string fcmPriority = priority >= NotificationPriority.High ? "high" : "normal";
        
        _logger.LogInformation(
            "Sending {Priority} priority push to device: {DeviceToken}",
            fcmPriority, deviceToken);
        
        // TODO: Implement FCM with priority
        // var message = new FirebaseAdmin.Messaging.Message
        // {
        //     Token = deviceToken,
        //     Notification = new Notification { Title = title, Body = body },
        //     Android = new AndroidConfig
        //     {
        //         Priority = priority >= NotificationPriority.High 
        //             ? Priority.High 
        //             : Priority.Normal
        //     },
        //     Apns = new ApnsConfig
        //     {
        //         Headers = new Dictionary<string, string>
        //         {
        //             ["apns-priority"] = priority >= NotificationPriority.High ? "10" : "5"
        //         }
        //     }
        // };
        
        await Task.Delay(100, ct);
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to send push notification");
        return false;
    }
}---

### **STEP 7: Update Repository Query (Priority Queue)**

**File**: `FDAAPI.Infra.Persistence/Repositories/PgsqlNotificationLogRepository.cs`

**Modify query**:
public async Task<List<NotificationLog>> GetPendingAndRetryNotificationsAsync(int limit, CancellationToken ct)
{
    return await _context.NotificationLogs
        .Where(n => 
            (n.Status == "pending" && n.UpdatedAt <= DateTime.UtcNow) || // ✅ Respect delay
            (n.Status == "pending_delayed" && n.UpdatedAt <= DateTime.UtcNow) || // ✅ Delayed
            (n.Status == "pending_retry" && n.UpdatedAt <= DateTime.UtcNow)
        )
        .OrderByDescending(n => n.Priority) // ✅ PRIORITY FIRST!
        .ThenBy(n => n.CreatedAt)
        .Take(limit)
        .Include(n => n.User)
        .Include(n => n.Alert)
        .ToListAsync(ct);
}---

### **STEP 8: Update NotificationTemplateService**

**File**: `FDAAPI.Infra.Services/Notifications/NotificationTemplateService.cs`

**Add tier-specific templates**:sharp
public async Task<string> GenerateContentAsync(
    Alert alert, 
    SubscriptionTier userTier, 
    CancellationToken ct = default)
{
    var station = alert.Station?.Name ?? "your area";
    var severity = alert.Severity.ToUpper();
    
    // Tier-specific messaging
    string prefix = userTier switch
    {
        SubscriptionTier.Authority => "🚨 [AUTHORITY ALERT] ",
        SubscriptionTier.Premium => "⚠️ [PRIORITY] ",
        _ => ""
    };
    
    string actionText = userTier switch
    {
        SubscriptionTier.Authority => "Immediate action may be required. Check dashboard for details.",
        SubscriptionTier.Premium => "Priority alert - check app for details.",
        _ => "Check app for more information."
    };
    
    return $"{prefix}{severity} FLOOD ALERT: Water level at {station} has reached {alert.CurrentValue}m. {actionText}";
}---

## 🧪 **5. TEST CASES**

### **TEST CASE 1: Authority User - Critical Alert (All Channels, Immediate)**

**Setup**:
-- Create Authority user subscription
INSERT INTO "PricingPlans" ("Id", "Code", "Name", "Tier")
VALUES ('33333333-3333-3333-3333-333333333333', 'AUTHORITY', 'Authority Access', 2);

INSERT INTO "UserSubscriptions" ("Id", "UserId", "PlanId", "StartDate", "EndDate", "Status")
VALUES (
    'sub-authority', 
    'user-gov-001', 
    '33333333-3333-3333-3333-333333333333', 
    NOW(), 
    NOW() + INTERVAL '1 year', 
    'active'
);

-- Create critical alert
INSERT INTO "Alerts" ("Id", "StationId", "Severity", "Priority", "CurrentValue", "Status")
VALUES ('alert-critical-001', 'station-abc', 'critical', 3, 4.5, 'open');**Expected**:
- ✅ User tier fetched as `Authority`
- ✅ Priority determined as `Critical`
- ✅ 3 NotificationLogs created: SMS + Push (high priority) + Email
- ✅ All dispatched immediately (dispatchDelay = 0)
- ✅ MaxRetries = 5 for Authority
- ✅ Push notification sent with `fcmPriority = "high"`

**Verification**:
SELECT 
    "Channel", 
    "Priority", 
    "Status", 
    "MaxRetries",
    EXTRACT(EPOCH FROM ("UpdatedAt" - "CreatedAt")) AS "DelaySeconds"
FROM "NotificationLogs"
WHERE "AlertId" = 'alert-critical-001'
ORDER BY "Priority" DESC, "CreatedAt";

-- Expected:
-- Channel | Priority | Status  | MaxRetries | DelaySeconds
-- SMS     | 3        | pending | 5          | 0
-- Push    | 3        | pending | 5          | 0
-- Email   | 3        | pending | 5          | 0---

### **TEST CASE 2: Premium User - Warning Alert (Push + Email, Immediate)**

**Setup**:
INSERT INTO "UserSubscriptions" ("Id", "UserId", "PlanId", "StartDate", "EndDate", "Status")
VALUES (
    'sub-premium', 
    'user-premium-001', 
    '22222222-2222-2222-2222-222222222222', 
    NOW(), 
    NOW() + INTERVAL '1 month', 
    'active'
);

INSERT INTO "Alerts" ("Id", "StationId", "Severity", "Priority", "CurrentValue", "Status")
VALUES ('alert-warning-001', 'station-xyz', 'warning', 2, 3.2, 'open');**Expected**:
- ✅ User tier = `Premium`
- ✅ Priority = `Medium`
- ✅ 2 NotificationLogs: Push + Email (NO SMS for warning)
- ✅ Dispatched immediately (delay = 0)
- ✅ MaxRetries = 3

---

### **TEST CASE 3: Free User - Critical Alert (Push only, 1 min delay)**

**Setup**:
-- Free user (no active subscription)
INSERT INTO "Users" ("Id", "Email") VALUES ('user-free-001', 'free@example.com');

INSERT INTO "Alerts" ("Id", "StationId", "Severity", "Priority", "CurrentValue", "Status")
VALUES ('alert-critical-free', 'station-abc', 'critical', 1, 4.0, 'open');**Expected**:
- ✅ User tier = `Free` (default)
- ✅ Priority = `Medium` (downgraded for Free users)
- ✅ 1 NotificationLog: Push only
- ✅ Delayed 60 seconds (dispatchDelay = 60)
- ✅ MaxRetries = 1 (limited for Free)
- ✅ Status = "pending_delayed" initially

**Verification**:
SELECT 
    "Status",
    EXTRACT(EPOCH FROM ("UpdatedAt" - "CreatedAt")) AS "DelaySeconds"
FROM "NotificationLogs"
WHERE "AlertId" = 'alert-critical-free';

-- Expected:
-- Status          | DelaySeconds
-- pending_delayed | 60---

### **TEST CASE 4: Free User - Caution Alert (Delayed 5 min)**

**Setup**:
INSERT INTO "Alerts" ("Id", "StationId", "Severity", "Priority", "CurrentValue", "Status")
VALUES ('alert-caution-free', 'station-xyz', 'caution', 0, 2.5, 'open');**Expected**:
- ✅ Priority = `Low`
- ✅ Delayed 300 seconds (5 minutes)
- ✅ MaxRetries = 0 (no retries for low priority Free)

---

### **TEST CASE 5: Priority Queue (Multiple Users, Different Tiers)**

**Setup**:
-- Create 3 alerts at same time
INSERT INTO "Alerts" VALUES 
    ('alert-1', 'station-abc', NOW(), 'critical', 3, 'open'), -- Authority user
    ('alert-2', 'station-abc', NOW(), 'warning', 2, 'open'),  -- Premium user
    ('alert-3', 'station-abc', NOW(), 'critical', 1, 'open'); -- Free user

-- Trigger DispatchNotificationsJob**Expected Dispatch Order**:
Status: ✅ Already exists
Expected Dispatch Order:
1. Alert-1 (Authority, Priority=3, Delay=0) → Dispatched first
2. Alert-2 (Premium, Priority=2, Delay=0) → Dispatched second
3. Alert-3 (Free, Priority=1, Delay=60) → Dispatched after 1 min
Verification:
SELECT 
    "AlertId",
    "Priority",
    "Status",
    "SentAt"
FROM "NotificationLogs"
ORDER BY "SentAt" ASC NULLS LAST;

-- SentAt should show Authority sent first, then Premium, then Free (after delay)
6. MOBILE APP INTEGRATION (Visual Distinction)
Push Notification Payload
High Priority (Authority/Premium Critical):
{  "notification": {    "title": "🚨 [CRITICAL ALERT]",    "body": "Severe flooding detected at Station ABC - 4.5m",    "sound": "emergency.wav",    "badge": 1,    "color": "#FF0000"  },  "android": {    "priority": "high",    "notification": {      "channelId": "flood_critical",      "importance": "high",      "visibility": "public"    }  },  "apns": {    "headers": {      "apns-priority": "10"    },    "payload": {      "aps": {        "sound": "critical.wav",        "badge": 1,        "interruption-level": "critical"      }    }  },  "data": {    "alert_id": "alert-123",    "priority": "critical",    "tier": "authority",    "action": "OPEN_MAP"  }}
Standard Priority (Free users):
{  "notification": {    "title": "Flood Alert",    "body": "Water level rising at Station ABC",    "sound": "default",    "badge": 1  },  "android": {    "priority": "normal"  },  "apns": {    "headers": {      "apns-priority": "5"    }  },  "data": {    "alert_id": "alert-456",    "priority": "low",    "tier": "free"  }}
🎨 7. UI/UX RECOMMENDATIONS (for Mobile Team)
Notification Styling by Tier
// Flutter exampleclass FloodAlertNotification extends StatelessWidget {  final String tier;  final String priority;    @override  Widget build(BuildContext context) {    Color backgroundColor;    IconData icon;        switch (tier) {      case 'authority':        backgroundColor = Colors.red[900]!;        icon = Icons.emergency;        break;      case 'premium':        backgroundColor = Colors.orange[700]!;        icon = Icons.warning_amber;        break;      default:        backgroundColor = Colors.blue[600]!;        icon = Icons.info;    }        return Container(      decoration: BoxDecoration(        color: backgroundColor,        border: tier == 'authority'           ? Border.all(color: Colors.yellow, width: 2)           : null      ),      child: ListTile(        leading: Icon(icon, color: Colors.white),        title: Text(alert.title),        subtitle: Text(alert.body),        trailing: tier == 'authority'           ? Chip(label: Text('URGENT'))           : null      )    );  }}
✅ 8. ACCEPTANCE CRITERIA
Must Have:
[ ] User tier fetched from user_subscriptions table
[ ] Authority users receive SMS + High-priority Push + Email for critical alerts
[ ] Premium users receive SMS only for critical, Push + Email for warning
[ ] Free users receive standard Push only
[ ] Priority queue: Authority dispatched first, then Premium, then Free
[ ] Free users' non-critical alerts delayed (1-10 minutes)
[ ] High-priority push notifications sent with fcmPriority = "high"
[ ] Tier-specific retry limits (Authority=5, Premium=3, Free=0-1)
Nice to Have:
[ ] Admin dashboard showing tier distribution of alert recipients
[ ] A/B testing different priority thresholds
[ ] User upgrade prompt when hitting Free tier limits
🚨 9. RISKS & MITIGATION
Risk	Impact	Mitigation
Authority users don't have active subscription	High	Seed Authority role users with permanent subscription
SMS cost explosion	High	Rate limit SMS to max 100/hour, use batch sends
Free users complain about delays	Medium	Show "Priority Access" upsell in-app during delay
Push priority ignored by OS	Medium	Test on real devices (Android/iOS), document behavior
📊 10. MONITORING QUERIES
Priority Distribution
SELECT 
    nl."Priority",
    COUNT(*) AS "Total",
    COUNT(CASE WHEN nl."Status" = 'sent' THEN 1 END) AS "Sent",
    AVG(EXTRACT(EPOCH FROM (nl."SentAt" - nl."CreatedAt"))) AS "AvgDeliverySeconds"
FROM "NotificationLogs" nl
WHERE nl."CreatedAt" >= NOW() - INTERVAL '24 hours'
GROUP BY nl."Priority"
ORDER BY nl."Priority" DESC;

Tier Performance
SELECT 
    p."Tier",
    COUNT(DISTINCT nl."UserId") AS "UniqueUsers",
    COUNT(*) AS "TotalNotifications",
    AVG(EXTRACT(EPOCH FROM (nl."SentAt" - nl."CreatedAt"))) AS "AvgDeliverySeconds"
FROM "NotificationLogs" nl
JOIN "UserSubscriptions" us ON nl."UserId" = us."UserId"
JOIN "PricingPlans" p ON us."PlanId" = p."Id"
WHERE nl."CreatedAt" >= NOW() - INTERVAL '7 days'
  AND us."Status" = 'active'
GROUP BY p."Tier"
ORDER BY p."Tier" DESC;