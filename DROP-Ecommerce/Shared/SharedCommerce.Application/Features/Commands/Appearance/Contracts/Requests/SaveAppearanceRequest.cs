using System.Text.Json;

namespace SharedCommerce.Application.Contracts.Appearance;

public sealed record SaveAppearanceRequest(JsonElement Settings);
