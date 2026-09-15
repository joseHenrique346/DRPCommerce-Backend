using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(supplier => supplier.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(supplier => supplier.enterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(supplier => supplier.companyName)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(200).WithMessage("{PropertyName} não pode ter mais de 200 caracteres.");

        RuleFor(supplier => supplier.contactName)
            .MaximumLength(150).When(supplier => !string.IsNullOrWhiteSpace(supplier.contactName))
            .WithMessage("{PropertyName} não pode ter mais de 150 caracteres.");

        RuleFor(supplier => supplier.addressLine)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(300).WithMessage("{PropertyName} não pode ter mais de 300 caracteres.");

        RuleFor(supplier => supplier.city)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(150).WithMessage("{PropertyName} não pode ter mais de 150 caracteres.");

        RuleFor(supplier => supplier.state)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(supplier => supplier.zipCode)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(20).WithMessage("{PropertyName} não pode ter mais de 20 caracteres.");

        RuleFor(supplier => supplier.country)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");
    }
}