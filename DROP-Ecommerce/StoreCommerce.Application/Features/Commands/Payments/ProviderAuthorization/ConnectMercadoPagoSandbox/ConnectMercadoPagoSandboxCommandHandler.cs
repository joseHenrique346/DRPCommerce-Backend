using StoreCommerce.Application.Integrations.Providers.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Integrations.MercadoPago;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.Integrations;
using StoreCommerce.Application.Interfaces.Payments;
using StoreCommerce.Domain.Enums.Integrations;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ConnectMercadoPagoSandboxCommandHandler(
    ICurrentIdentity actor,
    IProviderAuthorizationStore authorizations,
    IMercadoPagoAuthorizationClient client,
    ProviderAuthorizationSecurity security,
    ProviderAuthorizationAccessPolicy access,
    IProviderConnectionCredentials connections,
    IPaymentGatewayRegistry gateways,
    IOptions<MercadoPagoOptions> options,
    TimeProvider time)
    : IRequestHandler<ConnectMercadoPagoSandboxCommand>
{
    private MercadoPagoOptions Settings => options.Value;

    public async Task Handle(ConnectMercadoPagoSandboxCommand command, CancellationToken ct)
    {
        actor.RequirePermission(PermissionCodes.SettingsManage);
        Settings.RequireCheckout(IntegrationEnvironment.Sandbox);
        security.RequireKeyRing();

        if (string.IsNullOrWhiteSpace(Settings.SandboxAccessToken))
            throw new SecurityFailure(409, "MercadoPago.SandboxTokenRequired", "Configure o token do vendedor de teste no servidor.");

        var accountId = await client.SandboxAccountAsync(ct);
        await using var transaction = await authorizations.BeginSerializableTransactionAsync(ct);
        await access.RequireAsync(actor.EnterpriseId, actor.UserId, actor.MembershipId, null, "MercadoPago.AuthorizationRevoked", ct);
        // Prazo local curto para uma chave de teste estática; não inventa validade remota nem refresh token.
        await connections.SaveAuthorizedAsync(actor.EnterpriseId, IntegrationProviders.MercadoPago, IntegrationEnvironment.Sandbox,
            accountId, "sandbox-static", new ProviderTokens(Settings.SandboxAccessToken, null,
                time.GetUtcNow().UtcDateTime.AddHours(1), null), actor.UserId, ct);
        await gateways.SelectAsync(actor.EnterpriseId, IntegrationProviders.MercadoPago, IntegrationEnvironment.Sandbox, ct);
        await transaction.CommitAsync(ct);
    }
}
