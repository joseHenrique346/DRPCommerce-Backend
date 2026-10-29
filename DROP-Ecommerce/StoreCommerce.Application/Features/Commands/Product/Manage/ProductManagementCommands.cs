using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record UpdateProductDetailsCommand(long Id, string Name, string Slug, string Description, string SKU,
    string BarCode, string Brand, string ImageUrls, bool IsDigital) : IRequest<Result<Product>>;
public sealed record UpdateProductPricingCommand(long Id, decimal Price, decimal CostPrice) : IRequest<Result<Product>>;
public sealed record UpdateProductDimensionsCommand(long Id, decimal Weight, decimal Height, decimal Width, decimal Length) : IRequest<Result<Product>>;
public sealed record ChangeProductCategoryCommand(long Id, long CategoryId, long? SupplierId) : IRequest<Result<Product>>;
public sealed record SetProductActiveCommand(long Id, bool IsActive) : IRequest<Result<Product>>;
public sealed record EnableProductVariantsCommand(long Id) : IRequest<Result<Product>>;
