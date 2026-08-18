using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class GetByIdDropAuditLogQuery(long id) : IRequest<Result<DropAuditLog>> { }