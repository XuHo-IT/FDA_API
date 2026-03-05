# Prompt Template for Implementing New Features

## 📋 How to Use This Template

Copy the template below and fill in the specific details for your new feature. This ensures the AI assistant has all the context needed to implement the feature following the established patterns.

---

# PROMPT TEMPLATE

```markdown
# Context

Tôi đang develop FDA API với Domain-Centric Architecture (Clean Architecture + CQRS).

## Codebase hiện tại:

- **Architecture**: Domain-Centric với 4 layers (Domain, Application, Infrastructure, Presentation)
- **Framework**: ASP.NET Core 8.0 + FastEndpoints (KHÔNG dùng Controllers)
- **Database**: PostgreSQL với EF Core
- **Pattern**: CQRS với MediatR + FluentValidation
- **Authentication**: JWT + Refresh Token (đã hoàn thành)

## Quy ước đặt tên và cấu trúc:

### 1. Feature Naming Convention

- **Feature Group (Application layer)**: `FDAAPI.App.FeatG{số}_{FeatureName}`
  - Ví dụ: `FDAAPI.App.FeatG32_AreaCreate`, `FDAAPI.App.FeatG7_AuthLogin`
- **Feature (Presentation layer)**: `Feat{số}_{FeatureName}`
  - Ví dụ: `Endpoints/Feat32_AreaCreate/`, `Endpoints/Feat7_AuthLogin/`
- **Naming Pattern**:
  - Request: `{Feature}Request.cs`
  - Response: `{Feature}Response.cs`
  - Handler: `{Feature}Handler.cs`
  - Validator: `{Feature}RequestValidator.cs`
  - Endpoint: `{Feature}Endpoint.cs`
  - Request DTO: `{Feature}RequestDto.cs`
  - Response DTO: `{Feature}ResponseDto.cs`

### 2. Layer Structure
```

src/
├── Core/
│ ├── Domain/FDAAPI.Domain.RelationalDb/
│ │ ├── Entities/ # Entity classes
│ │ │ ├── {Entity}.cs
│ │ │ └── ...
│ │ └── Repositories/ # Repository interfaces only
│ │ ├── I{Entity}Repository.cs
│ │ └── ...
│ └── Application/FDAAPI.App.FeatG{số}_{FeatureName}/
│ ├── {Feature}Request.cs # Input model (MediatR Request)
│ ├── {Feature}Response.cs # Output model
│ ├── {Feature}Handler.cs # Business logic (MediatR Handler)
│ └── {Feature}RequestValidator.cs # FluentValidation validator
│
├── External/
│ ├── Infrastructure/
│ │ ├── Services/FDAAPI.Infra.Services/
│ │ │ └── {Service}/ # Reusable services
│ │ │ ├── I{Service}.cs
│ │ │ └── {Service}.cs
│ │ ├── Persistence/FDAAPI.Infra.Persistence/
│ │ │ └── Repositories/
│ │ │ └── Pgsql{Entity}Repository.cs # Repository implementations
│ │ └── Common/FDAAPI.Infra.Configuration/
│ │ └── ServiceExtensions.cs # DI registration
│ └── Presentation/FDAAPI.Presentation.FastEndpointBasedApi/
│ └── Endpoints/Feat{số}_{FeatureName}/
│ ├── {Feature}Endpoint.cs # FastEndpoint implementation
│ └── DTOs/
│ ├── {Feature}RequestDto.cs # HTTP request DTO
│ └── {Feature}ResponseDto.cs # HTTP response DTO

````

### 3. Request Pattern (Application Layer)
**Interface**: `IFeatureRequest<TResponse>` (wrapper của MediatR's `IRequest<TResponse>`)

**Implementation**:
```csharp
public sealed record {Feature}Request(
    Guid UserId,
    string Field1,
    decimal Field2
) : IFeatureRequest<{Feature}Response>;
````

### 4. Response Pattern (Application Layer)

**Base Properties**:

```csharp
public class {Feature}Response
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public {Feature}StatusCode StatusCode { get; set; }
    public {Feature}Dto? Data { get; set; }
}
```

**StatusCode Enum** (create in `FDAAPI.App.Common/Models/{Domain}/{Feature}StatusCode.cs`):

```csharp
public enum {Feature}StatusCode
{
    Success = 200,
    Created = 201,
    BadRequest = 400,
    NotFound = 404,
    Conflict = 409,
    InternalServerError = 500
}
```

### 5. Handler Pattern (Application Layer)

**Interface**: `IRequestHandler<TRequest, TResponse>` (from MediatR)

**Implementation**:

```csharp
public class {Feature}Handler : IRequestHandler<{Feature}Request, {Feature}Response>
{
    private readonly I{Entity}Repository _{entity}Repository;
    private readonly I{Mapper} _{mapper};
    private readonly I{Service} _{service};

