namespace DropCommerce.Domain.Entity;

public sealed class DropCatalogVariant : BaseEntity
{
    public long CatalogProductId { get; private set; }
    public DropCatalogProduct CatalogProduct { get; private set; } = null!;
    public string SKU { get; private set; } = string.Empty;
    public string OptionsJson { get; private set; } = "{}";
    public decimal Price { get; private set; }
    public int QuantityOnHand { get; private set; }
    public int QuantityReserved { get; private set; }
    public long? SourceVariantId { get; private set; }

    public void Reserve(int quantity)
    {
        if (quantity < 1 || quantity > QuantityOnHand - QuantityReserved)
            throw new InvalidOperationException("Saldo insuficiente.");

        QuantityReserved = checked(QuantityReserved + quantity);
    }

    public void Release(int quantity)
    {
        if (quantity < 1 || quantity > QuantityReserved)
            throw new InvalidOperationException("Quantidade reservada inválida.");

        QuantityReserved -= quantity;
    }

    public void Commit(int quantity)
    {
        if (quantity < 1 || quantity > QuantityReserved || quantity > QuantityOnHand)
            throw new InvalidOperationException("Saldo reservado insuficiente.");

        QuantityReserved -= quantity;
        QuantityOnHand -= quantity;
    }

    public void Import(long sourceVariantId, string sku, string optionsJson, decimal price, int quantity)
    {
        if (QuantityOnHand != 0 || QuantityReserved != 0 || quantity < 0)
            throw new InvalidOperationException("O saldo do destino não está livre para receber a transferência.");

        SourceVariantId = sourceVariantId;
        SKU = sku;
        OptionsJson = optionsJson;
        Price = price;
        QuantityOnHand = quantity;
    }

    public int Withdraw()
    {
        if (QuantityReserved != 0)
            throw new InvalidOperationException("Há unidades reservadas.");

        var quantity = QuantityOnHand;
        QuantityOnHand = 0;

        return quantity;
    }

    private DropCatalogVariant()
    {
    }

    public static DropCatalogVariant Create(DropCatalogProduct product, string sku, string optionsJson, decimal price, int quantity)
    {
        if (string.IsNullOrWhiteSpace(sku) || sku.Trim().Length > 100 || price <= 0 || price > 9999999999.99m ||
            decimal.Round(price, 2) != price || quantity < 0 || quantity > 1000000 || optionsJson.Length > 4000)
            throw new ArgumentException("Variação DROP inválida.");

        return new DropCatalogVariant
        {
            CatalogProduct = product, CatalogProductId = product.Id, SKU = sku.Trim(), OptionsJson = optionsJson,
            Price = price, QuantityOnHand = quantity
        };
    }
}
