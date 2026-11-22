using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class GetListByListIdDepartmentQuery(List<long> listId) : IRequest<Result<List<Department>>> { }
