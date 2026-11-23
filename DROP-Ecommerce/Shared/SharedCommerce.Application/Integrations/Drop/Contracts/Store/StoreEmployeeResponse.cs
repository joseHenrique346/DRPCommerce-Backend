using System.Text.Json.Serialization;

namespace SharedCommerce.Application.Contracts.Drop;

public sealed record StoreEmployeeResponse(
    [property: JsonPropertyName("employeeId")] long EmployeeId,
    [property: JsonPropertyName("enterpriseId")] long EnterpriseId,
    [property: JsonPropertyName("fullName")] string FullName,
    [property: JsonPropertyName("isActive")] bool IsActive);
