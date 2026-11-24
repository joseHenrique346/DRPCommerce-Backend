using System.Text.Json.Serialization;
using StoreCommerce.Application.Contracts.Identity;

namespace StoreCommerce.Application.Identity;

public record ConsumeActionRequest(string Token, string? NewPassword);
