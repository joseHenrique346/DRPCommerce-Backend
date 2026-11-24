using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropRegistrationCommandValidator : AbstractValidator<DeleteListDropRegistrationCommand>
{
    public DeleteListDropRegistrationCommandValidator()
    {
        RuleFor(x => x.ids).NotEmpty();
        RuleForEach(x => x.ids).GreaterThan(0);
    }
}
