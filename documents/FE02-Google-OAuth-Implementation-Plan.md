# FE-02: Google OAuth Login - Implementation Plan

**Feature Code**: FE-02
**Description**: Enable users to login with Google OAuth, with automatic account linking and default Citizen role assignment
**Architecture**: Domain-Centric (Clean Architecture + CQRS)
**Framework**: ASP.NET Core 8.0 + FastEndpoints

---

## Table of Contents

1. [Requirements](#requirements)
2. [Google OAuth Flow](#google-oauth-flow)
3. [Database Schema](#database-schema)
4. [Implementation Phases](#implementation-phases)
5. [Code Structure](#code-structure)
6. [Testing Strategy](#testing-strategy)
7. [Security Considerations](#security-considerations)

---

## Requirements

### Functional Requirements

1. **Google OAuth Integration**
   - Users can initiate login with Google
   - OAuth 2.0 Authorization Code Flow
   - Redirect to Google consent screen
   - Handle OAuth callback with authorization code
   - Exchange code for access token and ID token

2. **Account Linking**
   - **First Login**: Auto-create user account
     - Extract email from Google ID token
     - Assign default role: `USER` (Citizen)
     - Set provider: `google`
     - Mark email as verified
   - **Re-Login**: Link to existing account by email
     - If email exists with provider `local`, link to OAuth
     - Update provider to `google` or keep both methods

3. **Token Management**
   - Generate JWT access token (60 min expiration)
   - Generate refresh token (7 days expiration)
   - Store OAuth provider info (Google user ID, profile data)

4. **Error Handling**
   - Invalid authorization code
   - Callback state mismatch (CSRF protection)
   - Revoked OAuth consent
   - Network errors during token exchange

### Non-Functional Requirements

- **Security**: PKCE (Proof Key for Code Exchange) for OAuth flow
- **Performance**: Cache Google public keys for ID token verification
- **Scalability**: Stateless JWT tokens
- **Maintainability**: Follow existing architecture patterns

---

## Google OAuth Flow

### Standard OAuth 2.0 Authorization Code Flow

```
┌─────────────┐                                ┌──────────────┐
│   Client    │                                │ Google OAuth │
│ (Browser/   │                                │    Server    │
│   Mobile)   │                                └──────────────┘
└──────┬──────┘                                        │
       │                                               │
       │  1. GET /api/v1/auth/google                   │
       ├──────────────────────────────────────────────►│
       │  (Initiate OAuth)                             │
       │                                               │
       │  2. Redirect to Google Consent Screen        │
       │◄──────────────────────────────────────────────┤
       │  https://accounts.google.com/o/oauth2/v2/auth │
       │  ?client_id=...&redirect_uri=...&state=...    │
       │                                               │
       │  3. User grants consent                       │
       ├──────────────────────────────────────────────►│
       │                                               │
       │  4. Redirect back with authorization code     │
       │◄──────────────────────────────────────────────┤
       │  /api/v1/auth/google/callback                 │
       │  ?code=AUTH_CODE&state=CSRF_TOKEN             │
       │                                               │
┌──────▼──────┐                                        │
│  FDA API    │                                        │
│   Server    │  5. Exchange code for tokens          │
└─────────────┴────────────────────────────────────────►
                 POST https://oauth2.googleapis.com/token
                 {code, client_id, client_secret, redirect_uri}

              ◄────────────────────────────────────────
                 {access_token, id_token, refresh_token}

              6. Verify ID token (JWT)
              7. Extract user info (email, name, picture)
              8. Check user exists by email
              9. Create/Update user record
              10. Generate FDA API JWT tokens
              11. Return tokens to client
```

---

## Database Schema

### New Entity: `UserOAuthProvider`

Store OAuth provider information for users who login via Google/Facebook.

```csharp
public class UserOAuthProvider : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
{
    public Guid UserId { get; set; }
    public string Provider { get; set; } // "google", "facebook"
    public string ProviderUserId { get; set; } // Google user ID
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? AccessToken { get; set; } // Google access token (optional)
    public string? RefreshToken { get; set; } // Google refresh token (optional)
    public DateTime? TokenExpiresAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Audit fields
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    [JsonIgnore]
    public virtual User User { get; set; }
}
```

### SQL Schema (PostgreSQL)

```sql
CREATE TABLE "UserOAuthProviders" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" uuid NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "Provider" varchar(50) NOT NULL, -- google, facebook
    "ProviderUserId" varchar(255) NOT NULL, -- Google user ID
    "Email" varchar(255),
    "DisplayName" varchar(255),
    "ProfilePictureUrl" text,
    "AccessToken" text, -- Encrypted OAuth access token
    "RefreshToken" text, -- Encrypted OAuth refresh token
    "TokenExpiresAt" timestamptz,
    "LastLoginAt" timestamptz,

    "CreatedBy" uuid NOT NULL,
    "CreatedAt" timestamptz NOT NULL DEFAULT NOW(),
    "UpdatedBy" uuid NOT NULL,
    "UpdatedAt" timestamptz NOT NULL DEFAULT NOW(),

    CONSTRAINT "uq_user_oauth_provider" UNIQUE ("UserId", "Provider"),
    CONSTRAINT "uq_provider_user_id" UNIQUE ("Provider", "ProviderUserId")
);

CREATE INDEX "ix_user_oauth_user" ON "UserOAuthProviders"("UserId");
CREATE INDEX "ix_user_oauth_provider" ON "UserOAuthProviders"("Provider");
CREATE INDEX "ix_user_oauth_provider_user_id" ON "UserOAuthProviders"("Provider", "ProviderUserId");
```

### Update `Users` Table

No changes needed to `Users` table structure. The `Provider` field will remain `local` for email/password users and can be updated to `google` for OAuth-only users.

---

## Implementation Phases

### Phase 1: Domain Layer - Entities & Repositories

**Location**: `src/Core/Domain/FDAAPI.Domain.RelationalDb`

#### 1.1 Create Entity

**File**: `Entities/UserOAuthProvider.cs`

```csharp
namespace FDAAPI.Domain.RelationalDb.Entities;

public class UserOAuthProvider : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
{
    public Guid UserId { get; set; }
    public string Provider { get; set; } = string.Empty; // "google", "facebook"
    public string ProviderUserId { get; set; } = string.Empty; // Google user ID
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? AccessToken { get; set; } // Encrypted
    public string? RefreshToken { get; set; } // Encrypted
    public DateTime? TokenExpiresAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Audit fields
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    [JsonIgnore]
    public virtual User User { get; set; } = null!;
}
```

#### 1.2 Update User Entity

**File**: `Entities/User.cs`

Add navigation property:

```csharp
public class User : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
{
    // ... existing properties ...

    [JsonIgnore]
    public virtual ICollection<UserOAuthProvider> OAuthProviders { get; set; } = new List<UserOAuthProvider>();
}
```

#### 1.3 Create Repository Interface

**File**: `RepositoryContracts/IUserOAuthProviderRepository.cs`

```csharp
namespace FDAAPI.Domain.RelationalDb.RepositoryContracts;

public interface IUserOAuthProviderRepository
{
    Task<UserOAuthProvider?> GetByProviderUserIdAsync(string provider, string providerUserId, CancellationToken ct = default);
    Task<UserOAuthProvider?> GetByUserIdAndProviderAsync(Guid userId, string provider, CancellationToken ct = default);
    Task<List<UserOAuthProvider>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Guid> CreateAsync(UserOAuthProvider oauthProvider, CancellationToken ct = default);
    Task<bool> UpdateAsync(UserOAuthProvider oauthProvider, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
```

#### 1.4 Update AppDbContext

**File**: `RealationalDB/AppDbContext.cs`

```csharp
public class AppDbContext : DbContext
{
    // Add DbSet
    public DbSet<UserOAuthProvider> UserOAuthProviders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ... existing configurations ...

        // UserOAuthProvider configuration
        modelBuilder.Entity<UserOAuthProvider>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("ix_user_oauth_user");

            entity.HasIndex(e => e.Provider)
                .HasDatabaseName("ix_user_oauth_provider");

            entity.HasIndex(e => new { e.Provider, e.ProviderUserId })
                .IsUnique()
                .HasDatabaseName("uq_provider_user_id");

            entity.HasIndex(e => new { e.UserId, e.Provider })
                .IsUnique()
                .HasDatabaseName("uq_user_oauth_provider");

            entity.HasOne(e => e.User)
                .WithMany(u => u.OAuthProviders)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Provider).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ProviderUserId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.DisplayName).HasMaxLength(255);
        });
    }
}
```

---

### Phase 2: Infrastructure Layer - Services & Repositories

**Location**: `src/External/Infrastructure`

#### 2.1 Create Google OAuth Service

**File**: `Services/FDAAPI.Infra.Services/GoogleOAuthService.cs`

```csharp
namespace FDAAPI.Infra.Services;

public interface IGoogleOAuthService
{
    string GenerateAuthorizationUrl(string state);
    Task<GoogleTokenResponse> ExchangeCodeForTokenAsync(string code, CancellationToken ct = default);
    Task<GoogleUserInfo> GetUserInfoAsync(string accessToken, CancellationToken ct = default);
    Task<GoogleUserInfo> VerifyIdTokenAsync(string idToken, CancellationToken ct = default);
}

public class GoogleOAuthService : IGoogleOAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GoogleOAuthService> _logger;

    private string ClientId => _configuration["OAuth:Google:ClientId"]!;
    private string ClientSecret => _configuration["OAuth:Google:ClientSecret"]!;
    private string RedirectUri => _configuration["OAuth:Google:RedirectUri"]!;

    public GoogleOAuthService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<GoogleOAuthService> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public string GenerateAuthorizationUrl(string state)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "client_id", ClientId },
            { "redirect_uri", RedirectUri },
            { "response_type", "code" },
            { "scope", "openid email profile" },
            { "state", state },
            { "access_type", "offline" }, // Get refresh token
            { "prompt", "consent" } // Force consent screen
        };

        var query = string.Join("&", queryParams.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return $"https://accounts.google.com/o/oauth2/v2/auth?{query}";
    }

    public async Task<GoogleTokenResponse> ExchangeCodeForTokenAsync(string code, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();

        var requestBody = new Dictionary<string, string>
        {
            { "code", code },
            { "client_id", ClientId },
            { "client_secret", ClientSecret },
            { "redirect_uri", RedirectUri },
            { "grant_type", "authorization_code" }
        };

        var response = await client.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(requestBody),
            ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<GoogleTokenResponse>(json)!;
    }

    public async Task<GoogleUserInfo> VerifyIdTokenAsync(string idToken, CancellationToken ct)
    {
        // Verify ID token using Google's tokeninfo endpoint
        var client = _httpClientFactory.CreateClient();

        var response = await client.GetAsync(
            $"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}",
            ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var tokenInfo = JsonSerializer.Deserialize<GoogleTokenInfo>(json)!;

        // Verify audience matches client ID
        if (tokenInfo.Aud != ClientId)
        {
            throw new UnauthorizedAccessException("Invalid ID token audience");
        }

        return new GoogleUserInfo
        {
            Id = tokenInfo.Sub,
            Email = tokenInfo.Email,
            EmailVerified = tokenInfo.EmailVerified,
            Name = tokenInfo.Name,
            Picture = tokenInfo.Picture
        };
    }

    public async Task<GoogleUserInfo> GetUserInfoAsync(string accessToken, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync(
            "https://www.googleapis.com/oauth2/v2/userinfo",
            ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<GoogleUserInfo>(json)!;
    }
}

// DTOs
public class GoogleTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("id_token")]
    public string IdToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
}

public class GoogleUserInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("verified_email")]
    public bool EmailVerified { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("picture")]
    public string? Picture { get; set; }

    [JsonPropertyName("given_name")]
    public string? GivenName { get; set; }

    [JsonPropertyName("family_name")]
    public string? FamilyName { get; set; }
}

public class GoogleTokenInfo
{
    [JsonPropertyName("sub")]
    public string Sub { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("picture")]
    public string? Picture { get; set; }

    [JsonPropertyName("aud")]
    public string Aud { get; set; } = string.Empty; // Client ID

    [JsonPropertyName("exp")]
    public long Exp { get; set; } // Expiration timestamp
}
```

#### 2.2 Implement Repository

**File**: `Persistence/FDAAPI.Infra.Persistence/Repositories/PgsqlUserOAuthProviderRepository.cs`

```csharp
namespace FDAAPI.Infra.Persistence.Repositories;

public class PgsqlUserOAuthProviderRepository : IUserOAuthProviderRepository
{
    private readonly AppDbContext _context;

    public PgsqlUserOAuthProviderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserOAuthProvider?> GetByProviderUserIdAsync(
        string provider,
        string providerUserId,
        CancellationToken ct = default)
    {
        return await _context.UserOAuthProviders
            .Include(o => o.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(o =>
                o.Provider == provider &&
                o.ProviderUserId == providerUserId, ct);
    }

    public async Task<UserOAuthProvider?> GetByUserIdAndProviderAsync(
        Guid userId,
        string provider,
        CancellationToken ct = default)
    {
        return await _context.UserOAuthProviders
            .FirstOrDefaultAsync(o =>
                o.UserId == userId &&
                o.Provider == provider, ct);
    }

    public async Task<List<UserOAuthProvider>> GetByUserIdAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        return await _context.UserOAuthProviders
            .Where(o => o.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task<Guid> CreateAsync(
        UserOAuthProvider oauthProvider,
        CancellationToken ct = default)
    {
        oauthProvider.Id = Guid.NewGuid();
        oauthProvider.CreatedAt = DateTime.UtcNow;
        oauthProvider.UpdatedAt = DateTime.UtcNow;

        await _context.UserOAuthProviders.AddAsync(oauthProvider, ct);
        await _context.SaveChangesAsync(ct);

        return oauthProvider.Id;
    }

    public async Task<bool> UpdateAsync(
        UserOAuthProvider oauthProvider,
        CancellationToken ct = default)
    {
        oauthProvider.UpdatedAt = DateTime.UtcNow;

        _context.UserOAuthProviders.Update(oauthProvider);
        var result = await _context.SaveChangesAsync(ct);

        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.UserOAuthProviders.FindAsync(new object[] { id }, ct);
        if (entity == null) return false;

        _context.UserOAuthProviders.Remove(entity);
        var result = await _context.SaveChangesAsync(ct);

        return result > 0;
    }
}
```

---

### Phase 3: Application Layer - Handlers

**Location**: `src/Core/Application`

#### 3.1 FEAT10 - Google Login Initiate

**Project**: `FDAAPI.App.FeatG10`

**Files**:
- `GoogleLoginInitiateRequest.cs`
- `GoogleLoginInitiateResponse.cs`
- `GoogleLoginInitiateHandler.cs`

**GoogleLoginInitiateRequest.cs**:

```csharp
namespace FDAAPI.App.FeatG10;

public class GoogleLoginInitiateRequest
{
    public string? ReturnUrl { get; set; } // Optional redirect after login
}
```

**GoogleLoginInitiateResponse.cs**:

```csharp
namespace FDAAPI.App.FeatG10;

public class GoogleLoginInitiateResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string AuthorizationUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty; // CSRF token
}
```

**GoogleLoginInitiateHandler.cs**:

```csharp
namespace FDAAPI.App.FeatG10;

public class GoogleLoginInitiateHandler : IFeatureHandler<GoogleLoginInitiateRequest, GoogleLoginInitiateResponse>
{
    private readonly IGoogleOAuthService _googleOAuthService;
    private readonly ILogger<GoogleLoginInitiateHandler> _logger;

    public GoogleLoginInitiateHandler(
        IGoogleOAuthService googleOAuthService,
        ILogger<GoogleLoginInitiateHandler> logger)
    {
        _googleOAuthService = googleOAuthService;
        _logger = logger;
    }

    public async Task<GoogleLoginInitiateResponse> ExecuteAsync(
        GoogleLoginInitiateRequest request,
        CancellationToken ct)
    {
        try
        {
            // Generate CSRF state token
            var state = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            // TODO: Store state in cache with 5-minute expiration for verification
            // await _cache.SetAsync($"oauth:state:{state}", request.ReturnUrl, TimeSpan.FromMinutes(5), ct);

            // Generate Google OAuth authorization URL
            var authUrl = _googleOAuthService.GenerateAuthorizationUrl(state);

            _logger.LogInformation("Generated Google OAuth URL with state: {State}", state);

            return new GoogleLoginInitiateResponse
            {
                Success = true,
                Message = "Redirect to Google for authentication",
                AuthorizationUrl = authUrl,
                State = state
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating Google OAuth");

            return new GoogleLoginInitiateResponse
            {
                Success = false,
                Message = "Failed to initiate Google login"
            };
        }
    }
}
```

#### 3.2 FEAT11 - Google OAuth Callback

**Project**: `FDAAPI.App.FeatG11`

**Files**:
- `GoogleOAuthCallbackRequest.cs`
- `GoogleOAuthCallbackResponse.cs`
- `GoogleOAuthCallbackHandler.cs`

**GoogleOAuthCallbackRequest.cs**:

```csharp
namespace FDAAPI.App.FeatG11;

public class GoogleOAuthCallbackRequest
{
    public string Code { get; set; } = string.Empty; // Authorization code
    public string State { get; set; } = string.Empty; // CSRF token
}
```

**GoogleOAuthCallbackResponse.cs**:

```csharp
namespace FDAAPI.App.FeatG11;

public class GoogleOAuthCallbackResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto? User { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public List<string> Roles { get; set; } = new();
}
```

**GoogleOAuthCallbackHandler.cs** (~250 lines):

```csharp
namespace FDAAPI.App.FeatG11;

public class GoogleOAuthCallbackHandler : IFeatureHandler<GoogleOAuthCallbackRequest, GoogleOAuthCallbackResponse>
{
    private readonly IGoogleOAuthService _googleOAuthService;
    private readonly IUserRepository _userRepository;
    private readonly IUserOAuthProviderRepository _oauthProviderRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<GoogleOAuthCallbackHandler> _logger;

    public GoogleOAuthCallbackHandler(
        IGoogleOAuthService googleOAuthService,
        IUserRepository userRepository,
        IUserOAuthProviderRepository oauthProviderRepository,
        IRoleRepository roleRepository,
        IUserRoleRepository userRoleRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtTokenService,
        ILogger<GoogleOAuthCallbackHandler> logger)
    {
        _googleOAuthService = googleOAuthService;
        _userRepository = userRepository;
        _oauthProviderRepository = oauthProviderRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<GoogleOAuthCallbackResponse> ExecuteAsync(
        GoogleOAuthCallbackRequest request,
        CancellationToken ct)
    {
        try
        {
            // 1. Verify state token (CSRF protection)
            // TODO: Verify state exists in cache
            // var cachedState = await _cache.GetAsync($"oauth:state:{request.State}", ct);
            // if (cachedState == null)
            // {
            //     return new GoogleOAuthCallbackResponse
            //     {
            //         Success = false,
            //         Message = "Invalid or expired state token"
            //     };
            // }

            // 2. Exchange authorization code for tokens
            var tokenResponse = await _googleOAuthService.ExchangeCodeForTokenAsync(request.Code, ct);

            // 3. Verify ID token and extract user info
            var googleUser = await _googleOAuthService.VerifyIdTokenAsync(tokenResponse.IdToken, ct);

            _logger.LogInformation("Google user authenticated: {Email}", googleUser.Email);

            // 4. Check if OAuth provider record exists
            var oauthProvider = await _oauthProviderRepository.GetByProviderUserIdAsync(
                "google",
                googleUser.Id,
                ct);

            User user;

            if (oauthProvider != null)
            {
                // Existing user - re-login
                user = oauthProvider.User;

                // Update OAuth provider info
                oauthProvider.Email = googleUser.Email;
                oauthProvider.DisplayName = googleUser.Name;
                oauthProvider.ProfilePictureUrl = googleUser.Picture;
                oauthProvider.AccessToken = tokenResponse.AccessToken; // TODO: Encrypt
                oauthProvider.RefreshToken = tokenResponse.RefreshToken; // TODO: Encrypt
                oauthProvider.TokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                oauthProvider.LastLoginAt = DateTime.UtcNow;
                oauthProvider.UpdatedBy = user.Id;

                await _oauthProviderRepository.UpdateAsync(oauthProvider, ct);

                _logger.LogInformation("Existing Google user logged in: {UserId}", user.Id);
            }
            else
            {
                // New user - first login

                // Check if user exists by email (account linking)
                user = await _userRepository.GetByEmailAsync(googleUser.Email, ct);

                if (user == null)
                {
                    // Create new user account
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = googleUser.Email,
                        FullName = googleUser.Name,
                        AvatarUrl = googleUser.Picture,
                        Provider = "google",
                        Status = "ACTIVE",
                        EmailVerifiedAt = googleUser.EmailVerified ? DateTime.UtcNow : null,
                        CreatedBy = Guid.Empty, // System
                        CreatedAt = DateTime.UtcNow,
                        UpdatedBy = Guid.Empty,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _userRepository.CreateAsync(user, ct);

                    // Assign default USER role (Citizen)
                    var userRole = await _roleRepository.GetByCodeAsync("USER", ct);
                    if (userRole != null)
                    {
                        await _userRoleRepository.CreateAsync(new UserRole
                        {
                            UserId = user.Id,
                            RoleId = userRole.Id
                        }, ct);
                    }

                    _logger.LogInformation("New user created via Google OAuth: {UserId}", user.Id);
                }
                else
                {
                    // User exists with email - link Google account
                    _logger.LogInformation("Linking Google account to existing user: {UserId}", user.Id);
                }

                // Create OAuth provider record
                var newOAuthProvider = new UserOAuthProvider
                {
                    UserId = user.Id,
                    Provider = "google",
                    ProviderUserId = googleUser.Id,
                    Email = googleUser.Email,
                    DisplayName = googleUser.Name,
                    ProfilePictureUrl = googleUser.Picture,
                    AccessToken = tokenResponse.AccessToken, // TODO: Encrypt
                    RefreshToken = tokenResponse.RefreshToken, // TODO: Encrypt
                    TokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
                    LastLoginAt = DateTime.UtcNow,
                    CreatedBy = user.Id,
                    UpdatedBy = user.Id
                };

                await _oauthProviderRepository.CreateAsync(newOAuthProvider, ct);
            }

            // 5. Load user with roles
            var userWithRoles = await _userRepository.GetUserWithRolesAsync(user.Id, ct);
            if (userWithRoles == null)
            {
                return new GoogleOAuthCallbackResponse
                {
                    Success = false,
                    Message = "User not found after creation"
                };
            }

            var roles = await _userRoleRepository.GetRolesByUserIdAsync(user.Id, ct);
            var roleCodes = roles.Select(r => r.Code).ToList();

            // 6. Generate FDA API JWT tokens
            var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email, roleCodes);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            // 7. Store refresh token
            await _refreshTokenRepository.CreateAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedBy = user.Id,
                CreatedAt = DateTime.UtcNow
            }, ct);

            // 8. Update last login timestamp
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = user.Id;
            await _userRepository.UpdateAsync(user, ct);

            return new GoogleOAuthCallbackResponse
            {
                Success = true,
                Message = "Login successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FullName = user.FullName,
                    AvatarUrl = user.AvatarUrl,
                    Roles = roleCodes
                }
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during Google OAuth callback");

            return new GoogleOAuthCallbackResponse
            {
                Success = false,
                Message = "Failed to communicate with Google OAuth server"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Google OAuth callback");

            return new GoogleOAuthCallbackResponse
            {
                Success = false,
                Message = "An error occurred during login"
            };
        }
    }
}
```

---

### Phase 4: Presentation Layer - FastEndpoints

**Location**: `src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/Endpoints`

#### 4.1 FEAT10 - Google Login Initiate Endpoint

**Folder**: `Endpoints/Feat10/`

**GoogleLoginInitiateEndpoint.cs**:

```csharp
namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat10;

public class GoogleLoginInitiateEndpoint : Endpoint<GoogleLoginInitiateRequestDto, GoogleLoginInitiateResponseDto>
{
    private readonly IFeatureHandler<GoogleLoginInitiateRequest, GoogleLoginInitiateResponse> _handler;

    public GoogleLoginInitiateEndpoint(
        IFeatureHandler<GoogleLoginInitiateRequest, GoogleLoginInitiateResponse> handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/api/v1/auth/google");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Initiate Google OAuth login";
            s.Description = "Redirects user to Google consent screen for authentication.";
            s.ExampleRequest = new GoogleLoginInitiateRequestDto
            {
                ReturnUrl = "https://app.fda.gov.vn/dashboard"
            };
        });
    }

    public override async Task HandleAsync(GoogleLoginInitiateRequestDto req, CancellationToken ct)
    {
        var request = new GoogleLoginInitiateRequest
        {
            ReturnUrl = req.ReturnUrl
        };

        var response = await _handler.ExecuteAsync(request, ct);

        if (!response.Success)
        {
            await SendAsync(new GoogleLoginInitiateResponseDto
            {
                Success = false,
                Message = response.Message
            }, 400, ct);
            return;
        }

        await SendAsync(new GoogleLoginInitiateResponseDto
        {
            Success = true,
            Message = response.Message,
            AuthorizationUrl = response.AuthorizationUrl,
            State = response.State
        }, 200, ct);
    }
}
```

**DTOs/GoogleLoginInitiateRequestDto.cs**:

```csharp
namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat10.DTOs;

public class GoogleLoginInitiateRequestDto
{
    public string? ReturnUrl { get; set; }
}
```

**DTOs/GoogleLoginInitiateResponseDto.cs**:

```csharp
namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat10.DTOs;

public class GoogleLoginInitiateResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string AuthorizationUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
```

#### 4.2 FEAT11 - Google OAuth Callback Endpoint

**Folder**: `Endpoints/Feat11/`

**GoogleOAuthCallbackEndpoint.cs**:

```csharp
namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat11;

public class GoogleOAuthCallbackEndpoint : Endpoint<GoogleOAuthCallbackRequestDto, GoogleOAuthCallbackResponseDto>
{
    private readonly IFeatureHandler<GoogleOAuthCallbackRequest, GoogleOAuthCallbackResponse> _handler;

    public GoogleOAuthCallbackEndpoint(
        IFeatureHandler<GoogleOAuthCallbackRequest, GoogleOAuthCallbackResponse> handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/api/v1/auth/google/callback");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Google OAuth callback";
            s.Description = "Handles OAuth callback from Google with authorization code.";
        });
    }

    public override async Task HandleAsync(GoogleOAuthCallbackRequestDto req, CancellationToken ct)
    {
        var request = new GoogleOAuthCallbackRequest
        {
            Code = req.Code,
            State = req.State
        };

        var response = await _handler.ExecuteAsync(request, ct);

        if (!response.Success)
        {
            await SendAsync(new GoogleOAuthCallbackResponseDto
            {
                Success = false,
                Message = response.Message
            }, 400, ct);
            return;
        }

        await SendAsync(new GoogleOAuthCallbackResponseDto
        {
            Success = true,
            Message = response.Message,
            AccessToken = response.AccessToken,
            RefreshToken = response.RefreshToken,
            ExpiresAt = response.ExpiresAt,
            User = response.User == null ? null : new UserDto
            {
                Id = response.User.Id,
                Email = response.User.Email,
                PhoneNumber = response.User.PhoneNumber,
                FullName = response.User.FullName,
                AvatarUrl = response.User.AvatarUrl,
                Roles = response.User.Roles
            }
        }, 200, ct);
    }
}
```

**DTOs/GoogleOAuthCallbackRequestDto.cs**:

```csharp
namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat11.DTOs;

public class GoogleOAuthCallbackRequestDto
{
    [FromQueryParams]
    public string Code { get; set; } = string.Empty;

    [FromQueryParams]
    public string State { get; set; } = string.Empty;
}
```

**DTOs/GoogleOAuthCallbackResponseDto.cs**:

```csharp
namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat11.DTOs;

public class GoogleOAuthCallbackResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto? User { get; set; }
}
```

---

### Phase 5: Configuration & Registration

**File**: `src/External/Infrastructure/Common/FDAAPI.Infra.Configuration/ServiceExtensions.cs`

```csharp
// Register Google OAuth Service
services.AddHttpClient(); // Required for HTTP requests
services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();

// Register Repository
services.AddScoped<IUserOAuthProviderRepository, PgsqlUserOAuthProviderRepository>();

// Register Handlers
services.AddTransient<IFeatureHandler<GoogleLoginInitiateRequest, GoogleLoginInitiateResponse>, GoogleLoginInitiateHandler>();
services.AddTransient<IFeatureHandler<GoogleOAuthCallbackRequest, GoogleOAuthCallbackResponse>, GoogleOAuthCallbackHandler>();
```

**Update appsettings.json**:

```json
{
  "OAuth": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET",
      "RedirectUri": "http://localhost:5232/api/v1/auth/google/callback"
    }
  }
}
```

---

### Phase 6: Database Migration

```bash
# Create migration
dotnet ef migrations add GoogleOAuth \
  --project "src/Core/Domain/FDAAPI.Domain.RelationalDb/FDAAPI.Domain.RelationalDb.csproj" \
  --startup-project "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj" \
  --output-dir Migrations

# Apply migration
dotnet ef database update \
  --project "src/Core/Domain/FDAAPI.Domain.RelationalDb/FDAAPI.Domain.RelationalDb.csproj" \
  --startup-project "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj"
```

---

## Testing Strategy

### Test Cases

#### TEST 1: Initiate Google Login

**Request**:
```bash
curl -X GET "http://localhost:5232/api/v1/auth/google"
```

**Expected Response**:
```json
{
  "success": true,
  "message": "Redirect to Google for authentication",
  "authorizationUrl": "https://accounts.google.com/o/oauth2/v2/auth?client_id=...&state=...",
  "state": "base64_encoded_state"
}
```

#### TEST 2: First Login (Auto-Registration)

**Scenario**: New user logs in with Google for the first time

**Steps**:
1. Open `authorizationUrl` in browser
2. Login with Google account
3. Grant consent
4. Redirected to callback URL with `code` and `state`

**Expected Result**:
- New `User` record created
- `UserOAuthProvider` record created
- Default `USER` role assigned
- JWT tokens returned

#### TEST 3: Re-Login (Existing User)

**Scenario**: User who previously logged in with Google logs in again

**Expected Result**:
- No new `User` created
- `UserOAuthProvider` updated with new tokens
- JWT tokens returned

#### TEST 4: Account Linking

**Scenario**: User with existing email/password account logs in with Google (same email)

**Expected Result**:
- No new `User` created
- `UserOAuthProvider` linked to existing user
- User can now login with both methods

#### TEST 5: Callback State Mismatch

**Request**:
```bash
curl "http://localhost:5232/api/v1/auth/google/callback?code=VALID_CODE&state=INVALID_STATE"
```

**Expected Response**: 400 Bad Request
```json
{
  "success": false,
  "message": "Invalid or expired state token"
}
```

---

## Security Considerations

### 1. CSRF Protection

- Generate random `state` parameter for each OAuth request
- Store state in cache (Redis) with 5-minute expiration
- Verify state on callback

### 2. Token Security

- **Encrypt OAuth tokens** before storing in database (use ASP.NET Core Data Protection)
- **Never log sensitive tokens** (access_token, refresh_token)
- Store Google refresh token securely for future use

### 3. HTTPS Only

- OAuth redirect URIs must use HTTPS in production
- Set `Secure` and `HttpOnly` flags on cookies

### 4. Scope Minimization

- Only request `openid email profile` scopes
- Do not request unnecessary permissions

### 5. ID Token Verification

- Verify `aud` (audience) matches client ID
- Verify `iss` (issuer) is `https://accounts.google.com`
- Check token expiration (`exp` claim)

---

## Configuration Steps

### 1. Create Google OAuth Credentials

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create new project: "FDA API"
3. Enable Google+ API
4. Create OAuth 2.0 Client ID:
   - Application type: Web application
   - Authorized redirect URIs:
     - Development: `http://localhost:5232/api/v1/auth/google/callback`
     - Production: `https://api.fda.gov.vn/api/v1/auth/google/callback`
5. Copy Client ID and Client Secret to `appsettings.json`

### 2. Update appsettings.json

**Development**:
```json
{
  "OAuth": {
    "Google": {
      "ClientId": "123456789-abc.apps.googleusercontent.com",
      "ClientSecret": "GOCSPX-xxxxxxxxxxxx",
      "RedirectUri": "http://localhost:5232/api/v1/auth/google/callback"
    }
  }
}
```

**Production** (use environment variables):
```bash
export OAuth__Google__ClientId="..."
export OAuth__Google__ClientSecret="..."
export OAuth__Google__RedirectUri="https://api.fda.gov.vn/api/v1/auth/google/callback"
```

---

## Project Structure Summary

```
src/
├── Core/
│   ├── Application/
│   │   ├── FDAAPI.App.FeatG10/          # Google Login Initiate
│   │   │   ├── GoogleLoginInitiateRequest.cs
│   │   │   ├── GoogleLoginInitiateResponse.cs
│   │   │   └── GoogleLoginInitiateHandler.cs
│   │   └── FDAAPI.App.FeatG11/          # Google OAuth Callback
│   │       ├── GoogleOAuthCallbackRequest.cs
│   │       ├── GoogleOAuthCallbackResponse.cs
│   │       └── GoogleOAuthCallbackHandler.cs
│   └── Domain/
│       └── FDAAPI.Domain.RelationalDb/
│           ├── Entities/
│           │   └── UserOAuthProvider.cs
│           └── RepositoryContracts/
│               └── IUserOAuthProviderRepository.cs
├── External/
│   ├── Infrastructure/
│   │   ├── Services/
│   │   │   └── FDAAPI.Infra.Services/
│   │   │       ├── GoogleOAuthService.cs
│   │   │       └── IGoogleOAuthService.cs
│   │   └── Persistence/
│   │       └── FDAAPI.Infra.Persistence/
│   │           └── Repositories/
│   │               └── PgsqlUserOAuthProviderRepository.cs
│   └── Presentation/
│       └── FDAAPI.Presentation.FastEndpointBasedApi/
│           └── Endpoints/
│               ├── Feat10/
│               │   ├── GoogleLoginInitiateEndpoint.cs
│               │   └── DTOs/
│               │       ├── GoogleLoginInitiateRequestDto.cs
│               │       └── GoogleLoginInitiateResponseDto.cs
│               └── Feat11/
│                   ├── GoogleOAuthCallbackEndpoint.cs
│                   └── DTOs/
│                       ├── GoogleOAuthCallbackRequestDto.cs
│                       └── GoogleOAuthCallbackResponseDto.cs
```

---

## Implementation Checklist

### Phase 1: Domain Layer
- [ ] Create `UserOAuthProvider` entity
- [ ] Update `User` entity with navigation property
- [ ] Create `IUserOAuthProviderRepository` interface
- [ ] Update `AppDbContext` with OAuth configuration

### Phase 2: Infrastructure Layer
- [ ] Create `GoogleOAuthService` with DTOs
- [ ] Implement `PgsqlUserOAuthProviderRepository`
- [ ] Add `HttpClientFactory` registration

### Phase 3: Application Layer
- [ ] Create FEAT10 project and files
- [ ] Create FEAT11 project and files
- [ ] Implement handlers with business logic

### Phase 4: Presentation Layer
- [ ] Create FEAT10 endpoint and DTOs
- [ ] Create FEAT11 endpoint and DTOs

### Phase 5: Configuration
- [ ] Register services in `ServiceExtensions.cs`
- [ ] Update `appsettings.json` with OAuth config
- [ ] Configure Google Cloud Console credentials

### Phase 6: Migration
- [ ] Create EF Core migration
- [ ] Apply migration to database
- [ ] Verify tables created

### Phase 7: Testing
- [ ] Test initiate OAuth flow
- [ ] Test first login (auto-registration)
- [ ] Test re-login
- [ ] Test account linking
- [ ] Test state mismatch error
- [ ] Test invalid code error

### Phase 8: Documentation
- [ ] Update API documentation
- [ ] Create Postman collection
- [ ] Document configuration steps

---

## Future Enhancements

- [ ] Support Facebook OAuth
- [ ] Support Apple Sign-In
- [ ] Implement PKCE for enhanced security
- [ ] Add ability to unlink OAuth providers
- [ ] Admin dashboard to view OAuth connections
- [ ] Rate limiting for OAuth endpoints
