using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(customer => customer.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(customer => customer.enterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(customer => customer.fullName)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(200).WithMessage("{PropertyName} não pode ter mais de 200 caracteres.");

        RuleFor(customer => customer.email).NotNull();
        RuleFor(customer => customer.phone).NotNull();

        RuleFor(customer => customer.passwordHash)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(500).WithMessage("{PropertyName} não pode ter mais de 500 caracteres.");

        RuleFor(customer => customer.addressLine)
            .MaximumLength(300).When(customer => !string.IsNullOrWhiteSpace(customer.addressLine))
            .WithMessage("{PropertyName} não pode ter mais de 300 caracteres.");

        RuleFor(customer => customer.city)
            .MaximumLength(150).When(customer => !string.IsNullOrWhiteSpace(customer.city))
            .WithMessage("{PropertyName} não pode ter mais de 150 caracteres.");

        RuleFor(customer => customer.stateId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(customer => customer.zipCode)
            .MaximumLength(20).When(customer => !string.IsNullOrWhiteSpace(customer.zipCode))
            .WithMessage("{PropertyName} não pode ter mais de 20 caracteres.");

        RuleFor(customer => customer.country)
            .MaximumLength(100).When(customer => !string.IsNullOrWhiteSpace(customer.country))
            .WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(customer => customer.gender)
            .MaximumLength(50).When(customer => !string.IsNullOrWhiteSpace(customer.gender))
            .WithMessage("{PropertyName} não pode ter mais de 50 caracteres.");

        RuleFor(customer => customer.dateOfBirth)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");
    }
}
