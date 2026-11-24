using FluentValidation;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class DeactivateCustomerAccountCommandValidator : AbstractValidator<DeactivateCustomerAccountCommand>
{
    public DeactivateCustomerAccountCommandValidator()
    {
        RuleFor(Command => Command.Request).NotNull().SetValidator(new DeleteAccountRequestValidator());
    }
}
