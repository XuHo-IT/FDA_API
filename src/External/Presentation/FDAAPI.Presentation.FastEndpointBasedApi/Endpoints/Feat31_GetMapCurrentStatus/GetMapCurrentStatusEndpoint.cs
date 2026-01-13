using FastEndpoints;
using FDAAPI.App.Common.Models.Map;
using FDAAPI.App.FeatG31_GetMapCurrentStatus;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat31_GetMapCurrentStatus.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat31_GetMapCurrentStatus
{
    public class GetMapCurrentStatusEndpoint : Endpoint<GetMapCurrentStatusRequestDto, GetMapCurrentStatusResponseDto>
    {
        private readonly IMediator _mediator;

        public GetMapCurrentStatusEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/map/current-status");
            AllowAnonymous();
            Summary(s => {
                s.Summary = "Get current flood status of all stations";
                s.Description = "Returns GeoJSON FeatureCollection with latest sensor readings for map visualization";
                s.ExampleRequest = new GetMapCurrentStatusRequestDto
                {
                    MinLat = 10.5m,
                    MaxLat = 11.0m,
                    MinLng = 106.5m,
                    MaxLng = 107.0m,
                    Status = "active"
                };
            });
            Tags("Map", "FloodData", "GeoJSON");
        }

        public override async Task HandleAsync(GetMapCurrentStatusRequestDto req, CancellationToken ct)
        {
            var query = new GetMapCurrentStatusRequest(
                req.MinLat,
                req.MaxLat,
                req.MinLng,
                req.MaxLng,
                req.Status
            );

            var result = await _mediator.Send(query, ct);

            var statusCode = result.StatusCode switch
            {
                GetMapCurrentStatusResponseStatusCode.Success => 200,
                GetMapCurrentStatusResponseStatusCode.NoDataFound => 404,
                GetMapCurrentStatusResponseStatusCode.ValidationError => 400,
                _ => 500
            };

            var responseDto = new GetMapCurrentStatusResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Data
            };

            await SendAsync(responseDto, statusCode, ct);
        }
    }
}