using FluentValidation;
using MediatR;

namespace AuthService.Application.UseCases.Users.Commands.Login;
public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;
public record LoginResponse(string AccessToken, string RefreshToken);
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Um e-mail válido é obrigatório.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.");
    }
}