using StoreCommerce.Domain.Enums.Integrations;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;

namespace StoreCommerce.Application.Integrations.MercadoPago;

public sealed class MercadoPagoSignature(IOptions<MercadoPagoOptions> options, TimeProvider time)
{
    public string Verify(string paymentId, string requestId, string signature, IntegrationEnvironment environment = IntegrationEnvironment.Sandbox)
    {
        if (paymentId.Length is < 1 or > 40 || paymentId.Any(Character => !char.IsAsciiDigit(Character)))
            throw new SecurityFailure(401, "MercadoPago.InvalidSignature", "Identificador do pagamento inválido.");

        return MercadoPagoWebhookSignature.Verify(paymentId, requestId, signature,
            options.Value.SignatureSecret(environment), options.Value.SignatureToleranceSeconds, time.GetUtcNow());
    }
}
