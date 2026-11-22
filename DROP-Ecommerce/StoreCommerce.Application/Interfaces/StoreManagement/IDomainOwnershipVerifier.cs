using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Interfaces.StoreManagement;

public interface IDomainOwnershipVerifier
{
    Task<bool> VerifyAsync(string host, string expectedHash, CancellationToken cancellationToken);
}
