using FastEndpoints;
using FDAAPI.App.FeatG33_AreaListByUser;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat33_AreaListByUser.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat33_AreaListByUser
{
    public class AreaListByUserEndpoint : Endpoint<AreaListByUserRequestDto, AreaListByUserResponseDto>
    {
        private readonly IMediator _mediator;

        public AreaListByUserEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/areas-created");
            Policies("Authority");
            Summary(s =>
            {
                s.Summary = "List monitored areas created by that person";
                s.Description = "Retrieve a paginated list of geographic areas for the current user";
            });
            Tags("Area");
        }

        public override async Task HandleAsync(AreaListByUserRequestDto req, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null)
            {
                await SendAsync(new AreaListByUserResponseDto
                {
                    Success = false,
                    Message = "Unauthorized",
                    StatusCode = 401
                }, 401, ct);
                return;
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var query = new AreaListByUserRequest(userId, req.SearchTerm, req.PageNumber, req.PageSize);

            var result = await _mediator.Send(query, ct);

            var response = new AreaListByUserResponseDto
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
