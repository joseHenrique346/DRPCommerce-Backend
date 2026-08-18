using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteFraudSignalCommandValidator : AbstractValidator<DeleteFraudSignalCommand>
{
    public DeleteFraudSignalCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0);
    }
}