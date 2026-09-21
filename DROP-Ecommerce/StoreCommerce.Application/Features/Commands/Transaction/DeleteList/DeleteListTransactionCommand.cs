using MediatR;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public record class DeleteListTransactionCommand(List<long> ids) : IRequest<Result<bool>> { }
