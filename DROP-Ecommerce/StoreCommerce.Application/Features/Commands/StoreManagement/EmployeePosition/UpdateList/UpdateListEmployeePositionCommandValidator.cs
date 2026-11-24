using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListEmployeePositionCommandValidator : AbstractValidator<UpdateListEmployeePositionCommand>
{
    public UpdateListEmployeePositionCommandValidator()
    {
        RuleFor(command => command.commands).NotEmpty();
        RuleForEach(command => command.commands).SetValidator(new UpdateEmployeePositionCommandValidator());
    }
}
