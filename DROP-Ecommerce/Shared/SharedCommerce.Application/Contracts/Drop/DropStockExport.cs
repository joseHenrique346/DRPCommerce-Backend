namespace SharedCommerce.Application.Contracts.Drop;

public sealed record DropStockExport(Guid OperationId, long EnterpriseId, long ProductId, string Name,
    string Description, IReadOnlyList<DropStockVariant> Variants, DateTime? OriginalCreatedAt = null);
