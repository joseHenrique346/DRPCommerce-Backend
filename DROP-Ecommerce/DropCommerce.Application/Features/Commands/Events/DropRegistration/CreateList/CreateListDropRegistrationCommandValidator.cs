using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropRegistrationCommandValidator : AbstractValidator<CreateListDropRegistrationCommand>
{
    public CreateListDropRegistrationCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new CreateDropRegistrationCommandValidator());
    }
}
