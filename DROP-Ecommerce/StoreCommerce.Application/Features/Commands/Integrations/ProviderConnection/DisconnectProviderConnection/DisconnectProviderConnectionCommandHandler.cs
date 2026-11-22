using StoreCommerce.Application.Integrations.Providers.Interfaces;
using StoreCommerce.Application.Interfaces.Identity;
using MediatR;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Integrations;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

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
