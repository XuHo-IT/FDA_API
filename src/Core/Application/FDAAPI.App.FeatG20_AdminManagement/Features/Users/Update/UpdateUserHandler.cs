using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG20_AdminManagement.Common;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System.Linq;

namespace FDAAPI.App.FeatG20_AdminManagement.Features.Users.Update
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
                // 1. Get user
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

                // 2. Check if user was created by an Admin
                var creatorWithRoles = user.CreatedBy != Guid.Empty 
                    ? await _userRepository.GetUserWithRolesAsync(user.CreatedBy, ct) 
                    : null;
                
                bool isCreatedByAdmin = creatorWithRoles != null && 
                    creatorWithRoles.UserRoles.Any(ur => ur.Role.Code == "ADMIN" || ur.Role.Code == "SUPERADMIN");

                // 3. Update user basic info
                bool changed = false;
                bool isTryingToUpdateRestrictedFields = request.FullName != null || request.PhoneNumber != null || request.Status != null;

                if (isTryingToUpdateRestrictedFields && !isCreatedByAdmin)
                {
                    return new UpdateUserResponse
                    {
                        Success = false,
                        Message = "Sorry, you can't do that action. You can only update roles for users created by an administrator.",
                        StatusCode = AdminResponseStatusCode.UnknownError // Or a more specific code if available
                    };
                }

                // If created by admin, allow updating everything.
                if (isCreatedByAdmin)
                {
                    if (request.FullName != null) { user.FullName = request.FullName; changed = true; }
                    if (request.PhoneNumber != null) { user.PhoneNumber = request.PhoneNumber; changed = true; }
                    if (request.Status != null) { user.Status = request.Status; changed = true; }

                    if (changed)
                    {
                        user.UpdatedAt = DateTime.UtcNow;
                        await _userRepository.UpdateAsync(user, ct);
                    }
                }

                // 4. Update roles if provided (always allowed for admins)
                if (request.RoleNames != null)
                {
                    // Get current roles
                    var currentRoles = await _userRoleRepository.GetUserRolesAsync(user.Id, ct);
                    
                    // Remove roles that are not in the new list
                    foreach (var currentRole in currentRoles)
                    {
                        if (!request.RoleNames.Contains(currentRole.Name))
                        {
                            await _userRoleRepository.RemoveRoleFromUserAsync(user.Id, currentRole.Id, ct);
                        }
                    }

                    // Add roles that are in the new list but not in current roles
                    foreach (var roleName in request.RoleNames)
                    {
                        if (!currentRoles.Any(r => r.Name == roleName))
                        {
                            var role = await _roleRepository.GetByNameAsync(roleName, ct);
                            if (role != null)
                            {
                                await _userRoleRepository.AssignRoleToUserAsync(user.Id, role.Id, ct);
                            }
                        }
                    }
                }

                return new UpdateUserResponse
                {
                    Success = true,
                    Message = "User updated successfully",
                    StatusCode = AdminResponseStatusCode.Success
                };
            }
            catch (Exception ex)
            {
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







