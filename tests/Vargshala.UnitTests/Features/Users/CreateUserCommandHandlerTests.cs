using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Users.Commands.CreateUser;
using Vargshala.Application.Features.Users.Infrastructure;
using Vargshala.Application.Settings;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.UnitTests.Features.Users;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IEncryptionService> _encryptionServiceMock;
    private readonly IOptions<EncryptionSettings> _encryptionOptions;
    private readonly CreateUserCommandHandler _handler;
    private readonly Guid _orgId = Guid.NewGuid();
    private const string MasterKey = "aU5FU1RIQY5NUzU3Q1JFVEtFWTk4NzY1NDMyMUFCQ0RFRkdISUdLTE1OTw==";

    public CreateUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _currentUserMock = new Mock<ICurrentUser>();
        _encryptionServiceMock = new Mock<IEncryptionService>();
        _encryptionOptions = Options.Create(new EncryptionSettings { MasterKey = MasterKey });

        _currentUserMock.Setup(u => u.OrganizationId).Returns(_orgId);
        _currentUserMock.Setup(u => u.UserId).Returns(Guid.NewGuid());
        _encryptionServiceMock.Setup(es => es.Encrypt(It.IsAny<string>(), MasterKey)).Returns("encrypted-password");

        _handler = new CreateUserCommandHandler(
            _userRepositoryMock.Object,
            _currentUserMock.Object,
            _encryptionServiceMock.Object,
            _encryptionOptions);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesUserInCurrentOrganization()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.ExistsByEmailAndOrgAsync("john@test.com", _orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new CreateUserCommand("John", "Doe", "john@test.com", "9876543210", "Password123", UserRole.Teacher);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FirstName.Should().Be("John");
        result.Data.Role.Should().Be(UserRole.Teacher);

        _userRepositoryMock.Verify(m => m.AddAsync(It.Is<User>(u =>
            u.OrganizationId == _orgId &&
            u.FirstName == "John" &&
            u.Role == UserRole.Teacher), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithSuperAdminRole_ReturnsFailure()
    {
        // Arrange
        var command = new CreateUserCommand("John", "Doe", "john@test.com", null, "Password123", UserRole.SuperAdmin);

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
            _userRepositoryMock.Object,
            _currentUserMock.Object,
            _encryptionServiceMock.Object,
            _encryptionOptions);

        var command = new CreateUserCommand("John", "Doe", "john@test.com", null, "Password123", UserRole.Teacher);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("organization");
    }
}
