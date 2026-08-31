using FluentValidation;

namespace StoreCommerce.Domain.Entity.Category;

internal class CategoryValidator : AbstractValidator<Category>
{
    public CategoryValidator()
    {
        RuleFor(c => c.Id)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(c => c.CreatedAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");

        RuleFor(c => c.UpdatedAt)
            .NotEqual(default(DateTime)).When(c => c.UpdatedAt.HasValue)
            .WithMessage("{PropertyName} deve ser uma data válida quando fornecido.");

        RuleFor(c => c.EnterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(c => c.ParentCategoryId)
            .GreaterThan(0).When(c => c.ParentCategoryId.HasValue)
            .WithMessage("{PropertyName} deve ser maior que zero quando fornecido.");

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(c => c.Slug)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(150).WithMessage("{PropertyName} não pode ter mais de 150 caracteres.")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage("{PropertyName} deve conter apenas letras minúsculas, números e hífens.");

        RuleFor(c => c.Description)
            .MaximumLength(1000).When(c => !string.IsNullOrWhiteSpace(c.Dscription))
            .WithMessage("{PropertyName} não pode ter mais de 1000 caracteres.");

        RuleFor(c => c.ImageUrl)
            .MaximumLength(500).When(c => !string.IsNullOrWhiteSpace(c.ImageUrl))
            .WithMessage("{PropertyName} não pode ter mais de 500 caracteres.");

        RuleFor(c => c.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");
    }
}
