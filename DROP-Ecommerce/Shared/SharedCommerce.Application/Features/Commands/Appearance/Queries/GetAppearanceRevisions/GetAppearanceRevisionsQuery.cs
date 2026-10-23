using MediatR;
using SharedCommerce.Application.Contracts.Appearance;

namespace SharedCommerce.Application.Features.Commands;

public sealed record GetAppearanceRevisionsQuery(InputAppearanceTarget Target, long? BeforeNumber = null) : IRequest<IReadOnlyList<AppearanceRevisionResponse>>;
