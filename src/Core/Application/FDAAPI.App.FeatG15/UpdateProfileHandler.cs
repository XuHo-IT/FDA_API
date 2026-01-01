using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Services.IServices;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.AspNetCore.Http; // Added for IFormFile and related functionalities
using System.IO; // Added for Path.GetExtension

namespace FDAAPI.App.FeatG15
{
    /// <summary>
    /// Handler for updating user profile
    /// </summary>
    public class UpdateProfileHandler : IFeatureHandler<UpdateProfileRequest, UpdateProfileResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserProfileMapper _profileMapper;
        private readonly IImageStorageService _imageKitService; 

        public UpdateProfileHandler(
            IUserRepository userRepository,
            IUserProfileMapper profileMapper,
            IImageStorageService imageKitService) 
        {
            _userRepository = userRepository;
            _profileMapper = profileMapper;
            _imageKitService = imageKitService; 
        }

        public async Task<UpdateProfileResponse> ExecuteAsync(
            UpdateProfileRequest request,
            CancellationToken ct)
        {
            try
            {
                // 1. Validate input
                if (string.IsNullOrWhiteSpace(request.FullName) && request.AvatarFile == null && string.IsNullOrWhiteSpace(request.AvatarUrl)) // Modified
                {
                    return new UpdateProfileResponse
                    {
                        Success = false,
                        Message = "At least one field (FullName, AvatarFile, or AvatarUrl) must be provided",
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

                // 3. Get user
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

                bool hasChanges = false;

                // Handle FullName update
                if (!string.IsNullOrWhiteSpace(request.FullName) && user.FullName != request.FullName)
                {
                    user.FullName = request.FullName;
                    hasChanges = true;
                }

                // Handle AvatarFile upload or removal
                if (request.AvatarFile != null)
                {
                    // Image size validation (e.g., 5 MB limit)
                    const long maxFileSize = 5 * 1024 * 1024; // 5 MB
                    if (request.AvatarFile.Length > maxFileSize)
                    {
                        return new UpdateProfileResponse
                        {
                            Success = false,
                            Message = "Avatar image size must not exceed 5 MB",
                            StatusCode = UpdateProfileResponseStatusCode.InvalidInput
                        };
                    }

                    // Upload image to ImageKit
                    using (var stream = request.AvatarFile.OpenReadStream())
                    {
                        var fileName = $"avatar_{request.UserId}_{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(request.AvatarFile.FileName)}";
                        var imageUrl = await _imageKitService.UploadImageAsync(stream, fileName, "avatars");
                        if (string.IsNullOrWhiteSpace(imageUrl))
                        {
                            return new UpdateProfileResponse
                            {
                                Success = false,
                                Message = "Failed to upload avatar image",
                                StatusCode = UpdateProfileResponseStatusCode.UnknownError
                            };
                        }

                        if (user.AvatarUrl != imageUrl)
                        {
                            user.AvatarUrl = imageUrl;
                            hasChanges = true;
                        }
                    }
                }
                else if (request.AvatarUrl == "") 
                {
                    if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
                    {
                        user.AvatarUrl = null;
                        hasChanges = true;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(request.AvatarUrl)) 
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

                    if (user.AvatarUrl != request.AvatarUrl)
                    {
                        user.AvatarUrl = request.AvatarUrl;
                        hasChanges = true;
                    }
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

                // Update audit fields
                user.UpdatedBy = request.UserId;
                user.UpdatedAt = DateTime.UtcNow;

                // Save changes
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

                // Reload user with roles to get updated data
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
