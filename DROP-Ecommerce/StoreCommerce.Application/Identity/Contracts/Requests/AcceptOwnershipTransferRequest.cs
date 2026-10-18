using System.Text.Json.Serialization;
using StoreCommerce.Application.Contracts.Identity;

namespace StoreCommerce.Application.Identity;

public record AcceptOwnershipTransferRequest(string Token, string CurrentPassword);
