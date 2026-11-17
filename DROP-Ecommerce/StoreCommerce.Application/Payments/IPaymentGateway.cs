using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Payments;

/// <summary>
/// Porta para o gateway de pagamento. A implementação real (Mercado Pago, Stripe, Pagar.me...) fica na Infrastructure;
/// enquanto não existe integração, o ManualPaymentGateway registra cobranças confirmadas pelo backoffice.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Identificador gravado em Transaction.GatewayProvider.
    /// </summary>
    string Provider { get; }

    Task<PaymentProviderContext> GetContextAsync(long enterpriseId, CancellationToken cancellationToken);

    /// <summary>
    /// Consulta uma criação possivelmente concluída usando a correlação persistida. null significa resultado
    /// ainda desconhecido; nunca deve criar outra cobrança neste método.
    /// </summary>
    Task<PaymentSession?> RecoverPaymentAsync(PaymentRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Cria a cobrança do pedido. A confirmação é assíncrona: o resultado chega depois (webhook ou confirmação manual)
    /// e é aplicado por PaymentResultService.
    /// </summary>
    Task<PaymentSession> CreatePaymentAsync(PaymentRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Cancela uma cobrança ainda não paga (ex.: Pix ou boleto de um pedido expirado).
    /// </summary>
    Task CancelPaymentAsync(string gatewayReference, CancellationToken cancellationToken);

    /// <summary>
    /// Estorna um pagamento capturado.
    /// </summary>
    Task<RefundResult> RefundAsync(RefundRequest request, CancellationToken cancellationToken);
}

/// <param name="IdempotencyKey">Chave estável por pedido, para o gateway não cobrar duas vezes.</param>
public sealed record PaymentRequest(
    long EnterpriseId,
    long OrderId,
    long CustomerId,
    string CustomerName,
    string CustomerEmail,
    decimal Amount,
    string CurrencyCode,
    long TransactionMethodId,
    DateTime ExpiresAt,
    string IdempotencyKey,
    PaymentProviderContext Context,
    string ExternalReference);

public sealed record PaymentProviderContext(string Provider, IntegrationEnvironment Environment,
    string ExternalAccountId, long? ProviderConnectionId);

/// <summary>
/// Cobrança criada. CheckoutUrl (cartão/redirecionamento) e Instructions (código Pix, linha digitável) são exibidos ao cliente.
/// </summary>
public sealed record PaymentSession(string Reference, string? CheckoutUrl, string? Instructions, DateTime? ExpiresAt,
    string? PreferenceId = null, string? PaymentId = null);

public sealed record RefundRequest(long EnterpriseId, long OrderId, string GatewayReference, decimal Amount, string Reason, string IdempotencyKey);

public sealed record RefundResult(bool Succeeded, string Reference, string? FailureReason);

/// <summary>
/// Resultado de pagamento informado pelo gateway (webhook) ou pelo backoffice (confirmação manual).
/// </summary>
public enum PaymentResultStatus
{
    Approved = 1,
    Failed = 2
}
