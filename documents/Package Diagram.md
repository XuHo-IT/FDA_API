┌─────────────────────────────────────────────────────────────────────────────┐
│ PRESENTATION LAYER │
│ (FastEndpoints - HTTP Entry Point) │
│ │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ FDAAPI.Presentation.FastEndpointBasedApi │
│ │ ┌───────────────────────────────────────────────────────────────┐ │ │
│ │ │ Endpoints/ │ │ │
│ │ │ ├── Feat1/CreateWaterLevelEndpoint │ │ │
│ │ │ ├── Feat2/UpdateWaterLevelEndpoint │ │ │
│ │ │ ├── Feat3/GetWaterLevelEndpoint │ │ │
│ │ │ ├── Feat4/DeleteWaterLevelEndpoint │ │ │
│ │ │ ├── FeatG5/GetStaticDataEndpoint │ │ │
│ │ │ └── ... │ │ │
│ │ └───────────────────────────────────────────────────────────────┘ │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│ │ │
│ │ <<import>> │
│ ▼ │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│ APPLICATION LAYER │
│ (CQRS - Command Query Handlers) │
│ │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ FDAAPI.App.Common │ │
│ │ ├── Features/ (IFeatureHandler, IFeatureRequest, IFeatureResponse) │ │
│ │ ├── Caching/ │ │
│ │ ├── FileServices/ │ │
│ │ ├── Mail/ │ │
│ │ ├── Tokens/ │ │
│ │ └── Helpers/ │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│ │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ FDAAPI.App.FeatG1 (Create Water Level) │ │
│ │ ├── CreateWaterLevelHandler │ │
│ │ ├── CreateWaterLevelRequest │ │
│ │ └── CreateWaterLevelResponse │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│ │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ FDAAPI.App.FeatG2 (Update Water Level) │ │
│ │ ├── UpdateWaterLevelHandler │ │
│ │ ├── UpdateWaterLevelRequest │ │
│ │ └── UpdateWaterLevelResponse │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│ │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ FDAAPI.App.FeatG3 (Get Water Level - Query) │ │
│ │ ├── GetWaterLevelHandler │ │
│ │ ├── GetWaterLevelRequest │ │
│ │ └── GetWaterLevelResponse │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│ │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ FDAAPI.App.FeatG4 (Delete Water Level) │ │
│ │ ├── DeleteWaterLevelHandler │ │
│ │ ├── DeleteWaterLevelRequest │ │
│ │ └── DeleteWaterLevelResponse │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│ │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ FDAAPI.App.FeatG5 (Get Static Data - Query) │ │
│ │ ├── GetStaticDataHandler │ │
│ │ ├── GetStaticDataRequest │ │
│ │ └── GetStaticDataResponse │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│ │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ ... (More Feature Groups) │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│ │ │
│ │ <<import>> │
│ ▼ │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│ DOMAIN LAYER │
│ (Entities, Repository Interfaces, Business Rules) │
│ │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ FDAAPI.Domain.RelationalDb │ │
│ │ ┌───────────────────────────────────────────────────────────────┐ │ │
│ │ │ Entities/ │ │ │
│ │ │ ├── WaterLevel │ │ │
│ │ │ ├── StaticData/ │ │ │
│ │ │ └── ... │ │ │
│ │ ├───────────────────────────────────────────────────────────────┤ │ │
│ │ │ Entities.Base/ │ │ │
│ │ │ ├── IEntity │ │ │
│ │ │ ├── ICreatedEntity │ │ │
│ │ │ ├── IUpdatedEntity │ │ │
│ │ │ └── ITemporarilyRemovedEntity │ │ │
│ │ ├───────────────────────────────────────────────────────────────┤ │ │
│ │ │ Repositories/ (Interfaces) │ │ │
│ │ │ ├── IWaterLevelRepository │ │ │
│ │ │ └── Common/ │ │ │
│ │ ├───────────────────────────────────────────────────────────────┤ │ │
│ │ │ RealationalDB/ │ │ │
│ │ │ └── AppDbContext │ │ │
│ │ └───────────────────────────────────────────────────────────────┘ │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│ │ │
│ │ <<import>> (Infrastructure implements) │
│ ▼ │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│ INFRASTRUCTURE LAYER │
│ (Repository Implementations, External Services, Technical Details) │
│ │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ FDAAPI.Infra.Persistence │ │
│ │ ┌───────────────────────────────────────────────────────────────┐ │ │
│ │ │ Repositories/ (Implementations) │ │ │
│ │ │ ├── PgsqlWaterLevelRepository │ │ │
│ │ │ └── ... │ │ │
│ │ └───────────────────────────────────────────────────────────────┘ │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│ │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ FDAAPI.Infra.Configuration │ │
│ │ └── ServiceExtensions (DI Registration) │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│ │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ FDAAPI.Infras.FeatG1 (Infrastructure for Feature G1) │ │
│ │ └── ... │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│ │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ Future Infrastructure Services: │ │
│ │ ├── Redis Caching │ │
│ │ ├── Typesense Search │ │
│ │ ├── Quartz Background Jobs │ │
│ │ ├── SignalR Real-time │ │
│ │ ├── Cloudinary (Video) │ │
│ │ └── ImageKit (Images) │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘

