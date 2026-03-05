namespace FDAAPI.Infra.Services.Alerts
{
    /// <summary>
    /// Implementation of global threshold service
    /// Global thresholds:
    /// - 0-10cm: safe (level 0)
    /// - 10-20cm: caution (level 1)
    /// - 20-40cm: warning (level 2)
    /// - 40+cm: critical (level 3)
    /// </summary>
    public class GlobalThresholdService : IGlobalThresholdService
    {
        // Global thresholds (cm)
        private readonly Dictionary<string, (decimal min, decimal max)> _thresholds = new()
        {
            { "safe", (0, 10) },
            { "caution", (10, 20) },
            { "warning", (20, 40) },
            { "critical", (40, decimal.MaxValue) }
        };

        public (string severity, int severityLevel) CalculateSeverity(decimal waterLevelCm)
        {
            if (waterLevelCm >= 40) return ("critical", 3);
            if (waterLevelCm >= 20) return ("warning", 2);
            if (waterLevelCm >= 10) return ("caution", 1);
            return ("safe", 0);
        }

        public decimal GetThresholdForSeverity(string severity)
        {
            return severity.ToLower() switch
            {
                "safe" => 0,
                "caution" => 10,
                "warning" => 20,
                "critical" => 40,
                _ => throw new ArgumentException($"Unknown severity: {severity}")
            };
        }
    }
}