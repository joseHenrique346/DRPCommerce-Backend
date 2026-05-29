using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropProductCommandValidator : AbstractValidator<UpdateListDropProductCommand>
{
    public UpdateListDropProductCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new UpdateDropProductCommandValidator());
    }
}
