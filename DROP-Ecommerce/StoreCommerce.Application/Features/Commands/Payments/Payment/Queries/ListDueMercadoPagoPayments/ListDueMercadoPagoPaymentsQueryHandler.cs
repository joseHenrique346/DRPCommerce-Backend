using MediatR;
using StoreCommerce.Application.Integrations.MercadoPago;
using StoreCommerce.Application.Interfaces.Integrations;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ListDueMercadoPagoPaymentsQueryHandler(
    IProviderNotificationStore store,
    TimeProvider time)
    : IRequestHandler<ListDueMercadoPagoPaymentsQuery, List<MercadoPagoNotificationKey>>
{
    public async Task<List<MercadoPagoNotificationKey>> Handle(ListDueMercadoPagoPaymentsQuery command, CancellationToken ct)
    {
        var now = time.GetUtcNow().UtcDateTime;

        return await store.ReadDuePaymentsAsync(now, ct);
    }
}
