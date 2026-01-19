# FE-15: View Flood Alert History

> **Status**: Partially Implemented (G40 - GetAlertHistory exists)  
> **Priority**: MEDIUM (Data visualization & analytics)  
> **Estimated Effort**: 2-3 days (Enhance existing + add filters)

---

## 📋 **1. BUSINESS REQUIREMENTS**

### **Goal**
Provide comprehensive alert history with advanced filtering, pagination, and export capabilities for both users and administrators.

### **Key Features**
1. **User View**: See personal alert history for their monitored areas
2. **Admin View**: See system-wide alert history with user details
3. **Pagination**: Handle large datasets (thousands of alerts)
4. **Advanced Filters**:
   - Date range (from/to)
   - Area/Station
   - Severity (info, caution, warning, critical)
   - Status (open, resolved)
   - Notification status (sent, failed)
5. **Sorting**: By date (newest/oldest), severity (high/low)
6. **Export**: CSV/Excel download (optional)
7. **Statistics**: Summary metrics (total alerts, by severity, avg resolution time)

### **User Stories**

**Story 1: Citizen reviews alerts**
> "As a user, I want to see all flood alerts for my home area in the past month, so I can understand flooding patterns."

**Story 2: Admin investigates issues**
> "As an admin, I want to see all failed notifications for critical alerts, so I can improve delivery reliability."

**Story 3: User exports data**
> "As a researcher, I want to export alert history as CSV, so I can analyze trends in Excel."

---

## 🗄️ **2. DATABASE ANALYSIS & EXISTING CODE REVIEW**

### **Existing Implementation: FeatG40_GetAlertHistory**

Let me check what's already there:sharp
// FDAAPI.App.FeatG40_GetAlertHistory/GetAlertHistoryHandler.cs (Existing)
public class GetAlertHistoryHandler : IRequestHandler<GetAlertHistoryRequest, GetAlertHistoryResponse>
{
    private readonly IAlertRepository _alertRepo;
    
    public async Task<GetAlertHistoryResponse> Handle(GetAlertHistoryRequest request, CancellationToken ct)
    {
        // TODO: Check existing implementation
    }
}### **Current Entities (Already Good!)**

#### **Alert Entity** ✅
public class Alert
{
    public Guid Id { get; set; }
    public Guid AlertRuleId { get; set; }
    public Guid StationId { get; set; }
    public DateTime TriggeredAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string Status { get; set; } = "open"; // open, resolved
    public string Severity { get; set; } = "info";
    public NotificationPriority Priority { get; set; }
    public decimal CurrentValue { get; set; }
    public string Message { get; set; }
    public bool NotificationSent { get; set; }
    public int NotificationCount { get; set; }
    public DateTime? LastNotificationAt { get; set; }
    
    // Navigation
    public virtual AlertRule? AlertRule { get; set; }
    public virtual Station? Station { get; set; }
    public virtual ICollection<NotificationLog>? NotificationLogs { get; set; }
}**Good**: Has all necessary fields for history queries ✅

---

