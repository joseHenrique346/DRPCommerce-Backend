using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateListTransactionCommand(List<UpdateTransactionCommand> commands) : IRequest<Result<List<Transaction>>> { }
