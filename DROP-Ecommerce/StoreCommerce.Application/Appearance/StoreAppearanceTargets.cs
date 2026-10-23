using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Appearance;

public static class StoreAppearanceTargets
{
    public static InputAppearanceTarget Default => new(AppearanceTargetType.Store.Id);
}
