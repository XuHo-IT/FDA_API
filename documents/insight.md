# 📋 TÓM TẮT HIỂU BIẾT VỀ DỰ ÁN FDA API

> **Last Updated**: 2026-01-09
> **Version**: 2.0
> **Purpose**: Comprehensive overview of FDA API codebase architecture, patterns, and implementation guidelines

---

## 🎯 MỤC ĐÍCH DỰ ÁN

**FDA (Flood Detection & Alert) API** là hệ thống quản lý và cảnh báo lũ lụt sử dụng dữ liệu từ IoT sensors và cung cấp API cho ứng dụng mobile/web.

### Tech Stack
- **Architecture**: Domain-Centric (Clean Architecture + CQRS)
- **Framework**: ASP.NET Core 8.0
- **Database**: PostgreSQL với EF Core
- **Cache**: Redis (distributed cache + OAuth state)
- **API Framework**: FastEndpoints (không dùng Controllers)
- **Authentication**: JWT Bearer + OAuth 2.0 (Google)
- **Image Storage**: ImageKit
- **Total Features**: 27 features implemented (FeatG1-G27)

---

## 🏗️ KIẾN TRÚC 4 LAYERS

### Nguyên tắc Domain-Centric
```
┌─────────────────────────────────────┐
│   PRESENTATION (FastEndpoints)      │ ← HTTP Layer
└────────────┬────────────────────────┘
             │
┌────────────▼────────────────────────┐
│   APPLICATION (CQRS Handlers)       │ ← Business Workflow
└────────────┬────────────────────────┘
             │
┌────────────▼────────────────────────┐
│   DOMAIN (Entities, Interfaces)     │ ← Core Business Logic
└────────────┬────────────────────────┘
             │
┌────────────▼────────────────────────┐
│   INFRASTRUCTURE (PostgreSQL, etc)  │ ← Technical Details
└─────────────────────────────────────┘
```

### 1️⃣ **CORE/DOMAIN** - Entities & Repository Interfaces

**Location**: `src/Core/Domain/FDAAPI.Domain.RelationalDb/`

**Responsibilities**:
- Định nghĩa domain models (Entities)
- Định nghĩa repository interfaces
- Business rules và constraints
- **Không phụ thuộc** vào bất kỳ technology/framework nào

**Key Components**:
```
Entities/
├── User.cs                    # User entity với roles
├── Role.cs                    # Role hierarchy
├── UserRole.cs                # Many-to-many mapping
├── Station.cs                 # IoT sensor station
├── WaterLevel.cs              # Water level reading
├── RefreshToken.cs            # JWT refresh tokens
├── OtpCode.cs                 # Phone verification
└── UserOAuthProvider.cs       # OAuth linking

Entities.Base/
├── IEntity                    # Marker interface
├── EntityWithId<TEntityId>    # Base class với generic PK
├── ICreatedEntity<TKey>       # CreatedBy, CreatedAt
├── IUpdatedEntity<TKey>       # UpdatedBy, UpdatedAt
└── ITemporarilyRemovedEntity  # Soft delete support

Repositories/
├── IUserRepository
├── IRoleRepository
├── IStationRepository
├── IWaterLevelRepository
├── IRefreshTokenRepository
├── IOtpCodeRepository
└── IUserOAuthProviderRepository

RelationalDB/
└── AppDbContext.cs            # EF Core DbContext
```

**Entity Pattern Example**:
```csharp
public class User : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
{
    public string Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public string Provider { get; set; } = "local"; // local|google|facebook
    public string Status { get; set; } = "active";  // active|inactive|banned

    // Audit fields (from interfaces)
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual ICollection<UserRole> UserRoles { get; set; }

    [JsonIgnore]
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
}
```

---

### 2️⃣ **CORE/APPLICATION** - Business Logic (CQRS Pattern)

**Location**: `src/Core/Application/`

**Responsibilities**:
- Orchestrates business workflows
- Implements CQRS pattern (Commands + Queries)
- Validates business rules
- Coordinates repositories và services

**Structure**:
```
FDAAPI.App.Common/           # Shared interfaces & services
├── Features/
│   ├── IFeatureHandler.cs
│   ├── IFeatureRequest.cs
│   └── IFeatureResponse.cs
├── Services/
│   ├── IJwtTokenService.cs
│   ├── IPasswordHasher.cs
│   ├── IImageStorageService.cs
│   └── IUserProfileMapper.cs
└── Models/
    ├── AdminResponseStatusCode.cs
    └── StationStatusCode.cs

FDAAPI.App.FeatG1/           # Feature: Create Water Level
├── CreateWaterLevelRequest.cs
├── CreateWaterLevelResponse.cs
└── CreateWaterLevelHandler.cs

FDAAPI.App.FeatG7_AuthLogin/ # Feature: User Login
├── LoginRequest.cs
├── LoginResponse.cs
└── LoginHandler.cs

... (27 feature projects total)
```

