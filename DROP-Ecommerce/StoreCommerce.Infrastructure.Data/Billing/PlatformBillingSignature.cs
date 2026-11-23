using StoreCommerce.Application.Integrations.MercadoPago;
using StoreCommerce.Application.Interfaces.Billing;
using StoreCommerce.Application.Billing;
using Microsoft.Extensions.Options;

namespace StoreCommerce.Infrastructure.Data.Billing;

public sealed class PlatformBillingSignature(IOptions<PlatformBillingOptions> options, TimeProvider time) : IPlatformBillingSignature
{
    public string Verify(string paymentId, string requestId, string signature)
    {
        options.Value.RequireAccount();

        return MercadoPagoWebhookSignature.Verify(paymentId, requestId, signature,
            options.Value.WebhookSecret, options.Value.SignatureToleranceSeconds, time.GetUtcNow());
    }
}
