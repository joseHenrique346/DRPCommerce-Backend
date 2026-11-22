namespace StoreCommerce.Application.Contracts;

public sealed class DropContractAcknowledgmentScope
{
    public bool IsOpen { get; private set; }

    public IDisposable Begin()
    {
        if (IsOpen)
            throw new InvalidOperationException("Confirmação de contrato já aberta.");

        IsOpen = true;

        return new Lease(this);
    }

    private sealed class Lease(DropContractAcknowledgmentScope owner) : IDisposable
    {
        public void Dispose()
        {
            owner.IsOpen = false;
        }
    }
}
