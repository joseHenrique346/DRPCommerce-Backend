using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropOrderCommandValidator : AbstractValidator<CreateListDropOrderCommand>
{
    public CreateListDropOrderCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new CreateDropOrderCommandValidator());
    }
}
