using Microsoft.EntityFrameworkCore;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Context;

public partial class StoreCommerceDbContext
{
    private void GuardDropSalePayments()
    {
        foreach (var entry in ChangeTracker.Entries<DropSalePaymentIntent>().Where(Entry => Entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            if (_dropSalePayments.EnterpriseId != entry.Entity.EnterpriseId || entry.State == EntityState.Deleted ||
                entry.State == EntityState.Modified && entry.Properties.Any(Property => Property.IsModified &&
                    Property.Metadata.Name is not ("SubmittedAt" or "PreferenceId" or "CheckoutUrl" or "PaymentId" or "Status" or "Revision" or "AcknowledgedRevision" or "FulfillmentClosed" or "FulfillmentReviewRequired" or "NextCheckAt" or "UpdatedAt")))
                throw new SecurityFailure(403, "DropPayment.UseWorkflow", "Use o pagamento confiável de vendas DROP.");
        }
    }
}
