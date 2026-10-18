namespace StoreCommerce.Application.Identity;

// Only trusted application workflows can open the write scope.
public interface IIdentityOperationScope
{
    bool IsOpen { get; }
    IDisposable Begin();
}
