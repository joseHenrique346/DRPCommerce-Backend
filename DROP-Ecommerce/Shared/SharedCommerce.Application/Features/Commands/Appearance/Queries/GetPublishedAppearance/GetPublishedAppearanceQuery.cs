using MediatR;
using SharedCommerce.Application.Contracts.Appearance;

namespace SharedCommerce.Application.Features.Commands;

public sealed record GetPublishedAppearanceQuery(InputAppearanceTarget Target, long? RevisionId = null) : IRequest<AppearanceManifest>;
