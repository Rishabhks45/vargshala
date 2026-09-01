using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.Authentication.Commands.Login;
using Vargshala.Domain.Entities;
using Vargshala.Domain.Enums;

namespace Vargshala.UnitTests.Features.Authentication;

public class LoginCommandHandlerTests
{
    private readonly Mock<IVargshalaDbContext> _dbMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _dbMock = new Mock<IVargshalaDbContext>();
        _tokenServiceMock = new Mock<ITokenService>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _handler = new LoginCommandHandler(
            _dbMock.Object,
            _tokenServiceMock.Object,
            _passwordHasherMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsSuccessWithTokens()
    {
        // Arrange
        var user = CreateTestUser();
        var users = new List<User> { user }.AsQueryable();

        var mockSet = CreateMockDbSet(users);
        _dbMock.Setup(db => db.Users).Returns(mockSet.Object);

        _passwordHasherMock.Setup(ph => ph.Verify("ValidPassword123", user.PasswordHash))
            .Returns(true);
        _tokenServiceMock.Setup(ts => ts.GenerateAccessToken(It.IsAny<User>()))
            .Returns("test-access-token");
        _tokenServiceMock.Setup(ts => ts.GenerateRefreshToken())
            .Returns("test-refresh-token");

        var command = new LoginCommand("admin@test.com", "ValidPassword123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("test-access-token");
        result.Data.RefreshToken.Should().Be("test-refresh-token");
        result.Data.User.Email.Should().Be("admin@test.com");
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ReturnsFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var users = new List<User> { user }.AsQueryable();

        var mockSet = CreateMockDbSet(users);
        _dbMock.Setup(db => db.Users).Returns(mockSet.Object);

        _passwordHasherMock.Setup(ph => ph.Verify("WrongPassword", user.PasswordHash))
            .Returns(false);

        var command = new LoginCommand("admin@test.com", "WrongPassword");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ReturnsFailure()
    {
        // Arrange
        var users = new List<User>().AsQueryable();
        var mockSet = CreateMockDbSet(users);
        _dbMock.Setup(db => db.Users).Returns(mockSet.Object);

        var command = new LoginCommand("nonexistent@test.com", "AnyPassword");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ReturnsFailure()
    {
        // Arrange
        var user = CreateTestUser();
        user.IsActive = false;
        var users = new List<User> { user }.AsQueryable();

        var mockSet = CreateMockDbSet(users);
        _dbMock.Setup(db => db.Users).Returns(mockSet.Object);

        var command = new LoginCommand("admin@test.com", "ValidPassword123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("deactivated");
    }

    private static User CreateTestUser() => new()
    {
        Id = Guid.NewGuid(),
        OrganizationId = Guid.NewGuid(),
        FirstName = "Admin",
        LastName = "User",
        Email = "admin@test.com",
        PasswordHash = "hashed-password",
        Role = Role.OrganizationAdmin,
        IsActive = true,
        IsDeleted = false,
        CreatedAt = DateTime.UtcNow,
        Organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Test Org",
            Code = "TEST",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }
    };

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

// Async query provider helpers for mocking EF Core
internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        => new TestAsyncEnumerable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        => new TestAsyncEnumerable<TElement>(expression);

    public object? Execute(System.Linq.Expressions.Expression expression)
        => _inner.Execute(expression);

    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression,
        CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: new[] { typeof(System.Linq.Expressions.Expression) })!
            .MakeGenericMethod(resultType)
            .Invoke(this, new object[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
    public T Current => _inner.Current;
}
