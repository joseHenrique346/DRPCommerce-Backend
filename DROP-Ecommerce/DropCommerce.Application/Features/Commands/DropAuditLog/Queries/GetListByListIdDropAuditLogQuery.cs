using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class GetListByListIdDropAuditLogQuery(List<long> listId) : IRequest<Result<List<DropAuditLog>>> { }