# Vargshala Project Rules

## Mandatory

This repository is **Vargshala** — a Multi-tenant Coaching & Institute Management SaaS.

Follow the architecture and patterns documented in `/docs`.

Before creating or modifying code:

1. **Inspect existing code** first to preserve established patterns and naming.
2. **Determine the correct project/layer** using the Project Mapping below.
3. **Read only the relevant documentation** from `/docs` to maintain efficient context.
4. **Reuse existing classes/contracts/services** before introducing new abstractions.
5. **Do not create duplicate DTOs** — all request/response contracts belong in `src/Vargshala.Contracts`.
6. **Do not put database access in Vargshala.Web or Vargshala.API** — database queries belong exclusively in `src/Vargshala.Infrastructure` via `src/Vargshala.Application` handlers/repositories.
7. **Do not put business logic in API controllers** — controllers must remain thin dispatchers invoking Application commands/queries.
8. **Do not put JWT implementation details in Domain or Application** — security infrastructure belongs in `src/Vargshala.Infrastructure/Authentication`.
9. **Enforce `OrganizationId` tenant isolation** on every domain entity and query filter.
10. **Never trust client-supplied `OrganizationId` for authorization** — always resolve tenant context from the authenticated identity/claims principal (`ICurrentUserService` / `ITenantContext`).
11. **Never commit secrets, credentials, or production connection strings** to version control.
12. **Do not introduce microservices or external service bus complexity** without explicit architectural approval. Maintain the Clean Modular Monolith.

---

## Project Layer Mapping

| Layer / Responsibility | Target Project / Directory |
| :--- | :--- |
| **Domain Entities, Value Objects, Domain Events, Domain Exceptions** | `src/Vargshala.Domain` |
| **Application Use Cases, Commands, Queries, Handlers, Validators, Interfaces** | `src/Vargshala.Application` |
| **Shared DTOs, API Requests, Responses, Query Filters, Enums** | `src/Vargshala.Contracts` |
| **EF Core DbContext, Migrations, Repositories, JWT, File Storage, External Services** | `src/Vargshala.Infrastructure` |
| **ASP.NET Core Web API, Controllers, Middleware, Filters, Swagger/OpenAPI** | `src/Vargshala.API` |
| **Frontend UI (Blazor / React Web App), Client Services, ViewModels, Pages** | `src/Vargshala.Web` |
| **Unit Tests (Domain & Application Logic)** | `tests/Vargshala.UnitTests` |
| **Integration Tests (API Endpoints & Database Isolation)** | `tests/Vargshala.IntegrationTests` |

---

## Documentation Routing

When implementing or modifying features, refer to the corresponding documentation:

- **Product Overview & Vision**: [`/docs/00_PROJECT_OVERVIEW.md`](file:///d:/vargshala.com/docs/00_PROJECT_OVERVIEW.md)
- **Architecture & Layer Flow**: [`/docs/01_ARCHITECTURE.md`](file:///d:/vargshala.com/docs/01_ARCHITECTURE.md)
- **Project Structure & Namespaces**: [`/docs/02_PROJECT_STRUCTURE.md`](file:///d:/vargshala.com/docs/02_PROJECT_STRUCTURE.md)
- **Database, EF Core & Multi-tenancy**: [`/docs/03_DATABASE_RULES.md`](file:///d:/vargshala.com/docs/03_DATABASE_RULES.md)
- **API Conventions & Envelope Format**: [`/docs/04_API_CONVENTIONS.md`](file:///d:/vargshala.com/docs/04_API_CONVENTIONS.md)
- **Security & Tenant Isolation Rules**: [`/docs/05_SECURITY_RULES.md`](file:///d:/vargshala.com/docs/05_SECURITY_RULES.md)
- **C# / .NET Coding Standards**: [`/docs/06_CODING_STANDARDS.md`](file:///d:/vargshala.com/docs/06_CODING_STANDARDS.md)
- **Feature Development Lifecycle**: [`/docs/07_FEATURE_DEVELOPMENT.md`](file:///d:/vargshala.com/docs/07_FEATURE_DEVELOPMENT.md)
- **Testing Guide**: [`/docs/08_TESTING_GUIDE.md`](file:///d:/vargshala.com/docs/08_TESTING_GUIDE.md)
- **Git Workflow & Commit Guidelines**: [`/docs/09_GIT_WORKFLOW.md`](file:///d:/vargshala.com/docs/09_GIT_WORKFLOW.md)
- **File Placement Rules**: [`/docs/10_FILE_CREATION_RULES.md`](file:///d:/vargshala.com/docs/10_FILE_CREATION_RULES.md)
- **JWT & Auth Mechanism**: [`/docs/11_JWT_AUTHENTICATION.md`](file:///d:/vargshala.com/docs/11_JWT_AUTHENTICATION.md)
- **Feature Specifications (V1.0 Requirements)**: [`/docs/12_FEATURE_SPECIFICATION.md`](file:///d:/vargshala.com/docs/12_FEATURE_SPECIFICATION.md)
- **AI Agent Index & Decision Matrix**: [`/docs/13_AI_INDEX.md`](file:///d:/vargshala.com/docs/13_AI_INDEX.md)

---

## Critical Rules

1. **If you are unsure where a file belongs: DO NOT GUESS.** Inspect the repository and relevant documentation in `/docs` first.
2. **Never break Multi-Tenant Data Isolation.** All queries targeting tenant entities must be filtered by `OrganizationId`.
3. **Before completing any task, report:**
   - Files created
   - Files modified
   - Tests added/modified
   - Any architectural decision made
