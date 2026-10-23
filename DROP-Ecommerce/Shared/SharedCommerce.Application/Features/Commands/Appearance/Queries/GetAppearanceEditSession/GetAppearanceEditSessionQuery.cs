using MediatR;
using SharedCommerce.Application.Contracts.Appearance;

namespace SharedCommerce.Application.Features.Commands;

public sealed record GetAppearanceEditSessionQuery(InputAppearanceTarget Target) : IRequest<AppearanceEditResponse?>;
