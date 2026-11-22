using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListFraudSignalCommandValidator : AbstractValidator<UpdateListFraudSignalCommand>
{
    public UpdateListFraudSignalCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new UpdateFraudSignalCommandValidator());
    }
}
