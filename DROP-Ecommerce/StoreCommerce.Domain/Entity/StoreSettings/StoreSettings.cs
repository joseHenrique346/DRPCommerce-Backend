using System.Text.RegularExpressions;
using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

/// <summary>
/// Configurações operacionais da loja (1:1 com Enterprise) usadas nas regras de venda.
/// </summary>
public class StoreSettings : BaseEntity, ITenantEntity
{
    #region Constants
    public const int CurrencyCodeLength = 3;
    public const string DefaultCurrencyCode = "BRL";
    public const int MaxItemsPerOrderLimit = 10_000;
    public const int MinPendingOrderExpirationMinutes = 5;
    public const int MaxPendingOrderExpirationMinutes = 10_080;
    public const int DefaultPendingOrderExpirationMinutes = 60;
    #endregion

    #region Properties
    public long EnterpriseId { get; private set; }
    public string CurrencyCode { get; private set; } = DefaultCurrencyCode;
    public decimal MinOrderValue { get; private set; }
    public int? MaxItemsPerOrder { get; private set; }
    public bool AllowBackorder { get; private set; }
    public int PendingOrderExpirationMinutes { get; private set; }

    #region Navigation Properties
    public Enterprise Enterprise { get; private set; } = null!;
    #endregion

    #endregion

    #region Constructors
    protected StoreSettings() { }

    private StoreSettings(long enterpriseId)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));

        EnterpriseId = enterpriseId;
    }
    #endregion

    #region Functions
    /// <summary>
    /// Configuração padrão de uma loja nova: BRL, sem pedido mínimo, sem limite de itens,
    /// sem venda sem estoque e pedidos pendentes expirando em 60 minutos.
    /// </summary>
    public static StoreSettings CreateDefault(long enterpriseId)
    {
        var settings = new StoreSettings(enterpriseId);
        settings.UpdateDetails(DefaultCurrencyCode, 0, null, false, DefaultPendingOrderExpirationMinutes);

        return settings;
    }

    public static StoreSettings Create(long enterpriseId, string currencyCode, decimal minOrderValue, int? maxItemsPerOrder, bool allowBackorder, int pendingOrderExpirationMinutes)
    {
        var settings = new StoreSettings(enterpriseId);
        settings.UpdateDetails(currencyCode, minOrderValue, maxItemsPerOrder, allowBackorder, pendingOrderExpirationMinutes);

        return settings;
    }

    public void UpdateDetails(string currencyCode, decimal minOrderValue, int? maxItemsPerOrder, bool allowBackorder, int pendingOrderExpirationMinutes)
    {
        BaseValidate.ValidateNotNullOrEmpty(currencyCode, nameof(CurrencyCode));
        var currency = currencyCode.Trim().ToUpperInvariant();

        if (!Regex.IsMatch(currency, "^[A-Z]{3}$"))
            throw new ArgumentException($"{nameof(CurrencyCode)} deve ser um código ISO 4217 de 3 letras.");

        BaseValidate.ValidatePositiveOrZero(minOrderValue, nameof(MinOrderValue));

        if (maxItemsPerOrder.HasValue)
            BaseValidate.ValidateRange(maxItemsPerOrder.Value, 1, MaxItemsPerOrderLimit, nameof(MaxItemsPerOrder));

        BaseValidate.ValidateRange(pendingOrderExpirationMinutes, MinPendingOrderExpirationMinutes, MaxPendingOrderExpirationMinutes, nameof(PendingOrderExpirationMinutes));

        CurrencyCode = currency;
        MinOrderValue = minOrderValue;
        MaxItemsPerOrder = maxItemsPerOrder;
        AllowBackorder = allowBackorder;
        PendingOrderExpirationMinutes = pendingOrderExpirationMinutes;
    }

    /// <summary>
    /// Indica se a quantidade total de unidades do pedido está dentro do limite da loja.
    /// </summary>
    public bool IsWithinItemLimit(int totalQuantity) => !MaxItemsPerOrder.HasValue || totalQuantity <= MaxItemsPerOrder.Value;

    /// <summary>
    /// Indica se o valor dos produtos (subtotal) atinge o pedido mínimo da loja.
    /// </summary>
    public bool MeetsMinOrderValue(decimal subTotal) => subTotal >= MinOrderValue;
    #endregion
}
