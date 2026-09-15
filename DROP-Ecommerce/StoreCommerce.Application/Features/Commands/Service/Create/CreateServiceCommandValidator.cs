using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
        RuleFor(service => service.enterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(service => service.categoryId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(service => service.name)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(200).WithMessage("{PropertyName} não pode ter mais de 200 caracteres.");

        RuleFor(service => service.description)
            .MaximumLength(2000).When(service => !string.IsNullOrWhiteSpace(service.description))
            .WithMessage("{PropertyName} não pode ter mais de 2000 caracteres.");

        RuleFor(service => service.price)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(service => service.durationMinutes)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}