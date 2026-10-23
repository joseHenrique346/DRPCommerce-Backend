using MediatR;
using SharedCommerce.Application.Contracts.Appearance;

namespace SharedCommerce.Application.Features.Commands;

public sealed record GetAppearanceStatusQuery(InputAppearanceTarget Target) : IRequest<AppearanceStatusResponse>;