#### **NotificationLog Entity** ✅
public class NotificationLog
{
    public Guid UserId { get; set; }
    public Guid AlertId { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Destination { get; set; }
    public string Content { get; set; }
    public NotificationPriority Priority { get; set; }
    public int RetryCount { get; set; }
    public string Status { get; set; } = "pending"; // pending, sent, failed, delivered
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? ErrorMessage { get; set; }
}**Good**: Can join to show notification status ✅

---

### **What's Missing?**

#### **Problem 1: No Pagination Support**

Current query likely fetches all results:
// ❌ BAD - No pagination
var alerts = await _context.Alerts
    .Where(a => a.StationId == stationId)
    .ToListAsync();
// Could return 10,000+ records!**Solution**: Add pagination parameters:
public sealed record GetAlertHistoryRequest(
    Guid? UserId,
    int Page = 1,
    int PageSize = 20,
    // ... filters
) : IFeatureRequest<GetAlertHistoryResponse>;---

#### **Problem 2: Limited Filters**

**Solution**: Add comprehensive filter model:
public class AlertHistoryFilters
{
    public Guid? UserId { get; set; }
    public Guid? AreaId { get; set; }
    public Guid? StationId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<string>? Severities { get; set; } // ["warning", "critical"]
    public List<string>? Statuses { get; set; } // ["open", "resolved"]
    public bool? NotificationSent { get; set; }
    public string? OrderBy { get; set; } = "date_desc"; // date_desc, date_asc, severity_desc
}---

#### **Problem 3: No Statistics/Aggregations**

Users want to see:
- Total alerts in period
- Breakdown by severity
- Resolution time metrics
- Notification success rate

**Solution**: Add stats to response:
public class AlertHistoryStats
{
    public int TotalAlerts { get; set; }
    public int OpenAlerts { get; set; }
    public int ResolvedAlerts { get; set; }
    public Dictionary<string, int> BySeverity { get; set; }
    public double AvgResolutionTimeMinutes { get; set; }
    public int NotificationsSent { get; set; }
    public int NotificationsFailed { get; set; }
}---

## 🏗️ **3. ARCHITECTURE DESIGN**

### **Repository Layer Enhancement**

#### **IAlertRepository - Add Advanced Query Methods**

public interface IAlertRepository
{
    // ✅ Existing methods
    Task<Alert?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<Alert>> GetActiveAlertsByStationAsync(Guid stationId, CancellationToken ct);
    
    // ✅ NEW - Advanced query methods
    Task<(List<Alert> Alerts, int TotalCount)> GetPagedAlertsAsync(
        AlertHistoryFilters filters,
        int page,
        int pageSize,
        CancellationToken ct);
    
    Task<AlertHistoryStats> GetAlertStatisticsAsync(
        AlertHistoryFilters filters,
        CancellationToken ct);
    
