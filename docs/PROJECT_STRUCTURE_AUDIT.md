# Project Structure & Architecture Audit

**Scope:** Static review of the Vargshala solution structure, Clean Architecture boundaries, multi-tenancy protections, authorization layout, validation registration, and test setup.

**Status:** Findings only. No production code was changed as part of this audit.

## Executive summary

The solution has the intended top-level layers (`Domain`, `Contracts`, `Application`, `Infrastructure`, `API`, and `Web`) and a useful feature-oriented Application layout. However, its most important architectural invariant—shared-schema tenant isolation—is not globally enforced. There are also dependency-direction violations, infrastructure leakage into Application and Contracts, and incomplete authorization/test coverage.

The first remediation milestone should be tenant safety: make tenant ownership explicit and non-null, enforce it with global query filters, and add integration tests that prove one organization cannot read or mutate another organization's data.

## Findings

### P0 — Tenant isolation is not enforced globally

`VargshalaDbContext` only defines soft-delete filters. It does not constrain tenant entities by the organization resolved from the authenticated server-side user context. Any new query that omits an organization predicate can therefore return another tenant's data.

- Evidence: `src/Vargshala.Infrastructure/Persistence/VargshalaDbContext.cs:29-35`
- Example: `StudentRepository` lookup methods fetch by ID/code without an organization predicate: `src/Vargshala.Infrastructure/Persistence/Repositories/StudentRepository.cs:60-87`.

**Recommendation:** Introduce a server-side `ITenantContext`/`ICurrentUser` dependency in the DbContext and apply a query filter equivalent to `entity.OrganizationId == CurrentTenantId` for every tenant entity. Permit bypass only for a deliberately scoped SuperAdmin/platform operation.

### P0 — Tenant ownership is nullable or absent on tenant entities

The project rule requires every tenant entity to have a non-null `Guid OrganizationId`. Current exceptions include:

- `User.OrganizationId` is nullable: `src/Vargshala.Domain/Entities/User.cs:8`.
- `Coupon.OrganizationId` is nullable: `src/Vargshala.Domain/Entities/Coupon.cs:8`.
- `EmailTemplate.OrganizationId` is nullable: `src/Vargshala.Domain/Entities/EmailTemplate.cs:8`.
- `Student` has no direct `OrganizationId`: `src/Vargshala.Domain/Entities/Student.cs:5-46`.
- `Teacher` has no direct `OrganizationId`: `src/Vargshala.Domain/Entities/Teacher.cs:5-33`.
- `UserBranchAccess` has no direct `OrganizationId`: `src/Vargshala.Domain/Entities/UserBranchAccess.cs:3-20`.

Inferring ownership through `User` or `Branch` navigation properties is fragile: it makes safe filtering optional and complicates indexes, constraints, and future queries.

**Recommendation:** Make `OrganizationId` a required property on all tenant-owned entities, configure required foreign keys, populate it only from server-side context, and add it to relevant foreign-key and lookup indexes.

### P1 — Domain depends on Contracts

The Domain project references Contracts, despite the intended rule that Domain has zero external/project dependencies. Domain entities import contract enums such as `UserRole`, `EmailTemplateCategory`, and coupon enums.

- Project reference: `src/Vargshala.Domain/Vargshala.Domain.csproj:3-5`.
- Entity dependency examples: `src/Vargshala.Domain/Entities/User.cs:2`, `src/Vargshala.Domain/Entities/EmailTemplate.cs:1`, `src/Vargshala.Domain/Entities/Coupon.cs:1`.

**Recommendation:** Move domain concepts/enums into `Vargshala.Domain`; Contracts should expose API DTOs and may map to the Domain types. Remove the Domain → Contracts project reference.

### P1 — Application exposes EF Core persistence details

`IVargshalaDbContext` exposes `DbSet<T>` instances directly. Several Application handlers and helpers use EF Core queries, which bypasses the documented feature-repository pattern.

