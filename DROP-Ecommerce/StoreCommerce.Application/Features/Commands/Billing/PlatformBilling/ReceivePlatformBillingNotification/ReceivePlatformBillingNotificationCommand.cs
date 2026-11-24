using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ReceivePlatformBillingNotificationCommand(
    string Topic, string ResourceId, string RequestId, string Signature) : IIdentityRequest<bool>;
