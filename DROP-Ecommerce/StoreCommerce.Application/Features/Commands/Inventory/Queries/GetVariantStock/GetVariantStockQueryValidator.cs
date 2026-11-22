using FluentValidation;
using StoreCommerce.Application.Contracts.Inventory;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetVariantStockQueryValidator : AbstractValidator<GetVariantStockQuery>
{
    public GetVariantStockQueryValidator()
    {
        RuleFor(Query => Query.Id).GreaterThan(0);
    }
}
