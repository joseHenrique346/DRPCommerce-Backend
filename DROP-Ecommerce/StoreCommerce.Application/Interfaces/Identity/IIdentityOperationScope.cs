using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Interfaces.Identity;

// Only trusted application workflows can open the write scope.
public interface IIdentityOperationScope
{
    bool IsOpen { get; }
    IDisposable Begin();
}
