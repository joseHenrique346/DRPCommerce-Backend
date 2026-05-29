using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class UpdateListFraudSignalCommand(List<UpdateFraudSignalCommand> commands) : IRequest<Result<List<FraudSignal>>> { }
