using MediatR;
using SharedCommerce.Application.Contracts.Appearance;

namespace SharedCommerce.Application.Features.Commands;

public sealed record MigrateAppearanceCommand(long EnterpriseId, InputAppearanceTarget Target) : IRequest;
