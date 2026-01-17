using FDAAPI.Domain.RelationalDb.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class AlertHistoryDto
    {
        public Guid AlertId { get; set; }
        public Guid StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string StationCode { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;
        public NotificationPriority Priority { get; set; }
        public decimal WaterLevel { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime TriggeredAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string Status { get; set; } = string.Empty; // open, resolved

        // Notification details
        public List<NotificationDetailDto> Notifications { get; set; } = new();
    }
}
