using System.Text.Json;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using MediatR;
using DropCommerce.Application.Contracts;
using DropCommerce.Application.Result;

namespace DropCommerce.Application.Features.Commands;

public sealed class CompleteDropStockReturnCommandHandler(
    IDropStockSagaRepository persistence,
    IUnitOfWork unitOfWork,
    DropCommercialWriteScope writes)
    : IRequestHandler<CompleteDropStockReturnCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CompleteDropStockReturnCommand command, CancellationToken ct)
    {
        long tenant = command.Tenant;
        Guid operationId = command.OperationId;

        using var operation = writes.Begin("transfer", tenant);
        await using var tx = await persistence.BeginTransactionAsync(ct);
        await persistence.LockCommercialTenantAsync(tenant, ct);
        var intent = await persistence.GetReturnAsync(tenant, operationId, ct);
        var product = await persistence.GetProductAsync(intent.CatalogProductId, tenant, ct);
        intent.Complete(DateTime.UtcNow);
        product.CompleteReturn();
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return Result<bool>.Success(true);
    }
}
