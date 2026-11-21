namespace SharedCommerce.Application.Contracts.Drop;

public sealed record DropBuyerProfile(long CustomerId, long EnterpriseId, string Name, string Email,
    string AddressLine, string City, string State, string ZipCode);
public sealed record StartDropSalePayment(Guid OperationId, long DropOrderId, long CustomerId,
    decimal Amount, string Currency, DateTime ExpiresAt);
public sealed record DropSalePaymentReceipt(Guid OperationId, long EnterpriseId, long DropOrderId, long CustomerId,
    decimal Amount, string Currency, string Status, string? CheckoutUrl, string? PaymentId, long Revision);
