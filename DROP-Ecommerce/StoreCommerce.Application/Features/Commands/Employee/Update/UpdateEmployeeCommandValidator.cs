using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(e => e.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(e => e.enterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(e => e.fullName)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(e => e.email)
            .NotNull().WithMessage("{PropertyName} não pode ser nulo.");

        RuleFor(e => e.passwordHash)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(500).WithMessage("{PropertyName} não pode ter mais de 500 caracteres.");

        RuleFor(e => e.roleId)
            .NotNull().WithMessage("{PropertyName} não pode ser nulo.");

        RuleFor(e => e.departmentId)
            .NotNull().WithMessage("{PropertyName} não pode ser nulo.");

        RuleFor(e => e.hiredAt)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("{PropertyName} não pode estar no futuro.");
    }
}
