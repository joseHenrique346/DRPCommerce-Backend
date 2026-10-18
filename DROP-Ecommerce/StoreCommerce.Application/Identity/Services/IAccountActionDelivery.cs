using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Identity;

public interface IAccountActionDelivery
{
    bool IsAvailable { get; }
    Task DeliverAsync(string recipient, string token, AccountActionPurpose purpose, string publicBaseUrl, CancellationToken cancellationToken);
}
