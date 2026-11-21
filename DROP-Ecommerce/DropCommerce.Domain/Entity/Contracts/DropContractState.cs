namespace DropCommerce.Domain.Entity;

public sealed class DropContractState : BaseEntity
{
    public long EnterpriseId { get; private set; }
    public long Revision { get; private set; }
    public int CatalogVersion { get; private set; }
    public bool AllowsDrop { get; private set; }
    public int ProductLimit { get; private set; }
    public DateTime AppliedAt { get; private set; }

    private DropContractState()
    {
    }

    public static DropContractState Create(long enterpriseId)
    {
        return new DropContractState { EnterpriseId = enterpriseId };
    }

    public void Apply(long revision, int catalogVersion, bool allowsDrop, int productLimit, DateTime now)
    {
        if (revision <= Revision)
            return;

        Revision = revision;
        CatalogVersion = catalogVersion;
        AllowsDrop = allowsDrop;
        ProductLimit = productLimit;
        AppliedAt = now;
    }
}
