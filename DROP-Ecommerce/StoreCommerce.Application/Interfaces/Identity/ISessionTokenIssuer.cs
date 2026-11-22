using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Interfaces.Identity;

public interface ISessionTokenIssuer
{
    Task<(string Token, DateTime Expires)> IssueUserAsync(User user, Guid familyId, EnterpriseMembership? membership, CancellationToken cancellationToken);
    (string Token, DateTime Expires) IssueCustomer(Customer customer, Guid familyId);
}
