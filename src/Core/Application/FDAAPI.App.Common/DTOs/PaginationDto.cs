using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    /// <summary>
    /// Pagination information for large result sets
    /// </summary>
    public class PaginationDto
    {
        public bool HasMore { get; set; }
        public string? NextCursor { get; set; }
        public int TotalCount { get; set; }
        public int? CurrentPage { get; set; }
        public int? PageSize { get; set; }
        public int? TotalPages { get; set; }
    }
}
