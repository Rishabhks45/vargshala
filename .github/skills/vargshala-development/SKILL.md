# Vargshala Development Skill

## Purpose
Use this skill when modifying or extending the Vargshala solution. Follow the repository's existing layered architecture and vertical-slice conventions instead of introducing a new pattern.

## Repository Shape

Vargshala is a .NET 10 solution using C# 14, PostgreSQL/EF Core, ASP.NET Core Web API, MediatR, FluentValidation, and Blazor Server with Interactive Server rendering.

- `src/Vargshala.Domain` — entities, shared base entity, and domain exceptions. It must not depend on Application, Infrastructure, API, or Web.
- `src/Vargshala.Contracts` — HTTP/shared DTOs, request models, enums, response wrappers, pagination models, and validators shared by API and Web.
- `src/Vargshala.Application` — use cases and ports. MediatR commands/queries, handlers, validators, repository/service abstractions, mapping extensions, pipeline behaviors, and application settings live here.
- `src/Vargshala.Infrastructure` — adapters for persistence, authentication, encryption, email, PDF generation, and messaging authorization. It implements Application abstractions.
- `src/Vargshala.API` — REST API composition root, controllers, JWT setup, middleware, SignalR hub, and API-only services.
- `src/Vargshala.Web` — Blazor Server UI, cookie authentication, API HTTP clients, pages, reusable UI components, and client-side state/helpers.
- `tests/Vargshala.UnitTests` — xUnit unit tests with Moq and FluentAssertions.
- `tests/Vargshala.IntegrationTests` — integration-test project; add integration coverage here when the feature requires a real HTTP/host/database boundary.

Dependency direction is Domain <- Application <- Infrastructure, while API composes Application and Infrastructure. Web references Contracts and calls API over HTTP; it does not call Application or Infrastructure directly.

## Composition Roots

API registration and middleware order are defined in `src/Vargshala.API/Program.cs`:

1. Serilog configuration
2. MVC controllers, OpenAPI/Swagger, and SignalR
3. `AddApplicationServices()`
4. `AddInfrastructureServices(configuration)`
5. API SignalR notification service
6. JWT authentication
7. CORS policy for the Blazor origin
8. Development Swagger/Scalar endpoints
9. `ExceptionHandlingMiddleware`
10. HTTPS/static files/CORS
11. Authentication then authorization
12. Chat hub and controllers

Infrastructure registrations are centralized in `src/Vargshala.Infrastructure/DependencyInjection/InfrastructureServiceRegistration.cs`. Register new repository implementations, settings, and external adapters there. Application registrations are in `src/Vargshala.Application/DependencyInjection/ApplicationServiceRegistration.cs`; MediatR scans the Application assembly and validation/logging pipeline behaviors are registered globally.

Blazor registration and middleware are in `src/Vargshala.Web/Startup.cs`, called by `Program.cs` through `AddServices()` and `UseServices()`. Use the named clients `VargshalaApi.Anonymous` for login/refresh/register calls and `VargshalaApi` for authenticated calls. The authenticated client uses `JwtTokenHandler` for Bearer headers and synchronized 401 refresh.

## Feature Implementation Pattern

Prefer a vertical slice under `src/Vargshala.Application/Features/<Area>/<Feature>`:

```text
Feature/
  Commands/<VerbFeature>/
    <VerbFeature>Command.cs
    <VerbFeature>CommandHandler.cs
  Queries/<ReadFeature>/
    <ReadFeature>Query.cs
    <ReadFeature>QueryHandler.cs
  Infrastructure/
    I<Feature>Repository.cs
  <Feature>MappingExtensions.cs
```

Not every feature needs every folder. Keep a command/query and its handler together. Commands and queries return `ApiResponse<T>` (or `ApiResponse<PagedResponse<T>>`) and accept/pass `CancellationToken`.

Typical flow:

```text
Blazor .razor page
  -> Web.Services/<Feature>Service.cs
  -> API controller route
  -> IMediator.Send(command/query)
  -> ValidationBehavior, then LoggingBehavior
  -> Application handler
  -> Application repository abstraction
  -> Infrastructure repository / IVargshalaDbContext
  -> EF Core/PostgreSQL
  -> mapping extension -> DTO -> ApiResponse -> Web page
```

## Representative Students Flow

Use these files as the canonical example when adding an OrgAdmin CRUD feature:

