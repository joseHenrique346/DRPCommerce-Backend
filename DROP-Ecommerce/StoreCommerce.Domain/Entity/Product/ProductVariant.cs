using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;
using ProductRules = StoreCommerce.Domain.Entity.Product;

namespace StoreCommerce.Domain.Entity;

public class ProductVariant : BaseEntity, ITenantEntity, ISoftDeletable
{
    public const int NameMaxLength = 100;
    public long EnterpriseId { get; private set; }
    public long ProductId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string SKU { get; private set; } = string.Empty;
    public string BarCode { get; private set; } = string.Empty;
    public string AttributesJson { get; private set; } = "{}";
    public string CombinationKey { get; private set; } = string.Empty;
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }
    public decimal? Price { get; private set; }
    public decimal? CostPrice { get; private set; }
    public decimal? Weight { get; private set; }
    public decimal? Height { get; private set; }
    public decimal? Width { get; private set; }
    public decimal? Length { get; private set; }
    public int StockQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public long Version { get; private set; } = 1;
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Product Product { get; private set; } = null!;
    public int AvailableQuantity => (int)Math.Clamp((long)StockQuantity - ReservedQuantity, 0, int.MaxValue);

    protected ProductVariant()
    {
    }

    public static ProductVariant Create(Product product, string name, string sku, string barCode,
        IReadOnlyDictionary<string, string> attributes, bool isDefault = false, bool isActive = true)
    {
        var variant = new ProductVariant
        {
            Product = product,
            ProductId = product.Id,
            EnterpriseId = product.EnterpriseId,
            IsDefault = isDefault
        };
        variant.UpdateDetails(name, sku, barCode, attributes, isActive);

        return variant;
    }

    public void UpdateDetails(string name, string sku, string barCode, IReadOnlyDictionary<string, string> attributes, bool isActive)
    {
        BaseValidate.ValidateNotNullOrEmpty(name, nameof(Name));
        BaseValidate.ValidateMaxLength(name, NameMaxLength, nameof(Name));
        BaseValidate.ValidateNotNullOrEmpty(sku, nameof(SKU));
        BaseValidate.ValidateMaxLength(sku, ProductRules.SkuMaxLength, nameof(SKU));
        BaseValidate.ValidateMaxLength(barCode, ProductRules.BarCodeMaxLength, nameof(BarCode));

        if (attributes is null || attributes.Count > 8 || (!IsDefault && attributes.Count == 0) || (IsDefault && attributes.Count != 0))
            throw new ArgumentException("Informe de uma a oito opções para uma variante; o SKU padrão não possui opções.");

        var normalized = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var display = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, value) in attributes)
        {
            BaseValidate.ValidateNotNullOrEmpty(key, "Opção");
            BaseValidate.ValidateNotNullOrEmpty(value, "Valor da opção");
            BaseValidate.ValidateMaxLength(key, 50, "Opção");
            BaseValidate.ValidateMaxLength(value, 100, "Valor da opção");

            if (key.Any(char.IsControl) || value.Any(char.IsControl))
                throw new ArgumentException("As opções da variante não aceitam caracteres de controle.");

            if (!normalized.TryAdd(key.Trim().Normalize().ToUpperInvariant(), value.Trim().Normalize().ToUpperInvariant()))
                throw new ArgumentException("A variante contém opções duplicadas.");

            display.Add(key.Trim(), value.Trim());
        }

        Name = name.Trim();
        SKU = sku.Trim().ToUpperInvariant();
        BarCode = barCode?.Trim() ?? string.Empty;
        AttributesJson = JsonSerializer.Serialize(display);
        CombinationKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(normalized))));
        IsActive = isActive;
        Version = checked(Version + 1);
    }

    public void UpdatePricing(decimal? price, decimal? costPrice)
    {
        ProductRules.ValidatePricing(price ?? Product.Price, costPrice ?? Product.CostPrice);

        Price = price;
        CostPrice = costPrice;
        Version = checked(Version + 1);
    }

    public void UpdateDimensions(decimal? weight, decimal? height, decimal? width, decimal? length)
    {
        ProductRules.ValidateDimensions(Product.IsDigital, weight ?? Product.Weight, height ?? Product.Height,
            width ?? Product.Width, length ?? Product.Length);

        Weight = weight;
        Height = height;
        Width = width;
        Length = length;
        Version = checked(Version + 1);
    }

    public void ValidateAgainstProduct()
    {
        ProductRules.ValidatePricing(Price ?? Product.Price, CostPrice ?? Product.CostPrice);
        ProductRules.ValidateDimensions(Product.IsDigital, Weight ?? Product.Weight, Height ?? Product.Height,
            Width ?? Product.Width, Length ?? Product.Length);
    }

    public void AdjustStock(int delta, bool allowBackorder)
    {
        var next = checked(StockQuantity + delta);

        if (delta < 0 && !allowBackorder && (next < 0 || next < ReservedQuantity))
            throw new ArgumentException("O ajuste não pode deixar o estoque abaixo da quantidade reservada.");

        StockQuantity = next;
        Version = checked(Version + 1);
    }

    public void ReserveStock(int quantity, bool allowBackorder)
    {
        BaseValidate.ValidatePositive(quantity, nameof(quantity));

        if (!allowBackorder && AvailableQuantity < quantity)
            throw new ArgumentException("Estoque insuficiente para a reserva.");

        ReservedQuantity = checked(ReservedQuantity + quantity);
        Version = checked(Version + 1);
    }

    public void ReleaseStock(int quantity)
    {
        BaseValidate.ValidateRange(quantity, 1, ReservedQuantity, nameof(quantity));

        ReservedQuantity -= quantity;
        Version = checked(Version + 1);
    }

    public void CommitStock(int quantity)
    {
        BaseValidate.ValidateRange(quantity, 1, ReservedQuantity, nameof(quantity));
        var next = checked(StockQuantity - quantity);

        // A baixa honra a reserva aprovada, mesmo após mudanças na política de backorder.
        StockQuantity = next;
        ReservedQuantity -= quantity;
        Version = checked(Version + 1);
    }

    public void DisableDefault()
    {
        if (!IsDefault || StockQuantity != 0 || ReservedQuantity != 0)
            throw new ArgumentException("Zere o estoque do SKU padrão antes de habilitar variantes.");

        IsActive = false;
        Version = checked(Version + 1);
    }

    public void SoftDelete()
    {
        if (StockQuantity != 0 || ReservedQuantity != 0)
            throw new ArgumentException("Zere o estoque e libere as reservas antes de excluir o SKU.");

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        Version = checked(Version + 1);
    }
}
