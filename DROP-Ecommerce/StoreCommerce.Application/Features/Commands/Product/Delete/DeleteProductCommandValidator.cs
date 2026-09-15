using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(product => product.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}