**Feature Pattern** (mỗi feature có 3 components):

**1. Request (Input Model)**
```csharp
// Option 1: Record (immutable)
public sealed record LoginRequest(
    string? Identifier,
    string? Email,
    string? OtpCode,
    string? PhoneNumber,
    string? Password,
    string? DeviceInfo,
    string? IpAddress
) : IFeatureRequest<LoginResponse>;

// Option 2: Class (mutable)
public class CreateStationRequest : IFeatureRequest<CreateStationResponse>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}
```

**2. Handler (Business Logic)**
```csharp
public class LoginHandler : IRequestHandler<LoginRequest, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpCodeRepository _otpRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtService;

    public LoginHandler(
        IUserRepository userRepository,
        IOtpCodeRepository otpRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtService)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> Handle(
        LoginRequest request,
        CancellationToken ct)
    {
        // 1. Validate input
        // 2. Authenticate user (OTP or Password)
        // 3. Generate JWT tokens
        // 4. Return response

        return new LoginResponse
        {
            Success = true,
            Message = "Login successful",
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
}
```

**3. Response (Output Model)**
```csharp
public class LoginResponse : IFeatureResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserDto? User { get; set; }
}
```

---

### 3️⃣ **EXTERNAL/INFRASTRUCTURE** - Technical Implementations

**Location**: `src/External/Infrastructure/`

**Responsibilities**:
- Implements domain repository interfaces
- Provides external service integrations
- Manages database operations (EF Core)
- Configures dependency injection

**Structure**:
```
FDAAPI.Infra.Persistence/Repositories/
├── PgsqlUserRepository.cs
├── PgsqlStationRepository.cs
├── PgsqlWaterLevelRepository.cs
├── PgsqlRoleRepository.cs
├── PgsqlUserRoleRepository.cs
├── PgsqlRefreshTokenRepository.cs
├── PgsqlOtpCodeRepository.cs
└── PgsqlUserOAuthProviderRepository.cs

FDAAPI.Infra.Services/
├── Auth/
│   ├── JwtTokenService.cs        # JWT generation/validation
│   ├── PasswordHasher.cs         # PBKDF2 password hashing
│   └── ImageKitService.cs        # Image upload to ImageKit
├── OAuth/
│   ├── GoogleOAuthService.cs     # Google OAuth flow
│   └── GoogleOAuthModels.cs      # Google API DTOs
└── Cache/
    └── RedisStateCache.cs        # Redis-based state cache

FDAAPI.Infra.Configuration/
└── ServiceExtensions.cs          # DI registration hub
```

**Repository Implementation Example**:
```csharp
public class PgsqlUserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public PgsqlUserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<Guid> CreateAsync(User user, CancellationToken ct)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
        return user.Id;
    }
}
```

**Service Registration (ServiceExtensions.cs)**:
```csharp
// Application Services (MediatR + Custom Handlers)
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(SendOtpRequest).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(LoginRequest).Assembly);
        // ... all feature assemblies
    });

    // Custom handlers (non-MediatR)
    services.AddTransient<IFeatureHandler<CreateWaterLevelRequest, CreateWaterLevelResponse>,
                          CreateWaterLevelHandler>();

    return services;
}

// Infrastructure Services
public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
{
    services.AddScoped<IJwtTokenService, JwtTokenService>();
    services.AddScoped<IPasswordHasher, PasswordHasher>();
    services.AddScoped<IImageStorageService, ImageKitService>();
    services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();

    return services;
}

// Persistence Services (Repositories)
public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
{
    services.AddScoped<IUserRepository, PgsqlUserRepository>();
    services.AddScoped<IStationRepository, PgsqlStationRepository>();
    services.AddScoped<IWaterLevelRepository, PgsqlWaterLevelRepository>();
    // ... all repositories

    return services;
}

// Authentication & Authorization
public static IServiceCollection AddAuthenticationServices(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => { /* JWT config */ });

    services.AddAuthorization(options =>
    {
        options.AddPolicy("SuperAdmin", policy =>
            policy.RequireRole("SUPERADMIN"));
        options.AddPolicy("Admin", policy =>
            policy.RequireRole("ADMIN", "SUPERADMIN"));
        options.AddPolicy("Authority", policy =>
            policy.RequireRole("AUTHORITY", "ADMIN", "SUPERADMIN"));
        options.AddPolicy("User", policy =>
            policy.RequireRole("USER", "AUTHORITY", "ADMIN", "SUPERADMIN"));
    });

    return services;
}
```

