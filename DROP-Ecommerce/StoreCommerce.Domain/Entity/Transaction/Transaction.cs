using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Domain.Entity;

public class Transaction : BaseEntity
{
    #region Constants
    public const int GatewayReferenceMaxLength = 200;
    public const int GatewayProviderMaxLength = 200;
    public const int GatewayPayloadMaxLength = 4000;
    #endregion

    #region Properties
    public long OrderId { get; private set; }
    public long CustomerId { get; private set; }
    public long TransactionTypeId { get; private set; }
    public long TransactionMethodId { get; private set; }
    public long TransactionStatusId { get; private set; }
    public decimal Amount { get; private set; }
    public decimal Fee { get; private set; }
    public string GatewayReference { get; private set; }
    public string GatewayProvider { get; private set; }
    public string GatewayPayload { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }

    #region Navigation Properties
    public Order Order { get; private set; }
    public Customer Customer { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public TransactionMethod TransactionMethod { get; private set; }
    public TransactionStatus TransactionStatus { get; private set; }
    #endregion

    #endregion

    #region Constructor
    protected Transaction() { }

    private Transaction(long orderId, long customerId, long transactionTypeId, long transactionMethodId, long transactionStatusId, decimal amount, decimal fee, string gatewayReference, string gatewayProvider, string gatewayPayload, DateTime? paidAt, DateTime? refundedAt)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TransactionTypeId = transactionTypeId;
        TransactionMethodId = transactionMethodId;
        TransactionStatusId = transactionStatusId;
        Amount = amount;
        Fee = fee;
        GatewayReference = gatewayReference;
        GatewayProvider = gatewayProvider;
        GatewayPayload = gatewayPayload;
        PaidAt = paidAt;
        RefundedAt = refundedAt;
    }
    #endregion

    #region Functions
    public static Transaction Create(long orderId, long customerId, long transactionTypeId, long transactionMethodId, long transactionStatusId, decimal amount, decimal fee, string gatewayReference, string gatewayProvider, string gatewayPayload, DateTime? paidAt, DateTime? refundedAt)
    {
        BaseValidate.ValidatePositive(orderId, nameof(OrderId));
        BaseValidate.ValidatePositive(customerId, nameof(CustomerId));
        BaseValidate.ValidatePositive(transactionTypeId, nameof(TransactionTypeId));
        BaseValidate.ValidatePositive(transactionMethodId, nameof(TransactionMethodId));
        BaseValidate.ValidatePositive(transactionStatusId, nameof(TransactionStatusId));
        BaseValidate.ValidatePositive(amount, nameof(Amount));
        BaseValidate.ValidatePositiveOrZero(fee, nameof(Fee));
        BaseValidate.ValidateMaxLength(gatewayReference, GatewayReferenceMaxLength, nameof(GatewayReference));
        BaseValidate.ValidateMaxLength(gatewayProvider, GatewayProviderMaxLength, nameof(GatewayProvider));
        BaseValidate.ValidateMaxLength(gatewayPayload, GatewayPayloadMaxLength, nameof(GatewayPayload));
        BaseValidate.ValidateNullableNotFuture(paidAt, nameof(PaidAt));
        BaseValidate.ValidateNullableNotFuture(refundedAt, nameof(RefundedAt));

        return new Transaction(orderId, customerId, transactionTypeId, transactionMethodId, transactionStatusId, amount, fee, gatewayReference, gatewayProvider, gatewayPayload, paidAt, refundedAt);
    }

    public void UpdateDetails(long transactionTypeId, long transactionMethodId, long transactionStatusId, decimal amount, decimal fee, string gatewayReference, string gatewayProvider, string gatewayPayload)
    {
        BaseValidate.ValidatePositive(transactionTypeId, nameof(TransactionTypeId));
        BaseValidate.ValidatePositive(transactionMethodId, nameof(TransactionMethodId));
        BaseValidate.ValidatePositive(transactionStatusId, nameof(TransactionStatusId));
        BaseValidate.ValidatePositive(amount, nameof(Amount));
        BaseValidate.ValidatePositiveOrZero(fee, nameof(Fee));
        BaseValidate.ValidateMaxLength(gatewayReference, GatewayReferenceMaxLength, nameof(GatewayReference));
        BaseValidate.ValidateMaxLength(gatewayProvider, GatewayProviderMaxLength, nameof(GatewayProvider));
        BaseValidate.ValidateMaxLength(gatewayPayload, GatewayPayloadMaxLength, nameof(GatewayPayload));

        TransactionTypeId = transactionTypeId;
        TransactionMethodId = transactionMethodId;
        TransactionStatusId = transactionStatusId;
        Amount = amount;
        Fee = fee;
        GatewayReference = gatewayReference;
        GatewayProvider = gatewayProvider;
        GatewayPayload = gatewayPayload;
    }

