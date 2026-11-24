using StoreCommerce.Application.Integrations.MercadoPago;
using StoreCommerce.Application.Payments;

namespace StoreCommerce.Application.Interfaces.Payments;

public interface IMercadoPagoPaymentReader
{
    Task<MercadoPagoPayment> GetPaymentAsync(long enterpriseId, PaymentProviderContext context, string paymentId, CancellationToken ct);
    Task<IReadOnlyList<MercadoPagoPayment>> SearchPaymentsAsync(long enterpriseId, PaymentProviderContext context, string externalReference, CancellationToken ct);
}
