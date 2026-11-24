using MediatR;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Inventory;
using StoreCommerce.Application.Contracts.Product;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record GetVariantStockQuery(long Id) : IRequest<Result<ProductVariantResponse>>;
