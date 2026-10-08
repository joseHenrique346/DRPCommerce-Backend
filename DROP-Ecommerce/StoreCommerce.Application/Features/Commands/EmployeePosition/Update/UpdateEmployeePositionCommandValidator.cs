using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateEmployeePositionCommandValidator : AbstractValidator<UpdateEmployeePositionCommand>
{
    public UpdateEmployeePositionCommandValidator()
    {
        RuleFor(command => command.id).GreaterThan(0);
        RuleFor(command => command.name).NotEmpty().MaximumLength(255);
        RuleFor(command => command.description).MaximumLength(1000);
    }
}
