using FDAAPI.Domain.RelationalDb.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class NotificationDetailDto
    {
        public Guid NotificationId { get; set; }
        public NotificationChannel Channel { get; set; }
        public string ChannelName => Channel.ToString();
        public string Status { get; set; } = string.Empty;
        public string StatusName => Status.ToString();
        public DateTime? SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
