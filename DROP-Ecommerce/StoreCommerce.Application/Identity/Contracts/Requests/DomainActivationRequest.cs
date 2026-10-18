using System.Text.Json.Serialization;
using StoreCommerce.Application.Contracts.Identity;

namespace StoreCommerce.Application.Identity;

public record DomainActivationRequest(bool IsPrimary, bool IsActive);
