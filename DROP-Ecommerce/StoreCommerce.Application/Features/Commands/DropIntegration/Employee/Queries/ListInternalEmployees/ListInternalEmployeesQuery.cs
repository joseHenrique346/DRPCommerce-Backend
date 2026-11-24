using MediatR;
using SharedCommerce.Application.Contracts.Drop;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ListInternalEmployeesQuery(List<long> Ids) : IRequest<List<StoreEmployeeResponse>>;
