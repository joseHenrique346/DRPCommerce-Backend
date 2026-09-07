using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(s => s.enterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(s => s.companyName)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(s => s.contactName)
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(s => s.addressLine)
            .MaximumLength(500).WithMessage("{PropertyName} não pode ter mais de 500 caracteres.");

        RuleFor(s => s.city)
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(s => s.state)
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(s => s.zipCode)
            .MaximumLength(20).WithMessage("{PropertyName} não pode ter mais de 20 caracteres.");

        RuleFor(s => s.country)
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");
    }
}
