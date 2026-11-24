using FluentValidation;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateEmployeePositionCommandValidator : AbstractValidator<UpdateEmployeePositionCommand>
{
    public UpdateEmployeePositionCommandValidator()
    {
        RuleFor(command => command.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(command => command.name)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(EmployeePosition.NameMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

        RuleFor(command => command.description)
            .MaximumLength(EmployeePosition.DescriptionMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
    }
}
