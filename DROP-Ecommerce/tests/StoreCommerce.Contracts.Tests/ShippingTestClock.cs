namespace StoreCommerce.Contracts.Tests;

internal sealed class ShippingTestClock : TimeProvider
{
    public TimeSpan Offset { get; set; }

    public override DateTimeOffset GetUtcNow()
    {
        return DateTimeOffset.UtcNow + Offset;
    }
}
