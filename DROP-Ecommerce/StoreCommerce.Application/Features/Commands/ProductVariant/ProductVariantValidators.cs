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

public sealed class CreateProductVariantCommandValidator : AbstractValidator<CreateProductVariantCommand>
{
    public CreateProductVariantCommandValidator()
    {
        RuleFor(Command => Command.ProductId).GreaterThan(0);
        RuleFor(Command => Command.Request).NotNull();
        When(Command => Command.Request is not null, () =>
        {
            RuleFor(Command => Command.Request.Details).NotNull().SetValidator(new VariantDetailsRequestValidator());
        });
    }
}

public sealed class UpdateProductVariantCommandValidator : AbstractValidator<UpdateProductVariantCommand>
{
    public UpdateProductVariantCommandValidator()
    {
        RuleFor(Command => Command.ProductId).GreaterThan(0);
        RuleFor(Command => Command.Id).GreaterThan(0);
        RuleFor(Command => Command.Request).NotNull().SetValidator(new VariantDetailsRequestValidator());
    }
}

public sealed class UpdateProductVariantPricingCommandValidator : AbstractValidator<UpdateProductVariantPricingCommand>
{
    public UpdateProductVariantPricingCommandValidator()
    {
        RuleFor(Command => Command.ProductId).GreaterThan(0);
        RuleFor(Command => Command.Id).GreaterThan(0);
        RuleFor(Command => Command.Request).NotNull();
    }
}

public sealed class UpdateProductVariantDimensionsCommandValidator : AbstractValidator<UpdateProductVariantDimensionsCommand>
{
    public UpdateProductVariantDimensionsCommandValidator()
    {
        RuleFor(Command => Command.ProductId).GreaterThan(0);
        RuleFor(Command => Command.Id).GreaterThan(0);
        RuleFor(Command => Command.Request).NotNull();
    }
}

public sealed class DeleteProductVariantCommandValidator : AbstractValidator<DeleteProductVariantCommand>
{
    public DeleteProductVariantCommandValidator()
    {
        RuleFor(Command => Command.ProductId).GreaterThan(0);
        RuleFor(Command => Command.Id).GreaterThan(0);
    }
}

public sealed class GetProductVariantsQueryValidator : AbstractValidator<GetProductVariantsQuery>
{
    public GetProductVariantsQueryValidator()
    {
        RuleFor(Query => Query.ProductId).GreaterThan(0);
    }
}
