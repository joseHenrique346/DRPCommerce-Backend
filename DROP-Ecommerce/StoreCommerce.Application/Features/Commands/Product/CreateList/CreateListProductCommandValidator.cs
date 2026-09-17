using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListProductCommandValidator : AbstractValidator<CreateListProductCommand>
{
    public CreateListProductCommandValidator()
    {
        RuleFor(product => product.commands).NotEmpty();
        RuleForEach(product => product.commands).SetValidator(new CreateProductCommandValidator());
    }
}
