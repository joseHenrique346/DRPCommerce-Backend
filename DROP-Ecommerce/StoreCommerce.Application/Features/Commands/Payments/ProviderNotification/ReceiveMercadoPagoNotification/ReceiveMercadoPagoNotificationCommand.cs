using MediatR;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ReceiveMercadoPagoNotificationCommand(long ConnectionId, string PaymentId, string RequestId, string SignatureHeader) : IRequest;
