# FE-12: Receive Flood Alerts (Core Alert Engine)

> **Status**: Partially Implemented (G42, G43)  
> **Priority**: HIGH (Core feature)  
> **Estimated Effort**: 3-4 days (refinement + testing)

---

## 📋 **1. BUSINESS REQUIREMENTS**

### **Goal**
Automatically trigger flood alerts when water levels exceed configured thresholds, with intelligent cooldown and retry mechanisms.

### **Key Features**
1. **Threshold Monitoring**: Check sensor readings against AlertRules
2. **Cooldown Logic**: Prevent alert spam (configurable interval, e.g., 10 minutes)
3. **Multi-Channel Delivery**: FCM Push, Email, SMS
4. **Retry Policy**: Exponential backoff for failed deliveries (max 3 retries)
5. **Alert History**: Store all alerts and delivery attempts

---

## 🗄️ **2. DATABASE ANALYSIS & CHANGES**

### **Existing Entities (Already Good!)**

#### **Alert Entity** ✅
public class Alert : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
{
    public Guid AlertRuleId { get; set; }
    public Guid StationId { get; set; }
    public DateTime TriggeredAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string Status { get; set; } = "open"; // open, resolved
    public string Severity { get; set; } = "info"; // info, caution, warning, critical
    public NotificationPriority Priority { get; set; } = NotificationPriority.Low;
    public decimal CurrentValue { get; set; }
    public string Message { get; set; } = string.Empty;
    
    // Cooldown tracking (ALREADY EXISTS!)
    public bool NotificationSent { get; set; } = false;
    public int NotificationCount { get; set; } = 0;
    public DateTime? LastNotificationAt { get; set; } // ✅ For cooldown logic
    
    // Relationships
    public virtual AlertRule? AlertRule { get; set; }
    public virtual Station? Station { get; set; }
    public virtual ICollection<NotificationLog>? NotificationLogs { get; set; }
}**Status**: ✅ **NO CHANGES NEEDED** - Schema is complete!

---

#### **NotificationLog Entity** ✅sharp
public class NotificationLog : EntityWithId<Guid>
{
    public Guid UserId { get; set; }
    public Guid AlertId { get; set; }
    public NotificationChannel Channel { get; set; } // Push, Email, SMS, InApp
    public string Destination { get; set; } = string.Empty; // phone/email/device_token
    public string Content { get; set; } = string.Empty;
    public NotificationPriority Priority { get; set; }
    
    // Retry tracking (ALREADY EXISTS!)
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
    public string Status { get; set; } = "pending"; // pending, sent, failed, delivered
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Relationships
    public virtual User? User { get; set; }
    public virtual Alert? Alert { get; set; }
}**Status**: ✅ **NO CHANGES NEEDED** - Retry fields exist!

---

#### **AlertRule Entity** ✅sharp
public class AlertRule : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
{
    public Guid StationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RuleType { get; set; } = "threshold"; // threshold, rate_change, duration
    public decimal ThresholdValue { get; set; }
    public int? DurationMin { get; set; } // Must exceed threshold for X minutes
    public string Severity { get; set; } = "warning"; // info, caution, warning, critical
    public bool IsActive { get; set; } = true;
    public SubscriptionTier MinTierRequired { get; set; } = SubscriptionTier.Free;
    
    // Relationships
    public virtual Station? Station { get; set; }
    public virtual ICollection<Alert>? Alerts { get; set; }
}**Status**: ✅ **NO CHANGES NEEDED**

---

### ⚠️ **RECOMMENDED: Add Cooldown Configuration Table**

While cooldown can be hardcoded, a config table allows runtime adjustments:

public class AlertCooldownConfig : EntityWithId<Guid>
{
    public string Severity { get; set; } = "warning"; // info, caution, warning, critical
    public int CooldownMinutes { get; set; } = 10; // Default cooldown
    public int MaxNotificationsPerHour { get; set; } = 6;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}**Migration**:
// Add to AppDbContext
public DbSet<AlertCooldownConfig> AlertCooldownConfigs { get; set; }

// Configuration
modelBuilder.Entity<AlertCooldownConfig>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasIndex(e => e.Severity).IsUnique();
    
    // Seed default values
    entity.HasData(
        new { Id = Guid.NewGuid(), Severity = "info", CooldownMinutes = 30, MaxNotificationsPerHour = 2 },
        new { Id = Guid.NewGuid(), Severity = "caution", CooldownMinutes = 20, MaxNotificationsPerHour = 3 },
        new { Id = Guid.NewGuid(), Severity = "warning", CooldownMinutes = 10, MaxNotificationsPerHour = 6 },
        new { Id = Guid.NewGuid(), Severity = "critical", CooldownMinutes = 5, MaxNotificationsPerHour = 12 }
    );
});**Status**: 🟡 **OPTIONAL but RECOMMENDED**

---

## 🏗️ **3. ARCHITECTURE DESIGN**

### **Current Implementation Analysis**

#### ✅ **What's Already Good:**

1. **Background Service Pattern** (AlertProcessingJob)
   - Runs every 2 minutes
   - Uses MediatR to ProcessAlertsHandler
   - Properly scoped services

