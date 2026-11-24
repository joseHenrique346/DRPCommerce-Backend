using MediatR;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.Integrations;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ListPendingProviderNotificationsQueryHandler(
    ICurrentIdentity actor,
    IProviderNotificationStore store)
    : IRequestHandler<ListPendingProviderNotificationsQuery, List<ProviderNotificationSummary>>
{
    public async Task<List<ProviderNotificationSummary>> Handle(ListPendingProviderNotificationsQuery command, CancellationToken ct)
    {
        actor.RequirePermission(PermissionCodes.SettingsManage);

        return await store.ListPendingAsync(actor.EnterpriseId, ct);
    }
}
