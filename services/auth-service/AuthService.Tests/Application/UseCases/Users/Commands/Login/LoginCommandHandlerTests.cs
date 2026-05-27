using NSubstitute;
using Xunit;
using AuthService.Application.Interfaces;
using AuthService.Application.UseCases.Users.Commands.Login;
using AuthService.Domain.Entities;

namespace AuthService.Tests.Application.UseCases.Users.Commands.Login;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly ITokenService _tokenServiceMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _tokenServiceMock = Substitute.For<ITokenService>();
        
        _handler = new LoginCommandHandler(_userRepositoryMock, _tokenServiceMock);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange (Configuração do cenário de teste)
        var command = new LoginCommand("test@fiap.com.br", "password123");
        var user = new User(command.Email, "password123", "User");
        
        _userRepositoryMock.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(user);
            
        _tokenServiceMock.GenerateAccessToken(user).Returns("mocked-access-token");
        _tokenServiceMock.GenerateRefreshToken().Returns("mocked-refresh-token");

        // Act (Execução do método sob teste)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Validações dos resultados esperados)
        Assert.NotNull(result);
        Assert.Equal("mocked-access-token", result.AccessToken);
        Assert.Equal("mocked-refresh-token", result.RefreshToken);
        
        await _userRepositoryMock.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var command = new LoginCommand("wrong@fiap.com.br", "password123");
        _userRepositoryMock.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }
}