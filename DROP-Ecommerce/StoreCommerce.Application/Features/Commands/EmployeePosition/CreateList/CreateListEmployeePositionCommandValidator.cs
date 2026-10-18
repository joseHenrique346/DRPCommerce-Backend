using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListEmployeePositionCommandValidator : AbstractValidator<CreateListEmployeePositionCommand>
{
    public CreateListEmployeePositionCommandValidator()
    {
        RuleFor(command => command.commands).NotEmpty();
        RuleForEach(command => command.commands).SetValidator(new CreateEmployeePositionCommandValidator());
    }
}
