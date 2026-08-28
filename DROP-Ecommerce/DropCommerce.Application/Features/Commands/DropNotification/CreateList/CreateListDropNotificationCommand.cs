using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class CreateListDropNotificationCommand(List<CreateDropNotificationCommand> commands) : IRequest<Result<List<DropNotification>>> { }