    Task<List<Alert>> GetAlertsByUserAreasAsync(
        Guid userId,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken ct);
}---

### **Query Optimization Strategy**

#### **Indexes Needed**

-- Alert history queries
CREATE INDEX "IX_Alerts_TriggeredAt" ON "Alerts" ("TriggeredAt" DESC);
CREATE INDEX "IX_Alerts_StationId_TriggeredAt" ON "Alerts" ("StationId", "TriggeredAt" DESC);
CREATE INDEX "IX_Alerts_Status_Severity" ON "Alerts" ("Status", "Severity");

-- User-specific queries (via UserAlertSubscription)
CREATE INDEX "IX_UserAlertSubscriptions_UserId_StationId" ON "UserAlertSubscriptions" ("UserId", "StationId");

-- Notification logs for history
CREATE INDEX "IX_NotificationLogs_AlertId_Status" ON "NotificationLogs" ("AlertId", "Status");**Migration**:
dotnet ef migrations add AddAlertHistoryIndexes---

## 📝 **4. STEP-BY-STEP CODING PLAN**

### **STEP 1: Create Enhanced Models**

#### **AlertHistoryFilters (in FDAAPI.App.Common/Models/)**
public class AlertHistoryFilters
{
    public Guid? UserId { get; set; }
    public Guid? AreaId { get; set; }
    public Guid? StationId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<string>? Severities { get; set; }
    public List<string>? Statuses { get; set; }
    public bool? NotificationSent { get; set; }
    public string OrderBy { get; set; } = "date_desc";
}#### **AlertHistoryStats**
public class AlertHistoryStats
{
    public int TotalAlerts { get; set; }
    public int OpenAlerts { get; set; }
    public int ResolvedAlerts { get; set; }
    public Dictionary<string, int> BySeverity { get; set; } = new();
    public double AvgResolutionTimeMinutes { get; set; }
    public int NotificationsSent { get; set; }
    public int NotificationsFailed { get; set; }
}---

### **STEP 2: Enhance Repository (Add Pagination)**

#### **PgsqlAlertRepository.cs**

public async Task<(List<Alert> Alerts, int TotalCount)> GetPagedAlertsAsync(
    AlertHistoryFilters filters,
    int page,
    int pageSize,
    CancellationToken ct)
{
    var query = _context.Alerts
        .Include(a => a.Station)
        .Include(a => a.AlertRule)
        .Include(a => a.NotificationLogs)
        .AsQueryable();
    
    // ===== APPLY FILTERS ===== 
    
    // User filter (via UserAlertSubscription join)
    if (filters.UserId.HasValue)
    {
        query = query.Where(a => 
            _context.UserAlertSubscriptions.Any(s => 
                s.UserId == filters.UserId.Value && 
                s.StationId == a.StationId));
    }
    
    // Area filter
    if (filters.AreaId.HasValue)
    {
        query = query.Where(a => 
            _context.UserAlertSubscriptions.Any(s => 
                s.AreaId == filters.AreaId.Value && 
                s.StationId == a.StationId));
    }
    
    // Station filter
    if (filters.StationId.HasValue)
    {
        query = query.Where(a => a.StationId == filters.StationId.Value);
    }
    
    // Date range filter
    if (filters.FromDate.HasValue)
    {
        query = query.Where(a => a.TriggeredAt >= filters.FromDate.Value);
    }
    
    if (filters.ToDate.HasValue)
    {
        query = query.Where(a => a.TriggeredAt <= filters.ToDate.Value);
    }
    
    // Severity filter
    if (filters.Severities != null && filters.Severities.Any())
    {
        query = query.Where(a => filters.Severities.Contains(a.Severity));
    }
    
    // Status filter
    if (filters.Statuses != null && filters.Statuses.Any())
    {
        query = query.Where(a => filters.Statuses.Contains(a.Status));
    }
    
    // Notification sent filter
    if (filters.NotificationSent.HasValue)
    {
        query = query.Where(a => a.NotificationSent == filters.NotificationSent.Value);
    }
    
    // ===== GET TOTAL COUNT (before pagination) ===== 
    var totalCount = await query.CountAsync(ct);
    
    // ===== APPLY SORTING ===== 
    query = filters.OrderBy switch
    {
        "date_asc" => query.OrderBy(a => a.TriggeredAt),
        "date_desc" => query.OrderByDescending(a => a.TriggeredAt),
        "severity_desc" => query.OrderByDescending(a => a.Severity).ThenByDescending(a => a.TriggeredAt),
        "severity_asc" => query.OrderBy(a => a.Severity).ThenByDescending(a => a.TriggeredAt),
        _ => query.OrderByDescending(a => a.TriggeredAt)
    };
    
    // ===== APPLY PAGINATION ===== 
    var skip = (page - 1) * pageSize;
    var alerts = await query
        .Skip(skip)
        .Take(pageSize)
        .AsNoTracking() // Read-only query
        .ToListAsync(ct);
    
    return (alerts, totalCount);
}---

#### **GetAlertStatisticsAsync**

public async Task<AlertHistoryStats> GetAlertStatisticsAsync(
    AlertHistoryFilters filters,
    CancellationToken ct)
{
    var query = _context.Alerts.AsQueryable();
    
    // Apply same filters as GetPagedAlertsAsync
    // ... (reuse filter logic) ...
    
    // Calculate statistics
    var stats = new AlertHistoryStats
    {
        TotalAlerts = await query.CountAsync(ct),
        OpenAlerts = await query.CountAsync(a => a.Status == "open", ct),
        ResolvedAlerts = await query.CountAsync(a => a.Status == "resolved", ct),
        
        // Breakdown by severity
        BySeverity = await query
            .GroupBy(a => a.Severity)
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Severity, x => x.Count, ct),
        
        // Average resolution time (for resolved alerts)
        AvgResolutionTimeMinutes = await query
            .Where(a => a.ResolvedAt.HasValue)
            .Select(a => EF.Functions.DateDiffMinute(a.TriggeredAt, a.ResolvedAt!.Value))
            .AverageAsync(ct),
        
        // Notification statistics
        NotificationsSent = await query
            .SelectMany(a => a.NotificationLogs!)
            .CountAsync(n => n.Status == "sent", ct),
        
        NotificationsFailed = await query
            .SelectMany(a => a.NotificationLogs!)
            .CountAsync(n => n.Status == "failed", ct)
    };
    
