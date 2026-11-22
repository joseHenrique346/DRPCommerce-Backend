using StoreCommerce.Domain.Interfaces.Tenancy;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Context;

public partial class StoreCommerceDbContext
{
    private readonly HashSet<EnterpriseMembership> _membershipContractChanges = [];

    private List<long> AccessContractTenants()
    {
        return ChangeTracker.Entries().Where(Entry =>
                Entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted &&
                Entry.Entity is EnterpriseMembership or EnterpriseInvitation or EnterpriseDomain)
            .Select(Entry => ((ITenantEntity)Entry.Entity).EnterpriseId)
            .Concat(ChangeTracker.Entries<Enterprise>().Where(Entry =>
                Entry.State == EntityState.Modified && ContractFieldsChanged(Entry)).Select(Entry => Entry.Entity.Id))
            .Distinct().OrderBy(Id => Id).ToList();
    }

    private async Task GuardAccessContractsAsync(CancellationToken ct)
    {
        foreach (var tenant in AccessContractTenants())
        {
            await Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM empresa WHERE id = {tenant} FOR UPDATE", ct);
            var stored = await Enterprises.IgnoreQueryFilters().AsNoTracking()
                .SingleAsync(Enterprise => Enterprise.Id == tenant, ct);
            var grant = ChangeTracker.Entries<Enterprise>().SingleOrDefault(Entry =>
                Entry.Entity.Id == tenant && Entry.State == EntityState.Modified && ContractFieldsChanged(Entry));
            var rights = EnterpriseEntitlements.Resolve(grant?.Entity.PlanId ?? stored.PlanId,
                grant is null ? stored.AddOnId : grant.Entity.AddOnId);

            if (grant is not null)
            {
                if (stored.ContractRevision != grant.Property(Enterprise => Enterprise.ContractRevision).OriginalValue)
                    throw new SecurityFailure(409, "Contract.RevisionChanged", "O contrato mudou durante a operação.");

                await ReconcileTeamContractAsync(tenant, rights, ct);
                await ReconcileDomainContractAsync(tenant, rights, ct);
            }

            await GuardMembershipCapacityAsync(tenant, rights, grant is not null, ct);
            GuardDomainContractWrites(tenant, rights);
        }

        CaptureAccessActivities();
    }

    private async Task ReconcileTeamContractAsync(long tenant, EnterpriseEntitlements rights, CancellationToken ct)
    {
        var members = await EnterpriseMemberships.IgnoreQueryFilters().Include(Membership => Membership.User)
            .Where(Membership => Membership.EnterpriseId == tenant).ToListAsync(ct);
        var active = TeamContractPolicy.ActiveAdditional(members);
        var selected = _contractWrites.RetainedMembershipIds;

        if (!TeamContractPolicy.IsValidSelection(selected, active, rights.EmployeeLimit))
            throw new SecurityFailure(400, "Team.InvalidSelection", "Escolha vínculos ativos da própria loja dentro do limite.");

        if (active.Count > rights.EmployeeLimit)
        {
            if (selected is null)
                throw new SecurityFailure(409, "Team.SelectionRequired", "O dono precisa escolher os acessos que permanecem.");

            foreach (var member in active.Where(Membership => !selected.Contains(Membership.Id)))
            {
                _membershipContractChanges.Add(member);
                member.BlockForContract(DateTime.UtcNow);
            }

            return;
        }

        var remaining = rights.EmployeeLimit - TeamContractPolicy.ActiveAdditional(members).Count;
        var pending = await EnterpriseInvitations.IgnoreQueryFilters().CountAsync(Invitation =>
            Invitation.EnterpriseId == tenant && Invitation.AcceptedAt == null && Invitation.RevokedAt == null &&
            Invitation.ExpiresAt > DateTime.UtcNow, ct);
        remaining = Math.Max(0, remaining - pending);

        foreach (var member in members.Where(Membership => Membership.IsContractBlocked && Membership.IsActive &&
            Membership.KindId != 1 && Membership.User.IsActive && !_membershipContractChanges.Contains(Membership))
            .OrderBy(Membership => Membership.ContractBlockedAt).ThenBy(Membership => Membership.JoinedAt)
            .ThenBy(Membership => Membership.Id).Take(remaining))
        {
            _membershipContractChanges.Add(member);
            member.UnblockForContract();
        }
    }

