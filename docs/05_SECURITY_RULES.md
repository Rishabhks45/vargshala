# 05. Security & Multi-Tenant Isolation Rules

## 1. Multi-Tenant Isolation Enforcement

1. **Zero Client Trust for Tenant Context**: Never accept `OrganizationId` from request bodies, query strings, or route parameters to authorize tenant actions.
2. **Context Resolution**: The tenant ID must ALWAYS be resolved server-side from the authenticated user's JWT claims (`ClaimsPrincipal`) via `ITenantContext` / `ICurrentUserService`.
3. **Repository Enforced Scoping**: All database queries must automatically include the tenant filter (`OrganizationId == currentTenantId`).
4. **404 Instead of 403 on Multi-Tenant Entity Lookups**: If User A in Organization 1 requests a resource ID belonging to Organization 2, return `404 Not Found` rather than `403 Forbidden` to prevent tenant ID enumeration attacks.

---

## 2. Authentication & Authorization Policies

- **Authentication Scheme**: ASP.NET Core Identity with Bearer JWT tokens.
- **Role Hierarchy**:
  - `SuperAdmin`: System-level administrative privileges.
  - `OrgAdmin`: Full control within own organization workspace.
  - `Teacher`: Scoped to assigned batches, subjects, and students.
  - `Student`: Scoped strictly to enrolled batches and personal records.
- **Policy-Based Authorization**: Endpoints must enforce role and permission policies:
  ```csharp
  [Authorize(Roles = "OrgAdmin,Teacher")]
  [HttpPost("batches/{batchId}/notes")]
  public async Task<ActionResult<ApiResponse<StudyMaterialDto>>> UploadNote(...)
  ```
- **Teacher Batch Authorization Filter**: Ensure teachers can only access batches assigned to them in `TeacherBatchSubject` relations.

---

## 3. Input Validation & Data Sanitization

- **FluentValidation**: Every Command and Request DTO must have an associated FluentValidation validator in `src/Vargshala.Application`.
- **Validation Pipeline**: Pipeline behavior must run before the handler. If validation fails, return standard `400 Bad Request` with list of validation errors.
- **XSS & HTML Sanitization**: Any rich-text or user-submitted descriptions (homework notes, announcements) must be HTML-encoded or sanitized before storage.

---

## 4. File Upload & Storage Security

- **Allowed File Extensions**:
  - Documents: `.pdf`, `.docx`, `.doc`, `.txt`
  - Images: `.jpg`, `.jpeg`, `.png`, `.webp`
  - Explicitly forbid executable files (`.exe`, `.dll`, `.sh`, `.bat`, `.js`, `.php`).
- **File Size Limits**:
  - Documents/Notes: Max 25 MB per file.
  - Profile Photos / Post-Quiz Images: Max 5 MB per image.
  - Chat Attachments: Max 10 MB per file.
- **MIME Type & Magic Number Verification**: Inspect binary magic numbers / headers, do not rely purely on filename extension.
- **Isolated Storage Paths**: Store files on disk/cloud using UUID filenames and organization-partitioned folder structures:
  `/{organizationId}/materials/{fileGuid}.pdf`. Never use user-supplied filenames directly on disk.

---

## 5. Rate Limiting & Tamper Prevention

- **Rate Limiting**: Apply ASP.NET Core rate limiting middleware on `/api/v1/auth/login` (e.g. 5 attempts per minute per IP) to prevent brute-force attacks.
- **Anti-Cheating Quiz Attempt Validation**:
  - Enforce server-side quiz expiration timestamps. Late submissions exceeding grace period must be rejected.
  - Log tab switch counts, fullscreen exit events, and IP addresses per quiz attempt.
