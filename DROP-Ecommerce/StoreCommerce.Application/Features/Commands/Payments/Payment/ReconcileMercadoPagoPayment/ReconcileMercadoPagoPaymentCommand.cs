using MediatR;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ReconcileMercadoPagoPaymentCommand(long EnterpriseId, long AttemptId) : IRequest;
