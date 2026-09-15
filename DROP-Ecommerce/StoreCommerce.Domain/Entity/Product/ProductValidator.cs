using FluentValidation;

namespace StoreCommerce.Domain.Entity.Product;

public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(p => p.Id)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(p => p.CreatedAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");

        RuleFor(p => p.UpdatedAt)
            .NotEqual(default(DateTime)).When(p => p.UpdatedAt.HasValue)
            .WithMessage("{PropertyName} deve ser uma data válida quando fornecido.");

        RuleFor(p => p.EnterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(p => p.CategoryId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(p => p.SupplierId)
            .GreaterThan(0).When(p => p.SupplierId.HasValue)
            .WithMessage("{PropertyName} deve ser maior que zero quando fornecido.");

        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(200).WithMessage("{PropertyName} não pode ter mais de 200 caracteres.");

        RuleFor(p => p.Slug)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(150).WithMessage("{PropertyName} não pode ter mais de 150 caracteres.")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage("{PropertyName} deve conter apenas letras minúsculas, números e hífens.");

        RuleFor(p => p.Description)
            .MaximumLength(2000).When(p => !string.IsNullOrWhiteSpace(p.Description))
            .WithMessage("{PropertyName} não pode ter mais de 2000 caracteres.");

        RuleFor(p => p.SKU)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(p => p.BarCode)
            .MaximumLength(50).When(p => !string.IsNullOrWhiteSpace(p.BarCode))
            .WithMessage("{PropertyName} não pode ter mais de 50 caracteres.");

        RuleFor(p => p.Price)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(p => p.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(p => p.Weight)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(p => p.Height)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(p => p.Width)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(p => p.Length)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(p => p.Brand)
            .MaximumLength(100).When(p => !string.IsNullOrWhiteSpace(p.Brand))
            .WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(p => p.ImageUrls)
            .MaximumLength(2000).When(p => !string.IsNullOrWhiteSpace(p.ImageUrls))
            .WithMessage("{PropertyName} não pode ter mais de 2000 caracteres.");
    }
}
