using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG83_FloodReportList
{
    public sealed class ListFloodReportsHandler : IRequestHandler<ListFloodReportsRequest, ListFloodReportsResponse>
    {
        private readonly IFloodReportRepository _reportRepository;

        public ListFloodReportsHandler(IFloodReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<ListFloodReportsResponse> Handle(ListFloodReportsRequest request, CancellationToken ct)
        {
            try
            {
                var (items, total) = await _reportRepository.ListAsync(
                    request.Status,
                    request.Severity,
                    request.From,
                    request.To,
                    request.PageNumber,
                    request.PageSize,
                    ct);

                return new ListFloodReportsResponse
                {
                    Success = true,
                    Message = "Flood reports retrieved successfully",
                    TotalCount = total,
                    Items = items.Select(r => new FloodReportListItem
                    {
                        Id = r.Id,
                        Latitude = r.Latitude,
                        Longitude = r.Longitude,
                        Address = r.Address,
                        Description = r.Description,
                        Severity = r.Severity,
                        TrustScore = r.TrustScore,
                        Status = r.Status,
                        ConfidenceLevel = r.ConfidenceLevel,
                        CreatedAt = r.CreatedAt,
                        Media = r.Media?.Select(m => new FloodReportMediaDto
                        {
                            Id = m.Id,
                            MediaType = m.MediaType,
                            MediaUrl = m.MediaUrl,
                            ThumbnailUrl = m.ThumbnailUrl,
                            CreatedAt = m.CreatedAt
                        }).ToList() ?? new List<FloodReportMediaDto>()
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                return new ListFloodReportsResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
    }
}


