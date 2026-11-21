namespace DropCommerce.Domain.Entity;

public sealed class DropCatalogProduct : BaseEntity
{
    public long EnterpriseId { get; private set; }
    public long? StoreProductId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsContractBlocked { get; private set; }
    public bool IsDeleted { get; private set; }
    public Guid? SourceExportId { get; private set; }
    public bool IsReturning { get; private set; }
    public bool IsReturned { get; private set; }
    public string SourceVariantIdsJson { get; private set; } = "[]";
    public DateTime? DeletedAt { get; private set; }
    public ICollection<DropCatalogVariant> Variants { get; private set; } = [];

    private DropCatalogProduct()
    {
    }

    public static DropCatalogProduct Create(long tenant, string name, string description)
    {
        if (tenant < 1 || string.IsNullOrWhiteSpace(name) || name.Trim().Length > 255 || description.Length > 10000)
            throw new ArgumentException("Dados do produto DROP inválidos.");

        return new DropCatalogProduct { EnterpriseId = tenant, Name = name.Trim(), Description = description };
    }

    public void Block()
    {
        IsContractBlocked = true;
    }

    public void Import(long productId, Guid exportId, string name, string description, IReadOnlyList<long> variantIds, DateTime? createdAt)
    {
        if (StoreProductId.HasValue && StoreProductId != productId || SourceExportId.HasValue && !IsReturned && SourceExportId != exportId)
            throw new InvalidOperationException("O estoque de origem já está associado a outra transferência.");

        StoreProductId = productId;
        SourceExportId = exportId;
        Name = name;
        Description = description;
        SourceVariantIdsJson = System.Text.Json.JsonSerializer.Serialize(variantIds);
        IsReturning = false;
        IsReturned = false;

        if (createdAt.HasValue)
            PreserveCreationDate(createdAt.Value);
    }

    public void StartReturn()
    {
        if (!StoreProductId.HasValue || !SourceExportId.HasValue || IsReturning || IsReturned)
            throw new InvalidOperationException("O produto não possui transferência disponível para retorno.");

        IsReturning = true;
    }

    public void CompleteReturn()
    {
        IsReturning = false;
        IsReturned = true;
    }

    public void Reactivate()
    {
        IsContractBlocked = false;
    }

    public void Delete(DateTime now)
    {
        IsDeleted = true;
        DeletedAt = now;
    }
}
