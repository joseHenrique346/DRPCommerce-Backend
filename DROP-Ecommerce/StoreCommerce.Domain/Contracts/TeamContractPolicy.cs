using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Domain.Contracts;

public static class TeamContractPolicy
{
    public static IReadOnlyList<EnterpriseMembership> ActiveAdditional(IEnumerable<EnterpriseMembership> members)
    {
        return members.Where(Membership => Membership.KindId != 1 && Membership.IsActive &&
            !Membership.IsContractBlocked && Membership.User.IsActive)
            .OrderBy(Membership => Membership.JoinedAt).ThenBy(Membership => Membership.Id).ToList();
    }

    public static bool IsValidSelection(IReadOnlyList<long>? selected, IReadOnlyList<EnterpriseMembership> active, int limit)
    {
        return selected is null || selected.Count <= limit && selected.Count <= 10000 &&
            selected.All(Id => Id > 0) && selected.Distinct().Count() == selected.Count &&
            selected.All(Id => active.Any(Membership => Membership.Id == Id));
    }
}
