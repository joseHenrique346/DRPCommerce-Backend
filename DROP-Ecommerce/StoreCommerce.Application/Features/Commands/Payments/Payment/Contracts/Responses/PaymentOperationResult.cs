using StoreCommerce.Domain.Enums.Payments;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Payments;

/// <summary>
/// Failed só é permitido quando o provedor comprova que a operação não foi aplicada.
/// Succeeded exige uma referência verificável. Erros/transporte/timeout são OutcomeUnknown.
/// </summary>
public sealed record PaymentOperationResult(PaymentOperationStatus Status, string? Reference = null);
