using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Enums
{
    public enum SubscriptionTier
    {
        Free = 0,       // FREE plan - basic alerts
        Premium = 1,    // PRO plan - priority alerts
        Monitor = 2     // MONITOR plan - monitoring agencies (was Authority)
    }
}
