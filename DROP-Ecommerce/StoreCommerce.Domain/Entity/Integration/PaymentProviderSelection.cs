using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public sealed class PaymentProviderSelection : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public string Provider { get; private set; } = IntegrationProviders.Manual;
    public IntegrationEnvironment Environment { get; private set; }

    private PaymentProviderSelection()
    {
    }

    public static PaymentProviderSelection Create(long enterpriseId, string provider, IntegrationEnvironment environment)
    {
        var selection = new PaymentProviderSelection { EnterpriseId = enterpriseId };
        selection.Select(provider, environment);

        return selection;
    }

    public void Select(string provider, IntegrationEnvironment environment)
    {
        IntegrationProviders.Validate(provider, environment);
        Provider = provider;
        Environment = environment;
    }
}
