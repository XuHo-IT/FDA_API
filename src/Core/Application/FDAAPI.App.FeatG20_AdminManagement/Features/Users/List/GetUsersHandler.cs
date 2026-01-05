using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.App.FeatG20_AdminManagement.Common;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG20_AdminManagement.Features.Users.List
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
                var (users, totalCount) = await _userRepository.GetUsersAsync(
                    request.SearchTerm,
                    request.Role,
                    request.Status,
                    request.PageNumber,
                    request.PageSize,
                    ct);

                var userDtos = users.Select(u => _profileMapper.MapToProfileDto(u));

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







