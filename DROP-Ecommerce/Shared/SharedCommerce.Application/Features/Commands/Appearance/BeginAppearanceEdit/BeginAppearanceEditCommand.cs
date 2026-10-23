using MediatR;
using SharedCommerce.Application.Contracts.Appearance;

namespace SharedCommerce.Application.Features.Commands;

public sealed record BeginAppearanceEditCommand(InputAppearanceTarget Target, BeginAppearanceEditRequest Request) : IRequest<AppearanceEditResponse>;
