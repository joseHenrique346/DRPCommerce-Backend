using FluentValidation;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class LoginCustomerCommandValidator : AbstractValidator<LoginCustomerCommand>
{
    public LoginCustomerCommandValidator()
    {
        RuleFor(Command => Command.Request)
            .NotNull()
            .WithErrorCode("Validation.InvalidInput")
            .WithMessage("Os dados informados são inválidos.");

        When(Command => Command.Request is not null, () =>
        {
            RuleFor(Command => Command.Request.Email)
                .Must(Email => !string.IsNullOrWhiteSpace(Email) && Email.Length <= 255)
                .WithErrorCode("Identity.InvalidEmail")
                .WithMessage("E-mail inválido.");
        });
    }
}
