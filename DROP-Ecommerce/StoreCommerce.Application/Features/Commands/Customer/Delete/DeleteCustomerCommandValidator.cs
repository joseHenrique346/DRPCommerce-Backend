using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
    public DeleteCustomerCommandValidator()
    {
        RuleFor(c => c.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}