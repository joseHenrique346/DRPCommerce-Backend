namespace SharedCommerce.Application.Contracts.Appearance;

public sealed record AppearanceRevisionResponse(
    long Id,
    long Number,
    ThemeIdentity Theme,
    string Checksum,
    long PublishedBy,
    DateTime PublishedAt,
    long? RestoredFromRevisionId,
    string Origin);