    public {Feature}Handler(
        I{Entity}Repository {entity}Repository,
        I{Mapper} {mapper},
        I{Service} {service})
    {
        _{entity}Repository = {entity}Repository;
        _{mapper} = {mapper};
        _{service} = {service};
    }

    public async Task<{Feature}Response> Handle(
        {Feature}Request request,
        CancellationToken ct)
    {
        // 1. Execute business logic
        // 2. Call repositories
        // 3. Use mapper to convert Entity → DTO
        // 4. Return response with StatusCode

        var entity = new {Entity}
        {
            Id = Guid.NewGuid(),
            Field1 = request.Field1,
            CreatedBy = request.UserId,
            CreatedAt = DateTime.UtcNow
        };

        await _{entity}Repository.CreateAsync(entity, ct);

        return new {Feature}Response
        {
            Success = true,
            Message = "{Entity} created successfully",
            StatusCode = {Feature}StatusCode.Created,
            Data = _{mapper}.MapToDto(entity)
        };
    }
}
```

**Note**: Validators chạy tự động qua `ValidationBehavior`, không cần validate trong Handler.

### 6. Validator Pattern (Application Layer)

**Implementation**:

```csharp
public class {Feature}RequestValidator : AbstractValidator<{Feature}Request>
{
    public {Feature}RequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Field1)
            .NotEmpty().WithMessage("Field1 is required.")
            .MaximumLength(255).WithMessage("Field1 must not exceed 255 characters.");

        RuleFor(x => x.Field2)
            .GreaterThan(0).WithMessage("Field2 must be greater than 0.");
    }
}
```

**Auto-Registration**: Validators tự động được đăng ký qua `AddValidatorsFromAssemblies()` trong `ServiceExtensions.cs`.

### 7. Mapper Pattern (Infrastructure Layer)

**Interface** (in `FDAAPI.App.Common/Services/Mapping/I{Entity}Mapper.cs`):

```csharp
public interface I{Entity}Mapper
{
    {Entity}Dto MapToDto({Entity} entity);
    List<{Entity}Dto> MapToDtoList(List<{Entity}> entities);
}
```

**Implementation** (in `FDAAPI.App.Common/Services/Mapping/{Entity}Mapper.cs`):

```csharp
public class {Entity}Mapper : I{Entity}Mapper
{
    public {Entity}Dto MapToDto({Entity} entity)
    {
        return new {Entity}Dto
        {
            Id = entity.Id,
            Field1 = entity.Field1,
            Field2 = entity.Field2,
            CreatedAt = entity.CreatedAt
        };
    }

    public List<{Entity}Dto> MapToDtoList(List<{Entity}> entities)
    {
        return entities.Select(MapToDto).ToList();
    }
}
```

**Registration** (in `ServiceExtensions.AddInfrastructureServices()`):

```csharp
services.AddScoped<I{Entity}Mapper, {Entity}Mapper>();
```

### 8. FastEndpoints Pattern (Presentation Layer)

**Inherit**: `Endpoint<TRequestDto, TResponseDto>`

**Implementation**:

```csharp
public class {Feature}Endpoint : Endpoint<{Feature}RequestDto, {Feature}ResponseDto>
{
    private readonly IMediator _mediator;

