using MediatR;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;

namespace AuthService.Application.UseCases.Users.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        
        if (user == null || user.PasswordHash != request.Password) 
        {
            throw new UnauthorizedAccessException("Credenciais inválidas.");
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenString = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken(refreshTokenString, TimeSpan.FromDays(7), user.Id);
        user.RefreshTokens.Add(refreshToken);

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new LoginResponse(accessToken, refreshTokenString);
    }
}