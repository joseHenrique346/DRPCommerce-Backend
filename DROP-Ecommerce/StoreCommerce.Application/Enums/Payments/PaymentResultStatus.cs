namespace StoreCommerce.Application.Enums.Payments;

/// <summary>
/// Resultado de pagamento informado pelo gateway (webhook) ou pelo backoffice (confirmação manual).
/// </summary>
public enum PaymentResultStatus
{
    Approved = 1,
    Failed = 2
}
