using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG84_FloodReportGetNearby
{
    public sealed class GetNearbyFloodReportsHandler : IRequestHandler<GetNearbyFloodReportsRequest, GetNearbyFloodReportsResponse>
    {
        private readonly IFloodReportRepository _reportRepository;

        public GetNearbyFloodReportsHandler(IFloodReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<GetNearbyFloodReportsResponse> Handle(GetNearbyFloodReportsRequest request, CancellationToken ct)
        {
            try
            {
                var timeWindow = TimeSpan.FromHours(request.Hours <= 0 ? 2 : request.Hours);
                var radius = request.RadiusMeters <= 0 ? 500 : request.RadiusMeters;

                var reports = await _reportRepository.FindNearbyPublishedReportsAsync(
                    request.Latitude,
                    request.Longitude,
                    radius,
                    timeWindow,
                    ct);

                var count = reports.Count;
                var consensusLevel = DetermineConsensusLevel(count);
                var message = GenerateConsensusMessage(count, request.Hours <= 0 ? 2 : request.Hours);

                var response = new GetNearbyFloodReportsResponse
                {
                    Success = true,
                    Message = "Nearby flood reports retrieved successfully",
                    Count = count,
                    ConsensusLevel = consensusLevel,
                    ConsensusMessage = message,
                    Reports = reports.Select(r => new NearbyFloodReportItem
                    {
                        Id = r.Id,
                        Latitude = r.Latitude,
                        Longitude = r.Longitude,
                        Severity = r.Severity,
                        CreatedAt = r.CreatedAt,
                        DistanceMeters = 0 // Optional: compute on app side if needed
                    }).ToList()
                };

                return response;
            }
            catch (Exception ex)
            {
                return new GetNearbyFloodReportsResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        private string DetermineConsensusLevel(int count)
        {
            return count switch
            {
                0 => "none",
                1 => "low",
                >= 2 and <= 3 => "moderate",
                >= 4 => "strong",
                _ => "none"
            };
        }

        private string GenerateConsensusMessage(int count, int hours)
        {
            if (count == 0)
                return "No other reports in this area";

            var unit = count == 1 ? "report" : "reports";
            return $"This area has {count} other {unit} in the last {hours} hours";
        }
    }
}


