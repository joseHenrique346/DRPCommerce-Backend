using StoreCommerce.Domain.Enums.Payments;
using System.Text.Json;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using OrderEntity = StoreCommerce.Domain.Entity.Order;

namespace StoreCommerce.Application.Contracts.Order;

public sealed record PaymentOperationInfoResponse(long Id, PaymentOperationKind Kind, PaymentOperationStatus Status, decimal Amount, string? CurrencyCode, string? Reference, string? ResolutionCode, DateTime? CompletedAt);