2. **Separation of Concerns**
   - **ProcessAlertsHandler**: Creates/updates alerts based on sensor data
   - **DispatchNotificationsHandler**: Sends notifications for pending alerts
   - **NotificationDispatchService**: Strategy pattern for multi-channel delivery

3. **Priority Routing** (PriorityRoutingService)
   - Maps severity → priority
   - Routes channels based on user tier

---

### ⚠️ **What Needs Improvement:**

#### **Problem 1: Cooldown Logic Not Implemented**

**Current Code** (ProcessAlertsHandler.cs:58-66):
if (existingAlert != null)
{
    // Just updates value, NO cooldown check ❌
    existingAlert.CurrentValue = (decimal)latestReading.Value;
    existingAlert.UpdatedAt = DateTime.UtcNow;
    await _alertRepo.UpdateAsync(existingAlert, ct);
    updated++;
}**Issue**: Updates every 2 minutes, causing notification spam!

**Solution**: Add cooldown check:
if (existingAlert != null)
{
    // ===== COOLDOWN LOGIC =====
    var cooldownMinutes = GetCooldownMinutes(existingAlert.Severity); // 10 mins for warning
    var timeSinceLastNotification = DateTime.UtcNow - existingAlert.LastNotificationAt;
    
    if (timeSinceLastNotification.HasValue && 
        timeSinceLastNotification.Value.TotalMinutes < cooldownMinutes)
    {
        // Within cooldown period, skip notification
        existingAlert.CurrentValue = (decimal)latestReading.Value;
        existingAlert.UpdatedAt = DateTime.UtcNow;
        await _alertRepo.UpdateAsync(existingAlert, ct);
        continue; // Don't trigger new notification
    }
    
    // Cooldown expired, allow new notification
    existingAlert.NotificationSent = false; // Reset flag to allow DispatchNotifications to process
    existingAlert.CurrentValue = (decimal)latestReading.Value;
    await _alertRepo.UpdateAsync(existingAlert, ct);
}---

#### **Problem 2: Retry Logic Incomplete**

**Current Code** (DispatchNotificationsHandler.cs:117-141):
if (success)
{
    notificationLog.Status = "sent";
    notificationLog.SentAt = DateTime.UtcNow;
    sent++;
}
else
{
    notificationLog.Status = "failed";
    notificationLog.ErrorMessage = "Delivery failed";
    failed++;
}

await _notificationLogRepo.UpdateAsync(notificationLog, ct);**Issue**: No retry mechanism! Just marks as "failed" once.

**Solution**: Implement exponential backoff:arp
if (success)
{
    notificationLog.Status = "sent";
    notificationLog.SentAt = DateTime.UtcNow;
    sent++;
}
else
{
    // ===== RETRY LOGIC =====
    notificationLog.RetryCount++;
    
    if (notificationLog.RetryCount < notificationLog.MaxRetries)
    {
        notificationLog.Status = "pending_retry";
        notificationLog.ErrorMessage = $"Delivery failed. Retry {notificationLog.RetryCount}/{notificationLog.MaxRetries}";
        
        // Exponential backoff: 5min, 15min, 45min
        var retryDelayMinutes = (int)Math.Pow(3, notificationLog.RetryCount) * 5;
        notificationLog.UpdatedAt = DateTime.UtcNow.AddMinutes(retryDelayMinutes);
        
        _logger.LogWarning(
            "Notification failed. Will retry in {DelayMinutes} minutes. " +
            "Attempt {RetryCount}/{MaxRetries}",
            retryDelayMinutes, notificationLog.RetryCount, notificationLog.MaxRetries);
    }
    else
    {
        notificationLog.Status = "failed";
        notificationLog.ErrorMessage = $"Delivery failed after {notificationLog.MaxRetries} attempts";
        
        _logger.LogError(
            "Notification permanently failed for User {UserId}, Channel {Channel}",
            notificationLog.UserId, notificationLog.Channel);
    }
    
    failed++;
}---

#### **Problem 3: No Retry Job for "pending_retry" Notifications**

**Current State**: DispatchNotificationsHandler only processes `notificationLog.Status = "pending"` (new notifications).

**Solution**: Modify GetPendingNotifications to include retries:
// In INotificationLogRepository
Task<List<NotificationLog>> GetPendingAndRetryNotificationsAsync(int limit, CancellationToken ct);

// Implementation
public async Task<List<NotificationLog>> GetPendingAndRetryNotificationsAsync(int limit, CancellationToken ct)
{
    return await _context.NotificationLogs
        .Where(n => 
            n.Status == "pending" || 
            (n.Status == "pending_retry" && n.UpdatedAt <= DateTime.UtcNow) // Retry delay expired
        )
        .OrderBy(n => n.Priority).ThenBy(n => n.CreatedAt)
        .Take(limit)
        .Include(n => n.User)
        .Include(n => n.Alert)
        .ToListAsync(ct);
}---