    public {Feature}Endpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/v1/{resource}");           // or Get, Put, Delete
        Policies("Admin", "Authority");                    // or AllowAnonymous()
        Summary(s =>
        {
            s.Summary = "Brief description";
            s.Description = "Detailed description";
            s.ExampleRequest = new {Feature}RequestDto { /* example */ };
        });
        Tags("{Category}", "{SubCategory}");
    }

    public override async Task HandleAsync({Feature}RequestDto req, CancellationToken ct)
    {
        // 1. Extract UserId from JWT claims (if needed)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim == null)
        {
            await SendAsync(new {Feature}ResponseDto
            {
                Success = false,
                Message = "Unauthorized",
                StatusCode = 401
            }, 401, ct);
            return;
        }

        var userId = Guid.Parse(userIdClaim.Value);

        // 2. Map DTO to MediatR Request
        var command = new {Feature}Request(
            userId,
            req.Field1,
            req.Field2
        );

        // 3. Send to MediatR (validation happens automatically)
        var result = await _mediator.Send(command, ct);

        // 4. Map Response to DTO
        var response = new {Feature}ResponseDto
        {
            Success = result.Success,
            Message = result.Message,
            StatusCode = (int)result.StatusCode,
            Data = result.Data
        };

        // 5. Send HTTP response with appropriate status code
        if (result.Success)
        {
            await SendAsync(response, 201, ct); // or 200 for updates
        }
        else
        {
            await SendAsync(response, (int)result.StatusCode, ct);
        }
    }
}
```

### 9. Repository Pattern

**Interface** (in Domain layer):

```csharp
public interface I{Entity}Repository
{
    Task<{Entity}?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<{Entity}>> GetAllAsync(CancellationToken ct = default);
    Task<Guid> CreateAsync({Entity} entity, CancellationToken ct = default);
    Task<bool> UpdateAsync({Entity} entity, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
```

**Implementation** (in Infrastructure layer):

```csharp
public class Pgsql{Entity}Repository : I{Entity}Repository
{
    private readonly AppDbContext _context;

    public Pgsql{Entity}Repository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<{Entity}?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.{Entities}
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    // ... other implementations
}
```

**Registration** (in `ServiceExtensions.cs`):

```csharp
services.AddScoped<I{Entity}Repository, Pgsql{Entity}Repository>();
```

### 10. Entity Pattern (Domain Layer)

**Base Entity**:

```csharp
public class {Entity} : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
{
    // Properties
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties (use [JsonIgnore] to prevent circular references)
    [JsonIgnore]
    public virtual ICollection<Related{Entity}> Related{Entities} { get; set; }
}
```

**AppDbContext Configuration**:

```csharp
modelBuilder.Entity<{Entity}>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasIndex(e => e.Name).IsUnique();
    entity.Property(e => e.Name).IsRequired();

    // Relationships
    entity.HasMany(e => e.Related{Entities})
        .WithOne(r => r.{Entity})
        .HasForeignKey(r => r.{Entity}Id)
        .OnDelete(DeleteBehavior.Cascade);
});
```

---

## Documents đã có (để tham khảo):

1. **Architecture & Workflow**:

   - `documents/general.md` - Domain-Centric Architecture principles
   - `documents/workflow.md` - Request flow diagram và CQRS pattern
   - `documents/Package Diagram.md` - Package structure

2. **Database Schema**:

   - `documents/db.md` - Full database schema (bao gồm auth tables)

3. **Implementation Examples**:
   - `documents/FE01-Authentication-Complete-Documentation.md` - Full auth implementation (FEAT6-9)
   - Existing features: FEAT1-5 (WaterLevel CRUD + Static Data)

---

## Features đã implement (để tham khảo pattern):

### ✅ FEAT32: CreateArea (Ví dụ MediatR pattern - RECOMMENDED)

- **Endpoint**: `POST /api/v1/areas`
- **Auth**: `Policies("Authority")`
- **Pattern**: MediatR + FluentValidation + Mapper
- **Files**:
  - `FDAAPI.App.FeatG32_AreaCreate/CreateAreaRequest.cs` (sealed record)
  - `FDAAPI.App.FeatG32_AreaCreate/CreateAreaResponse.cs`
  - `FDAAPI.App.FeatG32_AreaCreate/CreateAreaHandler.cs` (IRequestHandler)
  - `FDAAPI.App.FeatG32_AreaCreate/CreateAreaRequestValidator.cs` (AbstractValidator)
  - `FDAAPI.App.Common/Models/Areas/AreaStatusCode.cs`
  - `FDAAPI.App.Common/Services/Mapping/IAreaMapper.cs`
  - `FDAAPI.App.Common/Services/Mapping/AreaMapper.cs`
  - `Endpoints/Feat32_AreaCreate/CreateAreaEndpoint.cs` (inject IMediator)
  - `Endpoints/Feat32_AreaCreate/DTOs/CreateAreaRequestDto.cs`
  - `Endpoints/Feat32_AreaCreate/DTOs/CreateAreaResponseDto.cs`

### ✅ FEAT7: Login (Ví dụ complex business logic)

- **Endpoint**: `POST /api/v1/auth/login`
- **Auth**: `AllowAnonymous()`
- **Pattern**: MediatR + Complex Handler (dual auth: OTP + Password)
- **Files**:
  - `FDAAPI.App.FeatG7_AuthLogin/LoginRequest.cs`
  - `FDAAPI.App.FeatG7_AuthLogin/LoginResponse.cs`
  - `FDAAPI.App.FeatG7_AuthLogin/LoginHandler.cs` (complex logic)
  - `Endpoints/Feat7_AuthLogin/LoginEndpoint.cs`

### ✅ FEAT33-38: Area Management (Full CRUD example)

- **FEAT33**: AreaListByUser - `GET /api/v1/areas/me` - Get user's areas
- **FEAT35**: AreaGet - `GET /api/v1/areas/{id}` - Get area by ID
- **FEAT36**: AreaUpdate - `PUT /api/v1/areas/{id}` - Update area
- **FEAT37**: AreaDelete - `DELETE /api/v1/areas/{id}` - Delete area
- **FEAT38**: AreaList - `GET /api/v1/areas` - Admin list all areas
- **Pattern**: MediatR + Mapper + StatusCode enum

### ✅ FEAT23-27: Station Management (Admin features)

- **FEAT23**: StationCreate - `POST /api/v1/stations`
- **FEAT24**: StationUpdate - `PUT /api/v1/stations/{id}`
- **FEAT25**: StationList - `GET /api/v1/stations`
- **FEAT26**: StationGet - `GET /api/v1/stations/{id}`
- **FEAT27**: StationDelete - `DELETE /api/v1/stations/{id}`

---

## Authentication Setup (đã có sẵn):

### JWT Configuration

- **Access Token**: 60 minutes expiration
- **Refresh Token**: 7 days expiration
- **Algorithm**: HMAC-SHA256 (HS256)
- **ClockSkew**: TimeSpan.Zero (strict expiration)

### Available Roles

| Role      | Code      | Description        |
| --------- | --------- | ------------------ |
| Admin     | ADMIN     | Administrator      |
| Moderator | MODERATOR | Government Officer |
| User      | USER      | Citizen User       |

### Authorization Policies

```csharp
// In ServiceExtensions.cs - already configured
options.AddPolicy("Admin", policy => policy.RequireRole("ADMIN"));
options.AddPolicy("Moderator", policy => policy.RequireRole("MODERATOR"));
options.AddPolicy("User", policy => policy.RequireRole("USER", "ADMIN", "MODERATOR"));
```

### Middleware Pipeline (đã setup)

```csharp
app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthentication();  // MUST be before Authorization
app.UseAuthorization();   // MUST be after Authentication
app.UseFastEndpoints();   // MUST be after Auth middleware
```

### Using Authorization in Endpoints

```csharp
// Anonymous access
AllowAnonymous();

