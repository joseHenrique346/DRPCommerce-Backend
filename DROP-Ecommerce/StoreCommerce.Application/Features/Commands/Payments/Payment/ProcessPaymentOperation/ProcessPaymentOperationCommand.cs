using MediatR;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ProcessPaymentOperationCommand(long EnterpriseId, long OperationId) : IRequest;
