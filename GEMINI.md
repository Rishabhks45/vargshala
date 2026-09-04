# Antigravity & Gemini AI Rules — Vargshala

## MANDATORY PROJECT RULES & CONSTRAINTS

1. **Architecture**: Clean Architecture (.NET 10 + EF Core + MediatR + FluentValidation + Blazor).
2. **Multi-Tenancy**: `OrganizationId` tenant filter on every tenant entity/query. Never trust client tenant ID.
3. **Form Validation**: Always use **FluentValidation** (`AbstractValidator<T>`) + `<FluentValidationValidator />` + `CascadeMode.Stop`.
4. **UI Theme**: Theme 4 Deep Teal (`#004D40`, `#009488`, `#00796b`).
5. **Components**: Always use `<CustomSelect>`, `<EditForm>`, `<InputText>`, `<InputNumber>`, `<ValidationMessage>`, and `TableSortState`.
6. **No DB Access in Web/API**: Database queries strictly reside in `Infrastructure` & `Application`.
7. **Server-Side Data Querying Flow**: Mandatory for all lists: `IQueryable` ➔ `Search` (ILike) ➔ `Filters` ➔ `Sorting` (whitelisted dictionary) ➔ `CountAsync()` ➔ `Skip()` ➔ `Take()` ➔ `ToListAsync()`. Use `PagedRequest`, `PagedResponse<T>`, and `QueryableExtensions.ToPagedResultAsync()`. Never load full tables into memory.
