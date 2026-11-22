using System.Text.Json.Serialization;
using StoreCommerce.Application.Contracts.Identity;

namespace StoreCommerce.Application.Identity;

public record InvitationRequest(string Email, long KindId, List<long> AccessRoleIds);
