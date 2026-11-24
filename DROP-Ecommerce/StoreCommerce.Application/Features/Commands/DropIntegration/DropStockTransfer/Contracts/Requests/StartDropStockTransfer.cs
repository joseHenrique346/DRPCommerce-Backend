namespace StoreCommerce.Application.Contracts;

public sealed record StartDropStockTransfer(string IdempotencyKey, long ExpectedContractRevision);
