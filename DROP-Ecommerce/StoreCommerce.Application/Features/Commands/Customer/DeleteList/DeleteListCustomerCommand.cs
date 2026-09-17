using MediatR;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public record class DeleteListCustomerCommand(List<long> ids) : IRequest<Result<bool>> { }
