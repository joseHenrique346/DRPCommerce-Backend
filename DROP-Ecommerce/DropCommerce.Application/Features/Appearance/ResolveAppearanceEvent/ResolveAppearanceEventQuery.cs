using Commerce.Appearance.Contracts;
using MediatR;

namespace DropCommerce.Application.Features;

public sealed record ResolveAppearanceEventQuery(string Slug) : IRequest<InputAppearanceTarget>;
