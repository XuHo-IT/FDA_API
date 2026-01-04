using FastEndpoints;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG15;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat15.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat15
{
    public class UpdateProfileEndpoint : Endpoint<UpdateProfileRequestDto, UpdateProfileResponseDto>
    {
        private readonly IMediator _mediator;

        public UpdateProfileEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Put("/api/v1/user-profile");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            AllowFileUploads();
            Summary(s =>
            {
                s.Summary = "Update current user profile";
                s.Description = "Updates the authenticated user's profile information. Only FullName and AvatarUrl can be updated. At least one field must be provided.";
                s.ExampleRequest = new UpdateProfileRequestDto
                {
                    FullName = "John Doe",
                    AvatarUrl = "https://example.com/avatar.jpg"
                };
                s.ResponseExamples[200] = new UpdateProfileResponseDto
                {
                    Success = true,
                    Message = "Profile updated successfully",
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

        public override async Task HandleAsync(UpdateProfileRequestDto req, CancellationToken ct)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    await SendAsync(new UpdateProfileResponseDto
                    {
                        Success = false,
                        Message = "Invalid user authentication"
                    }, 401, ct);
                    return;
                }

                var command = new UpdateProfileRequest(userId, req.FullName, req.AvatarFile, req.AvatarUrl);

                var result = await _mediator.Send(command, ct);

                var response = new UpdateProfileResponseDto
                {
                    Success = result.Success,
                    Message = result.Message,
                    Profile = result.Profile
                };

                var statusCode = result.StatusCode switch
                {
                    UpdateProfileResponseStatusCode.Success => 200,
                    UpdateProfileResponseStatusCode.UserNotFound => 404,
                    UpdateProfileResponseStatusCode.InvalidInput => 400,
                    _ => 500
                };

                await SendAsync(response, statusCode, ct);
            }
            catch (Exception ex)
            {
                var errorDto = new UpdateProfileResponseDto
                {
                    Success = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
                await SendAsync(errorDto, 500, ct);
            }
        }
    }
}
