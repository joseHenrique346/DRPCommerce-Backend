using FluentValidation;
using StoreCommerce.Application.Contracts.Inventory;

namespace StoreCommerce.Application.Features.Commands;

public sealed class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(Command => Command.ProductVariantId).GreaterThan(0);
        RuleFor(Command => Command.Request).NotNull().SetValidator(new AdjustStockRequestValidator());
    }
}
