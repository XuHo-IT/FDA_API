using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Enums;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FDAAPI.App.FeatG42_ProcessAlerts
{
    public class ProcessAlertsHandler : IRequestHandler<ProcessAlertsRequest, ProcessAlertsResponse>
    {
        private readonly IAlertRuleRepository _ruleRepo;
        private readonly ISensorReadingRepository _sensorRepo;
        private readonly IAlertRepository _alertRepo;
        private readonly IAlertCooldownConfigRepository _cooldownRepo;
        private readonly ILogger<ProcessAlertsHandler> _logger;

        public ProcessAlertsHandler(
            IAlertRuleRepository ruleRepo,
            ISensorReadingRepository sensorRepo,
            IAlertRepository alertRepo,
            IAlertCooldownConfigRepository cooldownRepo,
            ILogger<ProcessAlertsHandler> logger)
        {
            _ruleRepo = ruleRepo;
            _sensorRepo = sensorRepo;
            _alertRepo = alertRepo;
            _cooldownRepo = cooldownRepo;
            _logger = logger;
        }

        public async Task<ProcessAlertsResponse> Handle(ProcessAlertsRequest request, CancellationToken ct)
        {
            int created = 0, updated = 0, skippedCooldown = 0;

            try
            {
                // 1. Get all active rules
                var rules = await _ruleRepo.GetActiveRulesAsync(ct);

                _logger.LogInformation("Processing {RuleCount} alert rules", rules.Count());

                foreach (var rule in rules)
                {
                    // 2. Get latest sensor reading for this station
                    var readings = await _sensorRepo.GetLatestReadingsByStationsAsync(
                        new[] { rule.StationId }, ct);

                    var latestReading = readings.FirstOrDefault();
                    if (latestReading == null)
                    {
                        _logger.LogDebug("No sensor reading found for station {StationId}", rule.StationId);
                        continue;
                    }

                    // 3. Check if threshold exceeded
                    bool thresholdExceeded = rule.RuleType.ToLower() switch
                    {
                        "threshold" => (decimal)latestReading.Value >= rule.ThresholdValue,
                        _ => false
                    };

                    if (!thresholdExceeded)
                    {
                        _logger.LogDebug(
                            "Threshold not exceeded for station {StationId}. Value: {Value}, Threshold: {Threshold}",
                            rule.StationId, latestReading.Value, rule.ThresholdValue);
                        continue;
                    }

                    // 4. Check if alert already exists (open)
                    var existingAlerts = await _alertRepo.GetActiveAlertsByStationAsync(
                        rule.StationId, ct);

                    var existingAlert = existingAlerts
                        .FirstOrDefault(a => a.AlertRuleId == rule.Id && a.Status == "open");

                    if (existingAlert != null)
                    {
                        // ===== COOLDOWN LOGIC ===== ✅ NEW
                        var cooldownMinutes = await _cooldownRepo.GetCooldownMinutesAsync(
                            existingAlert.Severity, ct);

                        var timeSinceLastNotification = existingAlert.LastNotificationAt.HasValue
                            ? DateTime.UtcNow - existingAlert.LastNotificationAt.Value
                            : (TimeSpan?)null;

                        bool cooldownActive = timeSinceLastNotification.HasValue &&
                                             timeSinceLastNotification.Value.TotalMinutes < cooldownMinutes;

                        if (cooldownActive)
                        {
                            // Within cooldown period - just update value, don't trigger notification
                            _logger.LogDebug(
                                "Alert {AlertId} within cooldown period ({Minutes}min). " +
                                "Last notification: {LastNotif}, Cooldown: {Cooldown}min",
                                existingAlert.Id,
                                timeSinceLastNotification.Value.TotalMinutes,
                                existingAlert.LastNotificationAt,
                                cooldownMinutes);

                            existingAlert.CurrentValue = (decimal)latestReading.Value;
                            existingAlert.UpdatedAt = DateTime.UtcNow;
                            existingAlert.UpdatedBy = Guid.Empty; // System user

                            await _alertRepo.UpdateAsync(existingAlert, ct);
                            skippedCooldown++;
                            continue; // Don't allow new notification
                        }

                        // Cooldown expired - allow new notification
                        _logger.LogInformation(
                            "Cooldown expired for Alert {AlertId}. Allowing new notification. " +
                            "Minutes since last: {Minutes}, Cooldown period: {Cooldown}",
                            existingAlert.Id,
                            timeSinceLastNotification?.TotalMinutes ?? 0,
                            cooldownMinutes);

                        existingAlert.CurrentValue = (decimal)latestReading.Value;
                        existingAlert.NotificationSent = false;
                        existingAlert.UpdatedAt = DateTime.UtcNow;
                        existingAlert.UpdatedBy = Guid.Empty;

                        await _alertRepo.UpdateAsync(existingAlert, ct);
                        updated++;
                    }
                    else
                    {
                        // Create new alert
                        var systemUserId = Guid.Empty; // TODO: Get from configuration

                        var newAlert = new Alert
                        {
                            Id = Guid.NewGuid(),
                            AlertRuleId = rule.Id,
                            StationId = rule.StationId,
                            TriggeredAt = DateTime.UtcNow,
                            Status = "open",
                            Severity = rule.Severity,
                            Priority = MapSeverityToPriority(rule.Severity),
                            CurrentValue = (decimal)latestReading.Value,
                            Message = $"Water level {latestReading.Value}{latestReading.Unit} exceeds threshold {rule.ThresholdValue} at {rule.Station?.Name ?? "station"}",
                            NotificationSent = false,
                            NotificationCount = 0,
                            LastNotificationAt = null, // No notification sent yet
                            CreatedBy = systemUserId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedBy = systemUserId,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _alertRepo.CreateAsync(newAlert, ct);
                        created++;

                        _logger.LogInformation(
                            "Created new alert {AlertId} for station {StationId}. Severity: {Severity}, Value: {Value}",
                            newAlert.Id, rule.StationId, rule.Severity, latestReading.Value);
                    }
                }

                // 5. Get pending alerts count
                var pendingAlerts = await _alertRepo.GetUnnotifiedAlertsAsync(1000, ct);

                _logger.LogInformation(
                    "Alert processing completed. Created: {Created}, Updated: {Updated}, " +
                    "Skipped (cooldown): {Skipped}, Pending notifications: {Pending}",
                    created, updated, skippedCooldown, pendingAlerts.Count());

                return new ProcessAlertsResponse
                {
                    Success = true,
                    Message = $"Processed {rules.Count()} rules",
                    AlertsCreated = created,
                    AlertsUpdated = updated,
                    AlertsPending = pendingAlerts.Count()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing alerts");
                return new ProcessAlertsResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        private NotificationPriority MapSeverityToPriority(string severity)
        {
            return severity.ToLower() switch
            {
                "critical" => NotificationPriority.Critical,
                "warning" => NotificationPriority.High,
                "caution" => NotificationPriority.Medium,
                "info" => NotificationPriority.Low,
                _ => NotificationPriority.Low
            };
        }
    }
}