### **Recommended Architecture Flow**
csharp
public class Alert : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
{
public Guid AlertRuleId { get; set; }
public Guid StationId { get; set; }
public DateTime TriggeredAt { get; set; }
public DateTime? ResolvedAt { get; set; }
public string Status { get; set; } = "open"; // open, resolved
public string Severity { get; set; } = "info"; // info, caution, warning, critical
public NotificationPriority Priority { get; set; } = NotificationPriority.Low;
public decimal CurrentValue { get; set; }
public string Message { get; set; } = string.Empty;
// Cooldown tracking (ALREADY EXISTS!)
public bool NotificationSent { get; set; } = false;
public int NotificationCount { get; set; } = 0;
public DateTime? LastNotificationAt { get; set; } // ✅ For cooldown logic
// Relationships
public virtual AlertRule? AlertRule { get; set; }
public virtual Station? Station { get; set; }
public virtual ICollection<NotificationLog>? NotificationLogs { get; set; }
}


**Status**: ✅ **NO CHANGES NEEDED** - Schema is complete!

---

#### **NotificationLog Entity** ✅sharp
public class NotificationLog : EntityWithId<Guid>
{
    public Guid UserId { get; set; }
    public Guid AlertId { get; set; }
    public NotificationChannel Channel { get; set; } // Push, Email, SMS, InApp
    public string Destination { get; set; } = string.Empty; // phone/email/device_token
    public string Content { get; set; } = string.Empty;
    public NotificationPriority Priority { get; set; }
    
    // Retry tracking (ALREADY EXISTS!)
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
    public string Status { get; set; } = "pending"; // pending, sent, failed, delivered
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Relationships
    public virtual User? User { get; set; }
    public virtual Alert? Alert { get; set; }
}
Status: ✅ NO CHANGES NEEDED - Retry fields exist!

AlertRule Entity ✅
public class AlertRule : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
{
    public Guid StationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RuleType { get; set; } = "threshold"; // threshold, rate_change, duration
    public decimal ThresholdValue { get; set; }
    public int? DurationMin { get; set; } // Must exceed threshold for X minutes
    public string Severity { get; set; } = "warning"; // info, caution, warning, critical
    public bool IsActive { get; set; } = true;
    public SubscriptionTier MinTierRequired { get; set; } = SubscriptionTier.Free;
    
    // Relationships
    public virtual Station? Station { get; set; }
    public virtual ICollection<Alert>? Alerts { get; set; }
}var cooldownExpired = !existingAlert.LastNotificationAt.HasValue ||
                          (DateTime.UtcNow - existingAlert.LastNotificationAt.Value).TotalMinutes >= cooldownMinutes;
    
    if (cooldownExpired)
    {
        // Allow new notification
        existingAlert.NotificationSent = false;
        _logger.LogInformation("Cooldown expired for Alert {AlertId}, allowing new notification", existingAlert.Id);
    }
    else
    {
        _logger.LogDebug("Alert {AlertId} within cooldown period, skipping notification", existingAlert.Id);
    }
    
    existingAlert.CurrentValue = (decimal)latestReading.Value;
    existingAlert.UpdatedAt = DateTime.UtcNow;
    await _alertRepo.UpdateAsync(existingAlert, ct);
    updated++;
}---

### **STEP 3: Refine DispatchNotificationsHandler (Add Retry Logic)**

**File**: `FDAAPI.App.FeatG43_DispatchNotifications/DispatchNotificationsHandler.cs`

**Changes**:
1. Modify to handle `pending_retry` status
2. Implement exponential backoff calculation
3. Update status to "pending_retry" or "failed" based on retry count

**Pseudocode**:
if (success)
{
    notificationLog.Status = "sent";
    notificationLog.SentAt = DateTime.UtcNow;
    sent++;
}
else
{
    notificationLog.RetryCount++;
    
    if (notificationLog.RetryCount < notificationLog.MaxRetries)
    {
        var retryDelay = CalculateExponentialBackoff(notificationLog.RetryCount); // 5min, 15min, 45min
        notificationLog.Status = "pending_retry";
        notificationLog.UpdatedAt = DateTime.UtcNow.AddMinutes(retryDelay);
        notificationLog.ErrorMessage = $"Retry {notificationLog.RetryCount}/{notificationLog.MaxRetries}";
        
        _logger.LogWarning("Scheduling retry in {DelayMinutes} minutes", retryDelay);
    }
    else
    {
        notificationLog.Status = "failed";
        notificationLog.ErrorMessage = "Max retries exceeded";
        
        _logger.LogError("Notification permanently failed: {LogId}", notificationLog.Id);
    }
    
    failed++;
}---

### **STEP 4: Update Repository Methods**

**File**: `FDAAPI.Infra.Persistence/Repositories/PgsqlNotificationLogRepository.cs`

**Add method**:
public async Task<List<NotificationLog>> GetPendingAndRetryNotificationsAsync(int limit, CancellationToken ct)
{
    return await _context.NotificationLogs
        .Where(n => 
            n.Status == "pending" || 
            (n.Status == "pending_retry" && n.UpdatedAt <= DateTime.UtcNow)
        )
        .OrderBy(n => n.Priority)
        .ThenBy(n => n.CreatedAt)
        .Take(limit)
        .Include(n => n.User)
        .Include(n => n.Alert)
        .ToListAsync(ct);
}**Update interface**: `INotificationLogRepository`

---

### **STEP 5: Test End-to-End**

