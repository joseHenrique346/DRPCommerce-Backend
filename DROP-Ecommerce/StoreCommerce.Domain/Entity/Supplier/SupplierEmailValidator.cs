using FluentValidation;

namespace StoreCommerce.Domain.Entity;

public class SupplierEmailValidator : AbstractValidator<SupplierEmail>
{
    public SupplierEmailValidator()
    {
        RuleFor(e => e.Value)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.")
            .EmailAddress().WithMessage("{PropertyName} deve ser um endereço de email válido.");
    }
}
