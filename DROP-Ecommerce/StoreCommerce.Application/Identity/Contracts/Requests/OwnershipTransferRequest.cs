using System.Text.Json.Serialization;
using StoreCommerce.Application.Contracts.Identity;

namespace StoreCommerce.Application.Identity;

public record OwnershipTransferRequest(long RecipientMembershipId, string CurrentPassword);
