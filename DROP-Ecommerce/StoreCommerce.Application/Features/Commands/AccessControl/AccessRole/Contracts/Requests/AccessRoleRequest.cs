using System.Text.Json.Serialization;
using StoreCommerce.Application.Contracts.Identity;

namespace StoreCommerce.Application.Identity;

public record AccessRoleRequest(string Name, string Description, bool IsActive, List<long> PermissionIds);
