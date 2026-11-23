using MediatR;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ProcessMercadoPagoNotificationCommand(long EnterpriseId, long Id) : IRequest;
