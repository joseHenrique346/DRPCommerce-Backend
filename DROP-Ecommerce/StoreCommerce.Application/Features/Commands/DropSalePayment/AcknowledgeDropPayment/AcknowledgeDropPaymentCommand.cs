using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using SharedCommerce.Application.Contracts.Drop;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Billing;
using StoreCommerce.Application.Payments;

namespace StoreCommerce.Application.Features.Commands;

public sealed record AcknowledgeDropPaymentCommand(long Tenant, Guid OperationId, long Revision, bool FulfillmentClosed, bool RequiresReview) : IIdentityRequest<bool>;