```

```

## Detailed Package Descriptions

The system uses a clean, layered architecture where the **Presentation Layer** receives HTTP requests and maps them into commands/queries handled by the **Application Layer**, which executes business workflows. The **Domain Layer** defines the core business entities, rules, and repository interfaces, while the **Infrastructure Layer** provides real database and external service implementations (e.g., PostgreSQL via EF Core). Finally, the **Building Block Layer** automatically registers services and handlers through reflection, making the system highly modular, maintainable, and easy to extend.

## **1. Presentation Layer**

Acts as the **API gateway** for the whole system.

It receives HTTP requests, validates input, maps DTOs to application commands/queries, and returns responses.

It also handles routing, authentication/authorization, and API documentation.

**Core idea:**

👉 **Transforms HTTP requests into application commands/queries and sends them to the correct handler.**

Includes:

- FastEndpoints route handlers (CRUD for water levels + static data)
- HTTP DTOs for request/response
- Program.cs for middleware + DI + startup

Uses Adapter + DTO patterns.

---

## **2. Application Layer**

Contains the **business use-case logic** (feature handlers) using the CQRS pattern.

**Core idea:**

👉 **Defines what the system can do (commands/queries) and how each operation should behave.**

- `Common` project defines shared interfaces (IFeatureHandler), caching, file, mail, token utilities.
- `FeatG1–G5` implement actual features:
  - G1: Create water level
  - G2: Update water level
  - G3: Get water level
  - G4: Delete water level
  - G5: Get static data

Each handler:

1. Validates business rules
2. Calls repositories
3. Returns structured responses

Uses Command, Query, and CQRS patterns.

---

## **3. Domain Layer**

The **core business model** of the system — independent of frameworks and databases.

**Core idea:**

👉 **Defines entities, business rules, and repository contracts (interfaces).**

Contains:

- Domain entities (e.g., `WaterLevel`)
- Business validation (value ≥ 0, location required)
- Repository interfaces (e.g., `IWaterLevelRepository`)
- Base entity contracts
- EF Core DbContext + migrations (technical detail)

Uses Repository + Domain Model patterns.

---

## **4. Infrastructure Layer**

Implements all **technical details** required by the domain and application layers.

**Core idea:**

👉 **Provides actual database, caching, and external service implementations.**

Includes:

- Persistence (EF Core PostgreSQL repositories)
  - e.g., `PgsqlWaterLevelRepository`
- Infrastructure configuration (DI registration)
- Feature-specific infrastructure (placeholders for future enhancements)

Defines:

- DbContext + PostgreSQL setup
- Repository implementations
- DI configuration for handlers, services, and database

Uses Repository + Adapter + Dependency Injection patterns.

---

## **5. Building Block Layer**

Handles **automatic service registration** via reflection.

**Core idea:**

👉 **Auto-discovers and registers handlers, repositories, and services to reduce manual DI work.**

Uses:

- Assembly scanning
- Convention-over-configuration
- Reflection

Benefits:

- Less boilerplate code
- Auto-registration of new features
