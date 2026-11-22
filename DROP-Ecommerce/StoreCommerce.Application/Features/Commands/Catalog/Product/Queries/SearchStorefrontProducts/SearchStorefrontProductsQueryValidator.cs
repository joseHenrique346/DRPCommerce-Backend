using FluentValidation;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SearchStorefrontProductsQueryValidator : AbstractValidator<SearchStorefrontProductsQuery>
{
    public SearchStorefrontProductsQueryValidator()
    {
        RuleFor(Query => Query.Request).NotNull().SetValidator(new ProductSearchRequestValidator());
    }
}
