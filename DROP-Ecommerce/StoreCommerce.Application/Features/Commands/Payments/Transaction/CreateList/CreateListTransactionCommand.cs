using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateListTransactionCommand(List<CreateTransactionCommand> commands) : IRequest<Result<List<Transaction>>> { }
