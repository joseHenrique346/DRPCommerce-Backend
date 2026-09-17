using MediatR;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public record class DeleteListOrderItemCommand(List<long> ids) : IRequest<Result<bool>> { }