    return stats;
}---

### **STEP 3: Refine/Enhance GetAlertHistory Feature**

#### **GetAlertHistoryRequest (Enhanced)**

public sealed record GetAlertHistoryRequest(
    Guid? UserId,           // For user-specific view
    Guid? AreaId,           // Filter by area
    Guid? StationId,        // Filter by station
    DateTime? FromDate,     // Date range start
    DateTime? ToDate,       // Date range end
    List<string>? Severities, // Filter by severity
    List<string>? Statuses, // Filter by status
    bool? NotificationSent, // Filter by notification status
    string OrderBy = "date_desc", // Sorting
    int Page = 1,           // Pagination
    int PageSize = 20,      // Items per page
    bool IncludeStats = false // Include statistics
) : IFeatureRequest<GetAlertHistoryResponse>;#### **Validator**

public class GetAlertHistoryRequestValidator : AbstractValidator<GetAlertHistoryRequest>
{
    public GetAlertHistoryRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");
        
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");
        
        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
            .WithMessage("ToDate must be after FromDate");
        
        RuleFor(x => x.OrderBy)
            .Must(o => new[] { "date_asc", "date_desc", "severity_asc", "severity_desc" }.Contains(o))
            .WithMessage("Invalid OrderBy value");
    }
}#### **Handler (Enhanced)**

public class GetAlertHistoryHandler : IRequestHandler<GetAlertHistoryRequest, GetAlertHistoryResponse>
{
    private readonly IAlertRepository _alertRepo;
    private readonly ILogger<GetAlertHistoryHandler> _logger;
    
    public GetAlertHistoryHandler(
        IAlertRepository alertRepo,
        ILogger<GetAlertHistoryHandler> logger)
    {
        _alertRepo = alertRepo;
        _logger = logger;
    }
    
