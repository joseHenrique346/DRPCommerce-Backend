using MediatR;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record GetProviderConnectionsQuery : IRequest<Result<IReadOnlyList<ProviderConnectionInfo>>>;
public sealed record DisconnectProviderConnectionCommand(string Provider, IntegrationEnvironment Environment) : IRequest<Result<bool>>;

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

public sealed class DisconnectProviderConnectionCommandHandler(IProviderConnectionStore connections, ICurrentIdentity actor, TimeProvider time)
    : WorkflowRequestHandler<DisconnectProviderConnectionCommand, bool>
{
    protected override async Task<bool> ExecuteAsync(DisconnectProviderConnectionCommand request, CancellationToken ct)
    {
        actor.RequirePermission(PermissionCodes.SettingsManage);
        IntegrationProviders.Validate(request.Provider, request.Environment);
        await connections.DisconnectAsync(actor.EnterpriseId, request.Provider, request.Environment, time.GetUtcNow().UtcDateTime, ct);

        return true;
    }
}
