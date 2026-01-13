Overall Request Flow

┌──────────────┐
│ Client  
│ (HTTP Req)  
└──────┬───────┘
│
▼
┌─────────────────────────────────────────────────────────┐
│ 1. PRESENTATION LAYER (FastEndpoints)  
│ - Receives HTTP Request  
│ - Validates Authorization  
│ - Validates Input (DTO Validation)  
│ - Maps HTTP DTO → Application Request  
└──────┬──────────────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────┐
│ 2. APPLICATION LAYER (Handler)  
│ - Receives Application Request  
│ - Validates Request (FluentValidation)
│ - Executes Business Logic  
│ - Uses Unit of Work for Transaction Management  
│ - Calls Repository Interfaces  
│ - Uses Infrastructure Services (Cache, File, etc.)  
│ - Maps Entity → Response DTO (Mappers)
└──────┬──────────────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────┐
│ 3. DOMAIN LAYER (Repository Interface)  
│ - Defines Data Access Contract  
│ - Enforces Business Rules  
└──────┬──────────────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────┐
│ 4. INFRASTRUCTURE LAYER (Repository Implementation)  
│ - Implements Repository Interface  
│ - Uses AppDbContext (EF Core)  
│ - Executes Database Operations  
└──────┬──────────────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────┐
│ 5. DATABASE (PostgreSQL)  
│ - Stores/Retrieves Data  
└──────┬──────────────────────────────────────────────────┘
│
▼ (Response flows back up)
┌─────────────────────────────────────────────────────────┐
│ 6. PRESENTATION LAYER │
│ - Maps Application Response → HTTP Response DTO  
│ - Returns JSON Response to Client  
└─────────────────────────────────────────────────────────┘

Detailed Workflow Example: Create Water Level

Step 1: HTTP Request (Presentation)
// Endpoint: POST /api/v1/water-levels
// DTO: CreateWaterLevelRequestDto
{
"locationName": "River Main",
"waterLevel": 2.5,
"unit": "meters"
}

**Actions**:

- FastEndpoints receives HTTP POST request
- Validates DTO structure
- Maps `CreateWaterLevelRequestDto` → `CreateWaterLevelRequest`

Step 2: Handler Execution (Application)
// CreateWaterLevelHandler.ExecuteAsync()

**Actions**:

- Validates business rules (e.g., water level ≥ 0) using FluentValidation
- Creates `WaterLevel` entity
- Calls `IWaterLevelRepository.CreateAsync()`
- Maps `WaterLevel` entity → `WaterLevelDto` using a Mapper
- Returns `CreateWaterLevelResponse` containing the DTO

Step 3: Repository Call (Domain → Infrastructure)
// PgsqlWaterLevelRepository.CreateAsync()

**Actions**:

- Uses `AppDbContext` to add entity
- Saves changes to PostgreSQL
- Returns created entity ID

Step 4: Response Flo
// Handler returns CreateWaterLevelResponse
// Endpoint maps to CreateWaterLevelResponseDto
// Returns HTTP 201 Created with JSON response

CQRS Pattern Workflow

Command Flow (Write Operations)

HTTP POST/PUT/DELETE
↓
FastEndpoint (DTO Validation)
↓
Command Handler (Business Logic)
↓
Unit of Work (Transaction)
↓
Repository (Save Changes)
↓
Database (Commit)

**Examples**: `CreateWaterLevelHandler`, `UpdateWaterLevelHandler`, `DeleteWaterLevelHandler`

#### Query Flow (Read Operations)

HTTP GET
↓
FastEndpoint (DTO Validation)
↓
Query Handler (Business Logic)
↓
Repository (Read Data)
↓
Cache Check (Redis) → Database (PostgreSQL)
↓
Return Data

**Examples**: `GetWaterLevelHandler`, `GetStaticDataHandler`

### Smaller Workflows

1. **Authorization Workflow**

HTTP Request
↓
FastEndpoint.Configure() → RequireAuthorization()
↓
JWT Token Validation
↓
User Claims Extraction
↓
Handler Receives User Context

2. **Caching Workflow**

Query Request
↓
Handler Checks Redis Cache
↓
Cache Hit? → Return Cached Data
↓
Cache Miss? → Query Database
↓
Store in Cache → Return Data

3.  **File Upload Workflow**

HTTP Multipart Request
↓
FastEndpoint Receives File
↓
Handler Validates File
↓
Cloudinary/ImageKit Service Upload
↓
Store URL in Database
↓
Return Response with File URL

4.  **Background Job Workflow**

Scheduled Trigger (Quartz)
↓
Job Handler Executes
↓
Uses Repository/Infrastructure Services
↓
Logs Results

5.  **Real-time Notification Workflow**

Business Event Occurs (Handler)
↓
SignalR Hub Notifies Clients
↓
Connected Clients Receive Update
