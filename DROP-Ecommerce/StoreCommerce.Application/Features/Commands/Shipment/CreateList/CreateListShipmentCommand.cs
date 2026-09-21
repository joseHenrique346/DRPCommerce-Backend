using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateListShipmentCommand(List<CreateShipmentCommand> commands) : IRequest<Result<List<Shipment>>> { }
