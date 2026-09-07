using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(c => c.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(c => c.enterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(c => c.fullName)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(c => c.passwordHash)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(500).WithMessage("{PropertyName} não pode ter mais de 500 caracteres.");

        RuleFor(c => c.addressLine)
            .MaximumLength(500).WithMessage("{PropertyName} não pode ter mais de 500 caracteres.");

        RuleFor(c => c.city)
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(c => c.state)
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(c => c.zipCode)
            .MaximumLength(20).WithMessage("{PropertyName} não pode ter mais de 20 caracteres.");

        RuleFor(c => c.country)
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(c => c.gender)
            .MaximumLength(50).WithMessage("{PropertyName} não pode ter mais de 50 caracteres.");

        RuleFor(c => c.dateOfBirth)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("{PropertyName} não pode estar no futuro.");
    }
}
