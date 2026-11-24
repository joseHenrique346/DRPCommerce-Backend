using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed record GetMyDropOrderQuery(long Id) : IRequest<DropOrderDetails?>;
