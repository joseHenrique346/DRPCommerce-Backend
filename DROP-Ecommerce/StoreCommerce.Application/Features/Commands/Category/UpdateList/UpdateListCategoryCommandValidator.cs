using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListCategoryCommandValidator : AbstractValidator<UpdateListCategoryCommand>
{
    public UpdateListCategoryCommandValidator()
    {
        RuleFor(category => category.commands).NotEmpty();
        RuleForEach(category => category.commands).NotNull().SetValidator(new UpdateCategoryCommandValidator());
    }
}
