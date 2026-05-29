using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class UpdateListDropAuditLogCommand(List<UpdateDropAuditLogCommand> commands) : IRequest<Result<List<DropAuditLog>>> { }
