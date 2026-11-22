using FluentValidation;
using StoreCommerce.Application.Contracts.Inventory;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetStockMovementsQueryValidator : AbstractValidator<GetStockMovementsQuery>
{
    public GetStockMovementsQueryValidator()
    {
        RuleFor(Query => Query.ProductVariantId).GreaterThan(0).When(Query => Query.ProductVariantId.HasValue);
        RuleFor(Query => Query.Page).InclusiveBetween(1, 100000);
        RuleFor(Query => Query.PageSize).InclusiveBetween(1, 100);
    }
}
