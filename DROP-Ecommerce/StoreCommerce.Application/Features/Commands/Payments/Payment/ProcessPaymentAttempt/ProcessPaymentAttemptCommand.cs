using MediatR;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ProcessPaymentAttemptCommand(long EnterpriseId, long AttemptId) : IRequest;
