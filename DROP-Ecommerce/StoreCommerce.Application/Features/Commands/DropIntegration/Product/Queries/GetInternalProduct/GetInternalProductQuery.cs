using MediatR;
using SharedCommerce.Application.Contracts.Drop;

namespace StoreCommerce.Application.Features.Commands;

public sealed record GetInternalProductQuery(long Id) : IRequest<StoreProductResponse?>;