// Require authentication (any role)
AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

// Require specific roles
Roles("ADMIN");
Roles("ADMIN", "MODERATOR");
Roles("USER", "ADMIN", "MODERATOR");

// Use policy
Policies("Admin");
```

---

# YÊU CẦU MỚI: [TÊN FEATURE]

## Feature Specification:

[Mô tả chi tiết feature cần implement]

**Example**:

```
Implement user profile management feature allowing users to:
- View their own profile
- Update profile information (full name, avatar)
- Admin can view any user's profile
```

---

## Requirements:

1. [Requirement 1 - Cụ thể, đo lường được]
2. [Requirement 2]
3. [Requirement 3]
   ...

**Example**:

```
1. Endpoint GET /api/v1/users/me - Get current user profile (authenticated users)
2. Endpoint PUT /api/v1/users/me - Update current user profile
3. Endpoint GET /api/v1/users/{id} - Get user by ID (Admin only)
4. Validate: FullName max 255 chars, Avatar URL must be valid
5. Track update history with UpdatedAt and UpdatedBy fields
```

---

## Technical Details:

### Entities cần tạo/sử dụng:

[Liệt kê entities - existing hoặc new]

**Example**:

```
- User (existing) - Will be updated
- UserAuditLog (new) - Track profile changes
```

### Business Logic:

[Mô tả logic nghiệp vụ chi tiết]

**Example**:

```
- User can only update their own profile
- Admin can view any user profile
- Avatar upload: validate file type (jpg, png), max 5MB
- Log all profile updates to UserAuditLog table
- Return 404 if user not found
- Return 403 if non-admin tries to view other user's profile
```

### Authorization Requirements:

[Chỉ rõ auth requirements cho từng endpoint]

**Example**:

```
- GET /api/v1/users/me: Require authentication (any role)
- PUT /api/v1/users/me: Require authentication (any role)
- GET /api/v1/users/{id}: Require role ADMIN
```

### Validation Rules:

[Liệt kê tất cả validation rules]

**Example**:

```
- FullName: Required, max 255 characters
- Email: Valid email format (if updating)
- PhoneNumber: Valid phone format, 10-11 digits
- AvatarUrl: Valid URL format, https only
```

### Database Changes:

[Mô tả changes cần thiết trong database]

**Example**:

```
- Add column: ProfileCompleteness (int) to Users table
- Create new table: UserAuditLogs
  - Id (uuid, PK)
  - UserId (uuid, FK to Users)
  - Action (varchar) - "UPDATE_PROFILE", "UPLOAD_AVATAR"
  - OldValue (jsonb)
  - NewValue (jsonb)
  - CreatedAt (timestamptz)
