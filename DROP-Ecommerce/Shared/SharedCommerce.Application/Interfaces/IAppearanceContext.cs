namespace SharedCommerce.Application.Interfaces;

public interface IAppearanceContext
{
    long EnterpriseId { get; }
    long UserId { get; }
    void RequireAdministrator();
    void RequireCatalogPublisher();
}
