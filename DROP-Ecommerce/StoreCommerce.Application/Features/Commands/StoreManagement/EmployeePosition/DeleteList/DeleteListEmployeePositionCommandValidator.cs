using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListEmployeePositionCommandValidator : AbstractValidator<DeleteListEmployeePositionCommand>
{
    public DeleteListEmployeePositionCommandValidator()
    {
        RuleFor(command => command.ids).NotEmpty();
        RuleForEach(command => command.ids).GreaterThan(0);
    }
}
