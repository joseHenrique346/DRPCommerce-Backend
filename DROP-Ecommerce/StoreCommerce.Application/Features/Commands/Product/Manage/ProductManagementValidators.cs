using FluentValidation;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateProductDetailsCommandValidator : AbstractValidator<UpdateProductDetailsCommand>
{
    public UpdateProductDetailsCommandValidator()
    {
        RuleFor(Command => Command.Id).GreaterThan(0);
        RuleFor(Command => Command.Name).NotEmpty().MaximumLength(Product.NameMaxLength);
        RuleFor(Command => Command.Slug).NotEmpty().MaximumLength(Product.SlugMaxLength).Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$");
        RuleFor(Command => Command.SKU).NotEmpty().MaximumLength(Product.SkuMaxLength);
        RuleFor(Command => Command.BarCode).MaximumLength(Product.BarCodeMaxLength);
        RuleFor(Command => Command.Brand).MaximumLength(Product.BrandMaxLength);
        RuleFor(Command => Command.Description).MaximumLength(Product.DescriptionMaxLength);
        RuleFor(Command => Command.ImageUrls).MaximumLength(Product.ImageUrlsMaxLength);
    }
}

public sealed class UpdateProductPricingCommandValidator : AbstractValidator<UpdateProductPricingCommand>
{
    public UpdateProductPricingCommandValidator()
    {
        RuleFor(Command => Command.Id).GreaterThan(0);
        RuleFor(Command => Command.Price).GreaterThan(0);
        RuleFor(Command => Command.CostPrice).GreaterThanOrEqualTo(0).LessThanOrEqualTo(Command => Command.Price);
    }
}

public sealed class UpdateProductDimensionsCommandValidator : AbstractValidator<UpdateProductDimensionsCommand>
{
    public UpdateProductDimensionsCommandValidator()
    {
        RuleFor(Command => Command.Id).GreaterThan(0);
        RuleFor(Command => Command.Weight).GreaterThanOrEqualTo(0);
        RuleFor(Command => Command.Height).GreaterThanOrEqualTo(0);
        RuleFor(Command => Command.Width).GreaterThanOrEqualTo(0);
        RuleFor(Command => Command.Length).GreaterThanOrEqualTo(0);
    }
}

public sealed class ChangeProductCategoryCommandValidator : AbstractValidator<ChangeProductCategoryCommand>
{
    public ChangeProductCategoryCommandValidator()
    {
        RuleFor(Command => Command.Id).GreaterThan(0);
        RuleFor(Command => Command.CategoryId).GreaterThan(0);
        RuleFor(Command => Command.SupplierId).GreaterThan(0).When(Command => Command.SupplierId.HasValue);
    }
}

public sealed class SetProductActiveCommandValidator : AbstractValidator<SetProductActiveCommand>
{
    public SetProductActiveCommandValidator()
    {
        RuleFor(Command => Command.Id).GreaterThan(0);
    }
}

public sealed class EnableProductVariantsCommandValidator : AbstractValidator<EnableProductVariantsCommand>
{
    public EnableProductVariantsCommandValidator()
    {
        RuleFor(Command => Command.Id).GreaterThan(0);
    }
}
