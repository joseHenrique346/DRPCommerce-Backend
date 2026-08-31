using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropProductCommandValidator : AbstractValidator<CreateListDropProductCommand>
{
    public CreateListDropProductCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new CreateDropProductCommandValidator());
    }
}
