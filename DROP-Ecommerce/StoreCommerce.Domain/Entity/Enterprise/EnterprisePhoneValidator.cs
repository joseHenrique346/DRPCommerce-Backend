using FluentValidation;

namespace StoreCommerce.Domain.Entity;

public class EnterprisePhoneValidator : AbstractValidator<EnterprisePhone>
{
    public EnterprisePhoneValidator()
    {
        RuleFor(p => p.Value)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(50).WithMessage("{PropertyName} não pode ter mais de 50 caracteres.")
            .Matches(@"^\+?[\d\s\-\(\)]{8,}$").WithMessage("{PropertyName} deve ser um número de telefone válido.");
    }
}