    public async Task<GetAlertHistoryResponse> Handle(
        GetAlertHistoryRequest request, 
        CancellationToken ct)
    {
        try
        {
            // Build filters
            var filters = new AlertHistoryFilters
            {
                UserId = request.UserId,
                AreaId = request.AreaId,
                StationId = request.StationId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Severities = request.Severities,
                Statuses = request.Statuses,
                NotificationSent = request.NotificationSent,
                OrderBy = request.OrderBy
            };
            
            // Get paginated alerts
            var (alerts, totalCount) = await _alertRepo.GetPagedAlertsAsync(
                filters, 
                request.Page, 
                request.PageSize, 
                ct);
            
            // Map to DTOs
            var alertDtos = alerts.Select(a => new AlertHistoryDto
            {
                Id = a.Id,
                StationId = a.StationId,
                StationName = a.Station?.Name,
                TriggeredAt = a.TriggeredAt,
                ResolvedAt = a.ResolvedAt,
                Status = a.Status,
                Severity = a.Severity,
                Priority = a.Priority.ToString(),
                CurrentValue = a.CurrentValue,
                Message = a.Message,
                NotificationSent = a.NotificationSent,
                NotificationCount = a.NotificationCount,
                NotificationChannels = a.NotificationLogs?
                    .Select(n => n.Channel.ToString())
                    .Distinct()
                    .ToList(),
                NotificationStatus = a.NotificationLogs?
                    .GroupBy(n => n.Status)
                    .ToDictionary(g => g.Key, g => g.Count())
            }).ToList();
            
            // Calculate pagination metadata
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
            
            var response = new GetAlertHistoryResponse
            {
                Success = true,
                Message = $"Retrieved {alertDtos.Count} alerts",
                Alerts = alertDtos,
                Pagination = new PaginationMetadata
                {
                    CurrentPage = request.Page,
                    PageSize = request.PageSize,
                    TotalItems = totalCount,
                    TotalPages = totalPages,
                    HasPreviousPage = request.Page > 1,
                    HasNextPage = request.Page < totalPages
                }
            };
            
            // Include statistics if requested
            if (request.IncludeStats)
            {
                response.Statistics = await _alertRepo.GetAlertStatisticsAsync(filters, ct);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alert history");
            return new GetAlertHistoryResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }
}---

### **STEP 4: Create Endpoints**

#### **Endpoint 1: Get User Alert History**

// GET /api/v1/alerts/history
public class GetUserAlertHistoryEndpoint : Endpoint<GetAlertHistoryRequestDto, GetAlertHistoryResponseDto>
{
    private readonly IMediator _mediator;
    
    public GetUserAlertHistoryEndpoint(IMediator mediator) => _mediator = mediator;
    
    public override void Configure()
    {
        Get("/api/v1/alerts/history");
        Policies("User");
        Summary(s =>
        {
            s.Summary = "Get user's alert history";
            s.Description = "Returns paginated alert history for user's monitored areas with filters";
            s.ExampleRequest = new GetAlertHistoryRequestDto
            {
                FromDate = DateTime.UtcNow.AddDays(-30),
                Severities = new List<string> { "warning", "critical" },
                Page = 1,
                PageSize = 20
            };
        });
        Tags("Alert History");
    }
    
    public override async Task HandleAsync(GetAlertHistoryRequestDto req, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        
        var query = new GetAlertHistoryRequest(
            UserId: userId, // ✅ Filter by current user
            AreaId: req.AreaId,
            StationId: req.StationId,
            FromDate: req.FromDate,
            ToDate: req.ToDate,
            Severities: req.Severities,
            Statuses: req.Statuses,
            NotificationSent: req.NotificationSent,
            OrderBy: req.OrderBy ?? "date_desc",
            Page: req.Page ?? 1,
            PageSize: req.PageSize ?? 20,
            IncludeStats: req.IncludeStats ?? false
        );
        
        var result = await _mediator.Send(query, ct);
        
        var response = new GetAlertHistoryResponseDto
        {
            Success = result.Success,
            Message = result.Message,
            Alerts = result.Alerts,
            Pagination = result.Pagination,
            Statistics = result.Statistics
        };
        
        await SendAsync(response, 200, ct);
    }
}---

#### **Endpoint 2: Get Admin Alert History (All Users)**

// GET /api/v1/admin/alerts/history
public class GetAdminAlertHistoryEndpoint : Endpoint<GetAlertHistoryRequestDto, GetAlertHistoryResponseDto>
{
    private readonly IMediator _mediator;
    
    public GetAdminAlertHistoryEndpoint(IMediator mediator) => _mediator = mediator;
    
    public override void Configure()
    {
        Get("/api/v1/admin/alerts/history");
        Policies("Admin");
        Summary(s =>
        {
            s.Summary = "Get system-wide alert history (Admin)";
            s.Description = "Returns all alerts with advanced filters and statistics";
        });
        Tags("Admin", "Alert History");
    }
    
    public override async Task HandleAsync(GetAlertHistoryRequestDto req, CancellationToken ct)
    {
        var query = new GetAlertHistoryRequest(
            UserId: req.UserId, // ✅ Admin can filter by specific user
            AreaId: req.AreaId,
            StationId: req.StationId,
            FromDate: req.FromDate,
            ToDate: req.ToDate,
            Severities: req.Severities,
            Statuses: req.Statuses,
            NotificationSent: req.NotificationSent,
            OrderBy: req.OrderBy ?? "date_desc",
            Page: req.Page ?? 1,
            PageSize: req.PageSize ?? 50, // Admin can see more per page
            IncludeStats: true // Always include stats for admin
        );
        
        var result = await _mediator.Send(query, ct);
        
        await SendAsync(new GetAlertHistoryResponseDto
        {
            Success = result.Success,
            Alerts = result.Alerts,
            Pagination = result.Pagination,
            Statistics = result.Statistics
        }, 200, ct);
    }
}---

#### **Endpoint 3: Get Alert Statistics Only**

// GET /api/v1/alerts/statistics
public class GetAlertStatisticsEndpoint : Endpoint<GetAlertHistoryRequestDto, AlertStatisticsResponseDto>
{
    private readonly IAlertRepository _alertRepo;
    
    public override void Configure()
    {
        Get("/api/v1/alerts/statistics");
        Policies("User");
        Summary(s =>
        {
            s.Summary = "Get alert statistics";
            s.Description = "Returns aggregated statistics without detailed alert list";
        });
        Tags("Alert History", "Statistics");
    }
    
    public override async Task HandleAsync(GetAlertHistoryRequestDto req, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        
        var filters = new AlertHistoryFilters
        {
            UserId = userId,
            FromDate = req.FromDate,
            ToDate = req.ToDate,
            Severities = req.Severities,
            Statuses = req.Statuses
        };
        
        var stats = await _alertRepo.GetAlertStatisticsAsync(filters, ct);
        
        await SendAsync(new AlertStatisticsResponseDto
        {
            Success = true,
            Statistics = stats
        }, 200, ct);
    }
}---

#### **Endpoint 4: Export Alert History (CSV)**

// GET /api/v1/alerts/history/export
public class ExportAlertHistoryEndpoint : Endpoint<GetAlertHistoryRequestDto>
{
    private readonly IAlertRepository _alertRepo;
    
    public override void Configure()
    {
        Get("/api/v1/alerts/history/export");
        Policies("User");
        Summary(s =>
        {
            s.Summary = "Export alert history as CSV";
            s.Description = "Downloads alert history in CSV format";
        });
        Tags("Alert History", "Export");
    }
    
    public override async Task HandleAsync(GetAlertHistoryRequestDto req, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        
        var filters = new AlertHistoryFilters
        {
            UserId = userId,
            FromDate = req.FromDate,
            ToDate = req.ToDate,
            Severities = req.Severities
        };
        
        // Get all alerts (no pagination for export)
        var (alerts, _) = await _alertRepo.GetPagedAlertsAsync(
            filters, 
            1, 
            10000, // Max export limit
            ct);
        
        // Generate CSV
        var csv = new StringBuilder();
        csv.AppendLine("Triggered At,Station,Severity,Status,Water Level,Message,Notification Sent");
        
        foreach (var alert in alerts)
        {
            csv.AppendLine($"{alert.TriggeredAt:yyyy-MM-dd HH:mm:ss}," +
                          $"{alert.Station?.Name}," +
                          $"{alert.Severity}," +
                          $"{alert.Status}," +
                          $"{alert.CurrentValue}m," +
                          $"\"{alert.Message}\"," +
                          $"{alert.NotificationSent}");
        }
        
        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"alert_history_{DateTime.UtcNow:yyyyMMdd}.csv";
        
        await SendBytesAsync(bytes, fileName, "text/csv", cancellation: ct);
    }
}---

## 🧪 **5. TEST CASES**

### **TEST CASE 1: Get Alert History with Pagination**

**Request**:
curl -X GET "http://localhost:5000/api/v1/alerts/history?page=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN"**Expected Response (200 OK)**:
{
  "success": true,
  "message": "Retrieved 10 alerts",
  "alerts": [
    {
      "id": "alert-123",
      "stationId": "station-abc",
      "stationName": "River Main Station",
      "triggeredAt": "2026-01-15T10:30:00Z",
      "resolvedAt": "2026-01-15T12:00:00Z",
      "status": "resolved",
      "severity": "warning",
      "priority": "Medium",
      "currentValue": 2.8,
      "message": "Water level 2.8m exceeded threshold",
      "notificationSent": true,
      "notificationCount": 2,
      "notificationChannels": ["Push", "Email"],
      "notificationStatus": {
        "sent": 2
      }
    }
    // ... 9 more alerts
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalItems": 145,
    "totalPages": 15,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}---

### **TEST CASE 2: Filter by Date Range & Severity**

**Request**:
curl -X GET "http://localhost:5000/api/v1/alerts/history?\
fromDate=2026-01-01T00:00:00Z&\
toDate=2026-01-31T23:59:59Z&\
severities=warning&severities=critical&\
page=1&pageSize=20" \
  -H "Authorization: Bearer $TOKEN"**Expected**:
- ✅ Only alerts from January 2026
- ✅ Only "warning" and "critical" severity
- ✅ Max 20 results per page
- ✅ Pagination metadata shows totalItems for filtered results

---

### **TEST CASE 3: Get Statistics Only**

**Request**:
curl -X GET "http://localhost:5000/api/v1/alerts/statistics?\
fromDate=2026-01-01T00:00:00Z&\
toDate=2026-01-31T23:59:59Z" \
  -H "Authorization: Bearer $TOKEN"**Expected Response**:
{
  "success": true,
  "statistics": {
    "totalAlerts": 145,
    "openAlerts": 12,
    "resolvedAlerts": 133,
    "bySeverity": {
      "info": 20,
      "caution": 45,
      "warning": 60,
      "critical": 20
    },
    "avgResolutionTimeMinutes": 87.5,
    "notificationsSent": 280,
    "notificationsFailed": 5
  }
}---

### **TEST CASE 4: Export CSV**

**Request**:
curl -X GET "http://localhost:5000/api/v1/alerts/history/export?\
fromDate=2026-01-01&toDate=2026-01-31" \
  -H "Authorization: Bearer $TOKEN" \
  -o alert_history.csv**Expected**:
- ✅ File downloaded as `alert_history.csv`
- ✅ Contains all alerts in CSV format
- ✅ Max 10,000 records (export limit)

**CSV Content**:
Triggered At,Station,Severity,Status,Water Level,Message,Notification Sent
2026-01-15 10:30:00,River Main Station,warning,resolved,2.8m,"Water level 2.8m exceeded threshold",true
2026-01-14 08:15:00,North Bridge Station,critical,open,4.2m,"Critical flooding detected",true
...---

### **TEST CASE 5: Admin View - All Users**

**Request** (Admin only):
curl -X GET "http://localhost:5000/api/v1/admin/alerts/history?\
userId=user-123&\
includeStats=true" \
  -H "Authorization: Bearer $ADMIN_TOKEN"**Expected**:
- ✅ Admin can see alerts for specific user
- ✅ Statistics included automatically
- ✅ More results per page (50 vs 20)

---

### **TEST CASE 6: Empty Results (No Alerts in Date Range)**

**Request**:
curl -X GET "http://localhost:5000/api/v1/alerts/history?\
fromDate=2025-01-01&toDate=2025-01-31" \
  -H "Authorization: Bearer $TOKEN"**Expected Response**:
{
  "success": true,
  "message": "Retrieved 0 alerts",
  "alerts": [],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 0,
    "totalPages": 0,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
}---

### **TEST CASE 7: Validation Error - Invalid Page**

**Request**:
curl -X GET "http://localhost:5000/api/v1/alerts/history?page=0" \
  -H "Authorization: Bearer $TOKEN"**Expected Response (400 Bad Request)**:
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    {
      "field": "page",
      "message": "Page must be greater than 0"
    }
  ]
}---

## 📱 **6. MOBILE UI RECOMMENDATIONS**

### **Alert History Screen Layout**


Alert Entity ✅
public class Alert
{
    public Guid Id { get; set; }
    public Guid AlertRuleId { get; set; }
    public Guid StationId { get; set; }
    public DateTime TriggeredAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string Status { get; set; } = "open"; // open, resolved
    public string Severity { get; set; } = "info";
    public NotificationPriority Priority { get; set; }
    public decimal CurrentValue { get; set; }
    public string Message { get; set; }
    public bool NotificationSent { get; set; }
    public int NotificationCount { get; set; }
    public DateTime? LastNotificationAt { get; set; }
    
