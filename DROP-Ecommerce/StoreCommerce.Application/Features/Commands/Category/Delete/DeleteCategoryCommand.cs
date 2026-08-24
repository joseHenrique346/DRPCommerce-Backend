using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity.Category;

namespace StoreCommerce.Application.Features.Commands;

public record class DeleteCategoryCommand(long id) : IRequest<Result<Category>> { }
