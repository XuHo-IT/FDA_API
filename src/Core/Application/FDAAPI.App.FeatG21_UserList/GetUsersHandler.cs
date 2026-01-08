using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.App.Common.Models.Admin;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FDAAPI.App.FeatG21_UserList
{
    public class GetUsersHandler : IRequestHandler<GetUsersRequest, GetUsersResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserProfileMapper _profileMapper;

        public GetUsersHandler(
            IUserRepository userRepository,
            IUserProfileMapper profileMapper)
        {
            _userRepository = userRepository;
            _profileMapper = profileMapper;
        }

        public async Task<GetUsersResponse> Handle(GetUsersRequest request, CancellationToken ct)
        {
            try
            {
                // 1. Fetch the list of users based on filters and pagination
                var (users, totalCount) = await _userRepository.GetUsersAsync(
                    request.SearchTerm,
                    request.Role,
                    request.Status,
                    request.PageNumber,
                    request.PageSize,
                    ct);

                var userList = users.ToList();

                // 2. Identify unique creators from the current user list to check their privileges
                var creatorIds = userList
                    .Where(u => u.CreatedBy != Guid.Empty)
                    .Select(u => u.CreatedBy)
                    .Distinct()
                    .ToList();

                // 3. Retrieve creator details including roles to determine who has Admin rights
                var creators = await _userRepository.GetUsersWithRolesByIdsAsync(creatorIds, ct);

                // 4. Create a HashSet of IDs belonging to creators who are Admins or SuperAdmins
                var adminCreatorIds = creators
                    .Where(c => c.UserRoles.Any(ur => ur.Role.Code == "ADMIN" || ur.Role.Code == "SUPERADMIN"))
                    .Select(c => c.Id)
                    .ToHashSet();

                // 5. Map entities to DTOs and classify them
                // You can add a property like 'IsAdminCreated' to your DTO if needed
                var userDtos = userList.Select(u =>
                {
                    var dto = _profileMapper.MapToProfileDto(u);

                    // Logic to distinguish between "Normal User" and "Admin-Created User"
                    bool isCreatedByAdmin = u.CreatedBy != Guid.Empty && adminCreatorIds.Contains(u.CreatedBy);

                    // Example: if your DTO has a field to store this classification
                     dto.IsAdminCreated = isCreatedByAdmin; 

                    return dto;
                });

                return new GetUsersResponse
                {
                    Success = true,
                    Message = "Users retrieved successfully",
                    StatusCode = AdminResponseStatusCode.Success,
                    Users = userDtos,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                // Error handling and response
                return new GetUsersResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = AdminResponseStatusCode.UnknownError
                };
            }
        }
    }
}