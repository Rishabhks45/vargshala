# 13. AI Assistant Routing Index & Decision Matrix

## 1. Quick Task Decision Matrix

When asked to perform a coding task, find your target scenario in the table below to know exactly which files and documentation to inspect:

| If your task is to... | Read these docs first | Edit/Create files in... |
| :--- | :--- | :--- |
| **Add a new database model / entity** | [`01_ARCHITECTURE.md`](./01_ARCHITECTURE.md)<br>[`03_DATABASE_RULES.md`](./03_DATABASE_RULES.md) | `src/Vargshala.Domain/Entities/`<br>`src/Vargshala.Infrastructure/Persistence/Configurations/` |
| **Create a new API endpoint / action** | [`04_API_CONVENTIONS.md`](./04_API_CONVENTIONS.md)<br>[`05_SECURITY_RULES.md`](./05_SECURITY_RULES.md) | `src/Vargshala.Contracts/`<br>`src/Vargshala.Application/Features/`<br>`src/Vargshala.API/Controllers/` |
| **Implement or modify Auth / Claims / Tenant Context** | [`05_SECURITY_RULES.md`](./05_SECURITY_RULES.md)<br>[`11_JWT_AUTHENTICATION.md`](./11_JWT_AUTHENTICATION.md) | `src/Vargshala.Infrastructure/Authentication/`<br>`src/Vargshala.API/Middlewares/` |
| **Build a new feature end-to-end** | [`07_FEATURE_DEVELOPMENT.md`](./07_FEATURE_DEVELOPMENT.md)<br>[`12_FEATURE_SPECIFICATION.md`](./12_FEATURE_SPECIFICATION.md) | Follow vertical slice: `Domain` ➔ `Contracts` ➔ `Application` ➔ `Infrastructure` ➔ `API` ➔ `Web` |
| **Write unit or integration tests** | [`08_TESTING_GUIDE.md`](./08_TESTING_GUIDE.md) | `tests/Vargshala.UnitTests/`<br>`tests/Vargshala.IntegrationTests/` |
| **Check where a file belongs** | [`02_PROJECT_STRUCTURE.md`](./02_PROJECT_STRUCTURE.md)<br>[`10_FILE_CREATION_RULES.md`](./10_FILE_CREATION_RULES.md) | Refer to file placement matrix in `10_FILE_CREATION_RULES.md` |
| **Check formatting, naming, C# idioms** | [`06_CODING_STANDARDS.md`](./06_CODING_STANDARDS.md) | All `.cs` files across solution |

---

## 2. Invariable Verification Rules

Before signaling completion on any prompt:
1. **Did you respect Tenant Isolation?** (`OrganizationId` filter applied, no client tenant trust).
2. **Did you keep controllers thin?** (Dispatches to Application via MediatR / Handler).
3. **Are DTOs placed in `Contracts`?** (No domain models leaked directly to API responses).
4. **Is validation implemented?** (FluentValidation validators created in `Application`).
5. **Are asynchronous methods using cancellation tokens?** (`cancellationToken` passed to all async EF Core calls).