- API endpoint: `src/Vargshala.API/Controllers/OrgAdmin/StudentsController.cs`
  - Route prefix: `api/v1/orgadmin/students`.
  - Apply `[ApiController]` and role authorization.
  - Keep controllers thin: bind request, create MediatR request, send it, and translate success/failure to HTTP status codes.
  - Existing endpoints use `GET` for reads, `POST` create, `PUT {id}` update, and `DELETE {id}` delete. Use `CreatedAtAction` for successful creates.
- Requests/DTOs/validators: `src/Vargshala.Contracts/Students/StudentDto.cs` and related request models/validators.
  - Shared contracts are the API/Web boundary; do not expose Domain entities from controllers.
  - Use FluentValidation for shape/range/length validation. Application command validators can compose request validators.
- Use cases: `src/Vargshala.Application/Features/OrgAdmin/Students/`
  - `CreateStudentCommand` wraps `CreateStudentRequest` and returns `ApiResponse<StudentDto>`.
  - `CreateStudentCommandHandler` obtains organization context from `ICurrentUser`, checks uniqueness, creates the related `User` and `Student`, saves once, and maps with `StudentMappingExtensions`.
  - `GetStudentsPagedQueryHandler` validates organization/branch scope, calls the repository, maps entities, and creates `PagedResponse<StudentDto>`.
- Repository port: `Features/OrgAdmin/Students/Infrastructure/IStudentRepository.cs`.
- Repository adapter: `src/Vargshala.Infrastructure/Persistence/Repositories/StudentRepository.cs`.
  - Use `AsNoTracking()` for read-only queries.
  - Centralize search and allowed sort expressions in repository mappings.
  - Use `QueryableExtensions.ToPagedResultAsync` for paging/search/sort.
  - Include explicit organization/branch scoping and soft-delete conditions.
- UI client: `src/Vargshala.Web/Services/IStudentService.cs` and `StudentService.cs`.
  - Call the matching API route using the authenticated named client.
  - Serialize requests with `PostAsJsonAsync`/`PutAsJsonAsync`; deserialize `ApiResponse<T>`.
  - Log exceptions and return a failed `ApiResponse` rather than leaking exceptions into a page.
- UI page: `src/Vargshala.Web/Components/Pages/OrgAdmin/Students.razor`.
  - Use `@page`, role `[Authorize]`, injected service interfaces, `@rendermode InteractiveServer`, loading/empty/error states, reusable UI components, pagination, and disposal for subscriptions/state events.

## Domain and Persistence Rules

- Put persistence-independent business entities in `src/Vargshala.Domain/Entities` and shared audit/soft-delete fields in `Domain/Common/BaseEntity.cs`.
- Put domain-specific exceptional conditions in `Domain/Exceptions/DomainException.cs`; let API middleware convert them to the standard failure response.
- Add one EF mapping class per entity under `Infrastructure/Persistence/Configurations` implementing `IEntityTypeConfiguration<TEntity>`.
- Add the `DbSet` and configuration only in Infrastructure. `VargshalaDbContext` applies configurations from its assembly.
- Soft delete is established through global query filters in `VargshalaDbContext`; do not bypass them unless an explicit administrative/audit use case requires `IgnoreQueryFilters()`.
- `SaveChangesAsync` assigns IDs/created timestamps, updates `UpdatedAt`, and normalizes unspecified `DateTime` values for PostgreSQL `timestamptz`.
- Create a migration for schema changes using the Infrastructure context/migrations assembly. Do not edit generated migration designer/snapshot files manually unless tooling requires it.
- Preserve relationships, unique indexes, and delete behavior in the matching configuration. For example, Student has a one-to-one User relationship and unique `UserId`/`StudentCode` indexes.

## Tenant, Role, and Security Rules

- Treat organization and branch scope as authorization boundaries, not merely UI filters.
- Obtain request context through `ICurrentUser`, implemented by Infrastructure `CurrentUser` from claims. Do not read `HttpContext` directly in Application handlers.
- For organization-owned queries, require a non-empty `OrganizationId` and filter by it in the repository.
- Validate requested branch IDs belong to the current organization before querying or mutating data.
- Keep role authorization on API controllers and Blazor pages. Match existing role names (`OrganizationAdmin`, `BranchAdmin`, `Student`, etc.) and existing numeric compatibility where present.
- Never return passwords, encryption keys, refresh tokens, or other secrets in DTOs/logs.
- Use the existing encryption/token abstractions (`IEncryptionService`, `ITokenService`) instead of implementing cryptography in a feature.
- Preserve API exception handling through `ExceptionHandlingMiddleware`; do not expose raw exception details in normal responses.

