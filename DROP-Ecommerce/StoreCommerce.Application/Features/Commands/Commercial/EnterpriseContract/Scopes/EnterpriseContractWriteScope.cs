namespace StoreCommerce.Application.Contracts;

/// <summary>
/// Escopo reservado à futura concessão confiável. Não existe endpoint HTTP de concessão nesta etapa.
/// </summary>
public sealed class EnterpriseContractWriteScope
{
    public long? EnterpriseId { get; private set; }
    public IReadOnlyList<long>? RetainedMembershipIds { get; private set; }

    public IDisposable Begin(long enterpriseId, IReadOnlyList<long>? retainedMembershipIds = null)
    {
        if (enterpriseId <= 0 || EnterpriseId.HasValue)
            throw new InvalidOperationException("Escopo de contrato inválido ou já aberto.");

        EnterpriseId = enterpriseId;
        RetainedMembershipIds = retainedMembershipIds?.ToArray();

        return new Lease(this);
    }

    private sealed class Lease(EnterpriseContractWriteScope owner) : IDisposable
    {
        public void Dispose()
        {
            owner.EnterpriseId = null;
            owner.RetainedMembershipIds = null;
        }
    }
}
