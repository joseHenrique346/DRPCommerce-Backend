namespace StoreCommerce.Domain.StaticEntity;

public sealed class EnterprisePlan : BaseStaticEntity
{
    public static readonly EnterprisePlan Common = new(1, "Comum",
        "Vitrine padrao, sem contas de funcionario, sem dominio proprio, sem tema personalizado e sem o modulo DROP.",
        allowsVitrine: true, allowsEmployees: false, allowsCustomDomain: false, allowsCustomTheme: false, includesDropModule: false);

    public static readonly EnterprisePlan Premium = new(2, "Premium",
        "Inclui contas de funcionario, dominio proprio e tema personalizado, sem o modulo DROP.",
        allowsVitrine: true, allowsEmployees: true, allowsCustomDomain: true, allowsCustomTheme: true, includesDropModule: false);

    public static readonly EnterprisePlan Drop = new(3, "Drop",
        "Plano completo, com contas de funcionario, dominio proprio, tema personalizado e o modulo DROP.",
        allowsVitrine: true, allowsEmployees: true, allowsCustomDomain: true, allowsCustomTheme: true, includesDropModule: true);

    public string Summary { get; }
    public bool AllowsVitrine { get; }
    public bool AllowsEmployees { get; }
    public bool AllowsCustomDomain { get; }
    public bool AllowsCustomTheme { get; }
    public bool IncludesDropModule { get; }

    private EnterprisePlan(long id, string description, string summary, bool allowsVitrine, bool allowsEmployees,
        bool allowsCustomDomain, bool allowsCustomTheme, bool includesDropModule) : base(id, description)
    {
        Summary = summary;
        AllowsVitrine = allowsVitrine;
        AllowsEmployees = allowsEmployees;
        AllowsCustomDomain = allowsCustomDomain;
        AllowsCustomTheme = allowsCustomTheme;
        IncludesDropModule = includesDropModule;
    }

    public static EnterprisePlan Default => Common;

    public static EnterprisePlan FromId(long id) => GetAll<EnterprisePlan>().FirstOrDefault(Plan => Plan.Id == id) ?? Default;
}
