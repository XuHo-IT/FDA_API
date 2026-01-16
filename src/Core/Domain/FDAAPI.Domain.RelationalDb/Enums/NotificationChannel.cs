using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Enums
{
    /// <summary>
    /// Notification delivery channels
    /// </summary>
    public enum NotificationChannel
    {
        Push = 1,       // Mobile push notification
        Email = 2,      // Email notification
        SMS = 3,        // SMS (premium feature)
        InApp = 4       // In-app notification bell
    }
}
