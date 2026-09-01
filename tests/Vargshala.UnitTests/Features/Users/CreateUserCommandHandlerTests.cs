using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.Users.Commands.CreateUser;
using Vargshala.Domain.Entities;

namespace Vargshala.UnitTests.Features.Users;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IVargshalaDbContext> _dbMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly CreateUserCommandHandler _handler;
    private readonly Guid _orgId = Guid.NewGuid();

    public CreateUserCommandHandlerTests()
    {
        _dbMock = new Mock<IVargshalaDbContext>();
        _currentUserMock = new Mock<ICurrentUser>();
        _passwordHasherMock = new Mock<IPasswordHasher>();

        _currentUserMock.Setup(u => u.OrganizationId).Returns(_orgId);
        _currentUserMock.Setup(u => u.UserId).Returns(Guid.NewGuid());
        _passwordHasherMock.Setup(ph => ph.Hash(It.IsAny<string>())).Returns("hashed");

        _handler = new CreateUserCommandHandler(
            _dbMock.Object,
            _currentUserMock.Object,
            _passwordHasherMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesUserInCurrentOrganization()
    {
        // Arrange
        var users = new List<User>().AsQueryable();
        var mockSet = CreateMockDbSet(users);
        _dbMock.Setup(db => db.Users).Returns(mockSet.Object);

        var command = new CreateUserCommand("John", "Doe", "john@test.com", "9876543210", "Password123", "Teacher");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FirstName.Should().Be("John");
        result.Data.Role.Should().Be("Teacher");

        mockSet.Verify(m => m.Add(It.Is<User>(u =>
            u.OrganizationId == _orgId &&
            u.FirstName == "John" &&
            u.Role == Domain.Enums.Role.Teacher)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidRole_ReturnsFailure()
    {
        // Arrange
        var command = new CreateUserCommand("John", "Doe", "john@test.com", null, "Password123", "InvalidRole");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid role");
    }

    [Fact]
    public async Task Handle_WithSuperAdminRole_ReturnsFailure()
    {
        // Arrange
        var command = new CreateUserCommand("John", "Doe", "john@test.com", null, "Password123", "SuperAdmin");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("SuperAdmin");
    }

    [Fact]
    public async Task Handle_WithNoOrganization_ReturnsFailure()
    {
        // Arrange
        _currentUserMock.Setup(u => u.OrganizationId).Returns((Guid?)null);

        var handler = new CreateUserCommandHandler(
            _dbMock.Object,
            _currentUserMock.Object,
            _passwordHasherMock.Object);

        var command = new CreateUserCommand("John", "Doe", "john@test.com", null, "Password123", "Teacher");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("organization");
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(data.Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        return mockSet;
    }
}

// Reuse async helpers (same as LoginCommandHandlerTests)
internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;
    internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;
    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression) => new TestAsyncEnumerable<TEntity>(expression);
    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression) => new TestAsyncEnumerable<TElement>(expression);
    public object? Execute(System.Linq.Expressions.Expression expression) => _inner.Execute(expression);
    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression) => _inner.Execute<TResult>(expression);
    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider).GetMethod(nameof(IQueryProvider.Execute), genericParameterCount: 1, types: new[] { typeof(System.Linq.Expressions.Expression) })!
            .MakeGenericMethod(resultType).Invoke(this, new object[] { expression });
        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(resultType).Invoke(null, new[] { executionResult })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression) { }
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;
    public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
    public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
    public T Current => _inner.Current;
}
