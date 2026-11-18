using FluentValidation;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class DeleteAccountRequestValidator : AbstractValidator<DeleteAccountRequest>
{
    public DeleteAccountRequestValidator()
    {
        RuleFor(Request => Request.CurrentPassword).NotEmpty().MaximumLength(128);
    }
}

public sealed class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserCommandValidator()
    {
        RuleFor(Command => Command.Request).NotNull().SetValidator(new DeleteAccountRequestValidator());
    }
}

public sealed class DeactivateCustomerAccountCommandValidator : AbstractValidator<DeactivateCustomerAccountCommand>
{
    public DeactivateCustomerAccountCommandValidator()
    {
        RuleFor(Command => Command.Request).NotNull().SetValidator(new DeleteAccountRequestValidator());
    }
}
