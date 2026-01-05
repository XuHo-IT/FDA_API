using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat14_ProfileGet.DTOs;
using FastEndpoints;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG14_ProfileGet;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat14_ProfileGet;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat14_ProfileGet{
    public class GetProfileEndpoint : EndpointWithoutRequest<GetProfileResponseDto>
    {
        private readonly IMediator _mediator;

        public GetProfileEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Get("/api/v1/user-profile");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Summary(s =>
            {
                s.Summary = "Get current user profile";
                s.Description = "Retrieves the authenticated user's profile information including roles, verification status, and account details.";
                s.ResponseExamples[200] = new GetProfileResponseDto
                {
                    Success = true,
                    Message = "Profile retrieved successfully",
                    Profile = new UserProfileDto
                    {
                        Id = Guid.NewGuid(),
                        Email = "user@example.com",
                        FullName = "John Doe",
                        PhoneNumber = "+84901234567",
                        AvatarUrl = "https://example.com/avatar.jpg",
                        Provider = "local",
                        Status = "ACTIVE",
                        Roles = new List<string> { "USER" },
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };
            });
            Tags("Profile", "User Management");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            try
            {
                var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    await SendAsync(new GetProfileResponseDto
                    {
                        Success = false,
                        Message = "Invalid user authentication"
                    }, 401, ct);
                    return;
                }

                var request = new GetProfileRequest(userId);

                var result = await _mediator.Send(request, ct);

                var response = new GetProfileResponseDto
                {
                    Success = result.Success,
                    Message = result.Message,
                    Profile = result.Profile
                };

                var statusCode = result.StatusCode switch
                {
                    GetProfileResponseStatusCode.Success => 200,
                    GetProfileResponseStatusCode.UserNotFound => 404,
                    _ => 500
                };

                await SendAsync(response, statusCode, ct);
            }
            catch (Exception ex)
            {
                var errorDto = new GetProfileResponseDto
                {
                    Success = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
                await SendAsync(errorDto, 500, ct);
            }
        }
    }
}









