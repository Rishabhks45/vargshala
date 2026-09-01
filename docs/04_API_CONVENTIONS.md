# 04. API Conventions & Contract Standards

## 1. RESTful Routing Principles

- All API endpoints must start with `/api/v1/`.
- Plural nouns for resource collections: `/api/v1/students`, `/api/v1/batches`, `/api/v1/fees`.
- Nested resources for strictly dependent entities: `/api/v1/batches/{batchId}/tasks`.
- Standard HTTP Verbs:
  - `GET`: Retrieve a resource or collection.
  - `POST`: Create a new resource or execute a non-idempotent action.
  - `PUT`: Complete update of an existing resource.
  - `PATCH`: Partial update of a resource.
  - `DELETE`: Soft delete a resource.

---

## 2. Standard Response Envelope (`ApiResponse<T>`)

Every API endpoint must return the standardized `ApiResponse<T>` envelope defined in `src/Vargshala.Contracts/Common/ApiResponse.cs`:

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string error, List<string>? errors = null) =>
        new() { Success = false, Message = error, Errors = errors ?? new List<string> { error } };
}
```

### Example Success Response:
```json
{
  "success": true,
  "message": "Student admitted successfully",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fullName": "Aarav Sharma",
    "rollNumber": "10-A-01",
    "batchName": "Class 10 - Batch A"
  },
  "errors": null,
  "timestamp": "2026-09-02T01:00:00Z"
}
```

### Example Error Response:
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    "Mobile number is invalid.",
    "BatchId does not exist in your organization."
  ],
  "timestamp": "2026-09-02T01:00:00Z"
}
```

---

## 3. Pagination Standards (`PagedResponse<T>`)

For list endpoints, use standard query parameters `pageNumber` (default: 1) and `pageSize` (default: 20, max: 100):

```csharp
public class PagedResponse<T> : ApiResponse<IReadOnlyList<T>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}
```

---

## 4. HTTP Status Code Mapping

- `200 OK`: Successful read or update.
- `201 Created`: Successful resource creation (include `Location` header or resource id).
- `400 Bad Request`: Validation failure or business rule violation.
- `401 Unauthorized`: Missing or invalid JWT token.
- `403 Forbidden`: Authenticated user lacks permission or is attempting cross-tenant access.
- `404 Not Found`: Resource does not exist or does not belong to the user's organization.
- `500 Internal Server Error`: Unhandled server exception (sanitized in production).
