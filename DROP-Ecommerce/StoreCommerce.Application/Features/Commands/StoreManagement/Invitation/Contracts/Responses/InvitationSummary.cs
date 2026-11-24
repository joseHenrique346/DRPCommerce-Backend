using System.Text.Json.Serialization;
using StoreCommerce.Application.Contracts.Identity;

namespace StoreCommerce.Application.Identity;

public record InvitationSummary(long Id, string Email, long KindId, DateTime ExpiresAt, DateTime? AcceptedAt, DateTime? RevokedAt);
