using Microsoft.EntityFrameworkCore;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Context;

public partial class StoreCommerceDbContext
{
    private void GuardDropContractPublications()
    {
        foreach (var entry in ChangeTracker.Entries<DropContractPublication>().Where(Entry =>
            Entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            if (!_dropAcknowledgments.IsOpen || entry.State != EntityState.Modified ||
                entry.Properties.Any(Property => Property.IsModified &&
                    Property.Metadata.Name is not ("AcknowledgedRevision" or "AcknowledgedAt" or "UpdatedAt")))
                throw new SecurityFailure(403, "DropContract.UseWorkflow", "Use a propagação confiável de contrato.");
        }
    }

    private async Task CaptureDropContractPublicationsAsync(CancellationToken ct)
    {
        var changes = ChangeTracker.Entries<Enterprise>().Where(Entry => Entry.State == EntityState.Added ||
            Entry.State == EntityState.Modified && ContractFieldsChanged(Entry)).ToList();

        foreach (var entry in changes)
        {
            var enterprise = entry.Entity;
            var publication = entry.State == EntityState.Added ? null :
                await Set<DropContractPublication>().IgnoreQueryFilters()
                    .SingleOrDefaultAsync(Publication => Publication.EnterpriseId == enterprise.Id, ct);

            if (publication is null)
                Add(DropContractPublication.Capture(enterprise, DateTime.UtcNow));
            else
                publication.Publish(enterprise.ContractRevision, DateTime.UtcNow);
        }
    }
}