    // Navigation
    public virtual AlertRule? AlertRule { get; set; }
    public virtual Station? Station { get; set; }
    public virtual ICollection<NotificationLog>? NotificationLogs { get; set; }
}

NotificationLog Entity ✅
public class NotificationLog
{
    public Guid UserId { get; set; }
    public Guid AlertId { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Destination { get; set; }
    public string Content { get; set; }
    public NotificationPriority Priority { get; set; }
    public int RetryCount { get; set; }
    public string Status { get; set; } = "pending"; // pending, sent, failed, delivered
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? ErrorMessage { get; set; }
} ] Filter by notification status
- [ ] Advanced stats (resolution time chart)
- [ ] Infinite scroll support

---

## 🚨 **8. PERFORMANCE OPTIMIZATION**

### **Query Performance Tips**

1. **Use AsNoTracking** for read-only queries
2. **Index key columns** (TriggeredAt, StationId, Status)
3. **Avoid N+1 queries** with Include()
4. **Limit Include depth** (max 2 levels)
5. **Cache statistics** (Redis, 5 min TTL)

### **Sample Cached Stats Implementation**

public async Task<AlertHistoryStats> GetAlertStatisticsAsync(
    AlertHistoryFilters filters, 
    CancellationToken ct)
{
    var cacheKey = $"alert_stats_{filters.UserId}_{filters.FromDate:yyyyMMdd}_{filters.ToDate:yyyyMMdd}";
    
    // Try cache first
    var cachedStats = await _cache.GetStringAsync(cacheKey, ct);
    if (cachedStats != null)
    {
        return JsonSerializer.Deserialize<AlertHistoryStats>(cachedStats)!;
    }
    
    // Calculate from DB
    var stats = await CalculateStatsFromDbAsync(filters, ct);
    
    // Cache for 5 minutes
    await _cache.SetStringAsync(
        cacheKey, 
        JsonSerializer.Serialize(stats),
        new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) },
        ct);
    
    return stats;
}---

