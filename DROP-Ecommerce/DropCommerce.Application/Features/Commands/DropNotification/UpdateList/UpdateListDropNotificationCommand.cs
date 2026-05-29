using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class UpdateListDropNotificationCommand(List<UpdateDropNotificationCommand> commands) : IRequest<Result<List<DropNotification>>> { }
