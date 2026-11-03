using StoreCommerce.Application.Payments;

namespace StoreCommerce.Infrastructure.Data.Payments;

/// <summary>
/// Gateway provisório: não chama serviço externo. A cobrança fica pendente até o backoffice confirmar o recebimento,
/// e o estorno é registrado como concluído para ser feito fora do sistema. Substitua pelo adaptador do gateway real
/// no registro de dependências.
/// </summary>
public sealed class ManualPaymentGateway : IPaymentGateway
{
    public const string ProviderName = "manual";

    public string Provider => ProviderName;

    public Task<PaymentSession> CreatePaymentAsync(PaymentRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new PaymentSession($"manual-{request.EnterpriseId}-{request.OrderId}", null,
            "Pagamento aguardando confirmação manual da loja.", request.ExpiresAt));

    public Task CancelPaymentAsync(string gatewayReference, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<RefundResult> RefundAsync(RefundRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new RefundResult(true, $"manual-refund-{request.EnterpriseId}-{request.OrderId}", null));
}
