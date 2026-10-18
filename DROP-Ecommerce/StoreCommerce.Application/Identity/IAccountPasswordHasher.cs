using StoreCommerce.Domain.Interfaces;
namespace StoreCommerce.Application.Identity;
public interface IAccountPasswordHasher
{
    string Hash(IAuthenticationAccount account, string password);
    bool Verify(IAuthenticationAccount account, string password, out bool needsRehash);
    void VerifyDummy(string password, bool customer);
}
