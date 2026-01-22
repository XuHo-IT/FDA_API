using FDAAPI.App.Common.DTOs;

namespace FDAAPI.App.FeatG70_AdminGetAlertStats
{
    public class AdminGetAlertStatsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AlertStatsDataDto? Data { get; set; }
    }
}