---

### 4️⃣ **EXTERNAL/PRESENTATION** - HTTP API (FastEndpoints)

**Location**: `src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/`

**Responsibilities**:
- Exposes HTTP endpoints
- Handles request/response DTOs
- Manages authentication/authorization
- Validates input
- Maps DTOs ↔ Application models

**Structure**:
```
Endpoints/
├── Feat1_CreateWaterLevel/
│   ├── CreateWaterLevelEndpoint.cs
│   └── DTOs/
│       ├── CreateWaterLevelRequestDto.cs
│       └── CreateWaterLevelResponseDto.cs
│
├── Feat7_AuthLogin/
│   ├── LoginEndpoint.cs
│   └── DTOs/
│       ├── LoginRequestDto.cs
│       └── LoginResponseDto.cs
│
└── ... (27 endpoint folders)

Program.cs                    # Startup & middleware configuration
appsettings.json             # Configuration
```

**Endpoint Pattern**:
```csharp
public class LoginEndpoint : Endpoint<LoginRequestDto, LoginResponseDto>
{
    private readonly IMediator _mediator;

    public LoginEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/v1/auth/login");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "User login";
            s.Description = "Authenticate user with phone+OTP or email+password";
            s.ExampleRequest = new LoginRequestDto
            {
                Identifier = "+84901234567",
                OtpCode = "123456"
            };
        });

        Tags("Authentication", "Login");
    }

    public override async Task HandleAsync(LoginRequestDto req, CancellationToken ct)
    {
        try
        {
            // 1. Map DTO → Application Request
            var command = new LoginRequest(
                req.Identifier,
                null,
                req.OtpCode,
                null,
                req.Password,
                req.DeviceInfo,
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            // 2. Send to handler via MediatR
            var result = await _mediator.Send(command, ct);

            // 3. Map Application Response → DTO
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
                    Roles = result.User.Roles
                } : null
            };

            // 4. Send HTTP response
            if (result.Success)
                await SendAsync(responseDto, 200, ct);
            else
                await SendAsync(responseDto, 401, ct);
        }
        catch (Exception ex)
        {
            await SendAsync(new LoginResponseDto
            {
                Success = false,
                Message = $"An unexpected error occurred: {ex.Message}"
            }, 500, ct);
        }
    }
}
```

**Authorization Examples**:
```csharp
// Public endpoint (no auth required)
AllowAnonymous();

// Authenticated users only (any role)
AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

// Role-based authorization
Policies("Admin");           // ADMIN or SUPERADMIN
Policies("Authority");       // AUTHORITY, ADMIN, or SUPERADMIN
Policies("User");            // Any authenticated user

// Multiple policies (AND logic)
Policies("Admin", "SomeOtherPolicy");
```

---

## 🔑 FEATURES ĐÃ IMPLEMENT (27 FEATURES)

### **Authentication & Security** (9 features)

| Feature | Endpoint | Method | Auth | Description |
|---------|----------|--------|------|-------------|
| FeatG6 | `/api/v1/auth/send-otp` | POST | Anonymous | Send OTP to phone |
| FeatG7 | `/api/v1/auth/login` | POST | Anonymous | Login with Phone+OTP or Email+Password |
| FeatG8 | `/api/v1/auth/refresh-token` | POST | Anonymous | Refresh access token |
| FeatG9 | `/api/v1/auth/logout` | POST | Authenticated | Revoke refresh token |
| FeatG10 | `/api/v1/auth/change-password` | POST | Authenticated | Change password |
| FeatG11 | `/api/v1/auth/set-password` | POST | Authenticated | Set password for OTP-only users |
| FeatG12 | `/api/v1/auth/google` | GET | Anonymous | Initiate Google OAuth |
| FeatG13 | `/api/v1/auth/google/callback` | GET | Anonymous | Google OAuth callback |
| FeatG16 | `/api/v1/auth/google/mobile` | POST | Anonymous | Google mobile login |
| FeatG17 | `/api/v1/auth/check-identifier` | POST | Anonymous | Check email/phone availability |

### **User Profile Management** (3 features)

