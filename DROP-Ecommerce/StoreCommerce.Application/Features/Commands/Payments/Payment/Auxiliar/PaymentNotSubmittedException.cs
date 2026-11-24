namespace StoreCommerce.Application.Payments;

/// <summary>
/// O adapter comprova que não chegou a enviar a escrita externa. Não usar depois de um POST/PUT iniciado.
/// </summary>
public sealed class PaymentNotSubmittedException() : Exception("A operação não foi enviada ao provedor.");
