using MediatR;
using Microsoft.Extensions.Options;
using StoreCommerce.Application.Integrations.MercadoPago;
using StoreCommerce.Application.Integrations.Providers.Interfaces;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetMercadoPagoIntegrationQueryHandler(
    ICurrentIdentity actor,
    IEntityRepository<PaymentProviderSelection> selections,
    IProviderConnectionStore connections,
    IOptions<MercadoPagoOptions> options)
    : IRequestHandler<GetMercadoPagoIntegrationQuery, MercadoPagoIntegrationSummary>
{
    public async Task<MercadoPagoIntegrationSummary> Handle(GetMercadoPagoIntegrationQuery command, CancellationToken ct)
    {
        actor.RequirePermission(PermissionCodes.SettingsManage);
        var selected = await selections.FindAsync(Selection => Selection.EnterpriseId == actor.EnterpriseId, ct, ignoreQueryFilters: true, tracking: false);
        var available = (await connections.ListAsync(actor.EnterpriseId, ct))
            .Where(Connection => Connection.Provider == IntegrationProviders.MercadoPago)
            .Select(Connection => new MercadoPagoConnectionSummary(Connection.Id, Connection.Environment, Connection.ExternalAccountId,
                Connection.IsConnected, Connection.AccessTokenExpiresAt, Connection.ConnectedAt, Connection.DisconnectedAt,
                options.Value.WebhookUrl(Connection.Id))).ToList();

        return new MercadoPagoIntegrationSummary(options.Value.Enabled, options.Value.AllowProduction, options.Value.ClientId,
            selected?.Provider ?? IntegrationProviders.Manual, selected?.Environment, available);
    }
}
