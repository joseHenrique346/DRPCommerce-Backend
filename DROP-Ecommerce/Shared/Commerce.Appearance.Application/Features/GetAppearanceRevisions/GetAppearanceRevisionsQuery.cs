using Commerce.Appearance.Contracts;
using MediatR;

namespace Commerce.Appearance.Application.Features;

public sealed record GetAppearanceRevisionsQuery(InputAppearanceTarget Target, long? BeforeNumber = null) : IRequest<IReadOnlyList<AppearanceRevisionResponse>>;
