using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class UserSummaryDto
    {
        public int TotalSubscribers { get; set; }
        public int ActiveSubscribers { get; set; }
        public int NewSubscribers24h { get; set; }
    }
}
