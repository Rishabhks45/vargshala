# 11. JWT Authentication & Tenant Context Flow

## 1. JWT Claims Structure

The JSON Web Token (JWT) issued on successful authentication carries identity, roles, and tenant context:

```json
{
  "sub": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "owner@vidyamandir.com",
  "name": "Rajesh Sharma",
  "role": "OrgAdmin",
  "org_id": "8a7b9c1d-2e3f-4a5b-6c7d-8e9f0a1b2c3d",
  "session_id": "b1c2d3e4-f5a6-7b8c-9d0e-1f2a3b4c5d6e",
  "exp": 1793740800,
  "iss": "VargshalaAPI",
  "aud": "VargshalaClient"
}
```

### Claim Definitions
- `sub`: `ApplicationUser.Id` (UUID).
- `role`: Role string (`SuperAdmin`, `OrgAdmin`, `Teacher`, `Student`).
- `org_id`: `OrganizationId` (UUID) identifying the active tenant workspace (Empty/Null for SuperAdmin).
- `session_id`: Active academic session ID.

---

## 2. Authentication & Tenant Resolution Middleware Flow

```
[ Incoming HTTP Request with Header: Authorization: Bearer <token> ]
                          │
                          ▼
            [ Authentication Middleware ]
   - Validates Token Signature, Issuer, Audience, Expiry
   - Sets HttpContext.User (ClaimsPrincipal)
                          │
                          ▼
            [ TenantResolutionMiddleware ]
   - Extracts "org_id" Claim from HttpContext.User
   - Validates Organization exists and isActive == true
   - Injects ITenantContext into Scoped DI Container
                          │
                          ▼
             [ Controller / API Handler ]
   - Operates within validated ITenantContext
                          │
                          ▼
          [ EF Core ApplicationDbContext ]
   - Automatically filters all queries by ITenantContext.OrganizationId
```

---

## 3. Token Expiration & Refresh Token Flow

1. **Access Token Lifetime**: Short-lived (e.g. 60 minutes) to minimize impact of compromised tokens.
2. **Refresh Token Lifetime**: Long-lived (e.g. 30 days), stored hashed in `RefreshTokens` table.
3. **Revocation & Invalidation**:
   - Refresh token is revoked immediately upon logout or password reset.
   - When an organization is deactivated by SuperAdmin, all active refresh tokens for that tenant are invalidated immediately.

---

## 4. `ITenantContext` and `ICurrentUserService` Interfaces

```csharp
public interface ITenantContext
{
    Guid OrganizationId { get; }
    bool HasTenant { get; }
    bool IsSuperAdminBypassed { get; }
    void SetTenant(Guid organizationId);
    void EnableSuperAdminBypass();
}

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    Guid? OrganizationId { get; }
    bool IsAuthenticated { get; }
}
```
