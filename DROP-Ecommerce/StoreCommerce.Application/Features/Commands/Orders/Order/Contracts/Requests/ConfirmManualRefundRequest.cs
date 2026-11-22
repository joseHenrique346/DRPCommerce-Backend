namespace StoreCommerce.Application.Contracts.Order;

public sealed record ConfirmManualRefundRequest(string Reference, string Note);
