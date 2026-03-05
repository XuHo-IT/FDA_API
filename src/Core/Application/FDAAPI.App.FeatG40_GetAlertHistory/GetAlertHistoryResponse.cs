using FDAAPI.App.Common.DTOs;

namespace FDAAPI.App.FeatG40_GetAlertHistory
{
    public class GetAlertHistoryResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<AlertHistoryDto> Alerts { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}