namespace DropCommerce.Application.Features.Commands;

public sealed record DropOrderDetails(long Id, decimal TotalAmount, long DropOrderStatusId, long DropOrderPaymentStatusId, List<DropOrderItemSummary> Items);
