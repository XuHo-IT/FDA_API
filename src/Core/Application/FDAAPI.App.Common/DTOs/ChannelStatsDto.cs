using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class ChannelStatsDto
    {
        public int Sent { get; set; }
        public int Failed { get; set; }
        public double SuccessRate { get; set; }
    }
}
