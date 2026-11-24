using System.Text.Json.Serialization;
using StoreCommerce.Application.Contracts.Identity;

namespace StoreCommerce.Application.Identity;

public record SessionDescription(Guid FamilyId, DateTime CreatedAt, DateTime ExpiresAt, bool IsCurrent);
