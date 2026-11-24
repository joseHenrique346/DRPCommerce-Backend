using StoreCommerce.Application.Payments;

namespace StoreCommerce.Application.Interfaces.Payments;

public interface IDropSaleGateway
{
    Task<PaymentProviderContext> ContextAsync(long enterpriseId, CancellationToken ct);
    Task<PaymentSession?> CreateAsync(PaymentRequest request, bool recoverOnly, CancellationToken ct);
    Task<DropSaleGatewayResult> ReadAsync(PaymentRequest request, CancellationToken ct);
}
