using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(category => category.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}