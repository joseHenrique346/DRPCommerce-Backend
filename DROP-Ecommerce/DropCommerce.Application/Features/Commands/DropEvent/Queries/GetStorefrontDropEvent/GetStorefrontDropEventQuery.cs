using DropCommerce.Application.Contracts;
using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed record GetStorefrontDropEventQuery(long Id) : IRequest<Result<DropStorefrontEventResponse?>>;
