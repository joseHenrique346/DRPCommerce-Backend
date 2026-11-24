using MediatR;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ExpireStockReservationsCommand(long EnterpriseId) : IRequest;
