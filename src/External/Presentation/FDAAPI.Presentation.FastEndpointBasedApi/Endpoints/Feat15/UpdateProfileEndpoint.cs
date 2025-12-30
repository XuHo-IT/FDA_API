using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG15;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat15.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat15
{
    /// <summary>
    /// Endpoint for updating current user profile
    /// </summary>
    public class UpdateProfileEndpoint : Endpoint<UpdateProfileRequestDto, UpdateProfileResponseDto>
    {
        private readonly IFeatureHandler<UpdateProfileRequest, UpdateProfileResponse> _handler;

        public UpdateProfileEndpoint(IFeatureHandler<UpdateProfileRequest, UpdateProfileResponse> handler)
        {
            _handler = handler;
        }

        public override void Configure()
        {
            Put("/api/v1/user-profile");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
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
                    Profile = new DTOs.UserProfileDto
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
                // Extract user ID from JWT claims
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

                // Map DTO to Request
                var appRequest = new UpdateProfileRequest
                {
                    UserId = userId,
                    FullName = req.FullName,
                    AvatarUrl = req.AvatarUrl
                };

                // Execute handler
                var result = await _handler.ExecuteAsync(appRequest, ct);

                // Map Response to DTO
                var response = new UpdateProfileResponseDto
                {
                    Success = result.Success,
                    Message = result.Message,
                    Profile = result.Profile != null ? new DTOs.UserProfileDto
                    {
                        Id = result.Profile.Id,
                        Email = result.Profile.Email,
                        FullName = result.Profile.FullName,
                        PhoneNumber = result.Profile.PhoneNumber,
                        AvatarUrl = result.Profile.AvatarUrl,
                        Provider = result.Profile.Provider,
                        Status = result.Profile.Status,
                        LastLoginAt = result.Profile.LastLoginAt,
                        PhoneVerifiedAt = result.Profile.PhoneVerifiedAt,
                        EmailVerifiedAt = result.Profile.EmailVerifiedAt,
                        Roles = result.Profile.Roles,
                        CreatedAt = result.Profile.CreatedAt,
                        UpdatedAt = result.Profile.UpdatedAt
                    } : null
                };

                // Send response with appropriate status code
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