```

---

## Expected Implementation:

Implement theo đúng pattern đã có, tuân thủ 8 phases sau:

### **Phase 1 - Domain Layer**

**Deliverables**:

- [ ] Tạo/update entities trong `src/Core/Domain/FDAAPI.Domain.RelationalDb/Entities/`
- [ ] Tạo repository interfaces trong `src/Core/Domain/FDAAPI.Domain.RelationalDb/Repositories/`
- [ ] Update `AppDbContext.cs` với DbSet và OnModelCreating configuration

**Acceptance Criteria**:

- Entities có đủ properties theo requirements
- Navigation properties có `[JsonIgnore]` attribute
- Repository interfaces có đầy đủ methods cần thiết
- AppDbContext có indexes, constraints, default values

---

### **Phase 2 - Infrastructure Layer**

**Deliverables**:

- [ ] Implement repositories trong `src/External/Infrastructure/Persistence/FDAAPI.Infra.Persistence/Repositories/`
  - Tên file: `Pgsql{Entity}Repository.cs`
- [ ] Tạo services (nếu cần) trong `src/External/Infrastructure/Services/FDAAPI.Infra.Services/`
  - Interface: `I{Service}.cs`
  - Implementation: `{Service}.cs`

**Acceptance Criteria**:

- Repository implementations use async/await với CancellationToken
- All methods handle exceptions appropriately
- Services are reusable và testable

---

### **Phase 3 - Application Layer**

**Deliverables**:

- [ ] Tạo project mới `src/Core/Application/FDAAPI.App.FeatG{số}_{FeatureName}/`
- [ ] Create `.csproj` file với references:
  ```xml
  <ItemGroup>
    <ProjectReference Include="..\..\Domain\FDAAPI.Domain.RelationalDb\FDAAPI.Domain.RelationalDb.csproj" />
    <ProjectReference Include="..\FDAAPI.App.Common\FDAAPI.App.Common.csproj" />
  </ItemGroup>
  ```
- [ ] Implement Request model: `{Feature}Request.cs`
  - Use `sealed record` syntax
  - Implement `IFeatureRequest<{Feature}Response>`
- [ ] Implement Response model: `{Feature}Response.cs`
  - Properties: `Success`, `Message`, `StatusCode`, `Data`
- [ ] Implement StatusCode enum trong `FDAAPI.App.Common/Models/{Domain}/{Feature}StatusCode.cs`
- [ ] Implement Validator: `{Feature}RequestValidator.cs`
  - Extend `AbstractValidator<{Feature}Request>`
  - Define validation rules với FluentValidation
- [ ] Implement Handler: `{Feature}Handler.cs`
  - Inject dependencies via constructor (repositories, mappers, services)
  - Implement `IRequestHandler<{Feature}Request, {Feature}Response>` từ MediatR
  - Business logic in `Handle` method (NOT `ExecuteAsync`)
  - Use mapper để convert Entity → DTO

**Acceptance Criteria**:

- Request model là `sealed record` với positional parameters
- Response model có đầy đủ properties (Success, Message, StatusCode, Data)
- StatusCode enum được định nghĩa trong FDAAPI.App.Common
- Validator có đầy đủ validation rules
- Handler implement `IRequestHandler<,>` từ MediatR
- Handler sử dụng mapper thay vì manual mapping
- Handler KHÔNG validate input (validation tự động qua ValidationBehavior)

---

### **Phase 4 - Mapper Layer (nếu cần Entity → DTO mapping)**

**Deliverables**:

- [ ] Tạo DTO trong `src/Core/Application/FDAAPI.App.Common/DTOs/{Entity}Dto.cs`
- [ ] Tạo Mapper Interface trong `src/Core/Application/FDAAPI.App.Common/Services/Mapping/I{Entity}Mapper.cs`
- [ ] Implement Mapper trong `src/Core/Application/FDAAPI.App.Common/Services/Mapping/{Entity}Mapper.cs`

**Acceptance Criteria**:

- DTO có đầy đủ properties cần thiết để expose ra API
- Mapper interface define `MapToDto()` và `MapToDtoList()`
- Mapper implementation convert Entity → DTO correctly
- Mapper được register trong `ServiceExtensions.AddInfrastructureServices()`

---

### **Phase 5 - Presentation Layer**

**Deliverables**:

- [ ] Tạo folder `src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/Endpoints/Feat{số}_{FeatureName}/`
- [ ] Implement Endpoint: `{Feature}Endpoint.cs`
  - Inject `IMediator` (NOT handler directly)
  - Configure route, policies, summary, tags
  - Extract UserId from JWT claims nếu cần
  - Map DTO → MediatR Request
  - Send request via `_mediator.Send()`
  - Map Response → DTO
  - Return appropriate HTTP status code
- [ ] Create `DTOs/` subfolder
- [ ] Implement Request DTO: `DTOs/{Feature}RequestDto.cs`
- [ ] Implement Response DTO: `DTOs/{Feature}ResponseDto.cs`

**Acceptance Criteria**:

- Endpoint inject `IMediator`, KHÔNG inject handler trực tiếp
- Endpoint configure: Route, Policies (hoặc AllowAnonymous), Tags, Summary
- DTOs properly map to/from MediatR Request/Response models
- HandleAsync flow: DTO → MediatR Request → `_mediator.Send()` → Response → DTO
- HTTP status codes match business logic (201 for Create, 200 for Update/Get, etc.)

**File Structure**:

```
Endpoints/Feat{số}_{FeatureName}/
├── {Feature}Endpoint.cs
└── DTOs/
    ├── {Feature}RequestDto.cs
    └── {Feature}ResponseDto.cs
