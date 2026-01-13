using FastEndpoints;
using FDAAPI.App.FeatG38_AreaList;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat33_AreaListByUser.DTOs;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat38_AreaList.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat38_AreaList
{
    public class AreaListEndpoint : Endpoint<AreaListRequestDto, AreaListResponseDto>
    {
        private readonly IMediator _mediator;

        public AreaListEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/areas");
            Policies("User");
            Summary(s =>
            {
                s.Summary = "List all areas";
                s.Description = "Retrieve a paginated list of geographic areas created by administrators";
            });
            Tags("Area");
        }

        public override async Task HandleAsync(AreaListRequestDto req, CancellationToken ct)
        {
            var query = new AreaListRequest(req.SearchTerm, req.PageNumber, req.PageSize);

            var result = await _mediator.Send(query, ct);

            var response = new AreaListResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode,
                Areas = result.Areas.Select(a => new AreaDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Latitude = a.Latitude,
                    Longitude = a.Longitude,
                    RadiusMeters = a.RadiusMeters,
                    AddressText = a.AddressText
                }).ToList(),
                TotalCount = result.TotalCount
            };

            await SendAsync(response, (int)result.StatusCode, ct);
        }
    }
}

