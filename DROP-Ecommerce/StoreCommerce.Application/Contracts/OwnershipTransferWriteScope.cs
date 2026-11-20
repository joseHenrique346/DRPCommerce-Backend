namespace StoreCommerce.Application.Contracts;

/// <summary>
/// Mantém a troca de dono como uma vaga única durante as duas gravações exigidas pelo índice parcial.
/// </summary>
public sealed class OwnershipTransferWriteScope
{
    public long? EnterpriseId { get; private set; }
    public long? RecipientMembershipId { get; private set; }

    public IDisposable Begin(long tenant, long recipientId)
    {
        if (tenant <= 0 || recipientId <= 0 || EnterpriseId.HasValue)
            throw new InvalidOperationException("Escopo de transferência inválido.");

        EnterpriseId = tenant;
        RecipientMembershipId = recipientId;

        return new Lease(this);
    }

    private sealed class Lease(OwnershipTransferWriteScope owner) : IDisposable
    {
        public void Dispose()
        {
            owner.EnterpriseId = null;
            owner.RecipientMembershipId = null;
        }
    }
}
