using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateListCustomerCommand(List<CreateCustomerCommand> commands) : IRequest<Result<List<Customer>>> { }
