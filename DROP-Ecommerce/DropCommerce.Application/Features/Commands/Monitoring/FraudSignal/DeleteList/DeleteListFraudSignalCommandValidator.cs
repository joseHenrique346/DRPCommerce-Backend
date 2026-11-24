using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListFraudSignalCommandValidator : AbstractValidator<DeleteListFraudSignalCommand>
{
    public DeleteListFraudSignalCommandValidator()
    {
        RuleFor(x => x.ids).NotEmpty();
        RuleForEach(x => x.ids).GreaterThan(0);
    }
}
