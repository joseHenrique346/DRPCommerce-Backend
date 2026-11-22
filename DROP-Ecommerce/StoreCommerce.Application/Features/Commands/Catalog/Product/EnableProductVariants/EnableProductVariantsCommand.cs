using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record EnableProductVariantsCommand(long Id) : IRequest<Result<Product>>;
