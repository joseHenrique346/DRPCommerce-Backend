using MediatR;
using SharedCommerce.Application.Contracts.Appearance;

namespace SharedCommerce.Application.Features.Commands;

public sealed record PreflightAppearanceMigrationQuery(InputAppearanceTarget Target) : IRequest<AppearanceMigrationPreflightResponse>;
