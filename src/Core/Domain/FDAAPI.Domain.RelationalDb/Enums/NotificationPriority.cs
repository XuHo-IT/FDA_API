using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Enums
{
    /// <summary>
    /// Priority levels for flood notifications
    /// Higher priority = faster delivery + more channels
    /// </summary>
    public enum NotificationPriority
    {
        Low = 0,        // Normal updates, push only
        Medium = 1,     // Caution level, push + email
        High = 2,       // Warning level, push + email + SMS (premium)
        Critical = 3    // Critical flooding, all channels + retry
    }
}
