using Commerce.Appearance.Contracts;
using MediatR;

namespace Commerce.Appearance.Application.Features;

public sealed record CancelAppearanceCommand(InputAppearanceTarget Target, long SessionId, long Version, bool Force = false) : IRequest;
