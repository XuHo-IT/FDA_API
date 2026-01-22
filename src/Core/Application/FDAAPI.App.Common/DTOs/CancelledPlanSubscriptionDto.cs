using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class CancelledPlanSubscriptionDto
    {
        public Guid SubscriptionId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string PreviousTier { get; set; } = string.Empty;
        public DateTime CancelledAt { get; set; }
        public string? CancelReason { get; set; }
    }
}
