using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Routing;

namespace FDAAPI.App.Common.Services.Mapping
{
    public class SafeRouteMapper : ISafeRouteMapper
    {
        public RouteDto MapToRouteDto(GraphHopperPath path, List<FloodWarningDto> warnings)
        {
            return new RouteDto
            {
                Geometry = path.Geometry,
                DistanceMeters = (decimal)path.Distance,
                DurationSeconds = (int)(path.Time / 1000),
                Instructions = path.Instructions.Select(i => new RouteInstructionDto
                {
                    Distance = (decimal)i.Distance,
                    Time = i.Time,
                    Text = i.Text
                }).ToList(),
                FloodRiskScore = CalculateFloodRiskScore(warnings)
            };
        }

        private decimal CalculateFloodRiskScore(List<FloodWarningDto> warnings)
        {
            if (!warnings.Any()) return 0;

            var criticalCount = warnings.Count(w => w.Severity == "critical");
            var warningCount = warnings.Count(w => w.Severity == "warning");
            var cautionCount = warnings.Count(w => w.Severity == "caution");

            var score = (criticalCount * 40) + (warningCount * 20) + (cautionCount * 10);
            return Math.Min(100, score);
        }
    }
}
