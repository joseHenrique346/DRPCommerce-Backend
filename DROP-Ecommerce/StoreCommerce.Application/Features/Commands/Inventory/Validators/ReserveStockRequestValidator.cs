using FluentValidation;
using StoreCommerce.Application.Contracts.Inventory;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ReserveStockRequestValidator : AbstractValidator<ReserveStockRequest>
{
    public ReserveStockRequestValidator()
    {
        RuleFor(Request => Request.IdempotencyKey).NotEmpty().MaximumLength(100);
        RuleFor(Request => Request.Reference).MaximumLength(200);
        RuleFor(Request => Request.Items).Cascade(CascadeMode.Stop).NotEmpty().Must(Items => Items.Count <= 100)
            .WithMessage("A reserva permite no máximo 100 SKUs.");
        RuleFor(Request => Request.Items).Must(Items => Items is null || Items.Where(Item => Item is not null)
            .Select(Item => Item.ProductVariantId).Distinct().Count() == Items.Count)
            .WithMessage("A reserva contém SKUs duplicados.");
        RuleForEach(Request => Request.Items).NotNull().ChildRules(Line =>
        {
            Line.RuleFor(Item => Item.ProductVariantId).GreaterThan(0);
            Line.RuleFor(Item => Item.Quantity).InclusiveBetween(1, 10000);
        });
    }
}
