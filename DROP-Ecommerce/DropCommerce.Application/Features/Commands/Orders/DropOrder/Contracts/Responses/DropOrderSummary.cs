namespace DropCommerce.Application.Features.Commands;

public sealed record DropOrderSummary(long Id, long DropEventId, decimal TotalAmount, long DropOrderStatusId, long DropOrderPaymentStatusId);
