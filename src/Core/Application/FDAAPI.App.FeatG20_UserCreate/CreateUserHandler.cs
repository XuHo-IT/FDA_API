using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Services;
using FDAAPI.App.Common.Models.Admin;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG20_UserCreate
{
    public class CreateUserHandler : IRequestHandler<CreateUserRequest, CreateUserResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IPasswordHasher _passwordHasher;

        public CreateUserHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<CreateUserResponse> Handle(CreateUserRequest request, CancellationToken ct)
        {
            try
            {
                // 1. Check if email exists
                var existingUserByEmail = await _userRepository.GetByEmailAsync(request.Email, ct);
                if (existingUserByEmail != null)
                {
                    return new CreateUserResponse
                    {
                        Success = false,
                        Message = "Email already exists",
                        StatusCode = AdminResponseStatusCode.EmailAlreadyExists
                    };
                }

                // 2. Check if phone exists (if provided)
                if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    var existingUserByPhone = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber, ct);
                    if (existingUserByPhone != null)
                    {
                        return new CreateUserResponse
                        {
                            Success = false,
                            Message = "Phone number already exists",
                            StatusCode = AdminResponseStatusCode.PhoneNumberAlreadyExists
                        };
                    }
                }

                // 3. Create user entity
                var userId = Guid.NewGuid();
                var user = new User
                {
                    Id = userId,
                    Email = request.Email,
                    PasswordHash = _passwordHasher.HashPassword(request.Password),
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Status = "ACTIVE",
                    Provider = "local",
                    CreatedBy = request.AdminId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _userRepository.CreateAsync(user, ct);

                // 4. Assign roles
                foreach (var roleName in request.RoleNames)
                {
                    var role = await _roleRepository.GetByNameAsync(roleName, ct);
                    if (role != null)
                    {
                        await _userRoleRepository.AssignRoleToUserAsync(userId, role.Id, ct);
                    }
                }

                return new CreateUserResponse
                {
                    Success = true,
                    Message = "User created successfully",
                    StatusCode = AdminResponseStatusCode.Success,
                    UserId = userId
                };
            }
            catch (Exception ex)
            {
                return new CreateUserResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = AdminResponseStatusCode.UnknownError
                };
            }
        }
    }
}

