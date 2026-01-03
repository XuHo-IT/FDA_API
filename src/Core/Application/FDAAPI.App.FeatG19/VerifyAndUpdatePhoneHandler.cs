using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.App.FeatG15;
using FDAAPI.Domain.RelationalDb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG19
{
    public class VerifyAndUpdatePhoneHandler : IFeatureHandler<VerifyAndUpdatePhoneRequest, UpdateProfileResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IOtpCodeRepository _otpRepository;
        private readonly IUserProfileMapper _profileMapper;

        public VerifyAndUpdatePhoneHandler(
            IUserRepository userRepository,
            IOtpCodeRepository otpRepository,
            IUserProfileMapper profileMapper)
        {
            _userRepository = userRepository;
            _otpRepository = otpRepository;
            _profileMapper = profileMapper;
        }

        public async Task<UpdateProfileResponse> ExecuteAsync(VerifyAndUpdatePhoneRequest request, CancellationToken ct)
        {
            // Check otp code
            var otp = await _otpRepository.GetLatestValidOtpByIdentifierAsync(request.NewPhoneNumber, ct);

            if (otp == null || otp.Code != request.OtpCode || otp.ExpiresAt < DateTime.UtcNow || otp.IsUsed)
            {
                return new UpdateProfileResponse
                {
                    Success = false,
                    Message = "The OTP code is invalid or has expired.",
                    StatusCode = UpdateProfileResponseStatusCode.InvalidInput
                };
            }

            // check if phone number is already used by another user
            var existingUser = await _userRepository.GetByPhoneNumberAsync(request.NewPhoneNumber, ct);
            if (existingUser != null && existingUser.Id != request.UserId)
            {
                return new UpdateProfileResponse
                {
                    Success = false,
                    Message = "This phone number is already in use by another account.",
                    StatusCode = UpdateProfileResponseStatusCode.InvalidInput
                };
            }

            // update user's phone number
            var user = await _userRepository.GetUserWithRolesAsync(request.UserId, ct);
            if (user == null) return new UpdateProfileResponse { Success = false, Message = "User not found" };

            user.PhoneNumber = request.NewPhoneNumber;
            user.PhoneVerifiedAt = DateTime.UtcNow; // verify phone number
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, ct);

            // otp mark as used
            await _otpRepository.MarkAsUsedAsync(otp.Id, ct);

            return new UpdateProfileResponse
            {
                Success = true,
                Message = "Update phonenumber success",
                Profile = _profileMapper.MapToProfileDto(user)
            };
        }
    }
}
