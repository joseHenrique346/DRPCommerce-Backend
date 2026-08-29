using FluentValidation;

namespace StoreCommerce.Domain.Entity;

public class EmployeeEmailValidator : AbstractValidator<EmployeeEmail>
{
    public EmployeeEmailValidator()
    {
        RuleFor(ee => ee.Value)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.")
            .EmailAddress().WithMessage("{PropertyName} possui formato de e-mail inválido.");
    }
}