- Interface: `src/Vargshala.Application/Abstractions/Persistence/IVargshalaDbContext.cs:6-17`.
- Examples include `CreateStudentCommandHandler`, `UpdateStudentCommandHandler`, `CreateTeacherCommandHandler`, and control-panel user handlers.

**Recommendation:** Keep data-access interfaces feature-specific (for example `IStudentRepository`), inject those into handlers, and keep EF Core/LINQ-to-provider details in Infrastructure repository implementations.

### P1 — API controllers contain use-case and infrastructure logic

Controllers are not consistently thin.

- `UploadsController` performs file validation, authorization decisions, filesystem path construction, directory creation, writes, and deletes: `src/Vargshala.API/Controllers/UploadsController.cs:51-249`.
- `EmailsController` directly invokes the email provider abstraction and builds test-email HTML: `src/Vargshala.API/Controllers/EmailsController.cs:82-150`.

**Recommendation:** Move these flows into Application commands and interfaces. The controller should bind input, dispatch one MediatR request, and translate the result to HTTP.

### P1 — Contracts contains Web/Blazor implementation code

`Vargshala.Contracts` contains a concrete upload service that depends on Blazor's `IBrowserFile`, `HttpClient`, JSON transport, and multipart upload behavior.

- Interface: `src/Vargshala.Contracts/Common/IFileUploadService.cs:1-27`.
- Concrete implementation: `src/Vargshala.Contracts/Common/FileUploadService.cs:1-159`.

This makes the shared DTO layer depend on UI/client concerns.

**Recommendation:** Keep only request/response DTOs in Contracts. Move `IFileUploadService` and `FileUploadService` to `Vargshala.Web` (or create a Web-specific client abstraction).

### P1 — Contract validators are not registered in the API composition root

The API calls `AddApplicationServices()`, which scans only the Application assembly for validators. Contract-side FluentValidation validators are registered in the Web startup path, not the API path.

- API composition: `src/Vargshala.API/Program.cs:17-18`.
- Application validator scanning: `src/Vargshala.Application/DependencyInjection/ApplicationServiceRegistration.cs:15`.
- Web validator scanning: `src/Vargshala.Web/Startup.cs:34-35`.

**Recommendation:** Decide the validation boundary explicitly. Register Contract validators in API if they validate HTTP DTOs, and keep business/use-case rules in Application command validators. Avoid duplicate rules across both layers.

### P1 — Tenant-scoped business codes are globally unique

The data model prevents different organizations from reusing common codes.

- `StudentCode` has a global unique index: `src/Vargshala.Infrastructure/Persistence/Configurations/StudentConfiguration.cs:48`.
- `EmployeeCode` has a global unique index: `src/Vargshala.Infrastructure/Persistence/Configurations/TeacherConfiguration.cs:38`.
- `Coupon.Code` has a global unique index despite an `OrganizationId` property: `src/Vargshala.Infrastructure/Persistence/Configurations/CouponConfiguration.cs:32-43`.

**Recommendation:** Use tenant-scoped, soft-delete-aware unique indexes such as `(OrganizationId, StudentCode)`, `(OrganizationId, EmployeeCode)`, and—if coupons are tenant-owned—`(OrganizationId, Code)`.

### P1 — UserBranchAccess bypasses entity conventions

`UserBranchAccess` does not inherit `BaseEntity`; it does not have `OrganizationId`, `IsDeleted`, or soft-delete audit fields. It is also omitted from the global soft-delete-filter list.

- Entity: `src/Vargshala.Domain/Entities/UserBranchAccess.cs:3-20`.
- DbContext filters: `src/Vargshala.Infrastructure/Persistence/VargshalaDbContext.cs:29-35`.

**Recommendation:** Treat it as a tenant entity: add required ownership and standard audit/soft-delete fields where appropriate, then protect it with tenant/global query filters.

### P1 — Privileged Blazor routes are missing explicit authorization attributes

