using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class CreateListFraudSignalCommand(List<CreateFraudSignalCommand> commands) : IRequest<Result<List<FraudSignal>>> { }
