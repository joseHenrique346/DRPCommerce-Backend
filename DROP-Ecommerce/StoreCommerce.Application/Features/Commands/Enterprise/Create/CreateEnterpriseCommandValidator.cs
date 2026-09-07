using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateEnterpriseCommandValidator : AbstractValidator<CreateEnterpriseCommand>
{
    public CreateEnterpriseCommandValidator()
    {
        RuleFor(e => e.tradeName)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(e => e.legalName)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(e => e.email)
            .NotNull().WithMessage("{PropertyName} não pode ser nulo.");

        RuleFor(e => e.phone)
            .NotNull().WithMessage("{PropertyName} não pode ser nulo.");

        RuleFor(e => e.addressLine)
            .MaximumLength(500).WithMessage("{PropertyName} não pode ter mais de 500 caracteres.");

        RuleFor(e => e.city)
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(e => e.state)
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(e => e.zipCode)
            .MaximumLength(20).WithMessage("{PropertyName} não pode ter mais de 20 caracteres.");

        RuleFor(e => e.country)
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");
    }
}
