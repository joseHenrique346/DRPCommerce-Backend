using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class GetByIdCategoryQuery(long id) : IRequest<Result<Category>> { }
