using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class GetListByListIdRoleQuery(List<long> listId) : IRequest<Result<List<Role>>> { }
