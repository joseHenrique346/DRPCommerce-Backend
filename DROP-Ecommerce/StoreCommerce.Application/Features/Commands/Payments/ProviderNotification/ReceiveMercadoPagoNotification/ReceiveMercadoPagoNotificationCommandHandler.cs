using MediatR;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Integrations.MercadoPago;
using StoreCommerce.Application.Interfaces.Integrations;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ReceiveMercadoPagoNotificationCommandHandler(
    IProviderNotificationStore store,
    MercadoPagoSignature signature,
    IOptions<MercadoPagoOptions> options,
    TimeProvider time)
    : IRequestHandler<ReceiveMercadoPagoNotificationCommand>
{
    public async Task Handle(ReceiveMercadoPagoNotificationCommand command, CancellationToken ct)
    {
        long connectionId = command.ConnectionId;
        var paymentId = command.PaymentId;
        var requestId = command.RequestId;
        var signatureHeader = command.SignatureHeader;

        var connection = await store.FindConnectionAsync(connectionId, ct) ?? throw new SecurityFailure(404, "MercadoPago.ConnectionNotFound", "Conexão não encontrada.");
        options.Value.RequireEnvironment(connection.Environment);
        var delivery = signature.Verify(paymentId, requestId, signatureHeader, connection.Environment);
        var notification = ProviderNotification.Create(connection, paymentId, delivery, time.GetUtcNow().UtcDateTime);
        await store.InsertNotificationAsync(notification, detachAfterInsert: true, ct);
    }
}
