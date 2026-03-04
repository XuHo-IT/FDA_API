using FastEndpoints;
using FDAAPI.App.FeatG81_FloodReportGet;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat81_FloodReportGet.DTOs;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat81_FloodReportGet
{
    public class GetFloodReportEndpoint : Endpoint<EmptyRequest, GetFloodReportResponseDto>
    {
        private readonly IMediator _mediator;

        public GetFloodReportEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/flood-reports/{id}");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get a flood report by ID";
                s.Description = "Retrieve a single flood report with all its media";
            });
            Description(b => b
                .Produces(200)
                .Produces(404));
            Tags("FloodReports");
        }

        public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
        {
            var reportId = Route<Guid>("id");

            var request = new GetFloodReportRequest(reportId);
            var result = await _mediator.Send(request, ct);

            var statusCode = result.Success ? 200 : 404;

            await SendAsync(new GetFloodReportResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Id = result.Id,
                ReporterUserId = result.ReporterUserId,
                Latitude = result.Latitude,
                Longitude = result.Longitude,
                Address = result.Address,
                Description = result.Description,
                Severity = result.Severity,
                TrustScore = result.TrustScore,
                Status = result.Status,
                ConfidenceLevel = result.ConfidenceLevel,
                Priority = result.Priority,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt,
                Media = result.Media?.ConvertAll(m => new FloodReportMediaDto
                {
                    Id = m.Id,
                    MediaType = m.MediaType,
                    MediaUrl = m.MediaUrl,
                    ThumbnailUrl = m.ThumbnailUrl,
                    CreatedAt = m.CreatedAt
                }) ?? new()
            }, statusCode, ct);
        }
    }
}
