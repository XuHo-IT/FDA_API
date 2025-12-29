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
- **Pattern**: CQRS với Feature Handlers
- **Authentication**: JWT + Refresh Token (đã hoàn thành)

## Quy ước đặt tên và cấu trúc:

### 1. Feature Naming Convention
- **Feature Group (Application layer)**: `FeatG{số}`
  - Ví dụ: `FDAAPI.App.FeatG1`, `FDAAPI.App.FeatG6`
- **Feature (Presentation layer)**: `Feat{số}`
  - Ví dụ: `Endpoints/Feat1/`, `Endpoints/Feat6/`
- **Naming Pattern**:
  - Request: `{Feature}Request.cs`
  - Response: `{Feature}Response.cs`
  - Handler: `{Feature}Handler.cs`
  - Endpoint: `{Feature}Endpoint.cs`
  - Request DTO: `{Feature}RequestDto.cs`
  - Response DTO: `{Feature}ResponseDto.cs`

### 2. Layer Structure
```
src/
├── Core/
│   ├── Domain/FDAAPI.Domain.RelationalDb/
│   │   ├── Entities/                          # Entity classes
│   │   │   ├── {Entity}.cs
│   │   │   └── ...
│   │   └── Repositories/                      # Repository interfaces only
│   │       ├── I{Entity}Repository.cs
│   │       └── ...
│   └── Application/FDAAPI.App.FeatG{số}/
│       ├── {Feature}Request.cs                # Input model
│       ├── {Feature}Response.cs               # Output model
│       └── {Feature}Handler.cs                # Business logic
│
├── External/
│   ├── Infrastructure/
│   │   ├── Services/FDAAPI.Infra.Services/
│   │   │   └── {Service}/                     # Reusable services
│   │   │       ├── I{Service}.cs
│   │   │       └── {Service}.cs
│   │   ├── Persistence/FDAAPI.Infra.Persistence/
│   │   │   └── Repositories/
│   │   │       └── Pgsql{Entity}Repository.cs # Repository implementations
│   │   └── Common/FDAAPI.Infra.Configuration/
│   │       └── ServiceExtensions.cs           # DI registration
│   └── Presentation/FDAAPI.Presentation.FastEndpointBasedApi/
│       └── Endpoints/Feat{số}/
│           ├── {Feature}Endpoint.cs           # FastEndpoint implementation
│           └── DTOs/
│               ├── {Feature}RequestDto.cs     # HTTP request DTO
│               └── {Feature}ResponseDto.cs    # HTTP response DTO
```

### 3. Handler Pattern (Application Layer)
**Interface**: `IFeatureHandler<TRequest, TResponse>`

**Implementation**:
```csharp
public class {Feature}Handler : IFeatureHandler<{Feature}Request, {Feature}Response>
{
    private readonly I{Entity}Repository _{entity}Repository;
    private readonly I{Service} _{service};

    public {Feature}Handler(
        I{Entity}Repository {entity}Repository,
        I{Service} {service})
    {
        _{entity}Repository = {entity}Repository;
        _{service} = {service};
    }

    public async Task<{Feature}Response> ExecuteAsync(
        {Feature}Request request,
        CancellationToken ct)
    {
        // 1. Validate input
        // 2. Execute business logic
        // 3. Call repositories
        // 4. Return response
    }
}
```

**Registration** (in `ServiceExtensions.cs`):
```csharp
services.AddTransient<IFeatureHandler<{Feature}Request, {Feature}Response>, {Feature}Handler>();
```

### 4. FastEndpoints Pattern (Presentation Layer)
**Inherit**: `Endpoint<TRequestDto, TResponseDto>`

