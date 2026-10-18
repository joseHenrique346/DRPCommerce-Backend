using System.Text.Json.Serialization;
using StoreCommerce.Application.Contracts.Identity;

namespace StoreCommerce.Application.Identity;

public record CustomerProfileRequest(string FullName, string Phone, string AddressLine, string City,
    long StateId, string ZipCode, string Country, string Gender, DateTime DateOfBirth);
