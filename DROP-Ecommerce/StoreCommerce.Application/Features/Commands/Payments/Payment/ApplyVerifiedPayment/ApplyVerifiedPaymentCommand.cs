using MediatR;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ApplyVerifiedPaymentCommand(VerifiedPaymentUpdate Update) : IRequest<Order>;
