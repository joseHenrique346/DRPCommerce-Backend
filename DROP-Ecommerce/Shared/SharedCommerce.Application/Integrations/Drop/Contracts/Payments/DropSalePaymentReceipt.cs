namespace SharedCommerce.Application.Contracts.Drop;

public sealed record DropSalePaymentReceipt(Guid OperationId, long EnterpriseId, long DropOrderId, long CustomerId,
    decimal Amount, string Currency, string Status, string? CheckoutUrl, string? PaymentId, long Revision);
