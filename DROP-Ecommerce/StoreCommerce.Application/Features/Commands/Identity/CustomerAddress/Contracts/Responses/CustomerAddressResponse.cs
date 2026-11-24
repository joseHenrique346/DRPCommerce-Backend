using StoreCommerce.Domain.StaticEntity;
using AddressEntity = StoreCommerce.Domain.Entity.CustomerAddress;

namespace StoreCommerce.Application.Contracts.Customers;

public sealed record CustomerAddressResponse(long Id, string Label, string RecipientName, string AddressLine, string City, long StateId, string State, string ZipCode, string Country, bool IsDefault)
{
    public static CustomerAddressResponse FromEntity(AddressEntity address) => new(
        address.Id, address.Label, address.RecipientName, address.AddressLine, address.City, address.StateId,
        BaseStaticEntity.GetAll<State>().First(state => state.Id == address.StateId).Description,
        address.ZipCode, address.Country, address.IsDefault);
}
