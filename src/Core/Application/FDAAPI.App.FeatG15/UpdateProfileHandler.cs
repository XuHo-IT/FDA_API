using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Services.IServices;
using FDAAPI.Domain.RelationalDb.Repositories;

namespace FDAAPI.App.FeatG15
{
    /// <summary>
    /// Handler for updating user profile
    /// </summary>
    public class UpdateProfileHandler : IFeatureHandler<UpdateProfileRequest, UpdateProfileResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserProfileMapper _profileMapper;

        public UpdateProfileHandler(
            IUserRepository userRepository,
            IUserProfileMapper profileMapper)
        {
            _userRepository = userRepository;
            _profileMapper = profileMapper;
        }

        public async Task<UpdateProfileResponse> ExecuteAsync(
            UpdateProfileRequest request,
            CancellationToken ct)
        {
            try
            {
                // 1. Validate input
                if (string.IsNullOrWhiteSpace(request.FullName) && string.IsNullOrWhiteSpace(request.AvatarUrl))
                {
                    return new UpdateProfileResponse
                    {
                        Success = false,
                        Message = "At least one field (FullName or AvatarUrl) must be provided",
                        StatusCode = UpdateProfileResponseStatusCode.InvalidInput
                    };
                }

                // 2. Validate FullName length if provided
                if (!string.IsNullOrWhiteSpace(request.FullName) && request.FullName.Length > 255)
                {
                    return new UpdateProfileResponse
                    {
                        Success = false,
                        Message = "FullName must not exceed 255 characters",
                        StatusCode = UpdateProfileResponseStatusCode.InvalidInput
                    };
                }

                // 3. Validate AvatarUrl format if provided (must be HTTPS)
                if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
                {
                    if (!Uri.TryCreate(request.AvatarUrl, UriKind.Absolute, out var uri) ||
                        (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
                    {
                        return new UpdateProfileResponse
                        {
                            Success = false,
                            Message = "AvatarUrl must be a valid HTTP or HTTPS URL",
                            StatusCode = UpdateProfileResponseStatusCode.InvalidInput
                        };
                    }
                }

                // 4. Get user
                var user = await _userRepository.GetUserWithRolesAsync(request.UserId, ct);
                if (user == null)
                {
                    return new UpdateProfileResponse
                    {
                        Success = false,
                        Message = "User not found",
                        StatusCode = UpdateProfileResponseStatusCode.UserNotFound
                    };
                }

                // 5. Update fields (only update provided fields)
                bool hasChanges = false;
                if (!string.IsNullOrWhiteSpace(request.FullName) && user.FullName != request.FullName)
                {
                    user.FullName = request.FullName;
                    hasChanges = true;
                }

                if (!string.IsNullOrWhiteSpace(request.AvatarUrl) && user.AvatarUrl != request.AvatarUrl)
                {
                    user.AvatarUrl = request.AvatarUrl;
                    hasChanges = true;
                }

                if (!hasChanges)
                {
                    // No changes made, return current profile
                    var currentProfile = _profileMapper.MapToProfileDto(user);
                    return new UpdateProfileResponse
                    {
                        Success = true,
                        Message = "No changes detected",
                        StatusCode = UpdateProfileResponseStatusCode.Success,
                        Profile = currentProfile
                    };
                }

                // 6. Update audit fields
                user.UpdatedBy = request.UserId;
                user.UpdatedAt = DateTime.UtcNow;

                // 7. Save changes
                var updateResult = await _userRepository.UpdateAsync(user, ct);
                if (!updateResult)
                {
                    return new UpdateProfileResponse
                    {
                        Success = false,
                        Message = "Failed to update profile",
                        StatusCode = UpdateProfileResponseStatusCode.UnknownError
                    };
                }

                // 8. Reload user with roles to get updated data
                var updatedUser = await _userRepository.GetUserWithRolesAsync(request.UserId, ct);
                var profile = updatedUser != null ? _profileMapper.MapToProfileDto(updatedUser) : null;

                return new UpdateProfileResponse
                {
                    Success = true,
                    Message = "Profile updated successfully",
                    StatusCode = UpdateProfileResponseStatusCode.Success,
                    Profile = profile
                };
            }
            catch (Exception ex)
            {
                return new UpdateProfileResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = UpdateProfileResponseStatusCode.UnknownError
                };
            }
        }
    }
}

