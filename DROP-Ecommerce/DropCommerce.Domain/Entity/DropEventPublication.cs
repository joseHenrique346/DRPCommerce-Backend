namespace DropCommerce.Domain.Entity;

public static class DropEventPublication
{
    public static bool IsPublished(DropEvent drop)
    {
        return !drop.IsDeleted && drop.DropEndsAt > DateTime.UtcNow && drop.DropEventStatusId is not (7 or 8) &&
            (drop.IsPublic || drop.DropEventStatusId != 1);
    }
}
