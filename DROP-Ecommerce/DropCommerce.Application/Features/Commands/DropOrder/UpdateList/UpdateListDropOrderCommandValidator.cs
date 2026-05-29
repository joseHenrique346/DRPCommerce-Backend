using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropOrderCommandValidator : AbstractValidator<UpdateListDropOrderCommand>
{
    public UpdateListDropOrderCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new UpdateDropOrderCommandValidator());
    }
}
