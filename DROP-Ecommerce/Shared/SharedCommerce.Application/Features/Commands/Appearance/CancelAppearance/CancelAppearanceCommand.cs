using MediatR;
using SharedCommerce.Application.Contracts.Appearance;

namespace SharedCommerce.Application.Features.Commands;

public sealed record CancelAppearanceCommand(InputAppearanceTarget Target, long SessionId, long Version, bool Force = false) : IRequest;
