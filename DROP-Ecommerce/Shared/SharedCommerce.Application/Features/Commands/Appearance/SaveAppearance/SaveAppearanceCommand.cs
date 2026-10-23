using MediatR;
using SharedCommerce.Application.Contracts.Appearance;

namespace SharedCommerce.Application.Features.Commands;

public sealed record SaveAppearanceCommand(InputAppearanceTarget Target, long SessionId, long Version, SaveAppearanceRequest Request) : IRequest<AppearanceEditResponse>;