**Test Scenarios**:
1. ✅ **Normal Alert Flow**: Sensor exceeds threshold → Alert created → Notification sent
2. ✅ **Cooldown Prevention**: Alert triggered again within 10 min → No new notification
3. ✅ **Cooldown Expiry**: After 10 min → New notification allowed
4. ✅ **Retry Success**: First attempt fails, 2nd succeeds after 5 min
5. ✅ **Retry Exhaustion**: 3 failures → Status "failed", no more retries
6. ✅ **Multi-User**: 10 users subscribed → All receive notifications

---

## 🔌 **5. API SPECIFICATIONS**

### **No New APIs Needed!**

FE-12 is primarily **backend engine logic**. Existing APIs already cover:

- **G39**: `POST /api/v1/alerts/subscribe` - Subscribe to alerts
- **G40**: `GET /api/v1/alerts/history` - View alert history
- **G41**: `PUT /api/v1/alerts/preferences` - Update preferences

**Optional Admin API** (for monitoring):
Status: ✅ NO CHANGES NEEDED


⚠️ RECOMMENDED: Add Cooldown Configuration Table
While cooldown can be hardcoded, a config table allows runtime adjustments:
public class AlertCooldownConfig : EntityWithId<Guid>
{
    public string Severity { get; set; } = "warning"; // info, caution, warning, critical
    public int CooldownMinutes { get; set; } = 10; // Default cooldown
    public int MaxNotificationsPerHour { get; set; } = 6;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}e);

-- Create user subscription
INSERT INTO "UserAlertSubscriptions" ("Id", "UserId", "StationId", "MinSeverity", "EnablePush", "EnableEmail")
VALUES ('sub-123', 'user-456', 'station-abc', 'warning', true, true);

-- Insert sensor reading that exceeds threshold
INSERT INTO "SensorReadings" ("Id", "StationId", "Value", "Unit", "MeasuredAt")
VALUES ('reading-789', 'station-abc', 3.2, 'm', NOW());**Expected**:
- ✅ Alert created with Status = "open", Severity = "warning"
- ✅ NotificationLog created for user (Push + Email channels)
- ✅ Notifications dispatched successfully
- ✅ Alert.NotificationSent = true, LastNotificationAt = NOW()

**Verification**:
SELECT * FROM "Alerts" WHERE "StationId" = 'station-abc' AND "Status" = 'open';
SELECT * FROM "NotificationLogs" WHERE "AlertId" = (SELECT "Id" FROM "Alerts" WHERE "StationId" = 'station-abc');---

### **TEST CASE 2: Cooldown Prevention**
**Scenario**: Water level still high within cooldown period (10 minutes)

**Setup**: (After Test Case 1)
-- Wait 3 minutes, insert another high reading
INSERT INTO "SensorReadings" ("Id", "StationId", "Value", "Unit", "MeasuredAt")
VALUES ('reading-790', 'station-abc', 3.5, 'm', NOW());**Expected**:
- ✅ Alert updated with new CurrentValue = 3.5
- ❌ NO new notification created (cooldown active)
- ✅ NotificationCount remains same

**Verification**:
SELECT "NotificationCount", "LastNotificationAt" 
FROM "Alerts" 
WHERE "StationId" = 'station-abc';

-- Should still show only 2 notifications (from Test Case 1)
SELECT COUNT(*) FROM "NotificationLogs" WHERE "AlertId" = 'alert-123';---

### **TEST CASE 3: Cooldown Expiry**
**Scenario**: 12 minutes pass, new notification allowed

**Setup**: (After Test Case 2)
-- Manually set LastNotificationAt to 12 minutes ago
UPDATE "Alerts" 
SET "LastNotificationAt" = NOW() - INTERVAL '12 minutes',
    "NotificationSent" = false
WHERE "StationId" = 'station-abc';

-- Trigger background job or wait for next cycle**Expected**:
- ✅ New notification created (cooldown expired)
- ✅ NotificationCount incremented
- ✅ LastNotificationAt updated to NOW()

---

### **TEST CASE 4: Retry Logic - Success on 2nd Attempt**
**Scenario**: First notification fails, succeeds on retry

