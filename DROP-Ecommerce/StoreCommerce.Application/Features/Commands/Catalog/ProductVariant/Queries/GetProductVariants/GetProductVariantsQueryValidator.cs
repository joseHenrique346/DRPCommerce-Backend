using FluentValidation;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetProductVariantsQueryValidator : AbstractValidator<GetProductVariantsQuery>
{
    public GetProductVariantsQueryValidator()
    {
        RuleFor(Query => Query.ProductId).GreaterThan(0);
    }
}
