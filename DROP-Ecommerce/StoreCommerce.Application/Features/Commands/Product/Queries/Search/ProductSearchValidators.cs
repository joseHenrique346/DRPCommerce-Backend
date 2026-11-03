using FluentValidation;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ProductSearchRequestValidator : AbstractValidator<ProductSearchRequest>
{
    public ProductSearchRequestValidator()
    {
        RuleFor(Request => Request.Term).MaximumLength(Product.NameMaxLength);
        RuleFor(Request => Request.Brand).MaximumLength(Product.BrandMaxLength);
        RuleFor(Request => Request.CategoryId).GreaterThan(0).When(Request => Request.CategoryId.HasValue);
        RuleFor(Request => Request.MinPrice).GreaterThanOrEqualTo(0).When(Request => Request.MinPrice.HasValue);
        RuleFor(Request => Request.MaxPrice).GreaterThanOrEqualTo(0).When(Request => Request.MaxPrice.HasValue);
        RuleFor(Request => Request).Must(Request => !Request.MinPrice.HasValue || !Request.MaxPrice.HasValue || Request.MinPrice <= Request.MaxPrice)
            .WithMessage("O preço mínimo não pode exceder o máximo.");
        RuleFor(Request => Request.Sort).Must(Sort => Sort is "price_asc" or "price_desc" or "newest" or "name");
        RuleFor(Request => Request.Page).InclusiveBetween(1, 100000);
        RuleFor(Request => Request.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class SearchProductsQueryValidator : AbstractValidator<SearchProductsQuery>
{
    public SearchProductsQueryValidator()
    {
        RuleFor(Query => Query.Request).NotNull().SetValidator(new ProductSearchRequestValidator());
    }
}

public sealed class SearchStorefrontProductsQueryValidator : AbstractValidator<SearchStorefrontProductsQuery>
{
    public SearchStorefrontProductsQueryValidator()
    {
        RuleFor(Query => Query.Request).NotNull().SetValidator(new ProductSearchRequestValidator());
    }
}

public sealed class GetProductBySlugQueryValidator : AbstractValidator<GetProductBySlugQuery>
{
    public GetProductBySlugQueryValidator()
    {
        RuleFor(Query => Query.Slug).NotEmpty().MaximumLength(Product.SlugMaxLength);
    }
}
