namespace SharedCommerce.Application.Contracts.Drop;

public record SessionValidationRequest(string AccessToken, long? StorefrontEnterpriseId);
