using SharedCommerce.Application.Contracts.Appearance;
using MediatR;

namespace DropCommerce.Application.Features;

public sealed record ResolveAppearanceEventQuery(string Slug) : IRequest<InputAppearanceTarget>;
