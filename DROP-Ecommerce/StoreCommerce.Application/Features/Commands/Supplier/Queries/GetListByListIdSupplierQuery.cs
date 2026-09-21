using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class GetListByListIdSupplierQuery(List<long> listId) : IRequest<Result<List<Supplier>>> { }
