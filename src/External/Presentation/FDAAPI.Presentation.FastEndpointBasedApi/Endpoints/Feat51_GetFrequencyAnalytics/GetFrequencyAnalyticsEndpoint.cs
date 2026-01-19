using FastEndpoints;
using FDAAPI.App.FeatG51_GetFrequencyAnalytics;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat51_GetFrequencyAnalytics.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat51_GetFrequencyAnalytics
{
    public class GetFrequencyAnalyticsEndpoint : Endpoint<GetFrequencyAnalyticsRequestDto, GetFrequencyAnalyticsResponseDto>
    {
        private readonly IMediator _mediator;

        public GetFrequencyAnalyticsEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/analytics/frequency");
            Policies("User");
            Summary(s => s.Summary = "Get frequency analytics data");
            Tags("Analytics", "Frequency");
        }

        public override async Task HandleAsync(GetFrequencyAnalyticsRequestDto req, CancellationToken ct)
        {
            var query = new GetFrequencyAnalyticsRequest(
                req.AdministrativeAreaId,
                req.StartDate,
                req.EndDate,
                req.BucketType
            );

            var result = await _mediator.Send(query, ct);

            var response = new GetFrequencyAnalyticsResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode,
                Data = result.Data != null ? new FrequencyAnalyticsDto
                {
                    AdministrativeAreaId = result.Data.AdministrativeAreaId,
                    AdministrativeAreaName = result.Data.AdministrativeAreaName,
                    BucketType = result.Data.BucketType,
                    DataPoints = result.Data.DataPoints.Select(dp => new FrequencyDataPointDto
                    {
                        TimeBucket = dp.TimeBucket,
                        EventCount = dp.EventCount,
                        ExceedCount = dp.ExceedCount,
                        CalculatedAt = dp.CalculatedAt
                    }).ToList()
                } : null
            };

            await SendAsync(response, (int)result.StatusCode, ct);
        }
    }
}

