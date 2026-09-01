# 06. C# & .NET Coding Standards

## 1. General Language & Runtime Conventions

- **Target Runtime**: .NET 8 / .NET 9 LTS.
- **Language Version**: C# 12+ (use primary constructors, collection expressions, pattern matching, file-scoped namespaces).
- **Nullability**: Nullable reference types (`<Nullable>enable</Nullable>`) are strictly enforced. Avoid null-forgiving operator (`!`) unless guaranteed by design.

---

## 2. Naming Conventions

| Identifier | Convention | Example |
| :--- | :--- | :--- |
| **Classes / Structs / Records** | PascalCase | `StudentProfile`, `AdmitStudentCommand` |
| **Interfaces** | IPascalCase | `IApplicationDbContext`, `ITenantContext` |
| **Methods / Properties** | PascalCase | `GetBatchesAsync()`, `OrganizationId` |
| **Private Fields** | _camelCase | `_dbContext`, `_tenantContext` |
| **Method Parameters / Local Variables** | camelCase | `studentId`, `batchList` |
| **Constants / Enums** | PascalCase | `MaxAttachmentSizeBytes`, `PaymentMode.Upi` |
| **Async Methods** | Suffix with `Async` | `CreateBatchAsync()`, `SaveChangesAsync()` |

---

## 3. Immutability & Modern C# Features

- Prefer `record` or `readonly record struct` for DTOs, Commands, and Queries in `Vargshala.Contracts` and `Vargshala.Application`:
  ```csharp
  public record CreateBatchCommand(
      string Name,
      string ClassGrade,
      Guid AcademicSessionId,
      Guid? SubjectId
  ) : IRequest<Result<Guid>>;
  ```
- Use primary constructors for dependency injection:
  ```csharp
  public class BatchesController(ISender mediator) : BaseApiController
  {
      [HttpPost]
      public async Task<ActionResult<ApiResponse<Guid>>> Create(CreateBatchRequest request)
      {
          var result = await mediator.Send(new CreateBatchCommand(request.Name, request.ClassGrade, request.AcademicSessionId, request.SubjectId));
          return HandleResult(result);
      }
  }
  ```

---

## 4. Async / Await & Resource Management

1. **Always pass `CancellationToken`** across all asynchronous repository calls, handlers, and external API requests:
   ```csharp
   public async Task<List<BatchDto>> Handle(GetBatchesQuery request, CancellationToken cancellationToken)
   {
       return await _dbContext.Batches
           .AsNoTracking()
           .Where(b => b.AcademicSessionId == request.SessionId)
           .ProjectToDto()
           .ToListAsync(cancellationToken);
   }
   ```
2. **Never use `.Result` or `.Wait()`**: Blocking on asynchronous tasks causes thread pool starvation and deadlocks. Always `await`.
3. **Use `AsNoTracking()` for Read Queries**: For read-only queries, always append `.AsNoTracking()` to reduce EF Core change-tracker overhead.

---

## 5. Error Handling & Result Pattern

- Avoid throwing exceptions for predictable domain validation or business logic failures. Use the `Result<T>` pattern:
  ```csharp
  public class Result<T>
  {
      public bool IsSuccess { get; }
      public T? Value { get; }
      public string? Error { get; }

      public static Result<T> Success(T value) => new(true, value, null);
      public static Result<T> Failure(string error) => new(false, default, error);
  }
  ```
- Throw specific domain exceptions (`DomainException`, `EntityNotFoundException`) only when unexpected invariants are broken.
