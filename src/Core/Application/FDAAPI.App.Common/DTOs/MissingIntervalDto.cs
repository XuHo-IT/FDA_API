using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    /// <summary>
    /// Represents a gap in sensor data
    /// </summary>
    public class MissingIntervalDto
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int DurationMinutes { get; set; }
    }
}
