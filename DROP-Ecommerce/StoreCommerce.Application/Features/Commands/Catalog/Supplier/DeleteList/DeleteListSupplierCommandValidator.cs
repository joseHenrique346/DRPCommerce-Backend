using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListSupplierCommandValidator : AbstractValidator<DeleteListSupplierCommand>
{
    public DeleteListSupplierCommandValidator()
    {
        RuleFor(supplier => supplier.ids).NotEmpty();
        RuleForEach(supplier => supplier.ids).GreaterThan(0);
    }
}
