using MediatR;
using Microsoft.Extensions.Options;
using StoreCommerce.Application.Integrations.MercadoPago;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.Integrations;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class BeginMercadoPagoAuthorizationCommandHandler(
    ICurrentIdentity actor,
    IProviderAuthorizationStore authorizations,
    IIdentityUnitOfWork unitOfWork,
    IntegrationWriteScope scope,
    IMercadoPagoAuthorizationClient client,
    MercadoPagoAuthorizationCryptography secrets,
    ProviderAuthorizationAccessPolicy access,
    IOptions<MercadoPagoOptions> options,
    TimeProvider time)
    : IRequestHandler<BeginMercadoPagoAuthorizationCommand, MercadoPagoAuthorizationStart>
{
    private MercadoPagoOptions Settings => options.Value;

    public async Task<MercadoPagoAuthorizationStart> Handle(BeginMercadoPagoAuthorizationCommand command, CancellationToken ct)
    {
        var environment = command.Environment;

        actor.RequirePermission(PermissionCodes.SettingsManage);
        Settings.RequireOAuth(environment);
        secrets.RequireKeyRing();
        var version = await access.RequireAsync(actor.EnterpriseId, actor.UserId, actor.MembershipId, null, ct);
        var state = MercadoPagoAuthorizationCryptography.Random();
        var browser = MercadoPagoAuthorizationCryptography.Random();
        var verifier = MercadoPagoAuthorizationCryptography.Random();
        var authorization = ProviderAuthorization.Create(actor.EnterpriseId, actor.UserId, actor.MembershipId, version,
            MercadoPagoAuthorizationCryptography.Hash(state), MercadoPagoAuthorizationCryptography.Hash(browser), secrets.ProtectVerifier(verifier), environment, time.GetUtcNow().UtcDateTime);
        authorizations.Add(authorization);

        using (scope.Begin(actor.EnterpriseId))
            await unitOfWork.CommitAsync(ct);

        var url = client.AuthorizationUrl(state, verifier);

        return new MercadoPagoAuthorizationStart(url, browser, authorization.ExpiresAt);
    }
}
