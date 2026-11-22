using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListSupplierCommandValidator : AbstractValidator<CreateListSupplierCommand>
{
    public CreateListSupplierCommandValidator()
    {
        RuleFor(supplier => supplier.commands).NotEmpty();
        RuleForEach(supplier => supplier.commands).SetValidator(new CreateSupplierCommandValidator());
    }
}
