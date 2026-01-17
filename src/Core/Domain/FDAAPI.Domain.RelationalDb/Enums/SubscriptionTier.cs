using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Enums
{
    /// <summary>
    /// User subscription tiers (matches pricing_plans.code)
    /// </summary>
    public enum SubscriptionTier
    {
        Free = 0,       // FREE plan - basic alerts, push only
        Premium = 1,    // PRO plan - all alerts, push + email + SMS
        Authority = 2   // GOV plan - priority routing, all channels, no limits
    }
}
