using FastEndpoints;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.FeatG76_LogPrediction;
using FDAAPI.App.FeatG77_GetPredictionComparisons;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG77_GetPredictionComparisons.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG77_GetPredictionComparisons
{
    public class GetPredictionComparisonsEndpoint : Endpoint<GetPredictionComparisonsRequestDto, GetPredictionComparisonsResponseDto>
    {
        private readonly IMediator _mediator;

        public GetPredictionComparisonsEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/predictions/comparisons");
            Policies("User");
            Summary(s =>
            {
                s.Summary = "Get prediction comparisons";
                s.Description = "Retrieves paginated list of verified predictions with comparison to actual data";
            });
            Tags("Predictions", "Analytics");
        }

        public override async Task HandleAsync(GetPredictionComparisonsRequestDto req, CancellationToken ct)
        {
            var request = new GetPredictionComparisonsRequest(
                AreaId: req.AreaId,
                StartDate: req.StartDate,
                EndDate: req.EndDate,
                IsVerified: req.IsVerified,
                MinAccuracy: req.MinAccuracy,
                Page: req.Page,
                Size: req.Size
            );

            var result = await _mediator.Send(request, ct);

            var statusCode = result.StatusCode switch
            {
                PredictionLogStatusCode.Success => 200,
                PredictionLogStatusCode.BadRequest => 400,
                _ => 500
            };

            await SendAsync(new GetPredictionComparisonsResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode,
                Data = result.Data != null ? new GetPredictionComparisonsDataDto
                {
                    Total = result.Data.Total,
                    Items = result.Data.Items,
                    Summary = result.Data.Summary
                } : null
            }, statusCode, ct);
        }
    }
}

