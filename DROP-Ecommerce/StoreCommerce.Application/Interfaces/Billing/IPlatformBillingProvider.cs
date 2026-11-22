using StoreCommerce.Application.Billing;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Interfaces.Billing;

public interface IPlatformBillingProvider
{
    Task ValidateAccountAsync(PlatformBillingCharge charge, CancellationToken ct);
    Task<PlatformBillingCheckout> CreateAsync(PlatformBillingCharge charge, CancellationToken ct);
    Task<PlatformBillingCheckout?> RecoverAsync(PlatformBillingCharge charge, CancellationToken ct);
    Task<PlatformBillingPayment> GetPaymentAsync(string paymentId, CancellationToken ct);
    Task<IReadOnlyList<PlatformBillingPayment>> SearchPaymentsAsync(string externalReference, CancellationToken ct);
}
