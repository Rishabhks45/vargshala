# 10. File Placement & Creation Rules

## 1. Strict File Placement Matrix

To maintain Clean Architecture boundaries and avoid circular dependencies, use this strict matrix:

| File Type | Allowed Project / Directory | Forbidden Locations |
| :--- | :--- | :--- |
| **Domain Entity** | `src/Vargshala.Domain/Entities/<Module>/` | `Application`, `Contracts`, `Infrastructure`, `API`, `Web` |
| **Domain Enum** | `src/Vargshala.Domain/Enums/` | `Application`, `API`, `Web` |
| **Domain Exception** | `src/Vargshala.Domain/Exceptions/` | `Application`, `API` |
| **Request / Response DTO** | `src/Vargshala.Contracts/<Module>/` | `Domain`, `Infrastructure`, `API` |
| **Shared Enum** | `src/Vargshala.Contracts/Enums/` | `API`, `Web` |
| **Command / Query Record** | `src/Vargshala.Application/Features/<Module>/Commands/` or `Queries/` | `Domain`, `Contracts`, `API`, `Infrastructure` |
| **FluentValidation Validator**| `src/Vargshala.Application/Features/<Module>/Validators/` | `Domain`, `Contracts`, `API` |
| **EF Core Entity Configuration**| `src/Vargshala.Infrastructure/Persistence/Configurations/` | `Domain`, `Application`, `API` |
| **EF Core Migration** | `src/Vargshala.Infrastructure/Persistence/Migrations/` | Any other directory |
| **API Controller** | `src/Vargshala.API/Controllers/` | `Domain`, `Application`, `Contracts`, `Infrastructure` |
| **Middleware** | `src/Vargshala.API/Middlewares/` | `Domain`, `Application`, `Contracts` |
| **UI Razor Component / Page** | `src/Vargshala.Web/Pages/` or `Components/` | `API`, `Application`, `Domain`, `Infrastructure` |

---

## 2. File Organization Rules

1. **One Class / Record Per File**: Every class, interface, enum, and record must reside in its own dedicated `.cs` file matching the type name.
2. **File-Scoped Namespaces**: Always use C# file-scoped namespaces (`namespace Vargshala.Domain.Entities.Students;`).
3. **Namespace Mirrors Folder Path**: The namespace of a file must exactly match its folder path relative to the project root.
4. **No Inline Logic in Controllers**: Controllers must not contain SQL queries, EF Core calls, business calculations, or validation algorithms.
5. **No Direct EF Core in Web UI**: Web/Client code must only communicate with the backend via HTTP API endpoints or SignalR hubs using `Contracts`.
