using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ProductReactivationSelection(
        EnterpriseContractState State,
        EnterpriseEntitlements Rights,
        int Published,
        int EligibleCount,
        IReadOnlyList<Product> Products);
