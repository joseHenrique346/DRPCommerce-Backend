using Microsoft.EntityFrameworkCore;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Contracts;

namespace StoreCommerce.Infrastructure.Data.Context;

public partial class StoreCommerceDbContext
{
    private async Task<AppearanceCapabilities> ReadAppearanceWriteCapabilitiesAsync(long tenant, CancellationToken ct)
    {
        var rights = await ReadAppearanceCapabilitiesAsync(tenant, "store", ct);
        var principal = _httpAccessor.HttpContext?.User;

        if (principal?.FindFirst(SharedCommerce.Application.Security.SecurityNames.Actor)?.Value == "user")
        {
            var userId = SharedCommerce.Application.Security.SecurityNames.Id(principal, "sub");
            var membershipId = SharedCommerce.Application.Security.SecurityNames.Id(principal,
                SharedCommerce.Application.Security.SecurityNames.Membership);

            if (!await EnterpriseMemberships.IgnoreQueryFilters().AnyAsync(Membership =>
                Membership.Id == membershipId && Membership.UserId == userId && Membership.EnterpriseId == tenant &&
                Membership.IsActive && !Membership.IsContractBlocked && Membership.KindId == 1 && Membership.User.IsActive, ct))
                throw new AppearanceFailure(403, "Appearance.OwnerRequired", "O dono principal mudou durante a operação.");
        }

        return rights;
    }

    public async Task<AppearanceCapabilities> ReadAppearanceCapabilitiesAsync(long tenant, string channel, CancellationToken ct)
    {
        if (Database.CurrentTransaction is not null)
            await Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM empresa WHERE id = {tenant} FOR UPDATE", ct);

        var enterprise = await Enterprises.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(Enterprise => Enterprise.Id == tenant && Enterprise.IsActive, ct)
            ?? throw new AppearanceFailure(404, "Appearance.TargetNotFound", "Loja indisponível.");
        var rights = EnterpriseEntitlements.Resolve(enterprise.PlanId, enterprise.AddOnId);

        if (channel is not ("store" or "drop"))
            throw new AppearanceFailure(400, "Appearance.InvalidChannel", "Canal de aparência inválido.");

        return new AppearanceCapabilities(tenant, channel, enterprise.ContractRevision,
            channel == "store" ? rights.AllowsStore : rights.AllowsDrop,
            channel == "store" ? rights.StoreThemeLimit : rights.DropThemeLimit,
            rights.AllowsLogoCustomization, rights.AllowsFullCustomization);
    }
}
