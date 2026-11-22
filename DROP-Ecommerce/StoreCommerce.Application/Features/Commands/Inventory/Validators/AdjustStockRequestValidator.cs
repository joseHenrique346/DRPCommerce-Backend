using FluentValidation;
using StoreCommerce.Application.Contracts.Inventory;

namespace StoreCommerce.Application.Features.Commands;

public sealed class AdjustStockRequestValidator : AbstractValidator<AdjustStockRequest>
{
    public AdjustStockRequestValidator()
    {
        RuleFor(Request => Request.QuantityChange).NotEqual(0);
        RuleFor(Request => Request.Reason).NotEmpty().MaximumLength(500);
        RuleFor(Request => Request.IdempotencyKey).NotEmpty().MaximumLength(100);
        RuleFor(Request => Request.ExpectedVersion).GreaterThan(0).When(Request => Request.ExpectedVersion.HasValue);
    }
}
