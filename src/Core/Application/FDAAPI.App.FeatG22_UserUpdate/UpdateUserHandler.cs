using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Admin;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System.Linq;

namespace FDAAPI.App.FeatG22_UserUpdate
{
    public class UpdateUserHandler : IRequestHandler<UpdateUserRequest, UpdateUserResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;

        public UpdateUserHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<UpdateUserResponse> Handle(UpdateUserRequest request, CancellationToken ct)
        {
            try
            {
                // 1. Retrieve user from repository
                var user = await _userRepository.GetByIdAsync(request.UserId, ct);
                if (user == null)
                {
                    return new UpdateUserResponse
                    {
                        Success = false,
                        Message = "User not found",
                        StatusCode = AdminResponseStatusCode.UserNotFound
                    };
                }

                // 2. Verify if the creator has Admin or SuperAdmin privileges
                var creatorWithRoles = user.CreatedBy != Guid.Empty
                    ? await _userRepository.GetUserWithRolesAsync(user.CreatedBy, ct)
                    : null;

                bool isCreatedByAdmin = creatorWithRoles != null &&
                    creatorWithRoles.UserRoles.Any(ur => ur.Role.Code == "ADMIN" || ur.Role.Code == "SUPERADMIN");

                // Flags to track modification status
                bool accountInfoChanged = false;
                bool rolesChanged = false;

                // 3. Process account information update (Restricted to users created by an Admin)
                bool isTryingToUpdateRestrictedFields = request.FullName != null || request.PhoneNumber != null || request.Status != null;

                if (isTryingToUpdateRestrictedFields)
                {
                    if (!isCreatedByAdmin)
                    {
                        return new UpdateUserResponse
                        {
                            Success = false,
                            Message = "Sorry, you cannot perform this action. You are only permitted to update user roles.",
                            StatusCode = AdminResponseStatusCode.UnknownError
                        };
                    }

                    // Update fields only if the value has changed
                    if (request.FullName != null && user.FullName != request.FullName) { user.FullName = request.FullName; accountInfoChanged = true; }
                    if (request.PhoneNumber != null && user.PhoneNumber != request.PhoneNumber) { user.PhoneNumber = request.PhoneNumber; accountInfoChanged = true; }
                    if (request.Status != null && user.Status != request.Status) { user.Status = request.Status; accountInfoChanged = true; }

                    if (accountInfoChanged)
                    {
                        user.UpdatedAt = DateTime.UtcNow;
                        await _userRepository.UpdateAsync(user, ct);
                    }
                }

                // 4. Update roles (English comments)
                if (request.RoleNames != null)
                {
                    var currentRoles = await _userRoleRepository.GetUserRolesAsync(user.Id, ct);
                    var currentRoleList = (currentRoles ?? Enumerable.Empty<Role>()).ToList();

                    // Normalize: Trim and UpperCase to avoid mismatches
                    var currentRoleCodes = currentRoleList.Select(r => r.Code.Trim().ToUpper()).ToList();
                    var newRoleCodes = request.RoleNames.Select(r => r.Trim().ToUpper()).ToList();

                    // Check if there is any actual difference
                    bool isRoleListDifferent = !currentRoleCodes.OrderBy(n => n)
                                                                .SequenceEqual(newRoleCodes.OrderBy(n => n));

                    if (isRoleListDifferent)
                    {
                        // A. REMOVE: Only roles that exist in DB but NOT in the request
                        foreach (var currentRole in currentRoleList)
                        {
                            if (!newRoleCodes.Contains(currentRole.Code.Trim().ToUpper()))
                            {
                                await _userRoleRepository.RemoveRoleFromUserAsync(user.Id, currentRole.Id, ct);
                                rolesChanged = true;
                            }
                        }

                        // B. ADD: Only roles that exist in the request but NOT in DB
                        foreach (var roleCode in request.RoleNames)
                        {
                            var normalizedCode = roleCode.Trim().ToUpper();
                            if (!currentRoleCodes.Contains(normalizedCode))
                            {
                                // Find role in DB by CODE
                                var role = await _roleRepository.GetByCodeAsync(roleCode, ct);
                                if (role != null)
                                {
                                    await _userRoleRepository.AssignRoleToUserAsync(user.Id, role.Id, ct);
                                    rolesChanged = true;
                                }
                            }
                        }
                    }
                }

                // 5. Consolidate response message based on updated components
                string finalMessage;
                if (accountInfoChanged && rolesChanged)
                    finalMessage = "Update account created by admin and user roles success";
                else if (accountInfoChanged)
                    finalMessage = "Update account created by admin success";
                else if (rolesChanged)
                    finalMessage = "Update user roles success";
                else
                    finalMessage = "No changes were applied";

                return new UpdateUserResponse
                {
                    Success = true,
                    Message = finalMessage,
                    StatusCode = AdminResponseStatusCode.Success
                };
            }
            catch (Exception ex)
            {
                // Exception handling and logging
                return new UpdateUserResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = AdminResponseStatusCode.UnknownError
                };
            }
        }
    }
}

