using DropCommerce.Infrastructure.Data.Integrations.Store.Contracts;
using Refit;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Clients;

public sealed record DropPaymentAcknowledgment(long Revision, bool FulfillmentClosed, bool RequiresReview = false);
