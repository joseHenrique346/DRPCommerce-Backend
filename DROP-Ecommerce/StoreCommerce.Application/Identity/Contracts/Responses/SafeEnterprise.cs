using System.Text.Json.Serialization;
using StoreCommerce.Application.Contracts.Identity;

namespace StoreCommerce.Application.Identity;

public record SafeEnterprise(long Id, string TradeName, bool IsActive);
