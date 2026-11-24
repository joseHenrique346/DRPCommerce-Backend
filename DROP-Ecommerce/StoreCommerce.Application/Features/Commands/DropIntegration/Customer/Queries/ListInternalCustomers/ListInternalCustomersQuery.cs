using MediatR;
using SharedCommerce.Application.Contracts.Drop;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ListInternalCustomersQuery(List<long> Ids) : IRequest<List<StoreCustomerResponse>>;
