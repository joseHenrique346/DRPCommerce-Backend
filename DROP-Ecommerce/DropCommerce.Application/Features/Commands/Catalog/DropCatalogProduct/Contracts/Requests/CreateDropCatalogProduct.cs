namespace DropCommerce.Application.Contracts;

public sealed record CreateDropCatalogProduct(string Name, string Description, IReadOnlyList<CreateDropCatalogVariant> Variants);
