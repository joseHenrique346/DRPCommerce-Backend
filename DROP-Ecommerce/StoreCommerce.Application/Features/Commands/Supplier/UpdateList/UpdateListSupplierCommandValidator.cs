using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListSupplierCommandValidator : AbstractValidator<UpdateListSupplierCommand>
{
    public UpdateListSupplierCommandValidator()
    {
        RuleFor(supplier => supplier.commands).NotEmpty();
        RuleForEach(supplier => supplier.commands).SetValidator(new UpdateSupplierCommandValidator());
    }
}
