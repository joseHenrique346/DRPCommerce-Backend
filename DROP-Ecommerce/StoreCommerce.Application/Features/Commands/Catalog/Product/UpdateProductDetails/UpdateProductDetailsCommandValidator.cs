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
