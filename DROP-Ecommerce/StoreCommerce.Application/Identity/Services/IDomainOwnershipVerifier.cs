namespace StoreCommerce.Application.Identity;

public interface IDomainOwnershipVerifier
{
    Task<bool> VerifyAsync(string host, string expectedHash, CancellationToken cancellationToken);
}
