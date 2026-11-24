namespace SharedCommerce.Domain.Interfaces.Tenancy;

public interface ITenantProvider
{
    long GetEnterpriseId();
}
