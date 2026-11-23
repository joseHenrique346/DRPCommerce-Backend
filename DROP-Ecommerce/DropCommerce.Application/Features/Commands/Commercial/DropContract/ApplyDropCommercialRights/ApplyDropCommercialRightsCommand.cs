using MediatR;
using SharedCommerce.Application.Contracts.Drop;

namespace DropCommerce.Application.Features.Commands;

public sealed record ApplyDropCommercialRightsCommand(DropCommercialRights Rights) : IRequest;
