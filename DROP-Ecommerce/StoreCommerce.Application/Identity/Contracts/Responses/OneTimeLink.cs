using System.Text.Json.Serialization;
using StoreCommerce.Application.Contracts.Identity;

namespace StoreCommerce.Application.Identity;

public record OneTimeLink(long Id, string Token, DateTime ExpiresAt);
