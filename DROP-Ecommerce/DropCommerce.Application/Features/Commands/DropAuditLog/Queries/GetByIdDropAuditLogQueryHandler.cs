using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropAuditLogQueryHandler(IMediator mediator) : IRequestHandler<GetByIdDropAuditLogQuery, Result<DropAuditLog>>
{
    public async Task<Result<DropAuditLog>> Handle(GetByIdDropAuditLogQuery request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetListByListIdDropAuditLogQuery([request.id]), cancellationToken);
        return result.IsSuccess && result.Content.Count > 0
            ? Result<DropAuditLog>.Success(result.Content.First())
            : Result<DropAuditLog>.Failure("DropAuditLog não encontrado.");
    }
}
