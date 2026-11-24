using System.Text.Json.Serialization;
using StoreCommerce.Application.Contracts.Identity;

namespace StoreCommerce.Application.Identity;

public record SessionResult(string AccessToken, DateTime AccessTokenExpiresAt, bool RequiresEnterpriseSelection,
    IReadOnlyList<MembershipChoice> Enterprises, [property: JsonIgnore] string RefreshToken,
    [property: JsonIgnore] DateTime RefreshTokenExpiresAt);