```

---

### **Phase 6 - Configuration & Registration**

**Deliverables**:

- [ ] Update `src/External/Infrastructure/Common/FDAAPI.Infra.Configuration/ServiceExtensions.cs`:
  - **AddApplicationServices()**:
    - Add `using FDAAPI.App.FeatG{số}_{FeatureName};` at top
    - Add `typeof({Feature}Request).Assembly` to assemblies array
    - MediatR auto-registers handlers
    - FluentValidation auto-registers validators
  - **AddPersistenceServices()**:
    - Add repository registration: `services.AddScoped<I{Entity}Repository, Pgsql{Entity}Repository>();`
  - **AddInfrastructureServices()**:
    - Add mapper registration: `services.AddScoped<I{Entity}Mapper, {Entity}Mapper>();`
    - Add service registration nếu cần: `services.AddScoped<I{Service}, {Service}>();`
- [ ] Verify DI container configuration

**Acceptance Criteria**:

- Feature assembly được thêm vào MediatR assemblies array
- Repositories registered as Scoped
- Mappers registered as Scoped trong AddInfrastructureServices
- Validators tự động được discover qua AddValidatorsFromAssemblies
- Handlers tự động được discover qua MediatR
- No circular dependencies

**Example Registration**:

```csharp
// In AddApplicationServices() - Add using and assembly
using FDAAPI.App.FeatG32_AreaCreate;

var assemblies = new[]
{
    typeof(CreateAreaRequest).Assembly, // <- Add this line
    typeof(LoginRequest).Assembly,
    // ... other assemblies
};

// MediatR auto-registers handlers
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(assemblies);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// FluentValidation auto-registers validators
services.AddValidatorsFromAssemblies(assemblies);

// In AddInfrastructureServices() - Register mapper
services.AddScoped<IAreaMapper, AreaMapper>();

// In AddPersistenceServices() - Register repository
services.AddScoped<IAreaRepository, PgsqlAreaRepository>();
```

---

### **Phase 7 - Database Migration & Testing**

**Deliverables**:

- [ ] Create EF Core migration:
  ```bash
  dotnet ef migrations add {MigrationName} \
    --project "src/Core/Domain/FDAAPI.Domain.RelationalDb/FDAAPI.Domain.RelationalDb.csproj" \
    --startup-project "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj" \
    --output-dir Migrations
  ```
- [ ] Review migration file
- [ ] Apply migration:
  ```bash
  dotnet ef database update \
    --project "src/Core/Domain/FDAAPI.Domain.RelationalDb/FDAAPI.Domain.RelationalDb.csproj" \
    --startup-project "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj"
  ```
- [ ] Verify migration applied:
  ```bash
  dotnet ef migrations list \
    --project "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj"
  ```

**Test Data**:

- [ ] Provide seed data SQL scripts
- [ ] Provide test user credentials
- [ ] Provide sample request/response data

**Acceptance Criteria**:

- Migration creates all necessary tables/columns
- Migration includes indexes and constraints
- Seed data scripts execute without errors
- Database schema matches entity models

---

### **Phase 8 - Documentation**

**Deliverables**:

- [ ] Create file: `documents/FE{số}-{FeatureName}-Complete-Documentation.md`
- [ ] Include sections:
  1. **Implementation Summary**
     - Feature overview
     - Requirements implemented
     - Architecture details
  2. **API Endpoints**
     - Request/Response examples
     - HTTP methods and routes
     - Authorization requirements
  3. **Database Schema**
     - Tables created/modified
     - Indexes and constraints
     - Relationships
  4. **Test Cases** (minimum 5 test cases)
     - Test scenario description
     - cURL command
     - Expected response (200 OK, 400 Bad Request, etc.)
     - Database verification SQL queries
     - Validation checklist
  5. **Test Data**
     - Seed data SQL scripts
     - Pre-seeded users (with credentials)
     - Sample payloads

**Acceptance Criteria**:

- Documentation follows FE01-Authentication-Complete-Documentation.md format
- All endpoints documented with examples
- Test cases cover happy path and error cases
- SQL scripts are copy-paste ready

**Test Case Template**:

````markdown
### TEST CASE X: [Scenario Name]

**Scenario**: [Description]

**cURL**:

```bash
curl -X POST http://localhost:5000/api/v1/{resource} \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "field1": "value1",
    "field2": "value2"
  }'
```
````

**Expected Response (200 OK)**:

```json
{
  "success": true,
  "message": "Success message",
  "data": { ... }
}
```

**Validation**:

- ✅ Check 1
- ✅ Check 2

**Database Check**:

```sql
SELECT * FROM "{Table}" WHERE "Field" = 'value';
```

```

---

## Important Notes:

### ✅ DO's:
- **ALWAYS** follow existing pattern (xem FEAT32 AreaCreate và FEAT7 AuthLogin làm ví dụ)
- **ALWAYS** use TodoWrite tool để track progress qua 8 phases
- **ALWAYS** build và verify sau mỗi phase
- **ALWAYS** provide complete test data với SQL insert statements
- **ALWAYS** include cURL commands cho all test cases
- **ALWAYS** use async/await với CancellationToken
- **ALWAYS** add `[JsonIgnore]` to navigation properties
- **ALWAYS** use `sealed record` cho Request models
- **ALWAYS** use MediatR's `IRequestHandler<,>` (NOT custom `IFeatureHandler<,>`)
- **ALWAYS** inject `IMediator` vào Endpoint (NOT handler trực tiếp)
- **ALWAYS** create FluentValidation validator cho mỗi Request
- **ALWAYS** use Mapper để convert Entity → DTO
- **ALWAYS** define StatusCode enum trong FDAAPI.App.Common/Models
- **ALWAYS** register feature assembly trong ServiceExtensions.AddApplicationServices()
- **ALWAYS** register mapper trong ServiceExtensions.AddInfrastructureServices()
- **ALWAYS** register repository trong ServiceExtensions.AddPersistenceServices()

