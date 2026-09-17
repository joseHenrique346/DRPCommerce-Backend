using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateListCustomerCommand(List<UpdateCustomerCommand> commands) : IRequest<Result<List<Customer>>> { }
