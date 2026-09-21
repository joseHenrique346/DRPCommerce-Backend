using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListCategoryCommandValidator : AbstractValidator<CreateListCategoryCommand>
{
    public CreateListCategoryCommandValidator()
    {
        RuleFor(category => category.commands).NotEmpty();
        RuleForEach(category => category.commands).SetValidator(new CreateCategoryCommandValidator());
    }
}
