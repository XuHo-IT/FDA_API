using FastEndpoints;
using FDAAPI.App.FeatG33_AreaList;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat33_AreaList.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat33_AreaList
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
                s.Summary = "List user's monitored areas";
                s.Description = "Retrieve a paginated list of geographic areas for the current user";
            });
            Tags("Area");
        }

        public override async Task HandleAsync(AreaListRequestDto req, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null)
            {
                await SendAsync(new AreaListResponseDto
                {
                    Success = false,
                    Message = "Unauthorized",
                    StatusCode = 401
                }, 401, ct);
                return;
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var query = new AreaListRequest(userId, req.SearchTerm, req.PageNumber, req.PageSize);

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
