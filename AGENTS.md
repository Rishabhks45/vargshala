# Antigravity & AI Agent Mandatory Rules — Vargshala

Welcome to **Vargshala** — a Multi-tenant Coaching & Educational Institute Management SaaS built with Clean Architecture, ASP.NET Core (.NET 10), EF Core, FluentValidation, and Blazor Web with Tailwind CSS.

---

## 🚨 MANDATORY INVARIANTS (ALWAYS FOLLOW ON EVERY PROMPT)

### 1. Multi-Tenancy & Security Invariants
- **Shared Database / Shared Schema**: Every tenant entity MUST have `Guid OrganizationId`.
- **Global Query Filter**: Every EF Core query on tenant entities MUST apply `OrganizationId == CurrentTenantId`.
- **Never Trust Client Tenant ID**: Always resolve tenant context server-side from JWT claims (`ITenantContext` / `ICurrentUserService`).
- **Never commit secrets**: Never hardcode credentials, JWT secret keys, or connection strings.

### 2. Clean Architecture Layer Boundaries
- **`src/Vargshala.Domain`**: ZERO external dependencies. Contains Entities, Value Objects, Domain Events, Enums.
- **`src/Vargshala.Contracts`**: Shared Request/Response DTOs, Enums, and **FluentValidation Validators** (e.g. `StudentDtoValidator : AbstractValidator<StudentDto>`).
- **`src/Vargshala.Application`**: Use cases, MediatR Commands/Queries, Command Validators (`FluentValidation`), Application interfaces.
- **`src/Vargshala.Infrastructure`**: EF Core DbContext, Repositories, Migrations, JWT Auth, File Storage.
- **`src/Vargshala.API`**: Thin controllers dispatching to Application via MediatR and returning `ApiResponse<T>`. No business logic.
- **`src/Vargshala.Web`**: Blazor UI components, Pages, Client Services, ViewModels. Zero direct database access.

### 3. UI, Styling & Blazor Guidelines
- **Design System**: **Theme 4 — Deep Teal** (`#004D40` headers, `#009488` primary teal accents, `#00796b` gradients, slate neutrals).
- **Form Inputs**: ALWAYS use Blazor official input components: `<EditForm>`, `<InputText>`, `<InputNumber>`, `<ValidationMessage>`, `<CustomSelect>` with `<FluentValidationValidator />`.
- **Validation Standard**: Use **FluentValidation** (`AbstractValidator<T>`) with `CascadeMode.Stop` to prevent stacked validation errors.
- **Table Sorting**: Use reusable `TableSortState` (`Vargshala.Web.Common`) with 3-state cycle (`▲` ➔ `▼` ➔ `↕`).
- **Dropdowns**: ALWAYS use `<CustomSelect>` instead of native OS `<select>` elements to prevent blue browser highlights.
- **Tailwind Compilation**: In `src/Vargshala.Web`, compile using: `& ".\tailwindcss.exe" -i ".\Styles\app.css" -o ".\wwwroot\app.css" --minify`.

---

## 📚 Key Documentation Map
- **Architecture Overview**: [`docs/01_ARCHITECTURE.md`](file:///d:/vargshala.com/docs/01_ARCHITECTURE.md)
- **Database & Multi-Tenancy Rules**: [`docs/03_DATABASE_RULES.md`](file:///d:/vargshala.com/docs/03_DATABASE_RULES.md)
- **API Conventions**: [`docs/04_API_CONVENTIONS.md`](file:///d:/vargshala.com/docs/04_API_CONVENTIONS.md)
- **Security Rules**: [`docs/05_SECURITY_RULES.md`](file:///d:/vargshala.com/docs/05_SECURITY_RULES.md)
- **Feature Specification**: [`docs/12_FEATURE_SPECIFICATION.md`](file:///d:/vargshala.com/docs/12_FEATURE_SPECIFICATION.md)
- **AI Agent Index & Matrix**: [`docs/13_AI_INDEX.md`](file:///d:/vargshala.com/docs/13_AI_INDEX.md)
