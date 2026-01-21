using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FDAAPI.App.FeatG70_AdminGetAlertStats
{
    public class AdminGetAlertStatsHandler : IRequestHandler<AdminGetAlertStatsRequest, AdminGetAlertStatsResponse>
    {
        private readonly IAlertRepository _alertRepo;
        private readonly INotificationLogRepository _notificationLogRepo;
        private readonly IUserAlertSubscriptionRepository _subscriptionRepo;
        private readonly ILogger<AdminGetAlertStatsHandler> _logger;

        public AdminGetAlertStatsHandler(
            IAlertRepository alertRepo,
            INotificationLogRepository notificationLogRepo,
            IUserAlertSubscriptionRepository subscriptionRepo,
            ILogger<AdminGetAlertStatsHandler> logger)
        {
            _alertRepo = alertRepo;
            _notificationLogRepo = notificationLogRepo;
            _subscriptionRepo = subscriptionRepo;
            _logger = logger;
        }

        public async Task<AdminGetAlertStatsResponse> Handle(
            AdminGetAlertStatsRequest request,
            CancellationToken ct)
        {
            try
            {
                // ===== STEP 1: Set default date range (last 24 hours) =====
                var toDate = request.ToDate ?? DateTime.UtcNow;
                var fromDate = request.FromDate ?? toDate.AddHours(-24);

                // ===== STEP 2: Get Alert Statistics =====
                var totalAlerts = await _alertRepo.CountAlertsAsync(fromDate, toDate, ct);
                var alertsBySeverity = await _alertRepo.CountAlertsBySeverityAsync(fromDate, toDate, ct);
                var alertsByStatus = await _alertRepo.CountAlertsByStatusAsync(fromDate, toDate, ct);

                // ===== STEP 3: Get Notification Statistics =====
                var totalNotifications = await _notificationLogRepo.CountNotificationsAsync(fromDate, toDate, ct);
                var sentNotifications = await _notificationLogRepo.CountNotificationsByStatusAsync("sent", fromDate, toDate, ct);
                var failedNotifications = await _notificationLogRepo.CountNotificationsByStatusAsync("failed", fromDate, toDate, ct);
                var pendingNotifications = await _notificationLogRepo.CountNotificationsByStatusAsync("pending", fromDate, toDate, ct);
                var pendingRetries = await _notificationLogRepo.CountNotificationsByStatusAsync("pending_retry", fromDate, toDate, ct);

                var notificationsByChannel = await _notificationLogRepo.GetNotificationStatsByChannelAsync(fromDate, toDate, ct);
                var avgDeliveryTime = await _notificationLogRepo.GetAverageDeliveryTimeAsync(fromDate, toDate, ct);

                // ===== STEP 4: Get User Statistics =====
                var totalSubscribers = await _subscriptionRepo.CountActiveSubscribersAsync(ct);
                var newSubscribers24h = await _subscriptionRepo.CountNewSubscribersAsync(DateTime.UtcNow.AddHours(-24), ct);

                // ===== STEP 5: Build Response =====
                var channelStats = notificationsByChannel.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new ChannelStatsDto
                    {
                        Sent = kvp.Value.Sent,
                        Failed = kvp.Value.Failed,
                        SuccessRate = CalculateSuccessRate(kvp.Value.Sent, kvp.Value.Failed)
                    }
                );

                var response = new AdminGetAlertStatsResponse
                {
                    Success = true,
                    Message = "Retrieved successfully",
                    Data = new AlertStatsDataDto
                    {
                        Period = new PeriodDto
                        {
                            From = fromDate,
                            To = toDate
                        },
                        Alerts = new AlertSummaryDto
                        {
                            Total = totalAlerts,
                            BySeverity = alertsBySeverity,
                            ByStatus = alertsByStatus
                        },
                        Notifications = new NotificationSummaryDto
                        {
                            TotalCreated = totalNotifications,
                            TotalSent = sentNotifications,
                            TotalFailed = failedNotifications,
                            TotalPending = pendingNotifications,
                            ByChannel = channelStats,
                            AvgDeliveryTimeSeconds = Math.Round(avgDeliveryTime, 2),
                            PendingRetries = pendingRetries
                        },
                        Users = new UserSummaryDto
                        {
                            TotalSubscribers = totalSubscribers,
                            ActiveSubscribers = totalSubscribers,
                            NewSubscribers24h = newSubscribers24h
                        }
                    }
                };

                _logger.LogInformation(
                    "Admin stats retrieved for period {From} to {To}. " +
                    "Alerts: {Alerts}, Notifications: {Notifications}",
                    fromDate, toDate, totalAlerts, totalNotifications);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alert statistics");
                return new AdminGetAlertStatsResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        private double CalculateSuccessRate(int sent, int failed)
        {
            var total = sent + failed;
            if (total == 0) return 0;
            return Math.Round((sent / (double)total) * 100, 1);
        }
    }
}