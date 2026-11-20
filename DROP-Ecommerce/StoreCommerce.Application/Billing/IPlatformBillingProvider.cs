using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Billing;

/// <summary>
/// Termos definidos e persistidos pelo servidor antes de chamar o provedor.
/// Não representa renovação automática nem concede direitos sobre a loja.
/// </summary>
public sealed record PlatformBillingCharge(string ExternalReference, string AccountId,
    IntegrationEnvironment Environment, decimal Amount, string Currency, DateTime CheckoutExpiresAt);

public sealed record PlatformBillingCheckout(string PreferenceId, string CheckoutUrl);

public sealed record PlatformBillingPayment(string Id, string ExternalReference, string AccountId,
    decimal Amount, string Currency, string Status, decimal RefundedAmount, DateTime UpdatedAt);

public interface IPlatformBillingProvider
{
    Task ValidateAccountAsync(PlatformBillingCharge charge, CancellationToken ct);
    Task<PlatformBillingCheckout> CreateAsync(PlatformBillingCharge charge, CancellationToken ct);
    Task<PlatformBillingCheckout?> RecoverAsync(PlatformBillingCharge charge, CancellationToken ct);
    Task<PlatformBillingPayment> GetPaymentAsync(string paymentId, CancellationToken ct);
    Task<IReadOnlyList<PlatformBillingPayment>> SearchPaymentsAsync(string externalReference, CancellationToken ct);
}
