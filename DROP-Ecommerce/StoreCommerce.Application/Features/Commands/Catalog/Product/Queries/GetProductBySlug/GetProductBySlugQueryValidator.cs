using FluentValidation;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetProductBySlugQueryValidator : AbstractValidator<GetProductBySlugQuery>
{
    public GetProductBySlugQueryValidator()
    {
        RuleFor(Query => Query.Slug).NotEmpty().MaximumLength(Product.SlugMaxLength);
    }
}
