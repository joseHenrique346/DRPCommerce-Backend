using StoreCommerce.Application.Interfaces.Payments;
using StoreCommerce.Domain.Enums.Integrations;
using StoreCommerce.Domain.Enums.Payments;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Payments;

/// <summary>
/// Gateway provisório: não chama serviço externo. A cobrança fica pendente até o backoffice confirmar o recebimento,
/// e o estorno só é concluído após confirmação explícita do operador. Substitua pelo adaptador do gateway real
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

    public Task<PaymentOperationResult> ExecuteOperationAsync(PaymentOperationRequest request, CancellationToken cancellationToken)
    {
        var result = request.Kind == PaymentOperationKind.Cancel
            ? new PaymentOperationResult(PaymentOperationStatus.Succeeded, $"manual-cancel-{request.IdempotencyKey}")
            : new PaymentOperationResult(PaymentOperationStatus.AwaitingManualConfirmation);

        return Task.FromResult(result);
    }

    public Task<PaymentOperationResult> RecoverOperationAsync(PaymentOperationRequest request, CancellationToken cancellationToken)
    {
        return ExecuteOperationAsync(request, cancellationToken);
    }

    private static PaymentSession CreateSession(PaymentRequest request)
    {
        return new PaymentSession($"manual-{request.EnterpriseId}-{request.OrderId}", null,
            "Pagamento aguardando confirmação manual da loja.", request.ExpiresAt);
    }
}