| Feature | Endpoint | Method | Auth | Description |
|---------|----------|--------|------|-------------|
| FeatG14 | `/api/v1/profile` | GET | Authenticated | Get current user profile |
| FeatG15 | `/api/v1/profile` | PUT | Authenticated | Update profile |
| FeatG19 | `/api/v1/profile/verify-phone` | POST | Authenticated | Verify & update phone number |

### **User Management (Admin)** (3 features)

| Feature | Endpoint | Method | Auth | Description |
|---------|----------|--------|------|-------------|
| FeatG20 | `/api/v1/admin/users` | POST | Admin | Create new user |
| FeatG21 | `/api/v1/admin/users` | GET | Admin | List users (paginated) |
| FeatG22 | `/api/v1/admin/users/{id}` | PUT | Admin | Update user |

### **Station Management** (5 features)

| Feature | Endpoint | Method | Auth | Description |
|---------|----------|--------|------|-------------|
| FeatG23 | `/api/v1/stations` | POST | Admin | Create station |
| FeatG24 | `/api/v1/stations/{id}` | PUT | Admin | Update station |
| FeatG25 | `/api/v1/stations` | GET | Public/Admin | List stations (paginated) |
| FeatG26 | `/api/v1/stations/{id}` | GET | Public/Admin | Get station details |
| FeatG27 | `/api/v1/stations/{id}` | DELETE | Admin | Delete station |

### **Water Level Management** (4 features)

| Feature | Endpoint | Method | Auth | Description |
|---------|----------|--------|------|-------------|
| FeatG1 | `/api/v1/water-levels` | POST | Admin/Authority | Create water level |
| FeatG2 | `/api/v1/water-levels/{id}` | PUT | Admin/Authority | Update water level |
| FeatG3 | `/api/v1/water-levels/{id}` | GET | Public | Get water level |
| FeatG4 | `/api/v1/water-levels/{id}` | DELETE | Admin/Authority | Delete water level |

### **Static Data** (1 feature)

| Feature | Endpoint | Method | Auth | Description |
|---------|----------|--------|------|-------------|
| FeatG5 | `/api/v1/static-data` | GET | Anonymous | Get static reference data |

### **Media Upload** (1 feature)

| Feature | Endpoint | Method | Auth | Description |
|---------|----------|--------|------|-------------|
| FeatG18 | `/api/v1/upload/image` | POST | Authenticated | Upload image to ImageKit |

---

## 🔐 AUTHENTICATION & AUTHORIZATION

### **Role Hierarchy** (4 tiers)
```
SUPERADMIN (highest privilege)
    ↓
  ADMIN
    ↓
AUTHORITY (Government Officer)
    ↓
  USER (Citizen)
```

### **Authorization Policies**

| Policy | Roles Allowed | Use Case |
|--------|---------------|----------|
| `SuperAdmin` | SUPERADMIN | System administration |
| `Admin` | ADMIN, SUPERADMIN | User & station management |
| `Authority` | AUTHORITY, ADMIN, SUPERADMIN | Data entry, monitoring |
| `User` | All authenticated users | Profile, view data |
| `AllowAnonymous()` | Public access | Login, registration, public data |

### **JWT Configuration**

**Access Token**:
- **Lifetime**: 60 minutes
- **Algorithm**: HMAC-SHA256 (HS256)
- **Claims**:
  - `sub`: User ID (Guid)
  - `email`: User email
  - `jti`: JWT ID (Guid)
  - `role`: User roles (can be multiple)
  - `iat`: Issued at (Unix timestamp)

**Refresh Token**:
- **Lifetime**: 7 days
- **Storage**: Database (refresh_tokens table)
- **Features**:
  - Token rotation (old token revoked when refreshing)
  - Revocation support (logout)
  - Device tracking (DeviceInfo, IpAddress)

**Example JWT Payload**:
```json
{
  "sub": "123e4567-e89b-12d3-a456-426614174000",
  "email": "user@example.com",
  "role": ["USER", "AUTHORITY"],
  "jti": "987fcdeb-51a2-43f1-b789-123456789abc",
  "iat": 1704067200,
  "exp": 1704070800,
  "iss": "FDA_API",
  "aud": "FDA_Clients"
}
```

### **OAuth 2.0 (Google)**

**Web Flow** (FeatG12 + FeatG13):
1. User clicks "Login with Google"
2. Redirect to Google OAuth consent screen
3. User grants permission
4. Google redirects back with authorization code
5. Backend exchanges code for access token
6. Backend validates token, creates/updates user
7. Returns JWT tokens

