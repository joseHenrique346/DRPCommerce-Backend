using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class CreateDropReservationCommand(long dropEventId, long dropProductId, long customerId, long queueEntryId, long statusId, int quantity, decimal unitPrice, decimal totalAmount, string lockToken, DateTime reservedAt, DateTime expiresAt, DateTime? confirmedAt, DateTime? cancelledAt) : IRequest<Result<DropReservation>> { }