`AuthorizeRouteView` enforces authorization metadata declared by each page; hiding navigation entries does not itself protect direct navigation. Several control-panel pages have an `@page` directive but no `[Authorize]`/role attribute, including Users, Coupons, Health, Logs, Institutes, Push, Payments, and Subscriptions.

- Router: `src/Vargshala.Web/Components/Routes.razor:7-33`.
- Example unannotated route: `src/Vargshala.Web/Components/Pages/ControlPanel/Users.razor:1`.

**Recommendation:** Add a role-specific `[Authorize]` attribute to every protected page. Consider a layout, convention, or route-folder policy that defaults `/controlpanel/*` to `SuperAdmin,BackOffice` and requires explicit exceptions.

### P2 — Domain contains database schema and seed SQL

The Domain project contains SQL table definitions and seed scripts even though EF Core migrations are owned by Infrastructure.

- Location: `src/Vargshala.Domain/Db/Tables/`.
- EF migrations: `src/Vargshala.Infrastructure/Persistence/Migrations/`.

**Recommendation:** Move the SQL artifacts to `docs/`, a separate database project, or Infrastructure migration tooling. Keep Domain limited to business entities, value objects, events, and rules.

### P2 — Authorization role declarations are inconsistent

JWT role claims are emitted as enum names (for example `OrganizationAdmin`), while some controller attributes also include numeric role values (for example `"OrganizationAdmin,1"` and `"SuperAdmin,1001"`).

- Claim creation: `src/Vargshala.Infrastructure/Authentication/JwtTokenService.cs:27`.
- Example controller authorization: `src/Vargshala.API/Controllers/OrgAdmin/TeachersController.cs:17`.

**Recommendation:** Use centralized role constants and the single claim representation emitted by the token service. Remove numeric role literals from attributes.

### P2 — Configuration files require a secrets-history review

Tracked API/Web appsettings files contain secret-shaped configuration keys. Their values were intentionally not inspected in this audit. The `.gitignore` rules do not stop files that are already tracked from remaining in Git history.

- Ignore rules: `.gitignore:74-77`.
- Relevant tracked files: `src/Vargshala.API/appsettings.json`, `src/Vargshala.API/appsettings.Development.json`, and `src/Vargshala.Web/appsettings.json`.

**Recommendation:** Move all credentials, API keys, JWT signing keys, and connection strings to User Secrets for local development and environment/managed-secret storage for deployed environments. Rotate any secret that was ever committed.

### P2 — Test suite does not currently provide reliable architecture coverage

- The IntegrationTests project currently contains no discovered tests.
- Unit tests reference Infrastructure, weakening the isolation expected from a unit-test project: `tests/Vargshala.UnitTests/Vargshala.UnitTests.csproj:8-10`.
- Local `dotnet test Vargshala.sln --no-restore --verbosity minimal` built all projects, but the unit test assembly could not load because a local Windows Application Control policy blocked it. This is an environment restriction rather than confirmed test-code failure.

**Recommendation:** Add integration tests for tenant filtering, cross-tenant guessed-ID access, role authorization, and database unique indexes. Keep handler unit tests limited to Application/Domain and mocked abstractions. Verify tests in CI or an environment without the local policy restriction.

## Recommended remediation order

1. Define a required tenant-entity contract and add non-null `OrganizationId` to every tenant-owned record.
2. Implement server-side tenant context and EF global tenant filters; write cross-tenant integration tests before migrating more features.
3. Replace global code indexes with tenant-scoped indexes and migrate existing data safely.
4. Remove Domain → Contracts dependency by relocating Domain enums/concepts.
5. Move EF access behind feature repositories and move API controller logic into Application use cases.
6. Move Web-only upload client code out of Contracts; register validation deliberately in the appropriate host.
7. Protect every Blazor route explicitly and centralize role constants.
8. Review tracked configuration history, rotate exposed secrets if any, and strengthen tests/CI.

## Notes

- The working tree had pre-existing modified and untracked files during this audit. They were not changed.
- The audit is based on source inspection and one local test invocation; it is not a penetration test or a full runtime security assessment.
