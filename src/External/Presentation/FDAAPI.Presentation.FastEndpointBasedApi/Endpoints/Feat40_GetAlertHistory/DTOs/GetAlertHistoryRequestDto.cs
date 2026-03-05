using Microsoft.AspNetCore.Mvc;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat40_GetAlertHistory.DTOs
{
    public class GetAlertHistoryRequestDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Severity { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}