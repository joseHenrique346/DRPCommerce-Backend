using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class CreateDropAuditLogCommand(long dropEventId, long? customerId, long? employeeId, string action, string entityName, long entityId, string oldValues, string newValues, string ipAddress, string userAgent, DateTime ocurredAt) : IRequest<Result<DropAuditLog>> { }