## API Conventions

- Use the `api/v1` route prefix and existing area naming (`orgadmin`, `branchadmin`) for new endpoints.
- Return `ApiResponse<T>` consistently. For expected business failures, return a failed response and let the controller select the established status code (`BadRequest`, `NotFound`, or `Unauthorized`).
- Pass cancellation tokens from controller to MediatR and from handlers to repository/EF calls.
- Add OpenAPI metadata only when it improves an endpoint's discoverability; keep Swagger/Scalar behavior centralized in API extensions.
- Do not place database queries, mapping-heavy logic, or business rules in controllers.

## Contracts and Pagination

- Use `ApiResponse<T>` from `Contracts/Common/ApiResponse.cs` with `Success`, nullable `Data`, `Message`, and optional `Errors`.
- Use `PagedRequest`/`PagedResponse<T>` from `Contracts/Common/PagedResponse.cs`. Page numbers are one-based and page size is capped at 100.
- Keep sort input allow-listed through repository sort mappings; never concatenate raw client sort values into SQL.
- URL-encode optional query-string values in Web services, as `StudentService` does.
- Keep DTO naming and compatibility aliases consistent with existing contracts when older UI code depends on them.

## Blazor Web Conventions

- Pages belong under `src/Vargshala.Web/Components/Pages/<RoleArea>`; reusable controls belong under `Components/UI`.
- Use `AuthorizeRouteView` in `Components/Routes.razor` and page-level `[Authorize]` attributes for protected pages.
- Put API calls in an interface plus service pair under `Web/Services`, not directly in `.razor` markup.
- Inject service interfaces into pages. Keep page code focused on UI state, user interaction, and orchestration.
- Use the existing notification service, modal, pagination, badge, skeleton, and validation components before creating duplicates.
- Follow the current Tailwind/design-token conventions and preserve Interactive Server render mode where used.
- Use `BranchContextService` and existing BranchAdmin client services for branch selection/state instead of inventing a second branch-state mechanism.
- Dispose event subscriptions and SignalR-related resources in pages/services that implement `IDisposable`/`IAsyncDisposable`.

## Testing Conventions

- Add unit tests under a namespace/path matching the feature, for example `tests/Vargshala.UnitTests/Features/OrgAdmin/Students`.
- Test handlers in isolation with Moq for repository, current-user, token, encryption, and other abstractions. Arrange/Act/Assert is the established style.
- Use xUnit `[Fact]`, FluentAssertions, and explicit failure-message assertions for business failures.
- Cover success, missing entity, invalid input, inactive/deleted entity, tenant/branch boundary, and external-service failure paths.
- Add integration tests under `tests/Vargshala.IntegrationTests` when validating routing, middleware, authentication, serialization, or EF behavior. Do not assume this project already contains a test host; inspect its current contents first.

## New Feature Checklist

1. Identify the role/area and choose the closest existing vertical slice.
2. Add or update Contracts DTOs, request models, enums, and validators.
3. Add Application command/query records, validators, handlers, mapping, and repository interfaces.
4. Add Infrastructure repository/service implementation, EF configuration, and migration if schema changes.
5. Register new adapters in Infrastructure DI; register helpers in Application DI when needed.
6. Add a thin API controller endpoint with route, authorization, cancellation, and standard response handling.
7. Add a Web service interface/implementation using the correct named HttpClient.
8. Add or update the Blazor page/components with authorization, loading/error/empty states, notifications, and disposal.
9. Add focused unit tests and integration tests where boundary behavior matters.
10. Build the solution and run the relevant tests before finishing.

## Avoid

- Do not make Web pages reference Application handlers or Infrastructure repositories directly.
- Do not return EF entities from API endpoints.
- Do not duplicate tenant filtering only in the UI.
- Do not add a second response envelope, pagination model, authentication flow, or notification mechanism.
- Do not put business logic in controllers, HTTP client services, or Razor markup when it belongs in an Application handler.
- Do not remove soft-delete filters or authorization to make a failing query appear to work.
- Do not introduce a new architectural pattern without documenting why the existing vertical-slice pattern cannot support the requirement.
