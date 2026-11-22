namespace SharedCommerce.Application.Contracts.Drop;

public sealed record StartDropSalePayment(Guid OperationId, long DropOrderId, long CustomerId,
    decimal Amount, string Currency, DateTime ExpiresAt);
