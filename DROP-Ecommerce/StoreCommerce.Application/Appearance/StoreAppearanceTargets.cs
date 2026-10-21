using Commerce.Appearance.Contracts;
using Commerce.Appearance.Domain.StaticEntity;

namespace StoreCommerce.Application.Appearance;

public static class StoreAppearanceTargets
{
    public static InputAppearanceTarget Default => new(AppearanceTargetType.Store.Id);
}
