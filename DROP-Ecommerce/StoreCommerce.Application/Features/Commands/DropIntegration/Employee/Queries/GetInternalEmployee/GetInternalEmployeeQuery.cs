using MediatR;
using SharedCommerce.Application.Contracts.Drop;

namespace StoreCommerce.Application.Features.Commands;

public sealed record GetInternalEmployeeQuery(long Id) : IRequest<StoreEmployeeResponse?>;
