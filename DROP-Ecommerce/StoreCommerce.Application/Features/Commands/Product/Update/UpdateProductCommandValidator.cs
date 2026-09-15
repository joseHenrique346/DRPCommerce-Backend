using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(product => product.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(product => product.enterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(product => product.categoryId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(product => product.supplierId)
            .GreaterThan(0).When(product => product.supplierId.HasValue)
            .WithMessage("{PropertyName} deve ser maior que zero quando fornecido.");

        RuleFor(product => product.name)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(200).WithMessage("{PropertyName} não pode ter mais de 200 caracteres.");

        RuleFor(product => product.slug)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(150).WithMessage("{PropertyName} não pode ter mais de 150 caracteres.")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage("{PropertyName} deve conter apenas letras minúsculas, números e hífens.");

        RuleFor(product => product.description)
            .MaximumLength(2000).When(product => !string.IsNullOrWhiteSpace(product.description))
            .WithMessage("{PropertyName} não pode ter mais de 2000 caracteres.");

        RuleFor(product => product.sku)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(product => product.barCode)
            .MaximumLength(50).When(product => !string.IsNullOrWhiteSpace(product.barCode))
            .WithMessage("{PropertyName} não pode ter mais de 50 caracteres.");

        RuleFor(product => product.price)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(product => product.costPrice)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(product => product.weight)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(product => product.height)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(product => product.width)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(product => product.length)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(product => product.brand)
            .MaximumLength(100).When(product => !string.IsNullOrWhiteSpace(product.brand))
            .WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(product => product.imageUrls)
            .MaximumLength(2000).When(product => !string.IsNullOrWhiteSpace(product.imageUrls))
            .WithMessage("{PropertyName} não pode ter mais de 2000 caracteres.");
    }
}