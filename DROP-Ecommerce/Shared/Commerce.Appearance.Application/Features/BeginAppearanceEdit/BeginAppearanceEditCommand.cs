using Commerce.Appearance.Contracts;
using MediatR;

namespace Commerce.Appearance.Application.Features;

public sealed record BeginAppearanceEditCommand(InputAppearanceTarget Target, BeginAppearanceEditRequest Request) : IRequest<AppearanceEditResponse>;
