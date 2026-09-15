using FluentValidation;

namespace StoreCommerce.Domain.Entity;

public class CustomerPhoneValidator : AbstractValidator<CustomerPhone>
{
    public CustomerPhoneValidator()
    {
        RuleFor(cp => cp.Value)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(50).WithMessage("{PropertyName} não pode ter mais de 50 caracteres.")
            .Matches(@"^\+?[\d\s\-\(\)]{8,}$").WithMessage("{PropertyName} possui formato de telefone inválido.");
    }
}
