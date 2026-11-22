using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Interfaces.Identity;

namespace StoreCommerce.Application.Interfaces.Identity;
public interface IAccountPasswordHasher
{
    string Hash(IAuthenticationAccount account, string password);
    bool Verify(IAuthenticationAccount account, string password, out bool needsRehash);
    void VerifyDummy(string password, bool customer);
}
