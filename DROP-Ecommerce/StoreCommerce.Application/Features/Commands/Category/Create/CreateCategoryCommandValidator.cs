using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(category => category.enterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(category => category.parentCategoryId)
            .GreaterThan(0).When(category => category.parentCategoryId.HasValue)
            .WithMessage("{PropertyName} deve ser maior que zero quando fornecido.");

        RuleFor(category => category.name)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(category => category.slug)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(150).WithMessage("{PropertyName} não pode ter mais de 150 caracteres.")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage("{PropertyName} deve conter apenas letras minúsculas, números e hífens.");

        RuleFor(category => category.description)
            .MaximumLength(1000).When(category => !string.IsNullOrWhiteSpace(category.description))
            .WithMessage("{PropertyName} não pode ter mais de 1000 caracteres.");

        RuleFor(category => category.imageUrl)
            .MaximumLength(500).When(category => !string.IsNullOrWhiteSpace(category.imageUrl))
            .WithMessage("{PropertyName} não pode ter mais de 500 caracteres.");

        RuleFor(category => category.displayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");
    }
}