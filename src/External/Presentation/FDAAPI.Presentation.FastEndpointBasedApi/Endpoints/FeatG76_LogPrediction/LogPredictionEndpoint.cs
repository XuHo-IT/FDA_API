using FastEndpoints;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.FeatG76_LogPrediction;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG76_LogPrediction.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG76_LogPrediction
{
    public class LogPredictionEndpoint : Endpoint<LogPredictionRequestDto, LogPredictionResponseDto>
    {
        private readonly IMediator _mediator;

        public LogPredictionEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/api/v1/internal/log-prediction");
            AllowAnonymous(); // Internal API - can add API Key auth later
            Summary(s =>
            {
                s.Summary = "Log AI prediction";
                s.Description = "Internal API endpoint for AI system to log flood predictions";
            });
            Tags("Internal", "Predictions");
        }

        public override async Task HandleAsync(LogPredictionRequestDto req, CancellationToken ct)
        {
            var request = new LogPredictionRequest(
                AreaId: req.AreaId,
                PredictedProb: req.PredictedProb,
                AiProb: req.AiProb,
                PhysicsProb: req.PhysicsProb,
                RiskLevel: req.RiskLevel,
                StartTime: req.StartTime,
                EndTime: req.EndTime
            );

            var result = await _mediator.Send(request, ct);

            var statusCode = result.StatusCode switch
            {
                PredictionLogStatusCode.Created => 201,
                PredictionLogStatusCode.NotFound => 404,
                PredictionLogStatusCode.BadRequest => 400,
                _ => 500
            };

            await SendAsync(new LogPredictionResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode,
                Data = result.Data
            }, statusCode, ct);
        }
    }
}

