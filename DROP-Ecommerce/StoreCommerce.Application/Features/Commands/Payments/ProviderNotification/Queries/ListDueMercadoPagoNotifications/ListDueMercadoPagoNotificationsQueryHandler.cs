using MediatR;
using StoreCommerce.Application.Integrations.MercadoPago;
using StoreCommerce.Application.Interfaces.Integrations;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ListDueMercadoPagoNotificationsQueryHandler(
    IProviderNotificationStore store,
    TimeProvider time)
    : IRequestHandler<ListDueMercadoPagoNotificationsQuery, List<MercadoPagoNotificationKey>>
{
    public async Task<List<MercadoPagoNotificationKey>> Handle(ListDueMercadoPagoNotificationsQuery command, CancellationToken ct)
    {
        var now = time.GetUtcNow().UtcDateTime;

        return await store.ReadDueAsync(now, ct);
    }
}
