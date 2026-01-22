namespace FDAAPI.Infra.Services.Alerts
{
    public interface IGlobalThresholdService
    {
        /// <summary>
        /// Calculate severity level from water level in centimeters
        /// </summary>
        /// <param name="waterLevelCm">Water level in centimeters</param>
        /// <returns>Tuple of (severity name, severity level number)</returns>
        (string severity, int severityLevel) CalculateSeverity(decimal waterLevelCm);

        /// <summary>
        /// Get threshold value for a specific severity level
        /// </summary>
        /// <param name="severity">Severity name (safe, caution, warning, critical)</param>
        /// <returns>Minimum water level threshold in centimeters</returns>
        decimal GetThresholdForSeverity(string severity);
    }
}