### ❌ DON'Ts:
- **DON'T** create files unless necessary (prefer editing existing files)
- **DON'T** use ASP.NET Controllers (use FastEndpoints)
- **DON'T** use old `IFeatureHandler<,>` pattern (use MediatR's `IRequestHandler<,>`)
- **DON'T** inject handler directly into Endpoint (inject `IMediator`)
- **DON'T** validate input manually in Handler (use FluentValidation validator)
- **DON'T** manually map Entity → DTO in Handler (use Mapper)
- **DON'T** forget to add feature assembly to MediatR assemblies array
- **DON'T** forget to create StatusCode enum
- **DON'T** hardcode values (use configuration)
- **DON'T** create endpoints without authorization consideration
- **DON'T** forget to update ServiceExtensions.cs (3 locations: AddApplicationServices, AddInfrastructureServices, AddPersistenceServices)

### 🔧 Best Practices:
1. **MediatR Pattern**: Use `IRequestHandler<TRequest, TResponse>`, inject `IMediator` vào Endpoint
2. **FluentValidation**: Create validator class, validation tự động qua `ValidationBehavior`
3. **Mapper Pattern**: Create mapper interface + implementation, inject vào Handler
4. **StatusCode Enum**: Define custom enum cho mỗi feature domain
5. **Error Handling**: Use try-catch in Handler, return meaningful error messages with StatusCode
6. **Security**: Always consider authorization (Policies hoặc AllowAnonymous)
7. **Performance**: Use async/await, avoid N+1 queries
8. **Maintainability**: Keep handlers focused, single responsibility
9. **Testing**: Provide comprehensive test cases with cURL commands

---

## Questions to Clarify (nếu có):
[Liệt kê các questions cần clarify trước khi implement]

**Example**:
```

1. Should we send email notification when profile is updated?
2. What is the maximum file size for avatar upload?
3. Should we implement soft delete for user accounts?
4. Do we need pagination for user list endpoint?

````

---

## Build & Verify Commands:

### Build Solution
```bash
dotnet build "d:\Capstone Project\FDA_API\FDA_Api.sln"
````

### Run Application

```bash
cd "d:\Capstone Project\FDA_API\src\External\Presentation\FDAAPI.Presentation.FastEndpointBasedApi"
dotnet run
```

### Test Endpoint

```bash
# Anonymous endpoint
curl -X GET http://localhost:5000/api/v1/{resource}

# Authenticated endpoint
curl -X GET http://localhost:5000/api/v1/{resource} \
  -H "Authorization: Bearer {token}"
```

---

Hãy implement feature này theo đúng pattern và cấu trúc đã có, tuân thủ 8 phases và best practices.

## Key Points Summary:

✅ Use **MediatR** (`IRequestHandler<,>`) - NOT old `IFeatureHandler<,>`
✅ Inject **IMediator** vào Endpoint - NOT handler trực tiếp
✅ Use **FluentValidation** - validation tự động qua `ValidationBehavior`
✅ Use **Mapper** pattern - Entity → DTO conversion
✅ Define **StatusCode enum** trong `FDAAPI.App.Common/Models/{Domain}/`
✅ Use **sealed record** cho Request models
✅ Update **ServiceExtensions.cs** (3 locations: AddApplicationServices, AddInfrastructureServices, AddPersistenceServices)

````

---

# 🎯 EXAMPLE: Using the Template

## Example 1: User Profile Management