**Mobile Flow** (FeatG16):
1. Mobile app uses Google Sign-In SDK
2. Gets ID token from Google
3. Sends ID token to backend
4. Backend validates token directly
5. Returns JWT tokens

**Security**:
- CSRF protection via state parameter (cached in Redis)
- Token validation with Google public keys
- Auto-linking to existing accounts by email

---

## 📊 DATABASE SCHEMA (PostgreSQL)

### **Core Tables**

#### **users** (Authentication & Profile)
```sql
CREATE TABLE users (
    id UUID PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255),           -- Nullable for OTP-only users
    full_name VARCHAR(255),
    phone_number VARCHAR(50) UNIQUE,
    avatar_url TEXT,
    provider VARCHAR(50) DEFAULT 'local', -- local|google|facebook
    status VARCHAR(20) DEFAULT 'active',  -- active|inactive|banned
    last_login_at TIMESTAMPTZ,
    phone_verified_at TIMESTAMPTZ,
    email_verified_at TIMESTAMPTZ,
    created_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_by UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL
);

CREATE UNIQUE INDEX ix_users_email ON users(email);
CREATE UNIQUE INDEX ix_users_phone ON users(phone_number);
CREATE INDEX ix_users_status ON users(status);
```

#### **roles** (Authorization)
```sql
CREATE TABLE roles (
    id UUID PRIMARY KEY,
    code VARCHAR(50) UNIQUE NOT NULL,     -- SUPERADMIN|ADMIN|AUTHORITY|USER
    name VARCHAR(100) NOT NULL
);

-- Seed Data
INSERT INTO roles (id, code, name) VALUES
    (gen_random_uuid(), 'SUPERADMIN', 'Super Administrator'),
    (gen_random_uuid(), 'ADMIN', 'Administrator'),
    (gen_random_uuid(), 'AUTHORITY', 'Government Officer'),
    (gen_random_uuid(), 'USER', 'Citizen User');
```

#### **user_roles** (Many-to-Many)
```sql
CREATE TABLE user_roles (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role_id UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    CONSTRAINT uq_user_roles UNIQUE (user_id, role_id)
);

CREATE INDEX ix_user_roles_user ON user_roles(user_id);
CREATE INDEX ix_user_roles_role ON user_roles(role_id);
```

#### **refresh_tokens** (Session Management)
```sql
CREATE TABLE refresh_tokens (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token VARCHAR(255) UNIQUE NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    is_revoked BOOLEAN DEFAULT false,
    revoked_at TIMESTAMPTZ,
    device_info TEXT,
    ip_address VARCHAR(50),
    created_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL
);

CREATE UNIQUE INDEX ix_refresh_tokens_token ON refresh_tokens(token);
CREATE INDEX ix_refresh_tokens_user ON refresh_tokens(user_id);
CREATE INDEX ix_refresh_tokens_revoked ON refresh_tokens(is_revoked);
```

#### **otp_codes** (Phone Verification)
```sql
CREATE TABLE otp_codes (
    id UUID PRIMARY KEY,
    phone_number VARCHAR(50) NOT NULL,
    code VARCHAR(6) NOT NULL,             -- 6-digit OTP
    expires_at TIMESTAMPTZ NOT NULL,      -- 5 minutes from creation
    is_used BOOLEAN DEFAULT false,
    used_at TIMESTAMPTZ,
    attempt_count INT DEFAULT 0,          -- Track failed attempts
    created_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL
);

CREATE INDEX ix_otp_codes_phone ON otp_codes(phone_number);
CREATE INDEX ix_otp_codes_phone_used ON otp_codes(phone_number, is_used);
```

#### **stations** (IoT Sensor Stations)
```sql
CREATE TABLE stations (
    id UUID PRIMARY KEY,
    code VARCHAR(50) UNIQUE NOT NULL,     -- ST_DN_01
    name VARCHAR(255),
    location_desc TEXT,
    latitude NUMERIC(10,6),
    longitude NUMERIC(10,6),
    road_name VARCHAR(255),
    direction VARCHAR(100),               -- upstream|downstream|road section
    status VARCHAR(20),                   -- active|offline|maintenance
    installed_at TIMESTAMPTZ,
    last_seen_at TIMESTAMPTZ,
    created_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_by UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL
);

CREATE INDEX ix_station_status ON stations(status);
CREATE INDEX ix_station_geo ON stations(latitude, longitude);
```

