using SharedCommerce.Domain.Exceptions;

namespace StoreCommerce.Application.Integrations.MercadoPago;

public static class MercadoPagoPaymentPolicy
{
    public static void ValidatePayment(MercadoPagoPayment payment, string? externalReference, decimal amount, string? currency)
    {
        if (payment.ExternalReference != externalReference || payment.Amount != amount || payment.CurrencyCode != currency)
            throw new SecurityFailure(409, "MercadoPago.PaymentMismatch", "Valor, moeda ou correlação do pagamento não correspondem ao pedido.");
    }
}
