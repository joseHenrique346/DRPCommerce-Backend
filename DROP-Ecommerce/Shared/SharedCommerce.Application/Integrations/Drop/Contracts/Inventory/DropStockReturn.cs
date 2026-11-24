namespace SharedCommerce.Application.Contracts.Drop;

public sealed record DropStockReturn(Guid OperationId, Guid OriginalExportId, long EnterpriseId, long ProductId,
    IReadOnlyList<DropStockVariant> Variants);
