using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteSupplierCommandValidator : AbstractValidator<DeleteSupplierCommand>
{
    public DeleteSupplierCommandValidator()
    {
        RuleFor(supplier => supplier.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}