**Setup**:sharp
// Mock SmsService to fail first time, succeed second time
int attemptCount = 0;
_mockSmsService.Setup(x => x.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(() => ++attemptCount > 1); // Fail 1st, succeed 2nd**Expected**:
- ✅ 1st attempt: Status = "pending_retry", RetryCount = 1, UpdatedAt = NOW() + 5min
- ✅ (Wait 5 minutes)
- ✅ 2nd attempt: Status = "sent", SentAt = NOW()

**Verification**:
SELECT "Status", "RetryCount", "SentAt", "ErrorMessage" 
FROM "NotificationLogs" 
WHERE "Channel" = 3 -- SMS
ORDER BY "CreatedAt" DESC LIMIT 1;---

### **TEST CASE 5: Retry Exhaustion**
**Scenario**: Notification fails 3 times (max retries)

**Setup**:
// Mock EmailService to always fail
_mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(false);**Expected**:
- ✅ Attempt 1: Status = "pending_retry", RetryCount = 1
- ✅ Attempt 2: Status = "pending_retry", RetryCount = 2
- ✅ Attempt 3: Status = "failed", RetryCount = 3, ErrorMessage = "Max retries exceeded"

---

## 📊 **7. MONITORING & METRICS**

### **Key Metrics to Track**sharp
// Add to DispatchNotificationsResponse
public class DispatchNotificationsResponse
{
    // ... existing fields ...
    
    public Dictionary<NotificationChannel, int> SuccessByChannel { get; set; }
    public Dictionary<NotificationChannel, int> FailuresByChannel { get; set; }
    public double AvgDeliveryTimeSeconds { get; set; }
}### **Logging Enhancements**
_logger.LogInformation(
    "Notification dispatch completed. " +
    "Total: {Total}, Sent: {Sent}, Failed: {Failed}, " +
    "Avg delivery time: {AvgTime}s, " +
    "Push: {PushSent}/{PushTotal}, Email: {EmailSent}/{EmailTotal}, SMS: {SmsSent}/{SmsTotal}",
    created, sent, failed, avgDeliveryTime,
    successByChannel[NotificationChannel.Push], totalByChannel[NotificationChannel.Push],
    successByChannel[NotificationChannel.Email], totalByChannel[NotificationChannel.Email],
    successByChannel[NotificationChannel.SMS], totalByChannel[NotificationChannel.SMS]
);---

## ✅ **8. ACCEPTANCE CRITERIA**

### **Must Have:**
- [ ] Cooldown logic prevents spam (max 1 notification per 10 min for "warning")
- [ ] Failed notifications retry with exponential backoff (5min, 15min, 45min)
- [ ] Max 3 retry attempts, then marked as "failed"
- [ ] All 3 channels (Push, Email, SMS) work independently
- [ ] Alert history shows all notifications + delivery status
- [ ] Background jobs run without blocking each other

### **Nice to Have:**
- [ ] Configurable cooldown periods per severity level
- [ ] Admin dashboard showing notification stats
- [ ] Webhook integration for external systems

---

## 🚨 **9. RISKS & MITIGATION**

| Risk | Impact | Mitigation |
|------|--------|------------|
| SMS provider rate limits | High | Implement queue with max 10/sec, use Polly for rate limiting |
| Email marked as spam | Medium | Use verified domain, SPF/DKIM, transactional email service |
| Push token expires | Medium | Implement token refresh mechanism, fallback to Email |
| Database bottleneck | High | Add indexes on Alert.NotificationSent, NotificationLog.Status |
| Memory leak in background jobs | High | Use `using` for scoped services, monitor memory usage |

---

## 📚 **10. REFERENCES**

- **Existing Code**: `FDAAPI.App.FeatG42_ProcessAlerts`, `FDAAPI.App.FeatG43_DispatchNotifications`
- **Background Jobs**: `AlertProcessingJob.cs`, `NotificationDispatchJob.cs`
- **Services**: `NotificationDispatchService.cs`, `PriorityRoutingService.cs`
- **Retry Patterns**: [Polly Documentation](https://github.com/App-vNext/Polly)

---

**END OF FE-12 DOCUMENTATION**
Migration:
// Add to AppDbContext
public DbSet<AlertCooldownConfig> AlertCooldownConfigs { get; set; }

// Configuration
modelBuilder.Entity<AlertCooldownConfig>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasIndex(e => e.Severity).IsUnique();
    
    // Seed default values
    entity.HasData(
        new { Id = Guid.NewGuid(), Severity = "info", CooldownMinutes = 30, MaxNotificationsPerHour = 2 },
        new { Id = Guid.NewGuid(), Severity = "caution", CooldownMinutes = 20, MaxNotificationsPerHour = 3 },
        new { Id = Guid.NewGuid(), Severity = "warning", CooldownMinutes = 10, MaxNotificationsPerHour = 6 },
        new { Id = Guid.NewGuid(), Severity = "critical", CooldownMinutes = 5, MaxNotificationsPerHour = 12 }
    );
});

🏗️ 3. ARCHITECTURE DESIGN
Current Implementation Analysis
✅ What's Already Good:
Background Service Pattern (AlertProcessingJob)
Runs every 2 minutes
Uses MediatR to ProcessAlertsHandler
Properly scoped services
Separation of Concerns
ProcessAlertsHandler: Creates/updates alerts based on sensor data
DispatchNotificationsHandler: Sends notifications for pending alerts
NotificationDispatchService: Strategy pattern for multi-channel delivery
Priority Routing (PriorityRoutingService)
Maps severity → priority
Routes channels based on user tier

⚠️ What Needs Improvement:
Problem 1: Cooldown Logic Not Implemented
Current Code (ProcessAlertsHandler.cs:58-66):
if (existingAlert != null)
{
    // Just updates value, NO cooldown check ❌
    existingAlert.CurrentValue = (decimal)latestReading.Value;
    existingAlert.UpdatedAt = DateTime.UtcNow;
    await _alertRepo.UpdateAsync(existingAlert, ct);
    updated++;
}
Issue: Updates every 2 minutes, causing notification spam!
Solution: Add cooldown check:
if (existingAlert != null)
{
    // ===== COOLDOWN LOGIC =====
    var cooldownMinutes = GetCooldownMinutes(existingAlert.Severity); // 10 mins for warning
    var timeSinceLastNotification = DateTime.UtcNow - existingAlert.LastNotificationAt;
    
    if (timeSinceLastNotification.HasValue && 
        timeSinceLastNotification.Value.TotalMinutes < cooldownMinutes)
    {
        // Within cooldown period, skip notification
        existingAlert.CurrentValue = (decimal)latestReading.Value;
        existingAlert.UpdatedAt = DateTime.UtcNow;
        await _alertRepo.UpdateAsync(existingAlert, ct);
        continue; // Don't trigger new notification
    }
    
    // Cooldown expired, allow new notification
    existingAlert.NotificationSent = false; // Reset flag to allow DispatchNotifications to process
    existingAlert.CurrentValue = (decimal)latestReading.Value;
    await _alertRepo.UpdateAsync(existingAlert, ct);
}

