using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class UpdateListDropTransactionCommand(List<UpdateDropTransactionCommand> commands) : IRequest<Result<List<DropTransaction>>> { }
