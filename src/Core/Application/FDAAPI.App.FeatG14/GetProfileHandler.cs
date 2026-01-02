
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG14
{
    /// <summary>
    /// Handler for getting user profile
    /// </summary>
    public class GetProfileHandler : IFeatureHandler<GetProfileRequest, GetProfileResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserProfileMapper _profileMapper;

        public GetProfileHandler(
            IUserRepository userRepository,
            IUserProfileMapper profileMapper)
        {
            _userRepository = userRepository;
            _profileMapper = profileMapper;
        }

        public async Task<GetProfileResponse> ExecuteAsync(
            GetProfileRequest request,
            CancellationToken ct)
        {
            try
            {
                // 1. Get user with roles (already includes UserRoles and Roles via Include)
                var user = await _userRepository.GetUserWithRolesAsync(request.UserId, ct);
                if (user == null)
                {
                    return new GetProfileResponse
                    {
                        Success = false,
                        Message = "User not found",
                        StatusCode = GetProfileResponseStatusCode.UserNotFound
                    };
                }

                // 2. Map to DTO using mapper service
                var profile = _profileMapper.MapToProfileDto(user);

                return new GetProfileResponse
                {
                    Success = true,
                    Message = "Profile retrieved successfully",
                    StatusCode = GetProfileResponseStatusCode.Success,
                    Profile = profile
                };
            }
            catch (Exception ex)
            {
                return new GetProfileResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = GetProfileResponseStatusCode.UnknownError
                };
            }
        }
    }
}
