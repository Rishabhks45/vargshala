using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Features.Authentication.Commands.Login;
using Vargshala.Application.Features.Authentication.Infrastructure;
using Vargshala.Application.Settings;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.UnitTests.Features.Authentication;

public class LoginCommandHandlerTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IEncryptionService> _encryptionServiceMock;
    private readonly IOptions<EncryptionSettings> _encryptionOptions;
    private readonly LoginCommandHandler _handler;
    private const string MasterKey = "aU5FU1RIQY5NUzU3Q1JFVEtFWTk4NzY1NDMyMUFCQ0RFRkdISUdLTE1OTw==";

    public LoginCommandHandlerTests()
    {
        _authRepositoryMock = new Mock<IAuthRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _encryptionServiceMock = new Mock<IEncryptionService>();
        _encryptionOptions = Options.Create(new EncryptionSettings { MasterKey = MasterKey });

        _handler = new LoginCommandHandler(
            _authRepositoryMock.Object,
            _tokenServiceMock.Object,
            _encryptionServiceMock.Object,
            _encryptionOptions);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsSuccessWithTokens()
    {
        // Arrange
        var user = CreateTestUser();
        _authRepositoryMock.Setup(r => r.GetUserByEmailWithOrgAsync("admin@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _encryptionServiceMock.Setup(es => es.Decrypt(user.PasswordHash, MasterKey))
            .Returns("ValidPassword123");
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
        _authRepositoryMock.Setup(r => r.GetUserByEmailWithOrgAsync("admin@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _encryptionServiceMock.Setup(es => es.Decrypt(user.PasswordHash, MasterKey))
            .Returns("RealPassword123");

        var command = new LoginCommand("admin@test.com", "WrongPassword");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_WhenDecryptionThrowsException_ReturnsInvalidCredentialsFailure()
    {
        // Arrange
        var user = CreateTestUser();
        _authRepositoryMock.Setup(r => r.GetUserByEmailWithOrgAsync("admin@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _encryptionServiceMock.Setup(es => es.Decrypt(user.PasswordHash, MasterKey))
            .Throws(new ArgumentException("Invalid encrypted text format"));

        var command = new LoginCommand("admin@test.com", "Password123");

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
        _authRepositoryMock.Setup(r => r.GetUserByEmailWithOrgAsync("nonexistent@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

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
        _authRepositoryMock.Setup(r => r.GetUserByEmailWithOrgAsync("admin@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

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
        PasswordHash = "encrypted-password",
        Role = UserRole.OrganizationAdmin,
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
}
