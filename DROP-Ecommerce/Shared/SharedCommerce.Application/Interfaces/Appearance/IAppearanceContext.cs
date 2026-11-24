namespace SharedCommerce.Application.Interfaces.Appearance;

public interface IAppearanceContext
{
    long EnterpriseId { get; }
    long UserId { get; }
    void RequireAdministrator();
    void RequireCatalogPublisher();
}