#### **water_levels** (Sensor Readings)
```sql
CREATE TABLE water_levels (
    id BIGSERIAL PRIMARY KEY,
    location_name VARCHAR(255),
    value NUMERIC(10,2),
    unit VARCHAR(50),
    measured_at TIMESTAMPTZ,
    station_id UUID REFERENCES stations(id),
    distance NUMERIC(10,2),
    sensor_height NUMERIC(10,2),
    status VARCHAR(50),
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ                -- Soft delete support
);
```

#### **user_oauth_providers** (OAuth Linking)
```sql
CREATE TABLE user_oauth_providers (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    provider VARCHAR(50) NOT NULL,        -- google|facebook
    provider_user_id VARCHAR(255) NOT NULL,
    email VARCHAR(255),
    created_at TIMESTAMPTZ NOT NULL,
    CONSTRAINT uq_provider_user UNIQUE (provider, provider_user_id)
);

CREATE INDEX ix_oauth_user ON user_oauth_providers(user_id);
```

### **Database Features**

✅ **Constraints**:
- Primary keys (UUID for most tables)
- Unique constraints (email, phone_number, tokens)
- Foreign keys with CASCADE delete
- Check constraints (status enums)

✅ **Indexes**:
- Unique indexes for lookups (email, phone)
- Composite indexes for queries (user_id + role_id)
- Geo indexes for spatial queries (lat/lng)

✅ **Audit Trail**:
- CreatedBy, CreatedAt (who/when created)
- UpdatedBy, UpdatedAt (who/when last modified)
- Implemented via ICreatedEntity, IUpdatedEntity interfaces

✅ **Soft Delete**:
- DeletedAt timestamp (nullable)
- Allows data recovery
- Implemented via ITemporarilyRemovedEntity

✅ **Auto-Migration**:
- EF Core migrations in Domain layer
- Auto-applied on startup (Development/Staging)
- Manual migration recommended for Production

---

## 🎨 PATTERNS & BEST PRACTICES

### **1. CQRS Pattern (Command Query Responsibility Segregation)**

**Commands** (Write Operations):
```
CreateWaterLevel, UpdateWaterLevel, DeleteWaterLevel
CreateUser, UpdateUser
Login, Logout, SendOTP
```

**Queries** (Read Operations):
```
GetWaterLevel, GetProfile
ListUsers, ListStations
GetStaticData
```

**Benefits**:
- Separate optimization for reads vs writes
- Clear separation of concerns
- Easier to test
- Scalable (can use different data stores)

### **2. Result Object Pattern**

Instead of throwing exceptions, return result objects:

```csharp
// Good ✅
public class Response : IFeatureResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public TData Data { get; set; }
}

// Avoid ❌
throw new UnauthorizedException("Invalid credentials");
```

**Benefits**:
- Explicit error handling
- Better for API responses
- No performance overhead of exceptions

### **3. Repository Pattern**

**Interface in Domain** (contract):
```csharp
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    Task<Guid> CreateAsync(User user, CancellationToken ct);
}
```

**Implementation in Infrastructure** (technical detail):
```csharp
public class PgsqlUserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    // Implementation using EF Core
}
```

**Benefits**:
- Domain doesn't depend on EF Core
- Easy to switch database (PostgreSQL → MySQL)
- Mockable for unit tests

### **4. Dependency Injection (Constructor Injection)**

```csharp
public class LoginHandler : IRequestHandler<LoginRequest, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtService;

    // Dependencies injected via constructor
    public LoginHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }
}
```

**Lifetimes**:
- **Transient**: Handlers (created per request)
- **Scoped**: Repositories, DbContext (per HTTP request)
- **Singleton**: Configuration, HttpClient

### **5. Feature Organization (Vertical Slices)**

Each feature is self-contained:

```
FDAAPI.App.FeatG7_AuthLogin/
├── LoginRequest.cs         # Input
├── LoginResponse.cs        # Output
└── LoginHandler.cs         # Logic

Endpoints/Feat7_AuthLogin/
├── LoginEndpoint.cs        # HTTP handler
└── DTOs/
    ├── LoginRequestDto.cs  # HTTP input
    └── LoginResponseDto.cs # HTTP output
```

**Benefits**:
- Easy to find all code for a feature
- Can develop features independently
- Clear boundaries

### **6. Async/Await Pattern**

```csharp
// All I/O operations are async
public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
{
    return await _context.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.Email == email, ct);
}
```

**Benefits**:
- Non-blocking I/O
- Better scalability
- Proper cancellation support

### **7. DTO Mapping Pattern**

```
HTTP Request DTO → Application Request → Handler → Application Response → HTTP Response DTO
```

