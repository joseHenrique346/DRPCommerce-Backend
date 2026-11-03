namespace SharedCommerce.Application.Security;

/// <summary>
/// Catálogo de capacidades contratáveis por plano, compartilhado entre Store e Drop para evitar códigos divergentes.
/// </summary>
public static class EnterprisePlanCapabilities
{
    public const string Vitrine = "vitrine";
    public const string Employees = "employees";
    public const string CustomDomain = "custom_domain";
    public const string CustomTheme = "custom_theme";
    public const string Drop = "drop";
}
