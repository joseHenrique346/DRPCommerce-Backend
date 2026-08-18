using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteDropRegistrationCommandValidator : AbstractValidator<DeleteDropRegistrationCommand>
{
    public DeleteDropRegistrationCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0);
    }
}