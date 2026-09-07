using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(c => c.enterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(c => c.parentCategoryId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero quando fornecido.")
            .When(c => c.parentCategoryId.HasValue);

        RuleFor(c => c.name)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(c => c.slug)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(c => c.dscription)
            .MaximumLength(2000).WithMessage("{PropertyName} não pode ter mais de 2000 caracteres.");

        RuleFor(c => c.imageUrl)
            .MaximumLength(1000).WithMessage("{PropertyName} não pode ter mais de 1000 caracteres.");

        When(c => !string.IsNullOrEmpty(c.imageUrl), () =>
        {
            RuleFor(c => c.imageUrl)
                .Must(uri => Uri.TryCreate(uri!, UriKind.Absolute, out _))
                .WithMessage("{PropertyName} tem formato de URL inválido.");
        });

        RuleFor(c => c.displayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");
    }
}
