namespace StoreCommerce.Domain.Models;

public sealed record CustomerSearchRow(long Id, string FullName, string Email, string Phone, string City, long? StateId, bool IsActive, bool IsVerified, bool EmailConfirmed, int CompletedOrders, decimal TotalSpent, DateTime? LastOrderAt, DateTime CreatedAt);
