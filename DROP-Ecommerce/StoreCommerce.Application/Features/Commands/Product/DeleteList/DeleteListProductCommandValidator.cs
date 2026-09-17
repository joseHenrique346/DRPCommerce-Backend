using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListProductCommandValidator : AbstractValidator<DeleteListProductCommand>
{
    public DeleteListProductCommandValidator()
    {
        RuleFor(product => product.ids).NotEmpty();
        RuleForEach(product => product.ids).GreaterThan(0);
    }
}
