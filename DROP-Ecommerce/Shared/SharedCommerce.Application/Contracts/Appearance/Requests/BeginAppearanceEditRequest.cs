namespace SharedCommerce.Application.Contracts.Appearance;

public sealed record BeginAppearanceEditRequest(string ThemeKey, string ThemeVersion, bool UseDefaults = false);
