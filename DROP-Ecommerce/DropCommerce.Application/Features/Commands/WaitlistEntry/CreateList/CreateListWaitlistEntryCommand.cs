using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class CreateListWaitlistEntryCommand(List<CreateWaitlistEntryCommand> commands) : IRequest<Result<List<WaitlistEntry>>> { }
