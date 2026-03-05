using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class NotificationSummaryDto
    {
        public int TotalCreated { get; set; }
        public int TotalSent { get; set; }
        public int TotalFailed { get; set; }
        public int TotalPending { get; set; }
        public Dictionary<string, ChannelStatsDto> ByChannel { get; set; } = new();
        public double AvgDeliveryTimeSeconds { get; set; }
        public int PendingRetries { get; set; }
    }
}
