using FluentValidation;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateProductVariantCommandValidator : AbstractValidator<UpdateProductVariantCommand>
{
    public UpdateProductVariantCommandValidator()
    {
        RuleFor(Command => Command.ProductId).GreaterThan(0);
        RuleFor(Command => Command.Id).GreaterThan(0);
        RuleFor(Command => Command.Request).NotNull().SetValidator(new VariantDetailsRequestValidator());
    }
}
