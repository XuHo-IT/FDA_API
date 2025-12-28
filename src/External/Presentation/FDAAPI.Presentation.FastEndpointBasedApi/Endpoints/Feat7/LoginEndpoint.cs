using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG7;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat7.DTOs;
using UserDto = FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat7.DTOs.UserDto;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat7
{
    /// <summary>
    /// Endpoint for user login
    /// Supports dual authentication methods
    /// 
    /// Request Flow:
    ///   1. Client sends POST with (phone+OTP) OR (email+password)
    ///   2. Handler validates credentials
    ///   3. If valid, generates JWT access + refresh tokens
    ///   4. Returns tokens + user info
    /// </summary>
    public class LoginEndpoint : Endpoint<LoginRequestDto, LoginResponseDto>
    {
        private readonly IFeatureHandler<LoginRequest, LoginResponse> _handler;

        public LoginEndpoint(IFeatureHandler<LoginRequest, LoginResponse> handler)
        {
            _handler = handler;
        }

        public override void Configure()
        {
            // Define HTTP method and route
            Post("/api/v1/auth/login");

            // Allow anonymous access (login doesn't require auth)
            AllowAnonymous();

            // API documentation
            Summary(s =>
            {
                s.Summary = "User login";
                s.Description = "Authenticate user with phone+OTP or email+password. " +
                               "Phone login auto-registers new users with USER role. " +
                               "Email login requires existing account (Admin/Gov).";

                // Example 1: Phone + OTP Login
                s.ExampleRequest = new LoginRequestDto
                {
                    PhoneNumber = "+84901234567",
                    OtpCode = "123456"
                };

                s.ResponseExamples[200] = new LoginResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                    RefreshToken = "base64_encoded_refresh_token",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    User = new UserDto
                    {
                        Id = Guid.NewGuid(),
                        Email = "+84901234567@temp.fda.local",
                        PhoneNumber = "+84901234567",
                        Roles = new List<string> { "USER" }
                    }
                };
            });

            Tags("Authentication", "Login");
        }

        public override async Task HandleAsync(LoginRequestDto req, CancellationToken ct)
        {
            try
            {
                // Step 1: Map DTO to application request
                var appRequest = new LoginRequest
                {
                    // Phone + OTP
                    PhoneNumber = req.PhoneNumber,
                    OtpCode = req.OtpCode,

                    // Email + Password
                    Email = req.Email,
                    Password = req.Password,

                    // Device tracking
                    DeviceInfo = req.DeviceInfo,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                };

                // Step 2: Execute handler
                var result = await _handler.ExecuteAsync(appRequest, ct);

                // Step 3: Map to response DTO
                var responseDto = new LoginResponseDto
                {
                    Success = result.Success,
                    Message = result.Message,
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    ExpiresAt = result.ExpiresAt,
                    User = result.User != null ? new UserDto
                    {
                        Id = result.User.Id,
                        Email = result.User.Email,
                        FullName = result.User.FullName,
                        PhoneNumber = result.User.PhoneNumber,
                        AvatarUrl = result.User.AvatarUrl,
                        Roles = result.User.Roles
                    } : null
                };

                // Step 4: Send response
                if (result.Success)
                {
                    await SendAsync(responseDto, 200, ct);
                }
                else
                {
                    // 401 Unauthorized for invalid credentials
                    await SendAsync(responseDto, 401, ct);
                }
            }
            catch (Exception ex)
            {
                var errorDto = new LoginResponseDto
                {
                    Success = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
                await SendAsync(errorDto, 500, ct);
            }
        }
    }
}
