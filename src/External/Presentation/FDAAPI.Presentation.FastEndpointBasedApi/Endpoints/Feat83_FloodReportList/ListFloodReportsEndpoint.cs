using FastEndpoints;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat83_FloodReportList.DTOs;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat83_FloodReportList
{
    public class ListFloodReportsEndpoint : Endpoint<ListFloodReportsRequestDto, ListFloodReportsResponseDto>
    {
        private readonly IMediator _mediator;

        public ListFloodReportsEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/flood-reports");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "List flood reports";
                s.Description = "Retrieve paginated list of flood reports with optional filters";
            });
            Description(b => b.Produces(200));
            Tags("FloodReports");
        }

        public override async Task HandleAsync(ListFloodReportsRequestDto req, CancellationToken ct)
        {
            var request = new App.FeatG83_FloodReportList.ListFloodReportsRequest(
                req.Status,
                req.Severity,
                req.From,
                req.To,
                req.PageNumber,
                req.PageSize
            );

            var result = await _mediator.Send(request, ct);

            await SendAsync(new ListFloodReportsResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                TotalCount = result.TotalCount,
                Items = result.Items?.ConvertAll(item => new FloodReportListItemDto
                {
                    Id = item.Id,
                    Latitude = item.Latitude,
                    Longitude = item.Longitude,
                    Address = item.Address,
                    Description = item.Description,
                    Severity = item.Severity,
                    TrustScore = item.TrustScore,
                    Status = item.Status,
                    ConfidenceLevel = item.ConfidenceLevel,
                    CreatedAt = item.CreatedAt,
                    Media = item.Media?.ConvertAll(m => new FloodReportMediaDto
                    {
                        Id = m.Id,
                        MediaType = m.MediaType,
                        MediaUrl = m.MediaUrl,
                        ThumbnailUrl = m.ThumbnailUrl,
                        CreatedAt = m.CreatedAt
                    }) ?? new List<FloodReportMediaDto>()
                }) ?? new()
            }, 200, ct);
        }
    }
}
