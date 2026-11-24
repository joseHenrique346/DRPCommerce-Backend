using MediatR;
using SharedCommerce.Application.Contracts.Drop;

namespace StoreCommerce.Application.Features.Commands;

public sealed record GetInternalEnterpriseQuery(long Id) : IRequest<StoreEnterpriseResponse?>;