Problem 2: Retry Logic Incomplete
if (success)
{
    notificationLog.Status = "sent";
    notificationLog.SentAt = DateTime.UtcNow;
    sent++;
}
else
{
    // ===== RETRY LOGIC =====
    notificationLog.RetryCount++;
    
    if (notificationLog.RetryCount < notificationLog.MaxRetries)
    {
        notificationLog.Status = "pending_retry";
        notificationLog.ErrorMessage = $"Delivery failed. Retry {notificationLog.RetryCount}/{notificationLog.MaxRetries}";
        
        // Exponential backoff: 5min, 15min, 45min
        var retryDelayMinutes = (int)Math.Pow(3, notificationLog.RetryCount) * 5;
        notificationLog.UpdatedAt = DateTime.UtcNow.AddMinutes(retryDelayMinutes);
        
        _logger.LogWarning(
            "Notification failed. Will retry in {DelayMinutes} minutes. " +
            "Attempt {RetryCount}/{MaxRetries}",
            retryDelayMinutes, notificationLog.RetryCount, notificationLog.MaxRetries);
    }
    else
    {
        notificationLog.Status = "failed";
        notificationLog.ErrorMessage = $"Delivery failed after {notificationLog.MaxRetries} attempts";
        
        _logger.LogError(
            "Notification permanently failed for User {UserId}, Channel {Channel}",
            notificationLog.UserId, notificationLog.Channel);
    }
    
    failed++;
}

Problem 3: No Retry Job for "pending_retry" Notifications
Current State: DispatchNotificationsHandler only processes notificationLog.Status = "pending" (new notifications).
Solution: Modify GetPendingNotifications to include retries:
// In INotificationLogRepository
Task<List<NotificationLog>> GetPendingAndRetryNotificationsAsync(int limit, CancellationToken ct);

// Implementation
public async Task<List<NotificationLog>> GetPendingAndRetryNotificationsAsync(int limit, CancellationToken ct)
{
    return await _context.NotificationLogs
        .Where(n => 
            n.Status == "pending" || 
            (n.Status == "pending_retry" && n.UpdatedAt <= DateTime.UtcNow) // Retry delay expired
        )
        .OrderBy(n => n.Priority).ThenBy(n => n.CreatedAt)
        .Take(limit)
        .Include(n => n.User)
        .Include(n => n.Alert)
        .ToListAsync(ct);
}

Recommended Architecture Flow
┌────────────────────────────────────────────────────┐
│  AlertProcessingJob (Every 2 min)                   │
│  ├─ Check sensor readings                           │
│  ├─ Compare with AlertRules                         │
│  ├─ Create/Update Alerts                            │
│  └─ Apply COOLDOWN LOGIC ✅                         │
└────────────────────────────┬───────────────────────┘
                             │
                             ▼
┌────────────────────────────────────────────────────┐
│  DispatchNotificationsJob (Every 1 min)            │
│  ├─ Get pending & retry notifications              │
│  ├─ Dispatch via channels (Push/Email/SMS)         │
│  ├─ Apply RETRY LOGIC with exponential backoff ✅  │
│  └─ Update NotificationLog status                  │
└────────────────────────────────────────────────────┘

📝 4. STEP-BY-STEP CODING PLAN
STEP 1: Database Migration (Optional Config Table)
Files to create/modify:
src/Core/Domain/FDAAPI.Domain.RelationalDb/
  └── Entities/
      └── AlertCooldownConfig.cs (NEW)
  └── RealationalDB/
      └── Configurations/
          └── AlertCooldownConfigConfiguration.cs (NEW)
      └── AppDbContext.cs (MODIFY - add DbSet)
Action:
dotnet ef migrations add AddAlertCooldownConfigTable \
  --project "src/Core/Domain/FDAAPI.Domain.RelationalDb/FDAAPI.Domain.RelationalDb.csproj" \
  --startup-project "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj"

STEP 2: Refine ProcessAlertsHandler (Add Cooldown Logic)
File: FDAAPI.App.FeatG42_ProcessAlerts/ProcessAlertsHandler.cs
Changes:
Inject IAlertCooldownConfigRepository (or use hardcoded values)
Add cooldown check in the alert update logic
Reset NotificationSent flag when cooldown expires
Pseudocode:
if (existingAlert != null)
{
    var cooldownMinutes = await GetCooldownMinutesAsync(existingAlert.Severity, ct);
    var cooldownExpired = !existingAlert.LastNotificationAt.HasValue ||
                          (DateTime.UtcNow - existingAlert.LastNotificationAt.Value).TotalMinutes >= cooldownMinutes;
    
    if (cooldownExpired)
    {
        // Allow new notification
        existingAlert.NotificationSent = false;
        _logger.LogInformation("Cooldown expired for Alert {AlertId}, allowing new notification", existingAlert.Id);
    }
    else
    {
        _logger.LogDebug("Alert {AlertId} within cooldown period, skipping notification", existingAlert.Id);
    }
    
    existingAlert.CurrentValue = (decimal)latestReading.Value;
    existingAlert.UpdatedAt = DateTime.UtcNow;
    await _alertRepo.UpdateAsync(existingAlert, ct);
    updated++;
}

