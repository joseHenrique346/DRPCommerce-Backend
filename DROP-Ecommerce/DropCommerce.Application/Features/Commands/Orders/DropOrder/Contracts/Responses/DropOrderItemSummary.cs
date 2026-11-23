namespace DropCommerce.Application.Features.Commands;

public sealed record DropOrderItemSummary(string ItemName, int Quantity, decimal UnitPrice, decimal TotalPrice);
