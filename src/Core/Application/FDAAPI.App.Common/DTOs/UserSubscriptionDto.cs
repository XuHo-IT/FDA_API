using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class UserSubscriptionDto
    {
        public Guid SubscriptionId { get; set; }
        public Guid? StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public Guid? AreaId { get; set; }
        public string? AreaName { get; set; }
        public string MinSeverity { get; set; } = string.Empty;
        public bool EnablePush { get; set; }
        public bool EnableEmail { get; set; }
        public bool EnableSms { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}
