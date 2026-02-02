using FastEndpoints;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.FeatG76_LogPrediction;
using FDAAPI.App.FeatG78_GetPredictionAccuracyStats;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG78_GetPredictionAccuracyStats.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG78_GetPredictionAccuracyStats
{
    public class GetPredictionAccuracyStatsEndpoint : Endpoint<GetPredictionAccuracyStatsRequestDto, GetPredictionAccuracyStatsResponseDto>
    {
        private readonly IMediator _mediator;

        public GetPredictionAccuracyStatsEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/predictions/accuracy-stats");
            Policies("User");
            Summary(s =>
            {
                s.Summary = "Get prediction accuracy statistics";
                s.Description = "Retrieves accuracy statistics for predictions";
            });
            Tags("Predictions", "Analytics");
        }

        public override async Task HandleAsync(GetPredictionAccuracyStatsRequestDto req, CancellationToken ct)
        {
            var request = new GetPredictionAccuracyStatsRequest(
                AreaId: req.AreaId,
                StartDate: req.StartDate,
                EndDate: req.EndDate,
                GroupBy: req.GroupBy
            );

            var result = await _mediator.Send(request, ct);

            var statusCode = result.StatusCode switch
            {
                PredictionLogStatusCode.Success => 200,
                PredictionLogStatusCode.BadRequest => 400,
                _ => 500
            };

            await SendAsync(new GetPredictionAccuracyStatsResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode,
                Data = result.Data
            }, statusCode, ct);
        }
    }
}

