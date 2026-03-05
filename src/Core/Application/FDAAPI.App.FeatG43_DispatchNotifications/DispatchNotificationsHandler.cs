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
        private readonly IUserRepository _userRepo;
        private readonly INotificationLogRepository _notificationLogRepo;
        private readonly IPriorityRoutingService _routingService;
        private readonly INotificationTemplateService _templateService;
        private readonly INotificationDispatchService _dispatchService;
        private readonly ILogger<DispatchNotificationsHandler> _logger;
        private readonly IStationRepository _stationRepo;
        private readonly IAreaRepository _areaRepo;
        private readonly IUserSubscriptionRepository _userSubscriptionRepo;

        public DispatchNotificationsHandler(
            IAlertRepository alertRepo,
            IUserAlertSubscriptionRepository subscriptionRepo,
            IUserRepository userRepo,
            INotificationLogRepository notificationLogRepo,
            IPriorityRoutingService routingService,
            INotificationTemplateService templateService,
            INotificationDispatchService dispatchService,
            IStationRepository stationRepo,
            IAreaRepository areaRepo,
            ILogger<DispatchNotificationsHandler> logger,
            IUserSubscriptionRepository userSubscriptionRepo)
        {
            _alertRepo = alertRepo;
            _subscriptionRepo = subscriptionRepo;
            _userRepo = userRepo;
            _notificationLogRepo = notificationLogRepo;
            _routingService = routingService;
            _templateService = templateService;
            _dispatchService = dispatchService;
            _stationRepo = stationRepo;
            _areaRepo = areaRepo;
            _logger = logger;
            _userSubscriptionRepo = userSubscriptionRepo;
        }

        public async Task<DispatchNotificationsResponse> Handle(
            DispatchNotificationsRequest request,
            CancellationToken ct)
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

                        var subscriptions = new List<UserAlertSubscription>();

                        // 2a. Get station details
                        var station = alert.Station ?? await _stationRepo.GetByIdAsync(alert.StationId, ct);
                        if (station == null || !station.Latitude.HasValue || !station.Longitude.HasValue)
                        {
                            _logger.LogWarning("Station {StationId} not found or missing coordinates", alert.StationId);

                            // Mark alert as notified (no way to find subscribers)
                            alert.NotificationSent = true;
                            alert.NotificationCount = 0;
                            alert.LastNotificationAt = DateTime.UtcNow;
                            alert.UpdatedAt = DateTime.UtcNow;
                            await _alertRepo.UpdateAsync(alert, ct);
                            continue;
                        }

                        // 2b. Find areas containing this station (AREA-BASED SUBSCRIPTIONS)
                        var areasContainingStation = await _areaRepo.GetAreasContainingStationAsync(
                            alert.StationId,
                            station.Latitude.Value,
                            station.Longitude.Value,
                            ct);

                        if (areasContainingStation.Any())
                        {
                            var areaIds = areasContainingStation.Select(a => a.Id).ToList();
                            var areaSubscriptions = await _subscriptionRepo.GetByAreaIdsAsync(areaIds, ct);
                            subscriptions.AddRange(areaSubscriptions);

                            _logger.LogInformation(
                                "Found {Count} area-based subscriptions for station {StationId} in {AreaCount} areas",
                                areaSubscriptions.Count(), alert.StationId, areasContainingStation.Count);
                        }

                        // 2c. Also get direct station subscriptions (STATION-BASED - for Gov/Admin)
                        var stationSubscriptions = await _subscriptionRepo.GetByStationIdAsync(alert.StationId, ct);
                        subscriptions.AddRange(stationSubscriptions);

                        if (stationSubscriptions.Any())
                        {
                            _logger.LogInformation(
                                "Found {Count} direct station subscriptions for station {StationId}",
                                stationSubscriptions.Count(), alert.StationId);
                        }

                        // 2d. Deduplicate by UserId (avoid sending duplicate notifications)
                        subscriptions = subscriptions
                            .GroupBy(s => s.UserId)
                            .Select(g => g.First()) // Take first subscription per user
                            .ToList();

                        if (!subscriptions.Any())
                        {
                            _logger.LogDebug("No subscriptions found for station {StationId}", alert.StationId);

                            // Mark alert as notified even if no subscribers
                            alert.NotificationSent = true;
                            alert.NotificationCount = 0;
                            alert.LastNotificationAt = DateTime.UtcNow;
                            alert.UpdatedAt = DateTime.UtcNow;
                            await _alertRepo.UpdateAsync(alert, ct);
                            continue;
                        }

                        _logger.LogInformation(
                            "Total {Count} unique subscribers for station {StationId}",
                            subscriptions.Count, alert.StationId);

                        foreach (var subscription in subscriptions)
                        {
                            // 3. Check if user should be notified
                            var user = await _userRepo.GetByIdAsync(subscription.UserId, ct);
                            if (user == null)
                            {
                                _logger.LogWarning("User {UserId} not found", subscription.UserId);
                                continue;
                            }

                            // Get user tier (from pricing plan or default to Free)
                            var userTier = await _userSubscriptionRepo.GetUserTierAsync(user.Id, ct);

                            // Check severity threshold
                            if (!_routingService.ShouldNotifyUser(alert.Severity, userTier, subscription.MinSeverity))
                            {
                                _logger.LogDebug(
                                    "User {UserId} severity threshold not met. Alert: {AlertSeverity}, Min: {MinSeverity}",
                                    user.Id, alert.Severity, subscription.MinSeverity);
                                continue;
                            }

                            // Check quiet hours
                            if (IsInQuietHours(subscription.QuietHoursStart, subscription.QuietHoursEnd))
                            {
                                _logger.LogDebug("User {UserId} in quiet hours, skipping notification", user.Id);
                                continue;
                            }

                            // 4. Determine priority and available channels
                            var priority = _routingService.DeterminePriority(alert.Severity, userTier);
                            var availableChannels = _routingService.GetAvailableChannelsForTier(userTier);

                            // Filter by user preferences from UserAlertSubscription
                            var userChannels = new List<NotificationChannel>();
                            if (subscription.EnablePush && availableChannels.Contains(NotificationChannel.Push))
                                userChannels.Add(NotificationChannel.Push);
                            if (subscription.EnableEmail && availableChannels.Contains(NotificationChannel.Email))
                                userChannels.Add(NotificationChannel.Email);
                            if (subscription.EnableSms && availableChannels.Contains(NotificationChannel.SMS))
                                userChannels.Add(NotificationChannel.SMS);

                            // Always add InApp
                            if (availableChannels.Contains(NotificationChannel.InApp))
                                userChannels.Add(NotificationChannel.InApp);

                            if (!userChannels.Any())
                            {
                                _logger.LogDebug("No enabled channels for user {UserId}", user.Id);
                                continue;
                            }

                            // 5. Create notification logs for each channel
                            foreach (var channel in userChannels)
                            {
                                var destination = GetDestination(user, channel);
                                if (string.IsNullOrEmpty(destination))
                                {
                                    _logger.LogWarning(
                                        "No destination found for user {UserId}, channel {Channel}",
                                        user.Id, channel);
                                    continue;
                                }

                                var content = channel == NotificationChannel.SMS
                                    ? _templateService.GenerateSmsContent(alert, station!)
                                    : _templateService.GenerateBody(alert, station!, channel);

                                // Calculate dispatch delay based on tier and priority
                                var dispatchDelay = _routingService.GetDispatchDelaySeconds(userTier, priority);
                                var maxRetries = _routingService.GetMaxRetriesForTier(userTier, priority);

                                var notificationLog = new NotificationLog
                                {
                                    Id = Guid.NewGuid(),
                                    UserId = user.Id,
                                    AlertId = alert.Id,
                                    Channel = channel,
                                    Priority = priority,
                                    Destination = destination,
                                    Content = content,
                                    Title = _templateService.GenerateTitle(alert, priority),
                                    Status = "pending",
                                    RetryCount = 0,
                                    MaxRetries = maxRetries, // Tier-specific retries
                                    CreatedBy = Guid.Empty,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedBy = Guid.Empty,
                                    UpdatedAt = DateTime.UtcNow.AddSeconds(dispatchDelay) // Delayed dispatch time
                                };

                                await _notificationLogRepo.CreateAsync(notificationLog, ct);
                                created++;

                                _logger.LogDebug(
                                    "Created notification log {LogId} for user {UserId}, channel {Channel}",
                                    notificationLog.Id, user.Id, channel);
                            }
                        }

                        // 6. Dispatch pending notifications (including newly created)
                        var pendingNotifications = await _notificationLogRepo.GetPendingAndRetryNotificationsAsync(100, ct);

                        foreach (var notificationLog in pendingNotifications.Where(n => n.AlertId == alert.Id))
                        {
                            try
                            {
                                var user = notificationLog.User ?? await _userRepo.GetByIdAsync(notificationLog.UserId, ct);
                                if (user == null)
                                {
                                    _logger.LogWarning("User {UserId} not found for notification {LogId}",
                                        notificationLog.UserId, notificationLog.Id);
                                    continue;
                                }

                                // Attempt to send notification
                                bool success = await _dispatchService.DispatchNotificationAsync(
                                    notificationLog,
                                    user,
                                    ct);

                                // ===== RETRY LOGIC ===== ✅ NEW
                                if (success)
                                {
                                    notificationLog.Status = "sent";
                                    notificationLog.SentAt = DateTime.UtcNow;
                                    notificationLog.UpdatedAt = DateTime.UtcNow;
                                    sent++;

                                    _logger.LogInformation(
                                        "Notification {LogId} sent successfully via {Channel} to {Destination}",
                                        notificationLog.Id, notificationLog.Channel, notificationLog.Destination);
                                }
                                else
                                {
                                    // Send failed - implement retry logic
                                    notificationLog.RetryCount++;

                                    if (notificationLog.RetryCount < notificationLog.MaxRetries)
                                    {
                                        // Calculate exponential backoff: 5min, 15min, 45min
                                        var retryDelayMinutes = (int)Math.Pow(3, notificationLog.RetryCount) * 5;

                                        notificationLog.Status = "pending_retry";
                                        notificationLog.ErrorMessage = $"Delivery failed. Retry {notificationLog.RetryCount}/{notificationLog.MaxRetries}";
                                        notificationLog.UpdatedAt = DateTime.UtcNow.AddMinutes(retryDelayMinutes);

                                        _logger.LogWarning(
                                            "Notification {LogId} failed. Scheduling retry in {DelayMinutes} minutes. " +
                                            "Attempt {RetryCount}/{MaxRetries}",
                                            notificationLog.Id, retryDelayMinutes,
                                            notificationLog.RetryCount, notificationLog.MaxRetries);
                                    }
                                    else
                                    {
                                        // Max retries exceeded
                                        notificationLog.Status = "failed";
                                        notificationLog.ErrorMessage = $"Delivery failed after {notificationLog.MaxRetries} attempts";
                                        notificationLog.UpdatedAt = DateTime.UtcNow;

                                        _logger.LogError(
                                            "Notification {LogId} permanently failed for User {UserId}, Channel {Channel}. " +
                                            "Max retries ({MaxRetries}) exceeded",
                                            notificationLog.Id, notificationLog.UserId,
                                            notificationLog.Channel, notificationLog.MaxRetries);
                                    }

                                    failed++;
                                }

                                await _notificationLogRepo.UpdateAsync(notificationLog, ct);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex,
                                    "Error dispatching notification {LogId}",
                                    notificationLog.Id);
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

                _logger.LogInformation(
                    "Notification dispatch completed. " +
                    "Alerts processed: {AlertsProcessed}, " +
                    "Notifications created: {Created}, Sent: {Sent}, Failed: {Failed}",
                    alertsProcessed, created, sent, failed);

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
            List<NotificationChannel> availableChannels,
            UserAlertSubscription subscription)
        {
            var filtered = new List<NotificationChannel>();

            foreach (var channel in availableChannels)
            {
                bool enabled = channel switch
                {
                    NotificationChannel.Push => subscription.EnablePush,
                    NotificationChannel.Email => subscription.EnableEmail,
                    NotificationChannel.SMS => subscription.EnableSms,
                    NotificationChannel.InApp => true, // Always enabled
                    _ => false
                };

                if (enabled)
                {
                    filtered.Add(channel);
                }
            }

            return filtered;
        }

        private string GetDestination(User user, NotificationChannel channel)
        {
            return channel switch
            {
                NotificationChannel.Push => user.FcmToken ?? string.Empty,
                NotificationChannel.Email => user.Email,
                NotificationChannel.SMS => user.PhoneNumber ?? string.Empty,
                NotificationChannel.InApp => user.Id.ToString(),
                _ => string.Empty
            };
        }
    }
}