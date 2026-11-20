using Microsoft.EntityFrameworkCore;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Context;

public partial class StoreCommerceDbContext
{
    private void GuardPlatformBillingWrites()
    {
        foreach (var entry in ChangeTracker.Entries().Where(Entry =>
            Entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted &&
            Entry.Entity is PlatformSubscription or PlatformBillingInvoice or PlatformBillingNotification))
        {
            if (!_billingWrites.IsOpen || entry.State == EntityState.Deleted)
                throw new SecurityFailure(403, "Billing.UseWorkflow", "Use o fluxo financeiro da plataforma.");

            if (entry.State != EntityState.Modified)
                continue;

            var mutable = entry.Entity switch
            {
                PlatformSubscription => new[] { "OfferId", "RemoteId", "CheckoutUrl", "SubmittedAt", "RemoteStatus", "PeriodStart", "BillingAnchor", "PaidUntil",
                    "AppliedContractRevision", "PendingOfferId", "ChangeEffectiveAt", "IsUpgrade", "ChangeSynchronized", "RetainedMembershipIdsJson", "ReviewCode", "NextCheckAt", "ClosedAt", "UpdatedAt" },
                PlatformBillingInvoice => new[] { "SubmittedAt", "PreferenceId", "CheckoutUrl", "PaymentId", "PaymentUpdatedAt", "Status", "AppliedAt", "ReviewCode", "UpdatedAt" },
                _ => new[] { "NextCheckAt", "CompletedAt", "UpdatedAt" }
            };

            if (entry.Properties.Any(Property => Property.IsModified && !mutable.Contains(Property.Metadata.Name)))
                throw new SecurityFailure(403, "Billing.ImmutableTerms", "Os termos financeiros persistidos são imutáveis.");
        }
    }
}
