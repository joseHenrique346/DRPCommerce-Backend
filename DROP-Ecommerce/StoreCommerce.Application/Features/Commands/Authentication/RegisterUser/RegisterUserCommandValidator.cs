using FluentValidation;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(Command => Command.Request)
            .NotNull()
            .WithErrorCode("Validation.InvalidInput")
            .WithMessage("Os dados informados são inválidos.");

        When(Command => Command.Request is not null, () =>
        {
            RuleFor(Command => Command.Request.Password)
                .Must(AccountPasswordPolicy.IsValid)
                .WithErrorCode("Identity.PasswordPolicy")
                .WithMessage("A senha deve ter entre 12 e 128 caracteres.");

            RuleFor(Command => Command.Request.Email)
                .Must(Email => !string.IsNullOrWhiteSpace(Email) && Email.Length <= 255)
                .WithErrorCode("Identity.InvalidEmail")
                .WithMessage("E-mail inválido.");
        });
    }
}
