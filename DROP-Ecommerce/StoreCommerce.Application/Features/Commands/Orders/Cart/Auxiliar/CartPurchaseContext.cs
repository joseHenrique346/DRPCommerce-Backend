using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record CartPurchaseContext(long EnterpriseId, long CustomerId, StoreSettings Settings, IReadOnlySet<long> VisibleCategories);
