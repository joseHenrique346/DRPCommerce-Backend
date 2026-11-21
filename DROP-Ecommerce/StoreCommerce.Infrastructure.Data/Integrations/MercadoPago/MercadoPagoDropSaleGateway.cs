using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;

public sealed class MercadoPagoDropSaleGateway(IPaymentGatewayRegistry gateways, MercadoPagoGateway mercadoPago) : IDropSaleGateway
{
    public async Task<PaymentProviderContext> ContextAsync(long enterpriseId, CancellationToken ct)
    {
        var context = await gateways.GetContextAsync(enterpriseId, ct);

        if (context.Provider != IntegrationProviders.MercadoPago)
            throw new SecurityFailure(409, "DropPayment.ProviderRequired", "Conecte e selecione Mercado Pago nesta loja para vender pelo DROP.");

        return context;
    }

    public Task<PaymentSession?> CreateAsync(PaymentRequest request, bool recoverOnly, CancellationToken ct)
    {
        return recoverOnly ? mercadoPago.RecoverPaymentAsync(request, ct) : CreateNewAsync(request, ct);
    }

    private async Task<PaymentSession?> CreateNewAsync(PaymentRequest request, CancellationToken ct)
    {
        return await mercadoPago.CreatePaymentAsync(request, ct);
    }

    public async Task<DropSaleGatewayResult> ReadAsync(PaymentRequest request, CancellationToken ct)
    {
        var results = await mercadoPago.SearchPaymentsAsync(request.EnterpriseId, request.Context, request.ExternalReference, ct);

        foreach (var payment in results)
        {
            MercadoPagoGateway.ValidatePayment(payment, request.ExternalReference, request.Amount, request.CurrencyCode);

            if (payment.RefundedAmount != 0 || payment.Status is "refunded" or "charged_back")
                return new DropSaleGatewayResult("review_required", payment.Id);
        }

        var approved = results.Where(Payment => Payment.Status == "approved").ToList();

        if (approved.Count > 1)
            return new DropSaleGatewayResult("review_required", null);

        return approved.Count == 1 ? new DropSaleGatewayResult("approved", approved[0].Id) :
            new DropSaleGatewayResult("pending", null);
    }
}
