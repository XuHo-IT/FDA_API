namespace FDAAPI.App.Common.DTOs
{
    public class PlanSubscriptionDto
    {
        public Guid SubscriptionId { get; set; }
        public string PlanCode { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string Tier { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "active";
    }
}
