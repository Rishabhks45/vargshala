# 01. Architecture & Design Principles

## 1. Architectural Style: Clean Architecture + Modular Monolith

Vargshala follows **Clean Architecture** (Ports and Adapters / Hexagonal) organized as a **Modular Monolith**. 

```
                               ┌───────────────────────────┐
                               │     Vargshala.Web (UI)    │
                               └─────────────┬─────────────┘
                                             │ HTTP / SignalR
                                             ▼
                               ┌───────────────────────────┐
                               │      Vargshala.API        │
                               └──────┬─────────────┬──────┘
                                      │             │
                    ┌─────────────────┘             └─────────────────┐
                    ▼                                                 ▼
      ┌───────────────────────────┐                     ┌───────────────────────────┐
      │   Vargshala.Application   │◄────────────────────┤    Vargshala.Contracts    │
      └─────────────┬─────────────┘                     └───────────────────────────┘
                    │                                                 ▲
                    ▼                                                 │
      ┌───────────────────────────┐                                   │
      │     Vargshala.Domain      │                                   │
      └───────────────────────────┘                                   │
                    ▲                                                 │
                    │                                                 │
      ┌─────────────┴─────────────┐                                   │
      │ Vargshala.Infrastructure  ├───────────────────────────────────┘
      └───────────────────────────┘
```

---

## 2. Layer Responsibilities & Dependencies

### 2.1. `Vargshala.Domain` (Core)
- **Zero External Dependencies**: Contains pure C# business entities, value objects, domain events, domain enums, and custom domain exceptions.
- **Tenant Entity Foundation**: Implements `IMustHaveTenant` or `ITenantEntity` on all domain entities that belong to an organization.
- **Encapsulation**: Business invariants and domain logic (e.g. calculating overdue fee balance, validating quiz attempt boundaries) reside here.

### 2.2. `Vargshala.Application` (Use Cases)
- **Dependencies**: Depends ONLY on `Vargshala.Domain` and `Vargshala.Contracts`.
- **CQRS Pattern**: Organizes use cases into Commands (mutations) and Queries (reads) using MediatR / handler pattern.
- **Validation**: FluentValidation pipeline behaviors validating requests before executing handlers.
- **Interfaces**: Defines abstractions for repositories, unit of work, current user context (`ICurrentUserService`), tenant context (`ITenantContext`), file storage (`IFileStorageService`), and notification dispatchers (`INotificationService`).

### 2.3. `Vargshala.Contracts` (Shared Contracts)
- **Dependencies**: None.
- **Role**: Contains data transfer objects (DTOs), API request contracts, response envelopes (`ApiResponse<T>`, `Result<T>`), query parameters/filters, and shared enums.
- **Shared across API and Web/Mobile Clients** to eliminate contract duplication.

### 2.4. `Vargshala.Infrastructure` (Data & External Integrations)
- **Dependencies**: Implements interfaces defined in `Vargshala.Application`. References EF Core, Npgsql (PostgreSQL), ASP.NET Core Identity, JWT libraries, and file storage SDKs.
- **Multi-Tenancy Implementation**: Enforces global query filters in `ApplicationDbContext` scoped to `ITenantContext.OrganizationId`.
- **Repositories & Unit of Work**: Concrete EF Core database operations and migrations.
- **External Services**: Local file storage provider (V1) with seamless swap to S3/Blob storage (V2).

### 2.5. `Vargshala.API` (Presentation / Entry Point)
- **Dependencies**: References `Vargshala.Application`, `Vargshala.Infrastructure`, and `Vargshala.Contracts`.
- **Thin Controllers**: Acts as an HTTP dispatcher that parses JWT claims, delegates execution to Application handlers, and maps `Result<T>` to HTTP status codes.
- **Middleware Pipeline**: Global exception handling, Tenant Resolution Middleware, JWT Authentication/Authorization, Rate Limiting, and CORS.

### 2.6. `Vargshala.Web` (Frontend Client)
- **Role**: Modern responsive web application (Blazor / React + Tailwind CSS) providing:
  - Super Admin Dashboard
  - Organization Admin Portal
  - Teacher Academic Workspace
  - Student Learning Portal (Mobile-optimized)

---

## 3. Multi-Tenancy Strategy: Shared Database / Shared Schema

```
┌─────────────────────────────────────────────────────────────┐
│                     PostgreSQL Database                     │
│                                                             │
│  [Organizations]                                            │
│    ├── Id: org-101 (Vidya Mandir)                          │
│    └── Id: org-102 (Apex Academy)                           │
│                                                             │
│  [Students] (Filtered by OrganizationId)                   │
│    ├── Id: stu-1, OrgId: org-101, Name: "Aarav"            │
│    └── Id: stu-2, OrgId: org-102, Name: "Rohan"            │
│                                                             │
│  [Batches] (Filtered by OrganizationId)                    │
│    ├── Id: bat-1, OrgId: org-101, Name: "Class 10 - Batch A"│
│    └── Id: bat-2, OrgId: org-102, Name: "Class 12 - IIT"   │
└─────────────────────────────────────────────────────────────┘
```

1. **`OrganizationId` Mandatory Key**: Every tenant-owned entity inherits from `BaseTenantEntity` (`public Guid OrganizationId { get; set; }`).
2. **EF Core Global Query Filters**:
   ```csharp
   builder.Entity<Student>().HasQueryFilter(e => e.OrganizationId == _tenantContext.OrganizationId);
   ```
3. **Session Tenant Resolution**: `TenantMiddleware` extracts the tenant claim (`org_id`) from the validated JWT token and registers `ITenantContext` per scoped request.
4. **Super Admin Bypass**: Super Admin requests operate under a specialized `BypassTenantFilter` scope when explicitly authorized.