**Implementation**:
```csharp
public class {Feature}Endpoint : Endpoint<{Feature}RequestDto, {Feature}ResponseDto>
{
    private readonly IFeatureHandler<{Feature}Request, {Feature}Response> _handler;

    public {Feature}Endpoint(IFeatureHandler<{Feature}Request, {Feature}Response> handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/api/v1/{resource}");           // or Get, Put, Delete
        Roles("ADMIN", "MODERATOR");          // or AllowAnonymous()
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
        // 1. Map DTO to Request
        var appRequest = new {Feature}Request
        {
            Field1 = req.Field1,
            Field2 = req.Field2
        };

        // 2. Execute handler
        var result = await _handler.ExecuteAsync(appRequest, ct);

        // 3. Map Response to DTO
        var response = new {Feature}ResponseDto
        {
            Success = result.Success,
            Data = result.Data
        };

        // 4. Send response
        await SendAsync(response, cancellation: ct);
    }
}
```

### 5. Repository Pattern
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

### 6. Entity Pattern (Domain Layer)
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

### ✅ FEAT1: CreateWaterLevel (Ví dụ chuẩn để follow)
- **Endpoint**: `POST /api/v1/water-levels`
- **Auth**: `Roles("ADMIN", "GOV")`
- **Files**:
  - `FDAAPI.App.FeatG1/CreateWaterLevelRequest.cs`
  - `FDAAPI.App.FeatG1/CreateWaterLevelResponse.cs`
  - `FDAAPI.App.FeatG1/CreateWaterLevelHandler.cs`
  - `Endpoints/Feat1/CreateWaterLevelEndpoint.cs`
  - `Endpoints/Feat1/DTOs/CreateWaterLevelRequestDto.cs`
  - `Endpoints/Feat1/DTOs/CreateWaterLevelResponseDto.cs`

### ✅ FEAT2-4: WaterLevel CRUD
- UpdateWaterLevel, GetWaterLevel, DeleteWaterLevel
- Follow same pattern as FEAT1

### ✅ FEAT5: GetStaticData
- **Endpoint**: `GET /api/v1/static-data`
- **Auth**: `AllowAnonymous()`

### ✅ FEAT6-9: Authentication System
- **FEAT6**: SendOTP - `POST /api/v1/auth/send-otp`
- **FEAT7**: Login - `POST /api/v1/auth/login`
- **FEAT8**: RefreshToken - `POST /api/v1/auth/refresh-token`
- **FEAT9**: Logout - `POST /api/v1/auth/logout`
- **Details**: See `documents/FE01-Authentication-Complete-Documentation.md`

---

## Authentication Setup (đã có sẵn):

### JWT Configuration
- **Access Token**: 60 minutes expiration
- **Refresh Token**: 7 days expiration
- **Algorithm**: HMAC-SHA256 (HS256)
- **ClockSkew**: TimeSpan.Zero (strict expiration)

### Available Roles
| Role | Code | Description |
|---|---|---|
| Admin | ADMIN | Administrator |
| Moderator | MODERATOR | Government Officer |
| User | USER | Citizen User |

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
Implement theo đúng pattern đã có, tuân thủ 7 phases sau:

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
- [ ] Tạo project mới `src/Core/Application/FDAAPI.App.FeatG{số}/`
- [ ] Create `.csproj` file với references:
  ```xml
  <ItemGroup>
    <ProjectReference Include="..\..\Domain\FDAAPI.Domain.RelationalDb\FDAAPI.Domain.RelationalDb.csproj" />
    <ProjectReference Include="..\FDAAPI.App.Common\FDAAPI.App.Common.csproj" />
  </ItemGroup>
  ```
- [ ] Implement Request model: `{Feature}Request.cs`
- [ ] Implement Response model: `{Feature}Response.cs`
- [ ] Implement Handler: `{Feature}Handler.cs`
  - Inject dependencies via constructor
  - Implement `IFeatureHandler<TRequest, TResponse>`
  - Business logic in `ExecuteAsync` method

**Acceptance Criteria**:
- Request/Response models có đầy đủ properties
- Handler có proper error handling
- Handler follows single responsibility principle

---

### **Phase 4 - Presentation Layer**
**Deliverables**:
- [ ] Tạo folder `src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/Endpoints/Feat{số}/`
- [ ] Implement Endpoint: `{Feature}Endpoint.cs`
- [ ] Create `DTOs/` subfolder
- [ ] Implement Request DTO: `DTOs/{Feature}RequestDto.cs`
- [ ] Implement Response DTO: `DTOs/{Feature}ResponseDto.cs`

