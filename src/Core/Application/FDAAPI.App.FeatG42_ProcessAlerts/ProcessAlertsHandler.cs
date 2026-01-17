using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Enums;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG42_ProcessAlerts
{
    public class ProcessAlertsHandler : IRequestHandler<ProcessAlertsRequest, ProcessAlertsResponse>
    {
        private readonly IAlertRuleRepository _ruleRepo;
        private readonly ISensorReadingRepository _sensorRepo;
        private readonly IAlertRepository _alertRepo;

        public ProcessAlertsHandler(
            IAlertRuleRepository ruleRepo,
            ISensorReadingRepository sensorRepo,
            IAlertRepository alertRepo)
        {
            _ruleRepo = ruleRepo;
            _sensorRepo = sensorRepo;
            _alertRepo = alertRepo;
        }

        public async Task<ProcessAlertsResponse> Handle(ProcessAlertsRequest request, CancellationToken ct)
        {
            int created = 0, updated = 0;

            try
            {
                // 1. Get all active rules
                var rules = await _ruleRepo.GetActiveRulesAsync(ct);

                foreach (var rule in rules)
                {
                    // 2. Get latest sensor reading for this station
                    var readings = await _sensorRepo.GetLatestReadingsByStationsAsync(
                        new[] { rule.StationId }, ct);

                    var latestReading = readings.FirstOrDefault();
                    if (latestReading == null) continue;

                    // 3. Check if threshold exceeded
                    bool thresholdExceeded = rule.RuleType.ToLower() switch
                    {
                        "threshold" => (decimal)latestReading.Value >= rule.ThresholdValue,
                        _ => false
                    };

                    if (!thresholdExceeded) continue;

                    // 4. Check if alert already exists (open)
                    var existingAlerts = await _alertRepo.GetActiveAlertsByStationAsync(
                        rule.StationId, ct);

                    var existingAlert = existingAlerts
                        .FirstOrDefault(a => a.AlertRuleId == rule.Id && a.Status == "open");

                    if (existingAlert != null)
                    {
                        // Update existing alert
                        existingAlert.CurrentValue = (decimal)latestReading.Value;
                        existingAlert.UpdatedAt = DateTime.UtcNow;
                        existingAlert.UpdatedBy = Guid.Empty; // System user
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
                            CreatedBy = systemUserId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedBy = systemUserId,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _alertRepo.CreateAsync(newAlert, ct);
                        created++;
                    }
                }

                // 5. Get pending alerts count
                var pendingAlerts = await _alertRepo.GetUnnotifiedAlertsAsync(1000, ct);

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
                _ => NotificationPriority.Low
            };
        }
    }
}