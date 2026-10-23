using MediatR;
using SharedCommerce.Application.Contracts.Appearance;

namespace SharedCommerce.Application.Features.Commands;

public sealed record RollbackAppearanceCommand(InputAppearanceTarget Target, long RevisionId, RollbackAppearanceRequest Request) : IRequest<AppearanceManifest>;
