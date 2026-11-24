using SharedCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class DeleteListDropAuditLogCommand(List<long> ids) : IRequest<Result<bool>> { }
