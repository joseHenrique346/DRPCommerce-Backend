using System.Text.Json.Serialization;
using StoreCommerce.Application.Contracts.Identity;

namespace StoreCommerce.Application.Identity;

public record MembershipChoice(long MembershipId, long EnterpriseId, string TradeName, long KindId);
