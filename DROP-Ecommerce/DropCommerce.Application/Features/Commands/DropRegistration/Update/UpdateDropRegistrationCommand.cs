using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class UpdateDropRegistrationCommand(long id, long dropEventId, long customerId, long statusId, bool isEligible, string eligibilityReason, DateTime registeredAt, DateTime? eligibilityCheckedAt) : IRequest<Result<DropRegistration>> { }
