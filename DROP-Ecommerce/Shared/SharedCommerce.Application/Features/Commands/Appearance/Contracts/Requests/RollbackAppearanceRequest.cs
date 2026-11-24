namespace SharedCommerce.Application.Contracts.Appearance;

public sealed record RollbackAppearanceRequest(long? ExpectedRevisionId);
