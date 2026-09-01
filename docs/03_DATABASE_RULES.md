# 03. Database & EF Core Rules

## 1. Database Model & Multi-Tenancy Strategy

Vargshala uses a **Shared Database, Shared Schema** architecture on PostgreSQL with **`OrganizationId` Column-Based Tenant Isolation**.

### 1.1. Core Base Entities
All persistent entities must inherit from one of these base classes:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAtUtc { get; set; }
}

public abstract class BaseTenantEntity : BaseEntity, IMustHaveTenant
{
    public Guid OrganizationId { get; set; }
}
```

---

## 2. Global Query Filters for Multi-Tenancy

Every entity implementing `IMustHaveTenant` must have EF Core Global Query Filters applied in `ApplicationDbContext.OnModelCreating`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Apply global tenant filter to all IMustHaveTenant entities
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (typeof(IMustHaveTenant).IsAssignableFrom(entityType.ClrType))
        {
            var method = SetTenantFilterMethod.MakeGenericMethod(entityType.ClrType);
            method.Invoke(this, new object[] { modelBuilder });
        }
    }
}

private void SetTenantFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class, IMustHaveTenant
{
    modelBuilder.Entity<TEntity>().HasQueryFilter(e => 
        !e.IsDeleted && 
        (_tenantContext.IsSuperAdminBypassed || e.OrganizationId == _tenantContext.OrganizationId));
}
```

---

## 3. Database Invariants & Rules

1. **Composite Unique Indexes**: Any unique constraint on tenant data (such as `Student.RollNumber` or `Batch.BatchCode`) must be a composite index with `OrganizationId`:
   ```csharp
   builder.Entity<StudentProfile>()
          .HasIndex(s => new { s.OrganizationId, s.AdmissionNumber })
          .IsUnique();
   ```
2. **Soft Deletes**: Deletion of student records, teachers, batches, or study materials must set `IsDeleted = true` and `DeletedAtUtc = DateTime.UtcNow`. Hard deletes are forbidden for core entities.
3. **Audit Trail Automation**: Use an EF Core SaveChanges interceptor (`AuditableEntityInterceptor`) to automatically stamp `CreatedAtUtc`, `CreatedBy`, `LastModifiedAtUtc`, and `LastModifiedBy` from `ICurrentUserService`.
4. **Foreign Key Integrity**: Always define explicit cascade behaviors. Prefer `DeleteBehavior.Restrict` or `DeleteBehavior.NoAction` to prevent unintended cascade data loss across tenant relations.
5. **Money & Decimals**: Fee amounts and payment records must always use `decimal(18,2)`. Floating point types (`float`, `double`) are strictly prohibited for financial fields.
6. **Timezone Handling**: All timestamps stored in the database must be in UTC (`timestamptz` in PostgreSQL). Client timezones are applied in presentation layers.

---

## 4. Migrations & Seed Data

- Migrations live exclusively in `src/Vargshala.Infrastructure/Persistence/Migrations`.
- Seed data for default Super Admin accounts and default organization configurations are seeded through idempotent migration seeders (`DbInitializer.cs`).
- Never perform schema modifications outside EF Core migrations.
