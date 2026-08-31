using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropRegistrationCommandValidator : AbstractValidator<UpdateListDropRegistrationCommand>
{
    public UpdateListDropRegistrationCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new UpdateDropRegistrationCommandValidator());
    }
}
