using FluentValidation;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(category => category.parentCategoryId)
            .GreaterThan(0).When(category => category.parentCategoryId.HasValue)
            .WithMessage("{PropertyName} deve ser maior que zero quando fornecido.");

        RuleFor(category => category.name)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(Category.NameMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

        RuleFor(category => category.slug)
            .MaximumLength(Category.SlugMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.")
            .Matches(Category.SlugPattern).WithMessage("{PropertyName} deve conter apenas letras minúsculas, números e hífens.")
            .When(category => !string.IsNullOrWhiteSpace(category.slug));

        RuleFor(category => category.description)
            .MaximumLength(Category.DescriptionMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

        RuleFor(category => category.imageUrl)
            .MaximumLength(Category.ImageUrlMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

        RuleFor(category => category.displayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");
    }
}
