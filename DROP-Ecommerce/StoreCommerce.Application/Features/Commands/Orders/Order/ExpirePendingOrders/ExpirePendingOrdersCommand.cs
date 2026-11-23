using MediatR;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ExpirePendingOrdersCommand(long EnterpriseId) : IRequest<int>;
