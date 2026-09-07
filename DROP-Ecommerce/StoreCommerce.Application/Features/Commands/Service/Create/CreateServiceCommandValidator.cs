using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
        RuleFor(s => s.enterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(s => s.categoryId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(s => s.name)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(s => s.description)
            .MaximumLength(5000).WithMessage("{PropertyName} não pode ter mais de 5000 caracteres.");

        RuleFor(s => s.price)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(s => s.durationMinutes)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}
