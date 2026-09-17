using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListCategoryCommandValidator : AbstractValidator<UpdateListCategoryCommand>
{
    public UpdateListCategoryCommandValidator()
    {
        RuleFor(category => category.commands).NotEmpty();
        RuleForEach(category => category.commands).SetValidator(new UpdateCategoryCommandValidator());
    }
}
