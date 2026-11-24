using StoreCommerce.Application.Integrations.Providers.Interfaces;
using StoreCommerce.Application.Interfaces.Identity;
using MediatR;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Integrations;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetProviderConnectionsQueryHandler(IProviderConnectionStore connections, ICurrentIdentity actor)
    : WorkflowRequestHandler<GetProviderConnectionsQuery, IReadOnlyList<ProviderConnectionInfo>>
{
    protected override async Task<IReadOnlyList<ProviderConnectionInfo>> ExecuteAsync(GetProviderConnectionsQuery request, CancellationToken ct)
    {
        actor.RequirePermission(PermissionCodes.SettingsManage);
        var connectionsForStore = await connections.ListAsync(actor.EnterpriseId, ct);

        return connectionsForStore.Select(Connection => new ProviderConnectionInfo(Connection.Id, Connection.Provider,
            Connection.Environment, Connection.ExternalAccountId, Connection.Scopes, Connection.IsConnected,
            Connection.AccessTokenExpiresAt, Connection.RefreshTokenExpiresAt, Connection.ConnectedAt, Connection.DisconnectedAt)).ToList();
    }
}
