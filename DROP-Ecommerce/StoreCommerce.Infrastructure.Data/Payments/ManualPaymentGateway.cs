using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Integrations;

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

    public Task<PaymentProviderContext> GetContextAsync(long enterpriseId, CancellationToken cancellationToken)
    {
        return Task.FromResult(new PaymentProviderContext(Provider, IntegrationEnvironment.Sandbox, $"manual-{enterpriseId}", null));
    }

    public Task<PaymentSession> CreatePaymentAsync(PaymentRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(CreateSession(request));
    }

    public Task<PaymentSession?> RecoverPaymentAsync(PaymentRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult<PaymentSession?>(CreateSession(request));
    }

    public Task CancelPaymentAsync(string gatewayReference, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task<RefundResult> RefundAsync(RefundRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new RefundResult(true, $"manual-refund-{request.EnterpriseId}-{request.OrderId}", null));
    }

    private static PaymentSession CreateSession(PaymentRequest request)
    {
        return new PaymentSession($"manual-{request.EnterpriseId}-{request.OrderId}", null,
            "Pagamento aguardando confirmação manual da loja.", request.ExpiresAt);
    }
}
