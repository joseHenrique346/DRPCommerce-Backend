using System.Text.Json.Serialization;
using StoreCommerce.Application.Contracts.Identity;

namespace StoreCommerce.Application.Identity;

public record EnterpriseUpdateRequest(string TradeName, string LegalName, string Email, string Phone,
    string AddressLine, string City, long StateId, string ZipCode, string Country);