**Why separate DTOs?**:
- Presentation layer concerns (validation, formatting)
- Versioning (v1, v2 DTOs with same handler)
- Security (don't expose internal models)

---

## 🚀 WORKFLOW IMPLEMENT FEATURE MỚI

Theo template chi tiết trong [documents/Prompt-Template-For-New-Features.md](./Prompt-Template-For-New-Features.md)

### **Phase 1: Domain Layer** (Entities & Interfaces)

**Deliverables**:
- [ ] Create/update entities in `Entities/`
- [ ] Create repository interfaces in `Repositories/`
- [ ] Update `AppDbContext.cs` (DbSet, OnModelCreating)

**Example**:
```csharp
// Entities/Area.cs
public class Area : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    [JsonIgnore]
    public virtual User User { get; set; }
}

// Repositories/IAreaRepository.cs
public interface IAreaRepository
{
    Task<Area?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<Area>> GetByUserIdAsync(Guid userId, CancellationToken ct);
    Task<Guid> CreateAsync(Area area, CancellationToken ct);
}
```

---

### **Phase 2: Infrastructure Layer** (Implementations)

**Deliverables**:
- [ ] Implement repository in `Persistence/Repositories/`
- [ ] Create services if needed in `Services/`

**Example**:
```csharp
// PgsqlAreaRepository.cs
public class PgsqlAreaRepository : IAreaRepository
{
    private readonly AppDbContext _context;

    public async Task<Area?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Areas
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }
}
```

---

### **Phase 3: Application Layer** (Business Logic)

**Deliverables**:
- [ ] Create project `FDAAPI.App.FeatGX_{FeatureName}/`
- [ ] Create `.csproj` with proper references
- [ ] Create Request, Response, Handler

**Example**:
```csharp
// CreateAreaRequest.cs
public sealed record CreateAreaRequest(
    string Name,
    decimal Latitude,
    decimal Longitude,
    int RadiusMeters
) : IFeatureRequest<CreateAreaResponse>;

// CreateAreaResponse.cs
public class CreateAreaResponse : IFeatureResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public Guid? AreaId { get; set; }
}

// CreateAreaHandler.cs
public class CreateAreaHandler : IRequestHandler<CreateAreaRequest, CreateAreaResponse>
{
    private readonly IAreaRepository _areaRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public async Task<CreateAreaResponse> Handle(
        CreateAreaRequest request,
        CancellationToken ct)
    {
        // 1. Get current user ID from JWT
        var userId = Guid.Parse(_httpContextAccessor.HttpContext.User.FindFirst("sub").Value);

        // 2. Validate business rules
        if (request.RadiusMeters < 100 || request.RadiusMeters > 50000)
            return new CreateAreaResponse
            {
                Success = false,
                Message = "Radius must be between 100-50000 meters"
            };

        // 3. Create entity
        var area = new Area
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            RadiusMeters = request.RadiusMeters,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        // 4. Save to database
        var areaId = await _areaRepository.CreateAsync(area, ct);

        // 5. Return response
        return new CreateAreaResponse
        {
            Success = true,
            Message = "Area created successfully",
            AreaId = areaId
        };
    }
}
```

---

### **Phase 4: Presentation Layer** (HTTP API)

**Deliverables**:
- [ ] Create folder `Endpoints/FeatX_{FeatureName}/`
- [ ] Create Endpoint class
- [ ] Create DTOs folder with Request/Response DTOs

**Example**:
```csharp
// CreateAreaEndpoint.cs
public class CreateAreaEndpoint : Endpoint<CreateAreaRequestDto, CreateAreaResponseDto>
{
    private readonly IMediator _mediator;

    public CreateAreaEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/v1/areas");
        Policies("User"); // Authenticated users

        Summary(s =>
        {
            s.Summary = "Create monitored area";
            s.Description = "Create a geographic area for flood monitoring";
            s.ExampleRequest = new CreateAreaRequestDto
            {
                Name = "My Home",
                Latitude = 10.762622m,
                Longitude = 106.660172m,
                RadiusMeters = 1000
            };
        });

        Tags("Areas");
    }

    public override async Task HandleAsync(CreateAreaRequestDto req, CancellationToken ct)
    {
        var command = new CreateAreaRequest(
            req.Name,
            req.Latitude,
            req.Longitude,
            req.RadiusMeters
        );

        var result = await _mediator.Send(command, ct);

        var response = new CreateAreaResponseDto
        {
            Success = result.Success,
            Message = result.Message,
            AreaId = result.AreaId
        };

        await SendAsync(response, result.Success ? 201 : 400, ct);
    }
}

// DTOs/CreateAreaRequestDto.cs
public class CreateAreaRequestDto
{
    public string Name { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int RadiusMeters { get; set; }
}
```

---

### **Phase 5: Configuration & Registration**

**Deliverables**:
- [ ] Register handler in `ServiceExtensions.cs`
- [ ] Register repository in `ServiceExtensions.cs`

**Example**:
```csharp
// AddApplicationServices()
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateAreaRequest).Assembly);
});

// AddPersistenceServices()
services.AddScoped<IAreaRepository, PgsqlAreaRepository>();
```

---

### **Phase 6: Database Migration & Testing**

**Deliverables**:
- [ ] Create migration
- [ ] Apply migration
- [ ] Test with cURL/Postman

**Commands**:
```bash
# Create migration
dotnet ef migrations add AddAreasTable \
  --project "src/Core/Domain/FDAAPI.Domain.RelationalDb/FDAAPI.Domain.RelationalDb.csproj" \
  --startup-project "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj" \
  --output-dir Migrations

# Apply migration
dotnet ef database update \
  --project "src/Core/Domain/FDAAPI.Domain.RelationalDb/FDAAPI.Domain.RelationalDb.csproj" \
  --startup-project "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj"

# Test endpoint
curl -X POST http://localhost:5000/api/v1/areas \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "name": "My Home",
    "latitude": 10.762622,
    "longitude": 106.660172,
    "radiusMeters": 1000
  }'
```

---

### **Phase 7: Documentation**

**Deliverables**:
- [ ] Create `documents/FEX-{FeatureName}-Complete-Documentation.md`
- [ ] Include API specs, test cases, cURL examples

**Template sections**:
1. Implementation Summary
2. API Endpoints (Request/Response examples)
3. Database Schema
4. Test Cases (minimum 5)
5. Test Data (SQL scripts)

---

## 📁 DOCUMENTS QUAN TRỌNG

### **Architecture Documentation**
- [general.md](./general.md) - Domain-Centric principles
- [Architecture Principles.md](./Architecture%20Principles.md) - Layer responsibilities
- [workflow.md](./workflow.md) - Request flow diagrams
- [Package Diagram.md](./Package%20Diagram.md) - Package structure

### **Database Documentation**
- [db.md](./db.md) - Complete database schema (512 lines)

### **Implementation Guides**
- [Prompt-Template-For-New-Features.md](./Prompt-Template-For-New-Features.md) - Step-by-step template (850 lines)
- [FE01-Authentication-Complete-Documentation.md](./FE01-Authentication-Complete-Documentation.md) - Auth implementation example
- [FE02-Google-OAuth-Implementation-Plan.md](./FE02-Google-OAuth-Implementation-Plan.md) - OAuth flow

---

## ✅ CHECKLIST: SẴN SÀNG IMPLEMENT FEATURE MỚI

### **Understanding**
- [x] Hiểu rõ Domain-Centric Architecture (4 layers)
- [x] Hiểu CQRS pattern với MediatR
- [x] Biết 27 features đã có để tham khảo patterns
- [x] Hiểu database schema đầy đủ
- [x] Hiểu JWT authentication & authorization
- [x] Biết workflow 7 phases để implement
- [x] Nắm naming conventions & best practices

### **Tools & Environment**
- [ ] .NET 8 SDK installed
- [ ] PostgreSQL running
- [ ] Redis running (optional for caching)
- [ ] IDE (VS Code, Visual Studio, Rider)
- [ ] Git configured

### **Development Workflow**
1. Read [Prompt-Template-For-New-Features.md](./Prompt-Template-For-New-Features.md)
2. Define feature requirements clearly
3. Follow 7 phases sequentially
4. Test after each phase
5. Document thoroughly

---

## 🎯 NEXT STEPS

**You are now ready to implement new features!**

When you want to add a new feature:
1. Copy the prompt template from [Prompt-Template-For-New-Features.md](./Prompt-Template-For-New-Features.md)
2. Fill in your feature specifications
3. Follow the 7 phases
4. Refer to existing features (FeatG1-27) for patterns

**Example Features to Implement Next**:
- Area Management (user-defined monitoring zones)
- Alert Subscriptions (user notifications)
- Sensor Data Analytics (aggregations, predictions)
- Report Generation (PDF/CSV exports)

---

**Last Updated**: 2026-01-09
**Maintained By**: Development Team
**Questions?**: Check existing documentation or review implemented features
