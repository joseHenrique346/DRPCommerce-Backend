using MediatR;
using Microsoft.Extensions.Options;
using StoreCommerce.Application.Integrations.MercadoPago;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.Integrations;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class BeginMercadoPagoAuthorizationCommandHandler(
    ICurrentIdentity actor,
    IProviderAuthorizationStore authorizations,
    IIdentityUnitOfWork unitOfWork,
    IntegrationWriteScope scope,
    IMercadoPagoAuthorizationClient client,
    MercadoPagoAuthorizationCryptography secrets,
    ProviderAuthorizationSecurity security,
    ProviderAuthorizationAccessPolicy access,
    IOptions<MercadoPagoOptions> options,
    TimeProvider time)
    : IRequestHandler<BeginMercadoPagoAuthorizationCommand, ProviderAuthorizationStart>
{
    private MercadoPagoOptions Settings => options.Value;

    public async Task<ProviderAuthorizationStart> Handle(BeginMercadoPagoAuthorizationCommand command, CancellationToken ct)
    {
        var environment = command.Environment;

        actor.RequirePermission(PermissionCodes.SettingsManage);
        Settings.RequireOAuth(environment);
        security.RequireKeyRing();
        var version = await access.RequireAsync(actor.EnterpriseId, actor.UserId, actor.MembershipId, null,
            "MercadoPago.AuthorizationRevoked", ct);
        var state = ProviderAuthorizationSecurity.Random();
        var browser = ProviderAuthorizationSecurity.Random();
        var verifier = ProviderAuthorizationSecurity.Random();
        var authorization = ProviderAuthorization.Create(actor.EnterpriseId, actor.UserId, actor.MembershipId, version,
            ProviderAuthorizationSecurity.Hash(state), ProviderAuthorizationSecurity.Hash(browser), secrets.ProtectVerifier(verifier),
            IntegrationProviders.MercadoPago, environment, time.GetUtcNow().UtcDateTime);
        authorizations.Add(authorization);

        using (scope.Begin(actor.EnterpriseId))
            await unitOfWork.CommitAsync(ct);

        var url = client.AuthorizationUrl(state, verifier);

        return new ProviderAuthorizationStart(url, browser, authorization.ExpiresAt);
    }
}
