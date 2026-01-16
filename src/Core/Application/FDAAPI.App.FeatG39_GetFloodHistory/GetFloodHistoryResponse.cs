using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.FloodHistory;

namespace FDAAPI.App.FeatG39_GetFloodHistory
{
    public class GetFloodHistoryResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public FloodHistoryStatusCode StatusCode { get; set; }
        public FloodHistoryDto? Data { get; set; }
        public PaginationDto? Pagination { get; set; }
    }
}
