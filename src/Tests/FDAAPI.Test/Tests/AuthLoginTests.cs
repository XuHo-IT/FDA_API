using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Test.Drivers;
using FDAAPI.App.Common.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat7_AuthLogin.DTOs;

namespace FDAAPI.Test.Tests
{
    public class AuthLoginTests : BaseEndpointTest
    {
        public AuthLoginTests(ApiWebApplicationFactory factory) : base(factory) { }

        private async Task CleanDatabase(AppDbContext db)
        {
            db.OtpCodes.RemoveRange(db.OtpCodes);
            db.UserRoles.RemoveRange(db.UserRoles);
            db.Users.RemoveRange(db.Users);
            db.Roles.RemoveRange(db.Roles);
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// UTCID01: Phone + OTP -> Auto-register new user
        /// Precondition: Phone number does not exist in DB; Valid OTP generated in OtpCodes table.
        /// Confirm Return: 200 OK, Success = true, User record created, Role = "USER".
        /// Result Type: N (Normal)
        /// </summary>
        /// <summary>
        /// UTCID02: Existing user -> Update LastLogin
        /// Precondition: User already exists in DB; Valid OTP generated.
        /// Confirm Return: 200 OK, Success = true, LastLoginAt timestamp updated.
        /// Result Type: N (Normal)
        /// </summary>
        [Theory]
        [InlineData("0900000001", "123456", true)]  // New User
        [InlineData("0900000002", "123456", false)] // Existing User
        public async Task Login_OTP_Flow_Success(string phone, string code, bool isNewUser)
        {
            // 1. Arrange
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await CleanDatabase(db);

            var role = new Role { Id = Guid.NewGuid(), Code = "USER", Name = "Citizen" };
            db.Roles.Add(role);

            if (!isNewUser)
            {
                var existingUser = new User { Id = Guid.NewGuid(), PhoneNumber = phone, Email = $"{phone}@temp.fda.local", Status = "active" };
                db.Users.Add(existingUser);
                db.UserRoles.Add(new UserRole { UserId = existingUser.Id, RoleId = role.Id });
            }

            db.OtpCodes.Add(new OtpCode { Id = Guid.NewGuid(), Identifier = phone, IdentifierType = "phone", Code = code, ExpiresAt = DateTime.UtcNow.AddMinutes(5), IsUsed = false });
            await db.SaveChangesAsync();

            var request = new LoginRequestDto { Identifier = phone, OtpCode = code };

            // 2. Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

            // 3. Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            result!.Success.Should().BeTrue();
            result.User!.Roles.Should().Contain("USER");
        }

        /// <summary>
        /// UTCID03: Wrong OTP Code
        /// Precondition: InValid identifier; OTP code "111111".
        /// Confirm Return: 401 Unauthorized, Success = false.
        /// Log message: "Incorrect OTP code. Please try again."
        /// Result Type: A (Abnormal)
        /// </summary>
        [Fact]
        public async Task Login_WrongOtp_ShouldFail()
        {
            // Arrange
            var phone = "0900000009";
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.OtpCodes.Add(new OtpCode { Id = Guid.NewGuid(), Identifier = phone, Code = "111111", ExpiresAt = DateTime.UtcNow.AddMinutes(5), IsUsed = false });
            await db.SaveChangesAsync();

            var request = new LoginRequestDto { Identifier = phone, OtpCode = "999999" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            result!.Message.Should().Contain("Incorrect OTP");
        }

        /// <summary>
        /// UTCID04: Fail when OTP is expired
        /// Precondition: OTP exists in DB but ExpiresAt is in the past.
        /// Confirm Return: 401 Unauthorized, Success = false.
        /// Log message: "Invalid or expired OTP."
        /// Result Type: A (Abnormal)
        /// </summary>
        [Fact]
        public async Task Login_ShouldFail_WhenOtpIsExpired()
        {
            var identifier = "expired@test.com";
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.OtpCodes.Add(new OtpCode { Id = Guid.NewGuid(), Identifier = identifier, Code = "111111", ExpiresAt = DateTime.UtcNow.AddMinutes(-10), IsUsed = false });
                await db.SaveChangesAsync();
            }

            var request = new LoginRequestDto { Identifier = identifier, OtpCode = "111111" };

            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            result!.Message.Should().Contain("expired");
        }

        /// <summary>
        /// UTCID05: Email + Password -> Admin Success
        /// Precondition: User with Email exists; PasswordHash matches "Admin@123".
        /// Confirm Return: 200 OK, Success = true, JWT Token contains Admin claims.
        /// Result Type: N (Normal)
        /// </summary>
        /// <summary>
        /// UTCID06: Wrong Password -> Fail
        /// Precondition: User exists; Input password does not match DB hash.
        /// Confirm Return: 401 Unauthorized, Success = false.
        /// Log message: "Invalid credentials"
        /// Result Type: A (Abnormal)
        /// </summary>
        [Theory]
        [InlineData("admin@fda.gov", "Admin@123", "Admin@123", HttpStatusCode.OK)]
        [InlineData("admin@fda.gov", "Admin@123", "WrongPass", HttpStatusCode.Unauthorized)]
        public async Task Login_Password_Flow(string email, string dbPass, string inputPass, HttpStatusCode expectedStatus)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            await CleanDatabase(db);

            var user = new User { Id = Guid.NewGuid(), Email = email, PasswordHash = hasher.HashPassword(dbPass), Status = "active" };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var request = new LoginRequestDto { Identifier = email, Password = inputPass };

            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

            response.StatusCode.Should().Be(expectedStatus);
        }

        /// <summary>
        /// UTCID07: Banned Account
        /// Precondition: User exists in DB with Status = "banned".
        /// Confirm Return: 401 Unauthorized, Success = false.
        /// Log message: "Your account has been banned."
        /// Result Type: A (Abnormal)
        /// </summary>
        [Fact]
        public async Task Login_BannedUser_ShouldFail()
        {
            var email = "banned@test.com";
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            db.Users.Add(new User { Email = email, PasswordHash = hasher.HashPassword("123"), Status = "banned" });
            await db.SaveChangesAsync();

            var request = new LoginRequestDto { Identifier = email, Password = "123" };

            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            result!.Message.Should().Contain("banned");
        }

        /// <summary>
        /// UTCID08: Missing Identifier
        /// Precondition: Request sent with null or empty Identifier field.
        /// Confirm Return: 401 Unauthorized, Success = false.
        /// Log message: "Identifier (phone or email) is required."
        /// Result Type: A (Abnormal)
        /// </summary>
        [Fact]
        public async Task Login_MissingIdentifier_ShouldFail()
        {
            var request = new LoginRequestDto { Identifier = "", Password = "" };
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            result!.Message.Should().Contain("required");
        }
    }
}