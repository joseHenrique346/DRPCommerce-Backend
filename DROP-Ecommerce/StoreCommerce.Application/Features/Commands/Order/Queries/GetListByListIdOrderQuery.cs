using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class GetListByListIdOrderQuery(List<long> listId) : IRequest<Result<List<Order>>> { }
