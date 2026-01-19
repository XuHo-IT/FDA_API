using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class JobStatusDto
    {
        public Guid JobRunId { get; set; }
        public string JobType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public int? ExecutionTimeMs { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsCreated { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
