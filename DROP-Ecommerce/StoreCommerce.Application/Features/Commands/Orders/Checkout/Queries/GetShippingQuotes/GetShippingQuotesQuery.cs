using MediatR;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record GetShippingQuotesQuery(ShippingAddressRequest Request) : IRequest<Result<CheckoutShippingQuotesResponse>>;
