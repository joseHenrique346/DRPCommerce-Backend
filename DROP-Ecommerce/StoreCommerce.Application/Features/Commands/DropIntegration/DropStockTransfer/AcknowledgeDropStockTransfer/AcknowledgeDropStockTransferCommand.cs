using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Contracts;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using SharedCommerce.Application.Contracts.Drop;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Billing;
using StoreCommerce.Application.Payments;

namespace StoreCommerce.Application.Features.Commands;

public sealed record AcknowledgeDropStockTransferCommand(long Tenant, Guid OperationId, long CatalogProductId) : IIdentityRequest<bool>;