## 📊 **9. MONITORING QUERIES**

### **Slow Query Detection**
SELECT 
    "TriggeredAt",
    "StationId",
    "Severity",
    COUNT(*) OVER() AS "TotalRows"
FROM "Alerts"
WHERE "TriggeredAt" >= NOW() - INTERVAL '30 days'
ORDER BY "TriggeredAt" DESC
LIMIT 20;

-- Check execution time
EXPLAIN ANALYZE ...### **Index Usage Analysis**
SELECT 
    schemaname,
    tablename,
    indexname,
    idx_scan AS "times_used",
    idx_tup_read AS "tuples_read"
FROM pg_stat_user_indexes
WHERE tablename = 'Alerts'
ORDER BY idx_scan DESC;---

## 🎯 **10. FUTURE ENHANCEMENTS**

1. **Real-time Updates**: WebSocket/SignalR for live alert feed
2. **Advanced Analytics**: Charts showing trends over time
3. **Predictive Alerts**: "You usually get alerts at this time"
4. **Grouped View**: Group by area/station
5. **Heatmap**: Visual representation of alert frequency
6. **Export to PDF**: Formatted report generation
7. **Email Digest**: Weekly summary of alerts

---

**END OF FE-15 DOCUMENTATION**

What's Missing?
// ❌ BAD - No pagination
var alerts = await _context.Alerts
    .Where(a => a.StationId == stationId)
    .ToListAsync();
