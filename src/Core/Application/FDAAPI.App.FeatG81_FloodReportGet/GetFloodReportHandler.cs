using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG81_FloodReportGet
{
    public sealed class GetFloodReportHandler : IRequestHandler<GetFloodReportRequest, GetFloodReportResponse>
    {
        private readonly IFloodReportRepository _reportRepository;
        private readonly IFloodReportMediaRepository _mediaRepository;

        public GetFloodReportHandler(
            IFloodReportRepository reportRepository,
            IFloodReportMediaRepository mediaRepository)
        {
            _reportRepository = reportRepository;
            _mediaRepository = mediaRepository;
        }

        public async Task<GetFloodReportResponse> Handle(GetFloodReportRequest request, CancellationToken ct)
        {
            try
            {
                var report = await _reportRepository.GetByIdAsync(request.Id, ct);
                if (report == null)
                {
                    return new GetFloodReportResponse
                    {
                        Success = false,
                        Message = "Flood report not found"
                    };
                }

                var media = await _mediaRepository.GetByReportIdAsync(report.Id, ct);

                var response = new GetFloodReportResponse
                {
                    Success = true,
                    Message = "Flood report retrieved successfully",
                    Id = report.Id,
                    ReporterUserId = report.ReporterUserId,
                    Latitude = report.Latitude,
                    Longitude = report.Longitude,
                    Address = report.Address,
                    Description = report.Description,
                    Severity = report.Severity,
                    TrustScore = report.TrustScore,
                    Status = report.Status,
                    ConfidenceLevel = report.ConfidenceLevel,
                    Priority = report.Priority,
                    CreatedAt = report.CreatedAt,
                    UpdatedAt = report.UpdatedAt,
                    Media = media.Select(m => new FloodReportMediaItem
                    {
                        Id = m.Id,
                        MediaType = m.MediaType,
                        MediaUrl = m.MediaUrl,
                        ThumbnailUrl = m.ThumbnailUrl,
                        CreatedAt = m.CreatedAt
                    }).ToList()
                };

                return response;
            }
            catch (Exception ex)
            {
                return new GetFloodReportResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
    }
}


