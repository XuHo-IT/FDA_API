using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.App.FeatG40_GetAlertHistory
{
    public class GetAlertHistoryHandler : IRequestHandler<GetAlertHistoryRequest, GetAlertHistoryResponse>
    {
        private readonly AppDbContext _dbContext;
        private readonly INotificationLogRepository _notificationLogRepo;

        public GetAlertHistoryHandler(
            AppDbContext dbContext,
            INotificationLogRepository notificationLogRepo)
        {
            _dbContext = dbContext;
            _notificationLogRepo = notificationLogRepo;
        }

        public async Task<GetAlertHistoryResponse> Handle(GetAlertHistoryRequest request, CancellationToken ct)
        {
            try
            {
                // Build query: Get alerts that user was notified about
                var query = _dbContext.NotificationLogs
                    .Where(n => n.UserId == request.UserId)
                    .Where(n => n.Status == "sent" || n.Status == "delivered")
                    .AsQueryable();

                // Apply filters
                if (request.StartDate.HasValue)
                {
                    query = query.Where(n => n.CreatedAt >= request.StartDate.Value);
                }

                if (request.EndDate.HasValue)
                {
                    query = query.Where(n => n.CreatedAt <= request.EndDate.Value);
                }

                if (!string.IsNullOrEmpty(request.Severity))
                {
                    query = query.Where(n => n.Alert!.Severity.ToLower() == request.Severity.ToLower());
                }

                if (!string.IsNullOrEmpty(request.Status))
                {
                    query = query.Where(n => n.Status.ToString().ToLower() == request.Status.ToLower());
                }

                // Get total count before pagination (count distinct alerts)
                var totalCount = await query.Select(n => n.AlertId).Distinct().CountAsync(ct);

                // Step 1: Get paginated list of AlertIds (ordered by latest notification)
                var alertIds = await query
                    .GroupBy(n => n.AlertId)
                    .Select(g => new
                    {
                        AlertId = g.Key,
                        LatestCreatedAt = g.Max(n => n.CreatedAt)
                    })
                    .OrderByDescending(x => x.LatestCreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(x => x.AlertId)
                    .ToListAsync(ct);

                // Step 2: Get all notifications for these alerts
                var notifications = await _dbContext.NotificationLogs
                    .Where(n => alertIds.Contains(n.AlertId))
                    .Include(n => n.Alert)
                        .ThenInclude(a => a!.Station)
                    .ToListAsync(ct);

                // Step 3: Group in memory and build DTOs
                var alertGroups = notifications
                    .GroupBy(n => n.AlertId)
                    .OrderByDescending(g => g.Max(n => n.CreatedAt));

                // Build response DTOs
                var alerts = new List<AlertHistoryDto>();

                foreach (var group in alertGroups)
                {
                    var firstNotification = group.First();
                    var alert = firstNotification.Alert;

                    if (alert == null) continue;

                    var alertDto = new AlertHistoryDto
                    {
                        AlertId = alert.Id,
                        StationId = alert.StationId,
                        StationName = alert.Station?.Name ?? "Unknown",
                        StationCode = alert.Station?.Code ?? "",
                        Severity = alert.Severity,
                        Priority = alert.Priority,
                        WaterLevel = alert.CurrentValue,
                        Message = alert.Message,
                        TriggeredAt = alert.TriggeredAt,
                        ResolvedAt = alert.ResolvedAt,
                        Status = alert.Status,
                        Notifications = group.Select(n => new NotificationDetailDto
                        {
                            NotificationId = n.Id,
                            Channel = n.Channel,
                            Priority = n.Priority,
                            Status = n.Status,
                            SentAt = n.SentAt,
                            DeliveredAt = n.DeliveredAt,
                            ErrorMessage = n.ErrorMessage,
                            Title = n.Title
                        }).ToList()
                    };

                    alerts.Add(alertDto);
                }

                return new GetAlertHistoryResponse
                {
                    Success = true,
                    Message = $"Retrieved {alerts.Count} alerts",
                    Alerts = alerts,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                return new GetAlertHistoryResponse
                {
                    Success = false,
                    Message = $"Error retrieving alert history: {ex.Message}"
                };
            }
        }
    }
}