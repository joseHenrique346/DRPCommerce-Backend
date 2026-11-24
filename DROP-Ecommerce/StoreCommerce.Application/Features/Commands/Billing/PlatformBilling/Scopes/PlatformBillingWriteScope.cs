namespace StoreCommerce.Application.Billing;

public sealed class PlatformBillingWriteScope
{
    public bool IsOpen { get; private set; }

    public IDisposable Begin()
    {
        if (IsOpen)
            throw new InvalidOperationException("Escopo financeiro já aberto.");

        IsOpen = true;

        return new Lease(this);
    }

    private sealed class Lease(PlatformBillingWriteScope owner) : IDisposable
    {
        public void Dispose()
        {
            owner.IsOpen = false;
        }
    }
}