    /// <summary>
    /// Cria a transação de pagamento pendente do pedido, antes de o pedido ser salvo (o vínculo é feito pela navegação).
    /// </summary>
    public static Transaction CreatePendingPayment(Order order, long transactionMethodId, string gatewayProvider)
    {
        BaseValidate.ValidateNotNull(order, nameof(Order));
        ValidateMethod(transactionMethodId);
        BaseValidate.ValidateNotNullOrEmpty(gatewayProvider, nameof(GatewayProvider));
        BaseValidate.ValidateMaxLength(gatewayProvider, GatewayProviderMaxLength, nameof(GatewayProvider));

        return new Transaction
        {
            Order = order,
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TransactionTypeId = TransactionType.Payment.Id,
            TransactionMethodId = transactionMethodId,
            TransactionStatusId = TransactionStatus.Pending.Id,
            Amount = order.TotalAmount,
            Fee = 0,
            GatewayReference = string.Empty,
            GatewayProvider = gatewayProvider,
            GatewayPayload = string.Empty
        };
    }

    /// <summary>
    /// Registra o estorno do valor pago do pedido.
    /// </summary>
    public static Transaction CreateRefund(Transaction payment, string gatewayReference, DateTime now)
    {
        BaseValidate.ValidateNotNull(payment, nameof(Transaction));

        if (payment.TransactionTypeId != TransactionType.Payment.Id || payment.TransactionStatusId != TransactionStatus.Captured.Id)
            throw new ArgumentException("Somente pagamentos capturados podem ser estornados.");

        BaseValidate.ValidateMaxLength(gatewayReference, GatewayReferenceMaxLength, nameof(GatewayReference));
        payment.TransactionStatusId = TransactionStatus.Refunded.Id;
        payment.RefundedAt = now;

        return new Transaction
        {
            Order = payment.Order,
            OrderId = payment.OrderId,
            CustomerId = payment.CustomerId,
            TransactionTypeId = TransactionType.Refund.Id,
            TransactionMethodId = payment.TransactionMethodId,
            TransactionStatusId = TransactionStatus.Refunded.Id,
            Amount = payment.Amount,
            Fee = 0,
            GatewayReference = gatewayReference ?? string.Empty,
            GatewayProvider = payment.GatewayProvider,
            GatewayPayload = string.Empty,
            RefundedAt = now
        };
    }

    public bool IsPendingPayment => TransactionTypeId == TransactionType.Payment.Id && TransactionStatusId == TransactionStatus.Pending.Id;

    public bool IsCapturedPayment => TransactionTypeId == TransactionType.Payment.Id && TransactionStatusId == TransactionStatus.Captured.Id;

    /// <summary>
    /// Guarda os dados da cobrança devolvidos pelo gateway (referência e instruções para o cliente).
    /// </summary>
    public void AttachGatewayPayment(string gatewayReference, string gatewayPayload)
    {
        if (!IsPendingPayment)
            throw new ArgumentException("A cobrança só pode ser vinculada a um pagamento pendente.");

        BaseValidate.ValidateMaxLength(gatewayReference, GatewayReferenceMaxLength, nameof(GatewayReference));
        BaseValidate.ValidateMaxLength(gatewayPayload, GatewayPayloadMaxLength, nameof(GatewayPayload));

        GatewayReference = gatewayReference ?? string.Empty;
        GatewayPayload = gatewayPayload ?? string.Empty;
    }

    /// <summary>
    /// Pagamento aprovado. Aceita cobranças canceladas ou recusadas porque o gateway pode aprovar depois (ex.: Pix pago após o prazo);
    /// nesse caso o fluxo do pedido estorna o valor.
    /// </summary>
    public void MarkCaptured(string? gatewayReference, DateTime now)
    {
        if (TransactionTypeId != TransactionType.Payment.Id || TransactionStatusId == TransactionStatus.Captured.Id ||
            TransactionStatusId == TransactionStatus.Refunded.Id)
            throw new ArgumentException("Somente pagamentos não capturados podem ser capturados.");

        if (!string.IsNullOrWhiteSpace(gatewayReference))
        {
            BaseValidate.ValidateMaxLength(gatewayReference, GatewayReferenceMaxLength, nameof(GatewayReference));
            GatewayReference = gatewayReference.Trim();
        }

        TransactionStatusId = TransactionStatus.Captured.Id;
        PaidAt = now;
    }

    public void MarkFailed()
    {
        if (IsPendingPayment)
            TransactionStatusId = TransactionStatus.Failed.Id;
    }

    public void MarkCancelled()
    {
        if (IsPendingPayment)
            TransactionStatusId = TransactionStatus.Cancelled.Id;
    }

    private static void ValidateMethod(long transactionMethodId)
    {
        if (!BaseStaticEntity.GetAll<TransactionMethod>().Any(method => method.Id == transactionMethodId))
            throw new ArgumentException("Forma de pagamento inválida.");
    }

    public void UpdateStatus(long transactionStatusId)
    {
        BaseValidate.ValidatePositive(transactionStatusId, nameof(TransactionStatusId));

        TransactionStatusId = transactionStatusId;
    }
    #endregion
}
