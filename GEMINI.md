# Antigravity & Gemini AI Rules — Vargshala

## MANDATORY PROJECT RULES & CONSTRAINTS

1. **Architecture**: Clean Architecture (.NET 10 + EF Core + MediatR + FluentValidation + Blazor).
2. **Multi-Tenancy**: `OrganizationId` tenant filter on every tenant entity/query. Never trust client tenant ID.
3. **Form Validation**: Always use **FluentValidation** (`AbstractValidator<T>`) + `<FluentValidationValidator />` + `CascadeMode.Stop`.
4. **UI Theme**: Theme 4 Deep Teal (`#004D40`, `#009488`, `#00796b`).
5. **Components**: Always use `<CustomSelect>`, `<EditForm>`, `<InputText>`, `<InputNumber>`, `<ValidationMessage>`, and `TableSortState`.
6. **No DB Access in Web/API**: Database queries strictly reside in `Infrastructure` & `Application`.
7. **Server-Side Data Querying Flow**: Mandatory for all lists: `IQueryable` ➔ `Search` (ILike) ➔ `Filters` ➔ `Sorting` (whitelisted dictionary) ➔ `CountAsync()` ➔ `Skip()` ➔ `Take()` ➔ `ToListAsync()`. Use `PagedRequest`, `PagedResponse<T>`, and `QueryableExtensions.ToPagedResultAsync()`. Never load full tables into memory.
8. **Gold Standard Reference Implementation**: Strict adherence to [`src/Vargshala.Web/Components/Pages/ControlPanel/Users.razor`](file:///d:/vargshala.com/src/Vargshala.Web/Components/Pages/ControlPanel/Users.razor) for all UI pages: stat cards render directly without `@(_isLoading ? "..." : ...)`; table headers never blink (`table-fixed`, explicit percentage widths on `<th>`, `<TableSkeletonRows>` rendered only on initial load `@if (_isLoading && !_items.Any())`, rows preserved with smooth opacity transition during sort/filter updates); `HandleSortAsync` & `PagedRequest` standard; and [`src/Vargshala.Application/Features/Users`](file:///d:/vargshala.com/src/Vargshala.Application/Features/Users) for all backend features (Commands, Infrastructure/I{Feature}Repository, Queries, Infrastructure repository implementation, scoped DI).
