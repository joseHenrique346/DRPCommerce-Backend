using MediatR;
using SharedCommerce.Application.Contracts.Appearance;

namespace SharedCommerce.Application.Features.Commands;

public sealed record ResolvePublishedAppearanceQuery(
    long EnterpriseId,
    InputAppearanceTarget Target,
    long? RevisionId = null) : IRequest<OutputResolvedAppearance?>;
