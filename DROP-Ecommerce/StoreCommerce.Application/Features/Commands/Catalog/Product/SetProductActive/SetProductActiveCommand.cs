using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record SetProductActiveCommand(long Id, bool IsActive) : IRequest<Result<Product>>;
