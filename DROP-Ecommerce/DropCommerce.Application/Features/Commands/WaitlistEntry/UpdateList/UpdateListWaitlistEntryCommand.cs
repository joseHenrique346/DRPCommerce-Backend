using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class UpdateListWaitlistEntryCommand(List<UpdateWaitlistEntryCommand> commands) : IRequest<Result<List<WaitlistEntry>>> { }
