using MediatR;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public record class DeleteCategoryCommand(long id, long? moveProductsToCategoryId = null) : IRequest<Result<bool>>;
