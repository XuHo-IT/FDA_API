using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.FloodHistory;

namespace FDAAPI.App.FeatG40_GetFloodTrends
{
    public class GetFloodTrendsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public FloodHistoryStatusCode StatusCode { get; set; }
        public FloodTrendDto? Data { get; set; }
    }
}
