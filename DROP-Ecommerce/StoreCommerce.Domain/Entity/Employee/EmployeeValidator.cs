using FluentValidation;

namespace StoreCommerce.Domain.Entity;

public class EmployeeValidator : AbstractValidator<Employee>
{
    public EmployeeValidator()
    {
        RuleFor(e => e.Id)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(e => e.CreatedAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");

        RuleFor(e => e.UpdatedAt)
            .NotEqual(default(DateTime)).When(e => e.UpdatedAt.HasValue)
            .WithMessage("{PropertyName} deve ser uma data válida quando fornecido.");

        RuleFor(e => e.EnterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(e => e.FullName)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(200).WithMessage("{PropertyName} não pode ter mais de 200 caracteres.");

        RuleFor(e => e.Email)
            .NotNull().WithMessage("{PropertyName} não pode ser nulo.");

        RuleFor(e => e.PasswordHash)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(500).WithMessage("{PropertyName} não pode ter mais de 500 caracteres.");

        RuleFor(e => e.RoleId)
            .NotNull().WithMessage("{PropertyName} não pode ser nulo.");

        RuleFor(e => e.DepartmentId)
            .NotNull().WithMessage("{PropertyName} não pode ser nulo.");

        RuleFor(e => e.HiredAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");
    }
}
