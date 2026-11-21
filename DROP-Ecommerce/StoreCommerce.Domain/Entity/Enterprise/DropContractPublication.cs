using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public sealed class DropContractPublication : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public Enterprise Enterprise { get; private set; } = null!;
    public long Revision { get; private set; }
    public long AcknowledgedRevision { get; private set; }
    public DateTime ChangedAt { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }

    private DropContractPublication()
    {
    }

    public static DropContractPublication Capture(Enterprise enterprise, DateTime now)
    {
        return new DropContractPublication
        {
            Enterprise = enterprise,
            EnterpriseId = enterprise.Id,
            Revision = enterprise.ContractRevision,
            ChangedAt = now
        };
    }

    public void Publish(long revision, DateTime now)
    {
        if (revision <= Revision)
            return;

        Revision = revision;
        ChangedAt = now;
    }

    public void Acknowledge(long revision, DateTime now)
    {
        if (revision < 1 || revision > Revision)
            throw new ArgumentException("Revisão de confirmação inválida.");

        if (revision <= AcknowledgedRevision)
            return;

        AcknowledgedRevision = revision;
        AcknowledgedAt = now;
    }
}