    private async Task GuardMembershipCapacityAsync(long tenant, EnterpriseEntitlements rights, bool granting, CancellationToken ct)
    {
        var savedMembers = await EnterpriseMemberships.IgnoreQueryFilters().AsNoTracking()
            .Include(Membership => Membership.User).Where(Membership => Membership.EnterpriseId == tenant).ToListAsync(ct);
        var current = await EnterpriseMemberships.IgnoreQueryFilters().Include(Membership => Membership.User)
            .Where(Membership => Membership.EnterpriseId == tenant).ToListAsync(ct);
        var changed = ChangeTracker.Entries<EnterpriseMembership>().Where(Entry => Entry.Entity.EnterpriseId == tenant &&
            Entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList();

        foreach (var entry in changed)
        {
            var member = entry.Entity;

            if (entry.State == EntityState.Deleted)
                throw new SecurityFailure(403, "Team.DeactivateRequired", "Desative o vínculo para preservar o histórico.");

            if (entry.State == EntityState.Added)
            {
                if (member.IsContractBlocked)
                    throw new SecurityFailure(403, "Team.InvalidBlock", "O bloqueio comercial é controlado pelo servidor.");

                current.Add(member);

                if (member.User is null)
                    await entry.Reference(Membership => Membership.User).Query().IgnoreQueryFilters().LoadAsync(ct);
            }

            if (member.KindId == 1 && member.IsContractBlocked)
                throw new SecurityFailure(409, "Team.OwnerProtected", "O dono principal não pode ser bloqueado pelo plano.");

            if (entry.State == EntityState.Modified && entry.Property(Membership => Membership.IsContractBlocked).IsModified &&
                member.IsContractBlocked && !_membershipContractChanges.Contains(member))
                throw new SecurityFailure(403, "Team.InvalidBlock", "O bloqueio comercial é controlado pelo servidor.");

            if (entry.State == EntityState.Modified && entry.Property(Membership => Membership.IsContractBlocked).IsModified)
                AddStoreActivity(tenant, member.IsContractBlocked ? "membership_contract_blocked" :
                    member.IsActive ? "membership_contract_restored" : "membership_deactivated",
                    null, null, member.IsActive ? "team_limit" : "access_management",
                    new { membershipId = member.Id, kindId = member.KindId });
        }

        var transferRecipient = _ownershipTransfer.EnterpriseId == tenant ? _ownershipTransfer.RecipientMembershipId : null;
        var before = TeamContractPolicy.ActiveAdditional(savedMembers).Count(Membership => Membership.Id != transferRecipient);
        var after = TeamContractPolicy.ActiveAdditional(current).Count(Membership => Membership.Id != transferRecipient);
        var invitations = await EnterpriseInvitations.IgnoreQueryFilters().AsNoTracking().Where(Invitation =>
            Invitation.EnterpriseId == tenant).ToListAsync(ct);
        var now = DateTime.UtcNow;
        var pendingBefore = invitations.Count(Invitation => Invitation.IsUsable(now));
        var invitationChanges = ChangeTracker.Entries<EnterpriseInvitation>().Where(Entry =>
            Entry.Entity.EnterpriseId == tenant && Entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList();
        var pendingAfter = pendingBefore;

        foreach (var entry in invitationChanges)
        {
            var saved = invitations.SingleOrDefault(Invitation => Invitation.Id == entry.Entity.Id);
            pendingAfter += entry.State != EntityState.Deleted && entry.Entity.IsUsable(now) ? 1 : 0;
            pendingAfter -= saved?.IsUsable(now) == true ? 1 : 0;
        }

        if (after > rights.EmployeeLimit && (after > before || granting))
            throw new SecurityFailure(409, "Team.MemberLimit", "Não há vagas para ativar esses acessos administrativos.");

        if (!granting && after + pendingAfter > rights.EmployeeLimit && after + pendingAfter > before + pendingBefore)
            throw new SecurityFailure(409, "Team.InvitationLimit", "As vagas disponíveis já estão ocupadas por acessos ou convites válidos.");
    }

    private async Task ReconcileDomainContractAsync(long tenant, EnterpriseEntitlements rights, CancellationToken ct)
    {
        if (rights.AllowsCustomDomain)
            return;

        var domains = await EnterpriseDomains.IgnoreQueryFilters().Where(Domain => Domain.EnterpriseId == tenant).ToListAsync(ct);
        var platform = domains.Where(Domain => Domain.IsVerified &&
                DomainHostPolicy.IsPlatformHost(Domain.NormalizedHost, _securityOptions.StorefrontBaseDomain))
            .OrderByDescending(Domain => Domain.IsPrimary).ThenBy(Domain => Domain.Id).FirstOrDefault();

        if (platform is null)
        {
            var host = HostName.Normalize("loja-" + tenant + "-" + Guid.NewGuid().ToString("N")[..8] + "." +
                _securityOptions.StorefrontBaseDomain);
            platform = EnterpriseDomain.Create(tenant, host, false, false);
            platform.Verify();
            EnterpriseDomains.Add(platform);
        }

        // Libera o índice parcial antes de promover o fallback, dentro da transação do contrato.
        if (domains.Any(Domain => Domain.IsPrimary && Domain.Id != platform.Id))
            await Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE dominio_empresa SET is_primary = FALSE WHERE enterprise_id = {tenant} AND is_primary", ct);

        foreach (var domain in domains.Where(Domain =>
            !DomainHostPolicy.IsPlatformHost(Domain.NormalizedHost, _securityOptions.StorefrontBaseDomain)))
        {
            if (domain.IsActive || !domain.IsContractBlocked)
                AddStoreActivity(tenant, "domain_contract_blocked", null, null, "custom_domain_unavailable", new { domainId = domain.Id });

            domain.BlockForContract();
        }

        foreach (var domain in domains.Where(Domain => Domain.IsPrimary && Domain != platform))
            domain.SetPublication(false, domain.IsActive);

        platform.SetPublication(true, true);
    }

    private void GuardDomainContractWrites(long tenant, EnterpriseEntitlements rights)
    {
        foreach (var entry in ChangeTracker.Entries<EnterpriseDomain>().Where(Entry =>
            Entry.Entity.EnterpriseId == tenant && Entry.State is EntityState.Added or EntityState.Modified))
        {
            var domain = entry.Entity;
            var platform = DomainHostPolicy.IsPlatformHost(domain.NormalizedHost, _securityOptions.StorefrontBaseDomain);

            if (!platform && !rights.AllowsCustomDomain && (entry.State == EntityState.Added || domain.IsActive ||
                entry.Property(Domain => Domain.IsVerified).IsModified && domain.IsVerified))
                throw new SecurityFailure(409, "Domain.PlanRequired", "O contrato não permite cadastrar, verificar ou ativar domínio próprio.");
        }
    }
}
