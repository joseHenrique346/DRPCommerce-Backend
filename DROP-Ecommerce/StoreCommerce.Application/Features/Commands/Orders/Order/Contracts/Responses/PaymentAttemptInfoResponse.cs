using StoreCommerce.Domain.Enums.Payments;
using System.Text.Json;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using OrderEntity = StoreCommerce.Domain.Entity.Order;

namespace StoreCommerce.Application.Contracts.Order;

public sealed record PaymentAttemptInfoResponse(long Id, PaymentCreationStatus CreationStatus, string CurrencyCode, string? PreferenceId, IReadOnlyList<string> PaymentIds);
