using MediatR;
using StoreCommerce.Application.Contracts.Product;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record GetProductVariantsQuery(long ProductId) : IRequest<Result<List<ProductVariantResponse>>>;