```markdown
# YÊU CẦU MỚI: User Profile Management

## Feature Specification:
Implement user profile management feature allowing users to view and update their own profile information. Admin can view any user's profile.

## Requirements:
1. Endpoint GET /api/v1/users/me - Get current user profile (authenticated users)
2. Endpoint PUT /api/v1/users/me - Update current user profile (full name, avatar)
3. Endpoint GET /api/v1/users/{id} - Get user by ID (Admin only)
4. Validate: FullName max 255 chars, Avatar URL must be valid HTTPS
5. Track all profile updates in UserAuditLog table

## Technical Details:

### Entities cần tạo/sử dụng:
- User (existing) - Already exists, no changes needed
- UserAuditLog (new) - Track profile change history

### Business Logic:
- User can only view/update their own profile via /me endpoint
- Admin can view any user profile via /{id} endpoint
- Avatar must be HTTPS URL, validate format
- Log all updates: old value → new value with timestamp
- Return 404 if user not found
- Return 403 if non-admin tries to access /{id} endpoint

### Authorization Requirements:
- GET /api/v1/users/me: `AuthSchemes(JwtBearerDefaults.AuthenticationScheme)`
- PUT /api/v1/users/me: `AuthSchemes(JwtBearerDefaults.AuthenticationScheme)`
- GET /api/v1/users/{id}: `Roles("ADMIN")`

### Validation Rules:
- FullName: Max 255 characters
- AvatarUrl: Valid HTTPS URL format

### Database Changes:
Create new table UserAuditLogs:
- Id (uuid, PK)
- UserId (uuid, FK to Users)
- Action (varchar) - "UPDATE_PROFILE", "UPDATE_AVATAR"
- FieldChanged (varchar) - "FullName", "AvatarUrl"
- OldValue (text)
- NewValue (text)
- CreatedAt (timestamptz)
- CreatedBy (uuid)

## Questions to Clarify:
1. Should we send email notification when profile is updated?
   → **Answer**: No, not in this phase
2. What is the maximum file size for avatar upload?
   → **Answer**: Not handling upload, just storing URL. No size limit.
3. Should audit log be exposed via API?
   → **Answer**: No, admin only via database query for now
````

---

## Example 2: Area Management (Flood Monitoring Zones)

```markdown
# YÊU CẦU MỚI: Area Management (User's Monitored Zones)

## Feature Specification:

Implement area management allowing users to define geographic areas they want to monitor for flood warnings. Each area has a location (lat/lng) and radius.

## Requirements:

1. POST /api/v1/areas - Create new monitored area (authenticated users)
2. GET /api/v1/areas - List user's areas (authenticated users)
3. PUT /api/v1/areas/{id} - Update area (owner or admin)
4. DELETE /api/v1/areas/{id} - Delete area (owner or admin)
5. Validate: Name required, Latitude/Longitude valid ranges, Radius 100m-50km

## Technical Details:

### Entities cần tạo/sử dụng:

- Area (existing in db.md) - Create entity class
- User (existing) - Foreign key relationship

### Business Logic:

- User can create max 5 areas (free tier limit)
- Latitude: -90 to 90, Longitude: -180 to 180
- Radius: 100 to 50000 meters
- User can only edit/delete their own areas
- Admin can edit/delete any area
- Auto-set UserId from JWT claims

### Authorization Requirements:

- All endpoints: `AuthSchemes(JwtBearerDefaults.AuthenticationScheme)`
- Edit/Delete: Check ownership or admin role

### Validation Rules:

- Name: Required, max 255 chars
- Latitude: Required, -90 to 90
- Longitude: Required, -180 to 180
- Radius: Required, 100 to 50000 (meters)

### Database Changes:

Use existing Areas table from db.md:

- Add index on (user_id, created_at)
- Add check constraint: radius >= 100 AND radius <= 50000

## Questions to Clarify:

1. Should we geocode address_text automatically?
   → **Answer**: No, manual input for now
2. Can areas overlap?
   → **Answer**: Yes, no restrictions
3. Should we integrate with map API?
   → **Answer**: Not in this phase, just store coordinates
```

---

# 📚 Additional Resources

## Reference Documentation:

1. **Architecture**: `documents/general.md`
2. **Workflow**: `documents/workflow.md`
3. **Database Schema**: `documents/db.md`
4. **Auth Implementation**: `documents/FE01-Authentication-Complete-Documentation.md`

## Code Examples:

1. **MediatR Pattern (RECOMMENDED)**: `FEAT32` (CreateArea) - Best example
2. **Complex Logic**: `FEAT7` (Login) - Complex Handler with dual auth
3. **Full CRUD**: `FEAT33-38` (Area Management) - Complete CRUD operations
4. **Admin Features**: `FEAT23-27` (Station Management)

## Tools & Commands:

1. **Build**: `dotnet build`
2. **Migration**: `dotnet ef migrations add/remove/list`
3. **Run**: `dotnet run`
4. **Test**: `curl` commands

## Key Libraries:

- **MediatR**: Request/Response pattern, auto-discovers handlers
- **FluentValidation**: Validation rules, auto-discovers validators
- **FastEndpoints**: Lightweight alternative to ASP.NET Controllers
- **EF Core**: ORM for PostgreSQL

---

**Last Updated**: 2026-01-14
**Version**: 2.0.0
**Template Purpose**: Ensure consistency across feature implementations using MediatR + FluentValidation pattern
**Major Changes in v2.0.0**:

- Migrated from custom `IFeatureHandler<,>` to MediatR's `IRequestHandler<,>`
- Added FluentValidation with automatic validation via `ValidationBehavior`
- Added Mapper pattern for Entity → DTO conversion
- Added StatusCode enum pattern
- Updated naming convention to include underscore (FeatG{số}\_{FeatureName})
- Updated from 7 phases to 8 phases (added Mapper phase)