**Acceptance Criteria**:
- Endpoint configure: Route, Auth, Tags, Summary
- DTOs properly map to/from Request/Response models
- HandleAsync: DTO → Request → Handler → Response → DTO

**File Structure**:
```
Endpoints/Feat{số}/
├── {Feature}Endpoint.cs
└── DTOs/
    ├── {Feature}RequestDto.cs
    └── {Feature}ResponseDto.cs
```

---

### **Phase 5 - Configuration & Registration**
**Deliverables**:
- [ ] Update `src/External/Infrastructure/Common/FDAAPI.Infra.Configuration/ServiceExtensions.cs`:
  - Add handler registration in `AddApplicationServices()`
  - Add repository registration in `AddPersistenceServices()`
  - Add service registration in `AddInfrastructureServices()` (if needed)
- [ ] Update `src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/Program.cs` (if needed)
- [ ] Verify DI container configuration

**Acceptance Criteria**:
- All handlers registered as Transient
- All repositories registered as Scoped
- All services registered with appropriate lifetime
- No circular dependencies

**Example Registration**:
```csharp
// In AddApplicationServices()
services.AddTransient<IFeatureHandler<{Feature}Request, {Feature}Response>, {Feature}Handler>();

// In AddPersistenceServices()
services.AddScoped<I{Entity}Repository, Pgsql{Entity}Repository>();

// In AddInfrastructureServices() (if needed)
services.AddScoped<I{Service}, {Service}>();
```

---

### **Phase 6 - Database Migration & Testing**
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

### **Phase 7 - Documentation**
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
```markdown
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
- **ALWAYS** follow existing pattern (xem FEAT1 và FEAT6-9 làm ví dụ)
- **ALWAYS** use TodoWrite tool để track progress qua 7 phases
- **ALWAYS** build và verify sau mỗi phase
- **ALWAYS** provide complete test data với SQL insert statements
- **ALWAYS** include cURL commands cho all test cases
- **ALWAYS** use async/await với CancellationToken
- **ALWAYS** add `[JsonIgnore]` to navigation properties
- **ALWAYS** register dependencies với correct lifetime (Transient/Scoped)

### ❌ DON'Ts:
- **DON'T** create files unless necessary (prefer editing existing files)
- **DON'T** use ASP.NET Controllers (use FastEndpoints)
- **DON'T** skip error handling
- **DON'T** hardcode values (use configuration)
- **DON'T** create endpoints without authorization consideration
- **DON'T** skip validation
- **DON'T** forget to update ServiceExtensions.cs

### 🔧 Best Practices:
1. **Error Handling**: Use try-catch, return meaningful error messages
2. **Validation**: Validate input at both DTO and Handler level
3. **Security**: Always consider authorization requirements
4. **Performance**: Use async/await, avoid N+1 queries
5. **Maintainability**: Keep handlers focused, single responsibility
6. **Testing**: Provide comprehensive test cases

---

## Questions to Clarify (nếu có):
[Liệt kê các questions cần clarify trước khi implement]

**Example**:
```
1. Should we send email notification when profile is updated?
2. What is the maximum file size for avatar upload?
3. Should we implement soft delete for user accounts?
4. Do we need pagination for user list endpoint?
```

---

## Build & Verify Commands:

### Build Solution
```bash
dotnet build "d:\Capstone Project\FDA_API\FDA_Api.sln"
```

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

Hãy implement feature này theo đúng pattern và cấu trúc đã có, tuân thủ 7 phases và best practices.
```

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
```

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
1. **Simple CRUD**: `FEAT1-4` (WaterLevel)
2. **Complex Logic**: `FEAT6-9` (Authentication)
3. **Anonymous Endpoint**: `FEAT5` (Static Data)

## Tools & Commands:
1. **Build**: `dotnet build`
2. **Migration**: `dotnet ef migrations add/remove/list`
3. **Run**: `dotnet run`
4. **Test**: `curl` commands

---

**Last Updated**: 2025-12-29
**Version**: 1.0.0
**Template Purpose**: Ensure consistency across feature implementations
