using MediatR;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public record class DeleteListOrderCommand(List<long> ids) : IRequest<Result<bool>> { }
