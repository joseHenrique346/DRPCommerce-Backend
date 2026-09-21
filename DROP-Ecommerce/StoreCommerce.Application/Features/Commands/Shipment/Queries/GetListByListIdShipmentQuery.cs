using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class GetListByListIdShipmentQuery(List<long> listId) : IRequest<Result<List<Shipment>>> { }
