using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateEnterpriseCommandValidator : AbstractValidator<CreateEnterpriseCommand>
{
    public CreateEnterpriseCommandValidator()
    {
        RuleFor(enterprise => enterprise.tradeName)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(200).WithMessage("{PropertyName} não pode ter mais de 200 caracteres.");

        RuleFor(enterprise => enterprise.legalName)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(250).WithMessage("{PropertyName} não pode ter mais de 250 caracteres.");

        RuleFor(enterprise => enterprise.email)
            .NotNull().WithMessage("{PropertyName} não pode ser nulo.");

        RuleFor(enterprise => enterprise.email.Value)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.")
            .EmailAddress().WithMessage("{PropertyName} possui formato de e-mail inválido.");

        RuleFor(enterprise => enterprise.phone)
            .NotNull().WithMessage("{PropertyName} não pode ser nulo.");

        RuleFor(enterprise => enterprise.phone.Value)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(50).WithMessage("{PropertyName} não pode ter mais de 50 caracteres.")
            .Matches(@"^\+?[\d\s\-\(\)]{8,}$").WithMessage("{PropertyName} deve ser um número de telefone válido.");

        RuleFor(enterprise => enterprise.addressLine)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(300).WithMessage("{PropertyName} não pode ter mais de 300 caracteres.");

        RuleFor(enterprise => enterprise.city)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(150).WithMessage("{PropertyName} não pode ter mais de 150 caracteres.");

        RuleFor(enterprise => enterprise.state)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(enterprise => enterprise.zipCode)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(20).WithMessage("{PropertyName} não pode ter mais de 20 caracteres.");

        RuleFor(enterprise => enterprise.country)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");
    }
}