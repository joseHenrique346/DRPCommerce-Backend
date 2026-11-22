using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Context;

public partial class StoreCommerceDbContext
{
    private void CaptureAccessActivities()
    {
        foreach (var entry in ChangeTracker.Entries<EnterpriseMembership>().Where(Entry =>
            Entry.State is EntityState.Added or EntityState.Modified).ToList())
        {
            if (entry.Property(Membership => Membership.IsContractBlocked).IsModified)
                continue;

            var type = entry.State == EntityState.Added ? "membership_added" :
                entry.Property(Membership => Membership.IsActive).IsModified ?
                    entry.Entity.IsActive ? "membership_activated" : "membership_deactivated" :
                entry.Property(Membership => Membership.KindId).IsModified ? "membership_kind_changed" : null;

            if (type is not null)
                AddStoreActivity(entry.Entity.EnterpriseId, type, null, null, "access_management",
                    new { membershipId = entry.Entity.Id > 0 ? entry.Entity.Id : (long?)null, kindId = entry.Entity.KindId });
        }

        foreach (var entry in ChangeTracker.Entries<EnterpriseInvitation>().Where(Entry =>
            Entry.State is EntityState.Added or EntityState.Modified).ToList())
        {
            var type = entry.State == EntityState.Added ? "invitation_created" :
                entry.Property(Invitation => Invitation.AcceptedAt).IsModified ? "invitation_accepted" :
                entry.Property(Invitation => Invitation.RevokedAt).IsModified ? "invitation_revoked" : null;

            if (type is not null)
                AddStoreActivity(entry.Entity.EnterpriseId, type, null, null, "access_management",
                    new { invitationId = entry.Entity.Id > 0 ? entry.Entity.Id : (long?)null, kindId = entry.Entity.InvitedKindId });
        }

        foreach (var entry in ChangeTracker.Entries<EnterpriseDomain>().Where(Entry =>
            Entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList())
        {
            if (entry.Entity.IsContractBlocked && entry.Property(Domain => Domain.IsContractBlocked).IsModified)
                continue;

            var type = entry.State == EntityState.Added ? "domain_registered" :
                entry.State == EntityState.Deleted ? "domain_deleted" :
                entry.Property(Domain => Domain.IsVerified).IsModified ? "domain_verified" : "domain_publication_changed";
            AddStoreActivity(entry.Entity.EnterpriseId, type, null, null, "domain_management",
                new { domainId = entry.Entity.Id > 0 ? entry.Entity.Id : (long?)null, entry.Entity.IsActive, entry.Entity.IsPrimary });
        }
    }
}
