using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class UpdateFraudSignalCommand(long id, long customerId, long dropEventId, long queueEntryId, long signalTypeId, long severityId, string description, string ipAddress, string deviceFingerprint, bool isConfirmed, bool wasBlocked, DateTime detectedAt, DateTime? reviewedAt) : IRequest<Result<FraudSignal>> { }
