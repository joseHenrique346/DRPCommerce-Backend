namespace DropCommerce.Domain.Entity;

public sealed class DropCommercialActivity : BaseEntity
{
    public long EnterpriseId { get; private set; }
    public long Revision { get; private set; }
    public long? CatalogProductId { get; private set; }
    public long? ParticipationId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public DateTime OccurredAt { get; private set; }

    private DropCommercialActivity()
    {
    }

    public static DropCommercialActivity Record(long tenant, long revision, string action, DateTime now,
        long? catalogProductId = null, long? participationId = null)
    {
        return new DropCommercialActivity
        {
            EnterpriseId = tenant, Revision = revision, Action = action, OccurredAt = now,
            CatalogProductId = catalogProductId, ParticipationId = participationId
        };
    }
}
