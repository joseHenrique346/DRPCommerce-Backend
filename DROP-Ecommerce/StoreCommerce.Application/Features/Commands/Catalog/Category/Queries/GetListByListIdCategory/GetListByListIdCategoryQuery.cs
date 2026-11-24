using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class GetListByListIdCategoryQuery(List<long> listId) : IRequest<Result<List<Category>>> { }
