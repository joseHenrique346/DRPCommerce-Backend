using Commerce.Appearance.Contracts;
using MediatR;

namespace Commerce.Appearance.Application.Features;

public sealed record ResolvePublishedAppearanceQuery(
    long EnterpriseId,
    InputAppearanceTarget Target,
    long? RevisionId = null) : IRequest<OutputResolvedAppearance?>;
