namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat72_SubscribeToPlan.DTOs
{
    public class SubscribeToPlanRequestDto
    {
        public string PlanCode { get; set; } = "FREE"; // FREE, PREMIUM, MONITOR
        public int DurationMonths { get; set; } = 12;
    }
}