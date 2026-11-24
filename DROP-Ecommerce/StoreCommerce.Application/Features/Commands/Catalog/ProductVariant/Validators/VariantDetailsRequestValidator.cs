using FluentValidation;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class VariantDetailsRequestValidator : AbstractValidator<VariantDetailsRequest>
{
    public VariantDetailsRequestValidator()
    {
        RuleFor(Request => Request.Name).NotEmpty().MaximumLength(ProductVariant.NameMaxLength);
        RuleFor(Request => Request.SKU).NotEmpty().MaximumLength(Product.SkuMaxLength);
        RuleFor(Request => Request.BarCode).MaximumLength(Product.BarCodeMaxLength);
        RuleFor(Request => Request.Attributes).Cascade(CascadeMode.Stop).NotNull().Must(Attributes => Attributes.Count is >= 1 and <= 8);
    }
}
