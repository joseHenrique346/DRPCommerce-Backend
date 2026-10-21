using Commerce.Appearance.Contracts;
using Commerce.Appearance.Domain.StaticEntity;

namespace DropCommerce.Application.Appearance;

public static class DropAppearanceTargets
{
    public static InputAppearanceTarget Default => new(AppearanceTargetType.DropDefault.Id);

    public static InputAppearanceTarget Event(long eventId)
    {
        return new InputAppearanceTarget(AppearanceTargetType.DropSpecific.Id, eventId);
    }
}
