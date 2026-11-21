namespace SharedCommerce.Application.Contracts.Drop;

public sealed record DropStockVariant(long SourceVariantId, string SKU, string OptionsJson, decimal Price, int Quantity);
public sealed record DropStockExport(Guid OperationId, long EnterpriseId, long ProductId, string Name,
    string Description, IReadOnlyList<DropStockVariant> Variants, DateTime? OriginalCreatedAt = null);
public sealed record DropStockReturn(Guid OperationId, Guid OriginalExportId, long EnterpriseId, long ProductId,
    IReadOnlyList<DropStockVariant> Variants);
