using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class FrequencyDataPointDto
    {
        public DateTime TimeBucket { get; set; }
        public int EventCount { get; set; }
        public int ExceedCount { get; set; }
        public DateTime CalculatedAt { get; set; }
    }
}
