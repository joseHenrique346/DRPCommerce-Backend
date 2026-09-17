using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateListCategoryCommand(List<UpdateCategoryCommand> commands) : IRequest<Result<List<Category>>> { }
