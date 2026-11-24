using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Enums.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Interfaces.Identity;

public interface IAccountActionDelivery
{
    bool IsAvailable { get; }
    Task DeliverAsync(string recipient, string token, AccountActionPurpose purpose, string publicBaseUrl, CancellationToken cancellationToken);
}
