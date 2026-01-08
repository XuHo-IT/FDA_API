using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat10_AuthChangePassword.DTOs;
using FDAAPI.Test.Drivers;
using FDAAPI.App.Common.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FDAAPI.Test.Tests
{
    public class AuthChangePasswordTests : BaseEndpointTest
    {
        public AuthChangePasswordTests(ApiWebApplicationFactory factory) : base(factory) { }

        private async Task<User> SeedUser(AppDbContext db, IPasswordHasher hasher, string password = "OldPassword123!")
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = $"testuser_{Guid.NewGuid()}@fda.gov",
                PasswordHash = hasher.HashPassword(password),
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return user;
        }

        /// <summary>
        /// UTCID01: Change Password Successfully
        /// Precondition: User exists in DB; X-Test-UserId header provided; Current password matches DB hash; New password meets complexity requirements.
        /// Confirm Return: 200 OK, Success = true, Message contains "successfully".
        /// Result Type: N (Normal)
        /// </summary>
        [Fact]
        public async Task ChangePassword_ShouldSucceed()
        {
            // 1. Arrange
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var user = await SeedUser(db, hasher, "OldPassword123!");

            // Gán UserId vào Header để Handler nhận diện đúng User
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", user.Id.ToString());

            var request = new ChangePasswordRequestDto
            {
                CurrentPassword = "OldPassword123!",
                NewPassword = "NewSecurePassword456!",
                ConfirmPassword = "NewSecurePassword456!"
            };

            // 2. Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/change-password", request);

            // 3. Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ChangePasswordResponseDto>();
            result!.Success.Should().BeTrue();
        }

        /// <summary>
        /// UTCID2: New Password and Confirm Password do not match
        /// Precondition: User is authenticated via X-Test-UserId; NewPassword and ConfirmPassword fields are different.
        /// Confirm Return: 400 Bad Request, Success = false.
        /// Log message: "New password and confirmation password do not match"
        /// Result Type: A (Abnormal)
        /// </summary>
        [Fact]
        public async Task ChangePassword_ShouldFail_WhenPasswordsMismatch()
        {
            // 1. Arrange
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var user = await SeedUser(db, hasher);

            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", user.Id.ToString());

            var request = new ChangePasswordRequestDto
            {
                CurrentPassword = "OldPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "DifferentPassword123!"
            };

            // 2. Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/change-password", request);

            // 3. Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<ChangePasswordResponseDto>();
            result!.Message.Should().Contain("match");
        }

        /// <summary>
        /// UTCID3: Current password is incorrect
        /// Precondition: User is authenticated via X-Test-UserId; CurrentPassword provided does not match the stored PasswordHash.
        /// Confirm Return: 401 Unauthorized, Success = false.
        /// Log message: "Current password is incorrect"
        /// Result Type: A (Abnormal)
        /// </summary>
        [Fact]
        public async Task ChangePassword_ShouldFail_WhenCurrentPasswordIsWrong()
        {
            // 1. Arrange
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var user = await SeedUser(db, hasher, "RealPassword123!");

            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", user.Id.ToString());

            var request = new ChangePasswordRequestDto
            {
                CurrentPassword = "WrongPassword123!",
                NewPassword = "NewPassword456!",
                ConfirmPassword = "NewPassword456!"
            };

            // 2. Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/change-password", request);

            // 3. Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// UTCID4: New password is too weak
        /// Precondition: User is authenticated; NewPassword does not meet complexity (length, uppercase, digit, or special char).
        /// Confirm Return: 400 Bad Request, Success = false.
        /// Log message: "Password must be at least 8 characters long" or complexity error.
        /// Result Type: A (Abnormal)
        /// </summary>
        [Theory]
        [InlineData("short")] // < 8 chars
        [InlineData("nouppercase123!")]
        [InlineData("NONUMBER!!!!")]
        public async Task ChangePassword_ShouldFail_WhenPasswordIsWeak(string weakPassword)
        {
            // 1. Arrange
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var user = await SeedUser(db, hasher);

            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", user.Id.ToString());

            var request = new ChangePasswordRequestDto
            {
                CurrentPassword = "OldPassword123!",
                NewPassword = weakPassword,
                ConfirmPassword = weakPassword
            };

            // 2. Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/change-password", request);

            // 3. Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// UTCID5: New password same as current password
        /// Precondition: User is authenticated; NewPassword is identical to the current password stored in DB.
        /// Confirm Return: 400 Bad Request, Success = false.
        /// Log message: "New password must be different from current password"
        /// Result Type: A (Abnormal)
        /// </summary>
        [Fact]
        public async Task ChangePassword_ShouldFail_WhenNewPasswordSameAsOld()
        {
            // 1. Arrange
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            string samePassword = "SamePassword123!";
            var user = await SeedUser(db, hasher, samePassword);

            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", user.Id.ToString());

            var request = new ChangePasswordRequestDto
            {
                CurrentPassword = samePassword,
                NewPassword = samePassword,
                ConfirmPassword = samePassword
            };

            // 2. Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/change-password", request);

            // 3. Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<ChangePasswordResponseDto>();
            result!.Message.Should().Contain("different");
        }
    }
}