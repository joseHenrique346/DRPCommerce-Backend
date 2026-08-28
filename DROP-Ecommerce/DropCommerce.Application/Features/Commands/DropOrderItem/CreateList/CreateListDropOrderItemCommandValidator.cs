using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropOrderItemCommandValidator : AbstractValidator<CreateListDropOrderItemCommand>
{
    public CreateListDropOrderItemCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new CreateDropOrderItemCommandValidator());
    }
}
