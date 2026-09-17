using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListProductCommandValidator : AbstractValidator<UpdateListProductCommand>
{
    public UpdateListProductCommandValidator()
    {
        RuleFor(product => product.commands).NotEmpty();
        RuleForEach(product => product.commands).SetValidator(new UpdateProductCommandValidator());
    }
}
