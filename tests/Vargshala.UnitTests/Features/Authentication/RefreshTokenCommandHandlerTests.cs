using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Moq;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Features.Authentication.Commands.RefreshToken;
using Vargshala.Application.Features.Authentication.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.UnitTests.Features.Authentication;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly RefreshTokenCommandHandler _handler;
    private readonly Guid _userId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    public RefreshTokenCommandHandlerTests()
    {
        _authRepositoryMock = new Mock<IAuthRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _handler = new RefreshTokenCommandHandler(_authRepositoryMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenQueryDecodingTurnedPlusIntoSpace_StillMatchesStoredToken()
    {
        var user = CreateUser("abc+def/ghi=");
        var principal = CreatePrincipal(_userId);

        _tokenServiceMock.Setup(s => s.GetPrincipalFromExpiredToken("expired-access"))
            .Returns(principal);
        _authRepositoryMock.Setup(r => r.GetUserByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(s => s.GenerateAccessToken(user))
            .Returns("new-access-token");

        var command = new RefreshTokenCommand("expired-access", "abc def/ghi=");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("new-access-token");
        result.Data.RefreshToken.Should().Be("abc+def/ghi=");
        _authRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithMismatchedRefreshToken_ReturnsFailure()
    {
        var user = CreateUser("stored-token");
        var principal = CreatePrincipal(_userId);

        _tokenServiceMock.Setup(s => s.GetPrincipalFromExpiredToken("expired-access"))
            .Returns(principal);
        _authRepositoryMock.Setup(r => r.GetUserByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new RefreshTokenCommand("expired-access", "other-token");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid or expired refresh token.");
    }

    private User CreateUser(string refreshToken) => new()
    {
        Id = _userId,
        FirstName = "Admin",
        LastName = "User",
        Email = "admin@test.com",
        PasswordHash = "hash",
        Role = UserRole.OrganizationAdmin,
        IsActive = true,
        RefreshToken = refreshToken,
        RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(5)
    };

    private static ClaimsPrincipal CreatePrincipal(Guid userId)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
            authenticationType: JwtConstants.TokenType);
        return new ClaimsPrincipal(identity);
    }
}
