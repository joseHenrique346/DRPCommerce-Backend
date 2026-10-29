using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListCategoryCommandValidator : AbstractValidator<DeleteListCategoryCommand>
{
    public DeleteListCategoryCommandValidator()
    {
        RuleFor(category => category.ids).NotEmpty();
        RuleForEach(category => category.ids).GreaterThan(0);

        RuleFor(category => category.moveProductsToCategoryId)
            .GreaterThan(0).When(category => category.moveProductsToCategoryId.HasValue);
    }
}
