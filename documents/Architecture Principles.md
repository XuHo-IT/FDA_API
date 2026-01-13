### Domain-Centric Philosophy

┌─────────────────────────────────────────────────────────┐
│ PRESENTATION │
│ (FastEndpoints - HTTP Layer) │
└────────────────────┬────────────────────────────────────┘
│
┌────────────────────▼────────────────────────────────────┐
│ APPLICATION │
│ (Handlers - Workflow Coordination) │
└────────────────────┬────────────────────────────────────┘
│
┌────────────────────▼────────────────────────────────────┐
│ DOMAIN │
│ (Entities, Repositories, Unit of Work - Core Logic) │
└────────────────────┬────────────────────────────────────┘
│
┌────────────────────▼────────────────────────────────────┐
│ INFRASTRUCTURE │
│ (PostgreSQL, Redis, Typesense, SignalR, Cloudinary) │
└─────────────────────────────────────────────────────────┘

**Core Principle**: Domain is at the center and **does not depend on** any external technology.

### Layer Responsibilities

1.  **Core.Domain** - Business Logic Core

- **Purpose**: Contains pure business logic, independent of any technology
- **Contains**:
  - **Entities**: Domain models representing business concepts (e.g., `WaterLevel`)
  - **Repository Interfaces**: Contracts for data access (e.g., `IWaterLevelRepository`)
  - **Unit of Work Interface**: Transaction management contract
  - **Constraints**: Business rules and validations
- **Dependencies**: None (Pure C#)
- **Example**: `WaterLevel` entity with business rules

2.  **Core.Application** - Workflow Coordination

- **Purpose**: Orchestrates business workflows using CQRS pattern
- **Contains**:
  - **Handlers**: Execute business logic workflows
  - **Requests**: Command (Write) or Query (Read) requests
  - **Responses**: Command or Query responses
  - **Validators**: Input validation for Application requests (FluentValidation)
  - **Mappers**: Conversion between Domain Entities and DTOs
- **Dependencies**: Core.Domain only
- **Pattern**: CQRS (Command Query Responsibility Segregation)
  - **Commands**: `CreateWaterLevelRequest`, `UpdateWaterLevelRequest`, `DeleteWaterLevelRequest`
  - **Queries**: `GetWaterLevelRequest`, `GetStaticDataRequest`

### Standardized API Response Structure

To ensure consistency across all endpoints, the following structure is followed:

1.  **POST (Create)**:
    - **Status Code**: 201 Created
    - **Body**: Contains `Success`, `Message`, `StatusCode`, and `Data` (EntityDTO including the new ID).

2.  **GET (Read)**:
    - **Status Code**: 200 OK
    - **Body**: Contains `Success`, `Message`, `StatusCode`, and a specifically named property for the data (e.g., `Area`, `Areas`, `Station`, `Stations`).

3.  **PUT/PATCH (Update) & DELETE**:
    - **Status Code**: 200 OK or 204 No Content
    - **Body**: Contains `Success`, `Message`, and `StatusCode`.

4.  **Error Handling**:
    - **Status Code**: Appropriate 4xx or 5xx code
    - **Body**: Contains `Success: false`, `Message` (error description), and `StatusCode`.

3.  **External.Infrastructure** - Technical Implementations

- **Purpose**: Provides concrete implementations of domain interfaces
- **Contains**:
  - **Persistence**: Repository implementations (e.g., `PgsqlWaterLevelRepository`)
  - **PostgreSQL**: Database implementation via Entity Framework Core
  - **Redis**: Caching service
  - **Typesense**: Search engine implementation
  - **Quartz**: Background job scheduler
  - **SignalR**: Real-time communication
  - **Cloudinary/ImageKit**: Media storage services
- **Dependencies**: Core.Domain, Core.Application

4.  **External.Presentation** - HTTP Interface

- **Purpose**: Handles HTTP requests/responses using FastEndpoints
- **Contains**:
  - **Endpoints**: HTTP route handlers
  - **DTOs**: Data Transfer Objects for HTTP layer
  - **Authorization**: Authentication & authorization
  - **Validation**: Input validation
- **Dependencies**: Core.Application, External.Infrastructure

5.  **External.BuildingBlock** - Auto-Registration

- **Purpose**: Automatically discovers and registers
  - Feature Handlers
  - Repositories
  - Unit of Work implementations
- **Dependencies**: Core.Application, Core.Domain

---
