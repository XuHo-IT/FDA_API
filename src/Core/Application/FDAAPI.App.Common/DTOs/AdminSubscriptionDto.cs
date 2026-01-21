using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class AdminSubscriptionDto
    {
        public Guid SubscriptionId { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string? UserPhone { get; set; }
        public Guid? StationId { get; set; }
        public string? StationName { get; set; }
        public Guid? AreaId { get; set; }
        public string? AreaName { get; set; }
        public string MinSeverity { get; set; } = string.Empty;
        public bool EnablePush { get; set; }
        public bool EnableEmail { get; set; }
        public bool EnableSms { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