STEP 3: Refine DispatchNotificationsHandler (Add Retry Logic)
File: FDAAPI.App.FeatG43_DispatchNotifications/DispatchNotificationsHandler.cs
Changes:
Modify to handle pending_retry status
Implement exponential backoff calculation
Update status to "pending_retry" or "failed" based on retry count
Pseudocode:
if (success)
{
    notificationLog.Status = "sent";
    notificationLog.SentAt = DateTime.UtcNow;
    sent++;
}
else
{
    notificationLog.RetryCount++;
    
    if (notificationLog.RetryCount < notificationLog.MaxRetries)
    {
        var retryDelay = CalculateExponentialBackoff(notificationLog.RetryCount); // 5min, 15min, 45min
        notificationLog.Status = "pending_retry";
        notificationLog.UpdatedAt = DateTime.UtcNow.AddMinutes(retryDelay);
        notificationLog.ErrorMessage = $"Retry {notificationLog.RetryCount}/{notificationLog.MaxRetries}";
        
        _logger.LogWarning("Scheduling retry in {DelayMinutes} minutes", retryDelay);
    }
    else
    {
        notificationLog.Status = "failed";
        notificationLog.ErrorMessage = "Max retries exceeded";
        
        _logger.LogError("Notification permanently failed: {LogId}", notificationLog.Id);
    }
    
    failed++;
}

STEP 4: Update Repository Methods
File: FDAAPI.Infra.Persistence/Repositories/PgsqlNotificationLogRepository.cs
Add method:
public async Task<List<NotificationLog>> GetPendingAndRetryNotificationsAsync(int limit, CancellationToken ct)
{
    return await _context.NotificationLogs
        .Where(n => 
            n.Status == "pending" || 
            (n.Status == "pending_retry" && n.UpdatedAt <= DateTime.UtcNow)
        )
        .OrderBy(n => n.Priority)
        .ThenBy(n => n.CreatedAt)
        .Take(limit)
        .Include(n => n.User)
        .Include(n => n.Alert)
        .ToListAsync(ct);
}

STEP 5: Test End-to-End
Test Scenarios:
✅ Normal Alert Flow: Sensor exceeds threshold → Alert created → Notification sent
✅ Cooldown Prevention: Alert triggered again within 10 min → No new notification
✅ Cooldown Expiry: After 10 min → New notification allowed
✅ Retry Success: First attempt fails, 2nd succeeds after 5 min
✅ Retry Exhaustion: 3 failures → Status "failed", no more retries
✅ Multi-User: 10 users subscribed → All receive notifications

🔌 5. API SPECIFICATIONS
No New APIs Needed!
FE-12 is primarily backend engine logic. Existing APIs already cover:
G39: POST /api/v1/alerts/subscribe - Subscribe to alerts
G40: GET /api/v1/alerts/history - View alert history
G41: PUT /api/v1/alerts/preferences - Update preferences
Optional Admin API (for monitoring):
GET /api/v1/admin/alerts/stats
Response:
{
  "alertsCreated24h": 45,
  "notificationsSent24h": 120,
  "notificationsFailed24h": 3,
  "avgDeliveryTimeSeconds": 2.5,
  "pendingRetries": 5
}

🧪 6. TEST CASES
TEST CASE 1: Normal Alert Trigger
Scenario: Water level exceeds threshold for the first time
Setup:
-- Create AlertRule
INSERT INTO "AlertRules" ("Id", "StationId", "Name", "RuleType", "ThresholdValue", "Severity", "IsActive")
VALUES ('rule-123', 'station-abc', 'High Water Warning', 'threshold', 2.5, 'warning', true);

-- Create user subscription
INSERT INTO "UserAlertSubscriptions" ("Id", "UserId", "StationId", "MinSeverity", "EnablePush", "EnableEmail")
VALUES ('sub-123', 'user-456', 'station-abc', 'warning', true, true);

-- Insert sensor reading that exceeds threshold
INSERT INTO "SensorReadings" ("Id", "StationId", "Value", "Unit", "MeasuredAt")
VALUES ('reading-789', 'station-abc', 3.2, 'm', NOW());
Expected:
✅ Alert created with Status = "open", Severity = "warning"
✅ NotificationLog created for user (Push + Email channels)
✅ Notifications dispatched successfully
✅ Alert.NotificationSent = true, LastNotificationAt = NOW()
Verification:
SELECT * FROM "Alerts" WHERE "StationId" = 'station-abc' AND "Status" = 'open';
SELECT * FROM "NotificationLogs" WHERE "AlertId" = (SELECT "Id" FROM "Alerts" WHERE "StationId" = 'station-abc');

