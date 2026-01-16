using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Enums;
using FDAAPI.Domain.RelationalDb.Repositories;
using FDAAPI.Infra.Services.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FDAAPI.App.FeatG43_DispatchNotifications
{
    public class DispatchNotificationsHandler : IRequestHandler<DispatchNotificationsRequest, DispatchNotificationsResponse>
    {
        private readonly IAlertRepository _alertRepo;
        private readonly IUserAlertSubscriptionRepository _subscriptionRepo;
        private readonly INotificationLogRepository _notificationLogRepo;
        private readonly IUserRepository _userRepo;
        private readonly IPriorityRoutingService _routingService;
        private readonly INotificationTemplateService _templateService;
        private readonly INotificationDispatchService _dispatchService;
        private readonly ILogger<DispatchNotificationsHandler> _logger;

        public DispatchNotificationsHandler(
            IAlertRepository alertRepo,
            IUserAlertSubscriptionRepository subscriptionRepo,
            INotificationLogRepository notificationLogRepo,
            IUserRepository userRepo,
            IPriorityRoutingService routingService,
            INotificationTemplateService templateService,
            INotificationDispatchService dispatchService,
            ILogger<DispatchNotificationsHandler> logger)
        {
            _alertRepo = alertRepo;
            _subscriptionRepo = subscriptionRepo;
            _notificationLogRepo = notificationLogRepo;
            _userRepo = userRepo;
            _routingService = routingService;
            _templateService = templateService;
            _dispatchService = dispatchService;
            _logger = logger;
        }

        public async Task<DispatchNotificationsResponse> Handle(DispatchNotificationsRequest request, CancellationToken ct)
        {
            int created = 0, sent = 0, failed = 0, alertsProcessed = 0;

            try
            {
                // 1. Get alerts that haven't sent notifications yet
                var pendingAlerts = await _alertRepo.GetUnnotifiedAlertsAsync(100, ct);

                _logger.LogInformation("Found {Count} pending alerts to process", pendingAlerts.Count());

                foreach (var alert in pendingAlerts)
                {
                    try
                    {
                        alertsProcessed++;

                        // 2. Get subscriptions for this station
                        var subscriptions = await _subscriptionRepo.GetByStationIdAsync(alert.StationId, ct);

                        foreach (var subscription in subscriptions)
                        {
                            // 3. Check if user should be notified
                            var user = await _userRepo.GetByIdAsync(subscription.UserId, ct);
                            if (user == null) continue;

                            // Get user tier (from pricing plan or default to Free)
                            var userTier = SubscriptionTier.Free; // TODO: Get from user_subscriptions table

                            // Check severity threshold
                            if (!_routingService.ShouldNotifyUser(alert.Severity, userTier, subscription.MinSeverity))
                            {
                                continue;
                            }

                            // Check quiet hours
                            if (IsInQuietHours(subscription.QuietHoursStart, subscription.QuietHoursEnd))
                            {
                                _logger.LogDebug("Skipping notification for user {UserId} - quiet hours", user.Id);
                                continue;
                            }

                            // Check if already notified
                            var alreadyNotified = await _notificationLogRepo.IsUserNotifiedAsync(user.Id, alert.Id, ct);
                            if (alreadyNotified) continue;

                            // 4. Determine priority and channels
                            var priority = _routingService.DeterminePriority(alert.Severity, userTier);
                            var channels = _routingService.GetChannelsForPriority(priority, userTier);

                            // Filter channels based on user preferences
                            channels = FilterChannelsByPreferences(channels, subscription);

                            // 5. Create notification logs for each channel
                            foreach (var channel in channels)
                            {
                                var destination = GetDestination(user, channel);
                                if (string.IsNullOrEmpty(destination)) continue;

                                var content = _templateService.GenerateBody(alert, alert.Station!, channel);
                                var title = _templateService.GenerateTitle(alert, priority);

                                var notificationLog = new NotificationLog
                                {
                                    Id = Guid.NewGuid(),
                                    UserId = user.Id,
                                    AlertId = alert.Id,
                                    Channel = channel,
                                    Destination = destination,
                                    Content = content,
                                    Priority = priority,
                                    Status = "pending",
                                    RetryCount = 0,
                                    MaxRetries = 3,
                                    CreatedBy = Guid.Empty, // System
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedBy = Guid.Empty,
                                    UpdatedAt = DateTime.UtcNow
                                };

                                await _notificationLogRepo.CreateAsync(notificationLog, ct);
                                created++;

                                // 6. Try to dispatch immediately
                                var success = await _dispatchService.DispatchNotificationAsync(notificationLog, user, ct);

                                if (success)
                                {
                                    notificationLog.Status = "sent";
                                    notificationLog.SentAt = DateTime.UtcNow;
                                    sent++;
                                }
                                else
                                {
                                    notificationLog.Status = "failed";
                                    notificationLog.ErrorMessage = "Dispatch failed";
                                    failed++;
                                }

                                await _notificationLogRepo.UpdateAsync(notificationLog, ct);
                            }
                        }

                        // 7. Mark alert as notified
                        alert.NotificationSent = true;
                        alert.NotificationCount = created;
                        alert.LastNotificationAt = DateTime.UtcNow;
                        alert.UpdatedAt = DateTime.UtcNow;
                        await _alertRepo.UpdateAsync(alert, ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing alert {AlertId}", alert.Id);
                    }
                }

                return new DispatchNotificationsResponse
                {
                    Success = true,
                    Message = "Notification dispatch completed",
                    NotificationsCreated = created,
                    NotificationsSent = sent,
                    NotificationsFailed = failed,
                    AlertsProcessed = alertsProcessed
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DispatchNotificationsHandler");
                return new DispatchNotificationsResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        private bool IsInQuietHours(TimeSpan? start, TimeSpan? end)
        {
            if (!start.HasValue || !end.HasValue) return false;

            var now = DateTime.UtcNow.TimeOfDay;

            // Handle overnight quiet hours (e.g., 22:00 - 06:00)
            if (start.Value > end.Value)
            {
                return now >= start.Value || now <= end.Value;
            }

            return now >= start.Value && now <= end.Value;
        }

        private List<NotificationChannel> FilterChannelsByPreferences(
            List<NotificationChannel> channels,
            UserAlertSubscription subscription)
        {
            var filtered = new List<NotificationChannel>();

            foreach (var channel in channels)
            {
                bool enabled = channel switch
                {
                    NotificationChannel.Push => subscription.EnablePush,
                    NotificationChannel.Email => subscription.EnableEmail,
                    NotificationChannel.SMS => subscription.EnableSms,
                    NotificationChannel.InApp => true, // Always enabled
                    _ => false
                };

                if (enabled) filtered.Add(channel);
            }

            return filtered;
        }

        private string GetDestination(User user, NotificationChannel channel)
        {
            return channel switch
            {
                NotificationChannel.Push => "device_token_placeholder", // TODO: Get from user devices table
                NotificationChannel.Email => user.Email,
                NotificationChannel.SMS => user.PhoneNumber ?? "",
                NotificationChannel.InApp => user.Id.ToString(),
                _ => ""
            };
        }
    }
}