using FluentValidation;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(Command => Command.Request)
            .NotNull()
            .WithErrorCode("Validation.InvalidInput")
            .WithMessage("Os dados informados são inválidos.");

        When(Command => Command.Request is not null, () =>
        {
            RuleFor(Command => Command.Request.NewPassword)
                .Must(AccountPasswordPolicy.IsValid)
                .WithErrorCode("Identity.PasswordPolicy")
                .WithMessage("A senha deve ter entre 12 e 128 caracteres.");
        });
    }
}
