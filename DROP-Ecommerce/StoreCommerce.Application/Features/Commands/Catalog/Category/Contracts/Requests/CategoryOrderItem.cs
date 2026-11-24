using MediatR;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record CategoryOrderItem(long Id, int DisplayOrder);
