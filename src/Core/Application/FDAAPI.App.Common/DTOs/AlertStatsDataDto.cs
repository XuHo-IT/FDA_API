using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class AlertStatsDataDto
    {
        public PeriodDto Period { get; set; } = new();
        public AlertSummaryDto Alerts { get; set; } = new();
        public NotificationSummaryDto Notifications { get; set; } = new();
        public UserSummaryDto Users { get; set; } = new();
    }
}
