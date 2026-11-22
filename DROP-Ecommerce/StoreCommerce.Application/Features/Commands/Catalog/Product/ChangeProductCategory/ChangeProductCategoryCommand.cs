using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ChangeProductCategoryCommand(long Id, long CategoryId, long? SupplierId) : IRequest<Result<Product>>;
