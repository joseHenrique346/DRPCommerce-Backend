using MediatR;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Integrations.MelhorEnvio;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.Integrations;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class BeginMelhorEnvioAuthorizationCommandHandler(
    ICurrentIdentity actor,
    IProviderAuthorizationStore authorizations,
    IIdentityUnitOfWork unitOfWork,
    IntegrationWriteScope scope,
    IMelhorEnvioAuthorizationClient client,
    ProviderAuthorizationSecurity security,
    ProviderAuthorizationAccessPolicy access,
    TimeProvider time)
    : IRequestHandler<BeginMelhorEnvioAuthorizationCommand, ProviderAuthorizationStart>
{
    public async Task<ProviderAuthorizationStart> Handle(BeginMelhorEnvioAuthorizationCommand command, CancellationToken ct)
    {
        actor.RequirePermission(PermissionCodes.SettingsManage);
        BeginMelhorEnvioAuthorizationCommandValidator.Validate(command);

        var state = ProviderAuthorizationSecurity.Random();
        var url = client.AuthorizationUrl(command.Environment, state);
        security.RequireKeyRing();
        var version = await access.RequireAsync(actor.EnterpriseId, actor.UserId, actor.MembershipId, null,
            "MelhorEnvio.AuthorizationRevoked", ct);
        var browser = ProviderAuthorizationSecurity.Random();
        var authorization = ProviderAuthorization.Create(actor.EnterpriseId, actor.UserId, actor.MembershipId, version,
            ProviderAuthorizationSecurity.Hash(state), ProviderAuthorizationSecurity.Hash(browser), string.Empty,
            IntegrationProviders.MelhorEnvio, command.Environment, time.GetUtcNow().UtcDateTime);
        authorizations.Add(authorization);

        using (scope.Begin(actor.EnterpriseId))
            await unitOfWork.CommitAsync(ct);

        return new ProviderAuthorizationStart(url, browser, authorization.ExpiresAt);
    }
}
