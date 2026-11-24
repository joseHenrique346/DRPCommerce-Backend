namespace StoreCommerce.Domain.StaticEntity;

/// <summary>
/// Cada alternativa é indivisível; a loja tem no máximo um adicional vigente.
/// </summary>
public sealed class EnterpriseAddOn : BaseStaticEntity
{
    public static readonly EnterpriseAddOn SmallTeam = new(
        1, "Equipe pequena", "team-small", "employees", 1, 9.90m, [4, 2, 3, 5]);

    public static readonly EnterpriseAddOn LargeTeam = new(
        2, "Equipe ampliada", "team-large", "employees", 5, 29.90m, [2, 3, 5]);

    public static readonly EnterpriseAddOn SmallCatalog = new(
        3, "Catálogo pequeno", "catalog-small", "store_products", 100, 9.90m, [4, 2]);

    public static readonly EnterpriseAddOn LargeCatalog = new(
        4, "Catálogo ampliado", "catalog-large", "store_products", 500, 19.90m, [2, 3]);

    public static readonly EnterpriseAddOn Launches = new(
        5, "Lançamentos", "launches", "drop_products", 25, 14.90m, [3, 5]);

    public string Code { get; }
    public string ResourceCode { get; }
    public int Quantity { get; }
    public decimal MonthlyPrice { get; }
    public IReadOnlyList<long> EligiblePlanIds { get; }

    private EnterpriseAddOn() : base(0, string.Empty)
    {
        Code = string.Empty;
        ResourceCode = string.Empty;
        EligiblePlanIds = Array.Empty<long>();
    }

    private EnterpriseAddOn(long id, string description, string code, string resourceCode,
        int quantity, decimal monthlyPrice, long[] eligiblePlanIds)
        : base(id, description)
    {
        Code = code;
        ResourceCode = resourceCode;
        Quantity = quantity;
        MonthlyPrice = monthlyPrice;
        EligiblePlanIds = Array.AsReadOnly(eligiblePlanIds);
    }

    public bool IsEligibleFor(long planId)
    {
        return EligiblePlanIds.Contains(planId);
    }

    public static EnterpriseAddOn? Find(long id)
    {
        return GetAll<EnterpriseAddOn>().FirstOrDefault(AddOn => AddOn.Id == id);
    }

    public static EnterpriseAddOn FromId(long id)
    {
        return Find(id) ?? throw new ArgumentOutOfRangeException(nameof(id), "Adicional inexistente.");
    }
}
