using FastEndpoints;
using FDAAPI.App.FeatG52_GetSeverityAnalytics;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat52_GetSeverityAnalytics.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat52_GetSeverityAnalytics
{
    public class GetSeverityAnalyticsEndpoint : Endpoint<GetSeverityAnalyticsRequestDto, GetSeverityAnalyticsResponseDto>
    {
        private readonly IMediator _mediator;

        public GetSeverityAnalyticsEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/analytics/severity");
            Policies("User");
            Summary(s => s.Summary = "Get severity analytics data");
            Tags("Analytics", "Severity");
        }

        public override async Task HandleAsync(GetSeverityAnalyticsRequestDto req, CancellationToken ct)
        {
            var query = new GetSeverityAnalyticsRequest(
                req.AdministrativeAreaId,
                req.StartDate,
                req.EndDate,
                req.BucketType
            );

            var result = await _mediator.Send(query, ct);

            var response = new GetSeverityAnalyticsResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode,
                Data = result.Data != null ? new SeverityAnalyticsDto
                {
                    AdministrativeAreaId = result.Data.AdministrativeAreaId,
                    AdministrativeAreaName = result.Data.AdministrativeAreaName,
                    BucketType = result.Data.BucketType,
                    DataPoints = result.Data.DataPoints.Select(dp => new SeverityDataPointDto
                    {
                        TimeBucket = dp.TimeBucket,
                        MaxLevel = dp.MaxLevel,
                        AvgLevel = dp.AvgLevel,
                        MinLevel = dp.MinLevel,
                        DurationHours = dp.DurationHours,
                        ReadingCount = dp.ReadingCount,
                        CalculatedAt = dp.CalculatedAt
                    }).ToList()
                } : null
            };

            await SendAsync(response, (int)result.StatusCode, ct);
        }
    }
}

