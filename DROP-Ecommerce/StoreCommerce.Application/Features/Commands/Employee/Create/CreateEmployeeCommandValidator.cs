using FluentValidation;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(employee => employee.enterpriseMembershipId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(employee => employee.fullName)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(Employee.FullNameMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

        RuleFor(employee => employee.positionId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(employee => employee.departmentId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(employee => employee.hiredAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");
    }
}
