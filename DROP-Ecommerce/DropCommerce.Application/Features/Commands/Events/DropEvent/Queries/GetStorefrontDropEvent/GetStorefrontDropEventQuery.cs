using DropCommerce.Application.Contracts;
using SharedCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed record GetStorefrontDropEventQuery(long Id) : IRequest<Result<DropStorefrontEventResponse?>>;
