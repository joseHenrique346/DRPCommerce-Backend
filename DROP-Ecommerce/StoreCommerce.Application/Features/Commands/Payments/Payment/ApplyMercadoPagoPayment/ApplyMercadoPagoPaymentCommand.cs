using MediatR;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ApplyMercadoPagoPaymentCommand(ProviderConnection Connection, string PaymentId) : IRequest;
