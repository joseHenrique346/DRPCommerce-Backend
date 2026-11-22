using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropEventCommandValidator : AbstractValidator<UpdateListDropEventCommand>
{
    public UpdateListDropEventCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new UpdateDropEventCommandValidator());
    }
}
