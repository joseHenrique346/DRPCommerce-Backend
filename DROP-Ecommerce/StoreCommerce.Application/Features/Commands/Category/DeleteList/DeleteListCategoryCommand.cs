using MediatR;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public record class DeleteListCategoryCommand(List<long> ids, long? moveProductsToCategoryId = null) : IRequest<Result<bool>>;
