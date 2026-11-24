using FluentValidation;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(command => command.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(command => command.name)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(Department.NameMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

        RuleFor(command => command.description)
            .MaximumLength(Department.DescriptionMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
    }
}
