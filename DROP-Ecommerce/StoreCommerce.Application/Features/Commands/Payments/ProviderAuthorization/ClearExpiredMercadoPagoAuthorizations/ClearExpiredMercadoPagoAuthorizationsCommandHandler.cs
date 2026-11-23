using MediatR;
using StoreCommerce.Application.Interfaces.Integrations;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ClearExpiredMercadoPagoAuthorizationsCommandHandler(
    IProviderNotificationStore store,
    TimeProvider time)
    : IRequestHandler<ClearExpiredMercadoPagoAuthorizationsCommand, int>
{
    public async Task<int> Handle(ClearExpiredMercadoPagoAuthorizationsCommand command, CancellationToken ct)
    {
        var now = time.GetUtcNow().UtcDateTime;

        return await store.ClearExpiredAuthorizationsAsync(now, ct);
    }
}