// Could return 10,000+ records!
Solution: Add pagination parameters:
public sealed record GetAlertHistoryRequest(
    Guid? UserId,
    int Page = 1,
    int PageSize = 20,
    // ... filters
) : IFeatureRequest<GetAlertHistoryResponse>;

Problem 2: Limited Filters
Solution: Add comprehensive filter model:
public class AlertHistoryFilters
{
    public Guid? UserId { get; set; }
    public Guid? AreaId { get; set; }
    public Guid? StationId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<string>? Severities { get; set; } // ["warning", "critical"]
    public List<string>? Statuses { get; set; } // ["open", "resolved"]
    public bool? NotificationSent { get; set; }
    public string? OrderBy { get; set; } = "date_desc"; // date_desc, date_asc, severity_desc
}

Problem 3: No Statistics/Aggregations
Users want to see:
Total alerts in period
Breakdown by severity
Resolution time metrics
Notification success rate
Solution: Add stats to response:
public class AlertHistoryStats
{
    public int TotalAlerts { get; set; }
    public int OpenAlerts { get; set; }
    public int ResolvedAlerts { get; set; }
    public Dictionary<string, int> BySeverity { get; set; }
    public double AvgResolutionTimeMinutes { get; set; }
    public int NotificationsSent { get; set; }
    public int NotificationsFailed { get; set; }
}