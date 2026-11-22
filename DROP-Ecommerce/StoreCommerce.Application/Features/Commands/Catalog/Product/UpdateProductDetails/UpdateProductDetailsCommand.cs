using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record UpdateProductDetailsCommand(long Id, string Name, string Slug, string Description, string SKU,
    string BarCode, string Brand, string ImageUrls, bool IsDigital) : IRequest<Result<Product>>;
