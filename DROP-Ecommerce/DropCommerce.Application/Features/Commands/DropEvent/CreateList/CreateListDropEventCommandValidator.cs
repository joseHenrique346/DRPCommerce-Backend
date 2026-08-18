using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropEventCommandValidator : AbstractValidator<CreateListDropEventCommand>
{
    public CreateListDropEventCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new CreateDropEventCommandValidator());
    }
}
