using Microsoft.Extensions.Options;
using StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;

namespace StoreCommerce.Infrastructure.Data.Billing;

public sealed class PlatformBillingSignature(IOptions<PlatformBillingOptions> options, TimeProvider time)
{
    public string Verify(string paymentId, string requestId, string signature)
    {
        options.Value.RequireAccount();

        return MercadoPagoWebhookSignature.Verify(paymentId, requestId, signature,
            options.Value.WebhookSecret, options.Value.SignatureToleranceSeconds, time.GetUtcNow());
    }
}
