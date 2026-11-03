using SharedCommerce.Application.Security;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Identity;

/// <summary>
/// Converte as capacidades de um plano em códigos de claim, mantendo a emissão do token alinhada ao catálogo compartilhado.
/// </summary>
public static class EnterprisePlanCapabilityMapper
{
    public static IReadOnlyCollection<string> ToCapabilities(this EnterprisePlan plan)
    {
        var capabilities = new List<string>();

        if (plan.AllowsVitrine)
            capabilities.Add(EnterprisePlanCapabilities.Vitrine);

        if (plan.AllowsEmployees)
            capabilities.Add(EnterprisePlanCapabilities.Employees);

        if (plan.AllowsCustomDomain)
            capabilities.Add(EnterprisePlanCapabilities.CustomDomain);

        if (plan.AllowsCustomTheme)
            capabilities.Add(EnterprisePlanCapabilities.CustomTheme);

        if (plan.IncludesDropModule)
            capabilities.Add(EnterprisePlanCapabilities.Drop);

        return capabilities;
    }
}
