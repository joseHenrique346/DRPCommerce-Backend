using MediatR;
using SharedCommerce.Application.Contracts.Appearance;

namespace StoreCommerce.Application.Features.Commands;

public sealed record GetInternalAppearanceRightsQuery(long EnterpriseId, string Channel) : IRequest<AppearanceCapabilities>;
