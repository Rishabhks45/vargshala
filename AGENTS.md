# Antigravity Rules - Vargshala

Welcome to **Vargshala** — a multi-tenant coaching and educational institute management SaaS built with Clean Architecture, ASP.NET Core, EF Core, and modern web technologies.

## Quick Index for AI Agents

- **Mandatory Agent Rules**: See [`.agent/rules/vargshala.md`](file:///d:/vargshala.com/.agent/rules/vargshala.md)
- **AI Decision Matrix & Index**: See [`docs/13_AI_INDEX.md`](file:///d:/vargshala.com/docs/13_AI_INDEX.md)
- **Architecture Overview**: See [`docs/01_ARCHITECTURE.md`](file:///d:/vargshala.com/docs/01_ARCHITECTURE.md)
- **Feature Specifications (V1.0)**: See [`docs/12_FEATURE_SPECIFICATION.md`](file:///d:/vargshala.com/docs/12_FEATURE_SPECIFICATION.md)

## Core Architectural Invariants

1. **Shared Database / Shared Schema Multi-Tenancy**: Every tenant entity MUST have an `OrganizationId`. Every EF Core query MUST apply the global query filter for `OrganizationId`.
2. **Never Trust Client Tenant ID**: Tenant context is resolved server-side from JWT claims / `ITenantContext`.
3. **Clean Architecture Boundaries**:
   - `Domain` has ZERO external dependencies.
   - `Application` depends only on `Domain` and `Contracts`.
   - `Infrastructure` implements database, JWT, file storage, and external services.
   - `API` controllers only dispatch commands/queries to `Application` and return `ApiResponse<T>`.
   - `Web` communicates with `API` via typed client services using `Contracts`.
