# 08. Testing Guide & Quality Standards

## 1. Testing Strategy

Vargshala employs a two-tier automated testing strategy:
1. **Unit Tests (`tests/Vargshala.UnitTests`)**: Fast, in-memory execution covering domain invariants, business rules, application handlers, and validators.
2. **Integration Tests (`tests/Vargshala.IntegrationTests`)**: End-to-end API integration tests ensuring multi-tenant data isolation, security policies, and EF Core query execution.

---

## 2. Unit Testing Guidelines

### Frameworks & Libraries
- **Test Runner**: `xUnit`
- **Assertions**: `FluentAssertions`
- **Mocking**: `NSubstitute` or `Moq`
- **Data Generation**: `Bogus` (for realistic student/teacher test data)

### Testing Domain Logic Example
```csharp
[Fact]
public void CalculateOverdue_ShouldReturnCorrectBalance_WhenPaymentIsPartial()
{
    // Arrange
    var studentFee = new StudentFee
    {
        TotalAmount = 10000m,
        DueDate = DateTime.UtcNow.AddDays(-5)
    };
    studentFee.AddPayment(new FeePayment { AmountPaid = 4000m, PaymentDate = DateTime.UtcNow.AddDays(-2) });

    // Act
    var overdueAmount = studentFee.GetOverdueAmount();

    // Assert
    overdueAmount.Should().Be(6000m);
}
```

### Testing Application Handlers Example
```csharp
[Fact]
public async Task Handle_ShouldRejectStudentAdmission_WhenBatchBelongsToDifferentTenant()
{
    // Arrange
    var tenantContext = Substitute.For<ITenantContext>();
    tenantContext.OrganizationId.Returns(Guid.NewGuid()); // Tenant A

    var dbContext = CreateInMemoryDbContext();
    // Insert Batch belonging to Tenant B (different Guid)
    var command = new AdmitStudentCommand("Aarav", "9876543210", foreignBatchId);
    var handler = new AdmitStudentCommandHandler(dbContext, tenantContext);

    // Act & Assert
    var result = await handler.Handle(command, CancellationToken.None);
    result.IsSuccess.Should().BeFalse();
    result.Error.Should().Contain("Batch does not belong to your organization");
}
```

---

## 3. Integration Testing & Multi-Tenant Verification

### Key Multi-Tenancy Test Scenarios
1. **Tenant Isolation Test**: Verify that queries executed by Tenant A NEVER return records belonging to Tenant B.
2. **Cross-Tenant Mutation Test**: Verify that attempting to update or delete Tenant B's entity while authenticated as Tenant A returns `404 Not Found`.
3. **Super Admin Scope Test**: Verify that Super Admin can access organizations across tenants when operating with bypass enabled.

### Test Execution Command
```bash
dotnet test --logger "console;verbosity=detailed"
```
