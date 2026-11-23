using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Interfaces.Integrations;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ProviderAuthorizationAccessPolicy(
    IProviderAuthorizationStore authorizations)
{
    public async Task<long> RequireAsync(long enterpriseId, long userId, long membershipId, long? expectedVersion, CancellationToken ct)
    {
        var membership = await authorizations.FindMembershipAsync(enterpriseId, userId, membershipId, ct);

        if (membership is null || !membership.User.IsActive || !membership.Enterprise.IsActive ||
            expectedVersion.HasValue && membership.User.SecurityVersion != expectedVersion)
            throw new SecurityFailure(403, "MercadoPago.AuthorizationRevoked", "O acesso à loja mudou. Inicie uma nova autorização.");

        var allowed = membership.KindId == EnterpriseMembershipKind.PrimaryOwner.Id ||
            await authorizations.HasSettingsPermissionAsync(enterpriseId, membershipId, ct);

        if (!allowed)
            throw new SecurityFailure(403, "Security.PermissionRequired", "Permissão settings.manage necessária.");

        return membership.User.SecurityVersion;
    }
}
