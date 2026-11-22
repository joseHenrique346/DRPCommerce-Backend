namespace StoreCommerce.Domain.StaticEntity;

/// <summary>
/// Catálogo comercial. Os IDs 1, 2 e 3 preservam Comum, Premium e Drop, respectivamente.
/// </summary>
public sealed class EnterprisePlan : BaseStaticEntity
{
    public static readonly EnterprisePlan Free = new(
        1, "Gratuito", "free", 0m, 50, 0, 0, 1, 0,
        allowsVitrine: true, allowsCustomDomain: false, allowsLogoCustomization: false, allowsFullCustomization: false);

    public static readonly EnterprisePlan Initial = new(
        4, "Inicial", "initial", 49m, 100, 0, 1, 3, 0,
        allowsVitrine: true, allowsCustomDomain: false, allowsLogoCustomization: true, allowsFullCustomization: false);

    public static readonly EnterprisePlan Pro = new(
        2, "Pro", "pro", 99m, 500, 0, 3, 10, 0,
        allowsVitrine: true, allowsCustomDomain: true, allowsLogoCustomization: true, allowsFullCustomization: true);

    public static readonly EnterprisePlan Commerce = new(
        3, "DRPCommerce", "drpcommerce", 199m, 2000, 100, 5, 20, 10,
        allowsVitrine: true, allowsCustomDomain: true, allowsLogoCustomization: true, allowsFullCustomization: true);

    public static readonly EnterprisePlan Drop = new(
        5, "DRPDrop", "drpdrop", 149m, 0, 100, 5, 0, 10,
        allowsVitrine: false, allowsCustomDomain: true, allowsLogoCustomization: true, allowsFullCustomization: true);

    public string Code { get; }
    public string Summary { get; }
    public decimal MonthlyPrice { get; }
    public int StoreProductLimit { get; }
    public int DropProductLimit { get; }
    public int EmployeeLimit { get; }
    public int StoreThemeLimit { get; }
    public int DropThemeLimit { get; }
    public bool AllowsVitrine { get; }
    public bool AllowsEmployees { get; }
    public bool AllowsCustomDomain { get; }
    public bool AllowsCustomTheme { get; }
    public bool IncludesDropModule { get; }
    public bool AllowsLogoCustomization { get; }
    public bool AllowsFullCustomization { get; }

    private EnterprisePlan(long id, string description, string code, decimal monthlyPrice,
        int storeProductLimit, int dropProductLimit, int employeeLimit, int storeThemeLimit, int dropThemeLimit,
        bool allowsVitrine, bool allowsCustomDomain, bool allowsLogoCustomization, bool allowsFullCustomization)
        : base(id, description)
    {
        Code = code;
        Summary = description + ": recursos e limites por loja, conforme o catálogo comercial.";
        MonthlyPrice = monthlyPrice;
        StoreProductLimit = storeProductLimit;
        DropProductLimit = dropProductLimit;
        EmployeeLimit = employeeLimit;
        StoreThemeLimit = storeThemeLimit;
        DropThemeLimit = dropThemeLimit;
        AllowsVitrine = allowsVitrine;
        AllowsEmployees = employeeLimit > 0;
        AllowsCustomDomain = allowsCustomDomain;
        AllowsCustomTheme = allowsLogoCustomization;
        IncludesDropModule = dropProductLimit > 0;
        AllowsLogoCustomization = allowsLogoCustomization;
        AllowsFullCustomization = allowsFullCustomization;
    }

    public static EnterprisePlan Default => Free;

    public static EnterprisePlan? Find(long id)
    {
        return GetAll<EnterprisePlan>().FirstOrDefault(Plan => Plan.Id == id);
    }

    public static EnterprisePlan FromId(long id)
    {
        return Find(id) ?? throw new ArgumentOutOfRangeException(nameof(id), "Plano inexistente.");
    }
}