TEST CASE 2: Cooldown Prevention
Scenario: Water level still high within cooldown period (10 minutes)
Setup: (After Test Case 1)
-- Wait 3 minutes, insert another high reading
INSERT INTO "SensorReadings" ("Id", "StationId", "Value", "Unit", "MeasuredAt")
VALUES ('reading-790', 'station-abc', 3.5, 'm', NOW());
Expected:
✅ Alert updated with new CurrentValue = 3.5
❌ NO new notification created (cooldown active)
✅ NotificationCount remains same
Verification:
SELECT "NotificationCount", "LastNotificationAt" 
FROM "Alerts" 
WHERE "StationId" = 'station-abc';

-- Should still show only 2 notifications (from Test Case 1)
SELECT COUNT(*) FROM "NotificationLogs" WHERE "AlertId" = 'alert-123';

TEST CASE 3: Cooldown Expiry
Scenario: 12 minutes pass, new notification allowed
Setup: (After Test Case 2)
-- Manually set LastNotificationAt to 12 minutes agoUPDATE "Alerts" SET "LastNotificationAt" = NOW() - INTERVAL '12 minutes',    "NotificationSent" = falseWHERE "StationId" = 'station-abc';-- Trigger background job or wait for next cycle
Expected:
✅ New notification created (cooldown expired)
✅ NotificationCount incremented
✅ LastNotificationAt updated to NOW()
TEST CASE 4: Retry Logic - Success on 2nd Attempt
Scenario: First notification fails, succeeds on retry
Setup:
// Mock SmsService to fail first time, succeed second timeint attemptCount = 0;_mockSmsService.Setup(x => x.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))    .ReturnsAsync(() => ++attemptCount > 1); // Fail 1st, succeed 2nd
Expected:
✅ 1st attempt: Status = "pending_retry", RetryCount = 1, UpdatedAt = NOW() + 5min
✅ (Wait 5 minutes)
✅ 2nd attempt: Status = "sent", SentAt = NOW()
Verification:
SELECT "Status", "RetryCount", "SentAt", "ErrorMessage" FROM "NotificationLogs" WHERE "Channel" = 3 -- SMSORDER BY "CreatedAt" DESC LIMIT 1;
TEST CASE 5: Retry Exhaustion
Scenario: Notification fails 3 times (max retries)
Setup:
// Mock EmailService to always fail_mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))    .ReturnsAsync(false);
Expected:
✅ Attempt 1: Status = "pending_retry", RetryCount = 1
✅ Attempt 2: Status = "pending_retry", RetryCount = 2
✅ Attempt 3: Status = "failed", RetryCount = 3, ErrorMessage = "Max retries exceeded"
📊 7. MONITORING & METRICS
Key Metrics to Track
// Add to DispatchNotificationsResponsepublic class DispatchNotificationsResponse{    // ... existing fields ...        public Dictionary<NotificationChannel, int> SuccessByChannel { get; set; }    public Dictionary<NotificationChannel, int> FailuresByChannel { get; set; }    public double AvgDeliveryTimeSeconds { get; set; }}
Logging Enhancements
_logger.LogInformation(    "Notification dispatch completed. " +    "Total: {Total}, Sent: {Sent}, Failed: {Failed}, " +    "Avg delivery time: {AvgTime}s, " +    "Push: {PushSent}/{PushTotal}, Email: {EmailSent}/{EmailTotal}, SMS: {SmsSent}/{SmsTotal}",    created, sent, failed, avgDeliveryTime,    successByChannel[NotificationChannel.Push], totalByChannel[NotificationChannel.Push],    successByChannel[NotificationChannel.Email], totalByChannel[NotificationChannel.Email],    successByChannel[NotificationChannel.SMS], totalByChannel[NotificationChannel.SMS]);
✅ 8. ACCEPTANCE CRITERIA
Must Have:
[ ] Cooldown logic prevents spam (max 1 notification per 10 min for "warning")
[ ] Failed notifications retry with exponential backoff (5min, 15min, 45min)
[ ] Max 3 retry attempts, then marked as "failed"
[ ] All 3 channels (Push, Email, SMS) work independently
[ ] Alert history shows all notifications + delivery status
[ ] Background jobs run without blocking each other
Nice to Have:
[ ] Configurable cooldown periods per severity level
[ ] Admin dashboard showing notification stats
[ ] Webhook integration for external systems
🚨 9. RISKS & MITIGATION
Risk	Impact	Mitigation
SMS provider rate limits	High	Implement queue with max 10/sec, use Polly for rate limiting
Email marked as spam	Medium	Use verified domain, SPF/DKIM, transactional email service
Push token expires	Medium	Implement token refresh mechanism, fallback to Email
Database bottleneck	High	Add indexes on Alert.NotificationSent, NotificationLog.Status
Memory leak in background jobs	High	Use using for scoped services, monitor memory usage
📚 10. REFERENCES
Existing Code: FDAAPI.App.FeatG42_ProcessAlerts, FDAAPI.App.FeatG43_DispatchNotifications
Background Jobs: AlertProcessingJob.cs, NotificationDispatchJob.cs
Services: NotificationDispatchService.cs, PriorityRoutingService.cs
Retry Patterns: Polly Documentation