namespace DropCommerce.Application.Contracts;

public sealed class DropQueueOptions
{
    public string PurchaseIdentityKey { get; set; } = string.Empty;
    public int ConcurrentBuyersPerEvent { get; set; } = 20;
}
