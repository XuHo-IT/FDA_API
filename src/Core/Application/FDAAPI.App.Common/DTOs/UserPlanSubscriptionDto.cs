using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class UserPlanSubscriptionDto
    {
        public string Tier { get; set; } = "Free"; // Free, Premium, Monitor
        public string TierCode { get; set; } = "FREE";
        public string PlanName { get; set; } = "Free Plan";
        public string? Description { get; set; }
        public decimal PriceMonth { get; set; }
        public decimal PriceYear { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = "active";
        public List<string> AvailableChannels { get; set; } = new();
        public DispatchDelayDto DispatchDelay { get; set; } = new();
        public int MaxRetries { get; set; }
    }
}
