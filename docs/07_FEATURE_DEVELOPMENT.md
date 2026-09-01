# 07. Feature Development Lifecycle & Workflow

## 1. Vertical Slice Workflow for New Features

When implementing any new feature in Vargshala (e.g. creating a new feature like "Quiz Grading" or "Fee Receipt Generation"), follow this sequential 6-step lifecycle:

```
[1. Domain Entity] ──► [2. Contracts/DTO] ──► [3. Application Handler]
                                                         │
[6. Web UI]        ◄── [5. API Controller] ◄── [4. EF Configuration]
```

---

## 2. Step-by-Step Implementation Guide

### Step 1: Define Domain Entities & Invariants (`Vargshala.Domain`)
- Create the entity in `src/Vargshala.Domain/Entities/<FeatureName>/`.
- Inherit from `BaseTenantEntity` (if tenant-owned) or `BaseEntity`.
- Define private setters and domain methods to mutate state safely.
- Add any domain events if other modules need to react to changes.

### Step 2: Define Shared Contracts & DTOs (`Vargshala.Contracts`)
- Create request contracts (e.g. `CreateHomeworkRequest.cs`) and response DTOs (e.g. `HomeworkDetailsDto.cs`).
- Place them in `src/Vargshala.Contracts/<FeatureName>/`.

### Step 3: Implement Application Command / Query (`Vargshala.Application`)
- Create Command or Query record implementing `IRequest<Result<TResponse>>`.
- Create FluentValidation validator implementing `AbstractValidator<TCommand>`.
- Create Handler implementing `IRequestHandler<TCommand, Result<TResponse>>`.
- Inject `IApplicationDbContext`, `ITenantContext`, `ICurrentUserService`.

### Step 4: Configure EF Core Persistence (`Vargshala.Infrastructure`)
- Add `DbSet<Entity>` to `IApplicationDbContext` and `ApplicationDbContext`.
- Create Entity Configuration implementing `IEntityTypeConfiguration<TEntity>` in `src/Vargshala.Infrastructure/Persistence/Configurations/`.
- Configure table names, column lengths, foreign key behaviors, and composite tenant indexes.
- Generate and verify EF Core migration.

### Step 5: Expose API Endpoint (`Vargshala.API`)
- Create or update controller in `src/Vargshala.API/Controllers/<FeatureName>Controller.cs`.
- Add `[Authorize]` attribute with required roles/policies.
- Map HTTP request to Application command/query via `mediator.Send()`.
- Return standardized `ApiResponse<T>`.

### Step 6: Integrate with Frontend Client (`Vargshala.Web`)
- Create typed client service method calling the API endpoint.
- Bind API responses to UI components and views.
- Add client-side validation matching server-side validation rules.

---

## 3. Pre-Completion Checklist for Every Feature
- [ ] Tenant isolation verified: `OrganizationId` is automatically applied.
- [ ] No client-supplied tenant ID is accepted for authorization.
- [ ] FluentValidation covers all required fields and business constraints.
- [ ] EF Core query uses `.AsNoTracking()` for read queries.
- [ ] Soft-delete is respected (`IsDeleted = false`).
- [ ] API endpoint returns `ApiResponse<T>` with correct HTTP status code.
- [ ] Unit tests added in `tests/Vargshala.UnitTests`.
