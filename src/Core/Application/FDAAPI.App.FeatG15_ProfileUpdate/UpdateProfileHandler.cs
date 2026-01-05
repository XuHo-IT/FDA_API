using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Services;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace FDAAPI.App.FeatG15_ProfileUpdate
{
    public class UpdateProfileHandler : IRequestHandler<UpdateProfileRequest, UpdateProfileResponse>
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

        public async Task<UpdateProfileResponse> Handle(UpdateProfileRequest request, CancellationToken ct)
        {
            try
            {
                // Get user
                var user = await _userRepository.GetUserWithRolesAsync(request.UserId, ct);
                if (user == null)
                {
                    return new UpdateProfileResponse { Success = false, Message = "User not found", StatusCode = UpdateProfileResponseStatusCode.UserNotFound };
                }

                bool hasChanges = false;

                // handle full name update
                if (request.FullName != null && user.FullName != request.FullName)
                {
                    user.FullName = request.FullName;
                    hasChanges = true;
                }
                if (request.FullName == string.Empty)
                {
                    var fullName = await _userRepository.GetUserFullNameAsync(request.UserId, ct);
                    user.FullName = fullName;
                    hasChanges = true;
                }

                // Update avatar logic
                if (request.AvatarFile != null)
                {
                    const long maxFileSize = 5 * 1024 * 1024; // 5 MB
                    if (request.AvatarFile.Length > maxFileSize)
                    {
                        return new UpdateProfileResponse { Success = false, Message = "Avatar image size must not exceed 5 MB", StatusCode = UpdateProfileResponseStatusCode.InvalidInput };
                    }

                    using var stream = request.AvatarFile.OpenReadStream();
                    var fileName = $"avatar_{request.UserId}_{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(request.AvatarFile.FileName)}";
                    var imageUrl = await _imageKitService.UploadImageAsync(stream, fileName, "avatars");

                    if (!string.IsNullOrWhiteSpace(imageUrl))
                    {
                        user.AvatarUrl = imageUrl;
                        hasChanges = true;
                    }
                }
                // bind to existing URL
                else if (request.AvatarUrl == string.Empty && request.AvatarFile == null)
                {
                    if (user.AvatarUrl != request.AvatarUrl)
                    {
                        var currentAvatar = await _userRepository.GetAvatarUrlAsync(request.UserId, ct);
                        user.AvatarUrl = currentAvatar;
                        hasChanges = true;
                    }
                }

                // delete existing avatar
                else if (request.AvatarUrl == string.Empty)
                {
                    if (user.AvatarUrl != null)
                    {
                        user.AvatarUrl = null;
                        hasChanges = true;
                    }
                }

                if (!hasChanges)
                {
                    return new UpdateProfileResponse
                    {
                        Success = true,
                        Message = "No changes detected",
                        StatusCode = UpdateProfileResponseStatusCode.Success,
                        Profile = _profileMapper.MapToProfileDto(user)
                    };
                }

                user.UpdatedBy = request.UserId;
                user.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _userRepository.UpdateAsync(user, ct);
                if (!updateResult)
                {
                    return new UpdateProfileResponse { Success = false, Message = "Failed to update profile in database", StatusCode = UpdateProfileResponseStatusCode.UnknownError };
                }

                return new UpdateProfileResponse
                {
                    Success = true,
                    Message = "Profile updated successfully",
                    StatusCode = UpdateProfileResponseStatusCode.Success,
                    Profile = _profileMapper.MapToProfileDto(user)
                };
            }
            catch (Exception ex)
            {
                return new UpdateProfileResponse { Success = false, Message = $"Error: {ex.Message}", StatusCode = UpdateProfileResponseStatusCode.UnknownError };
            }
        }
    }
}






