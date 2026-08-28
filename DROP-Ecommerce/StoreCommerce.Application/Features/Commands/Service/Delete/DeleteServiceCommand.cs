using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity.Service;

namespace StoreCommerce.Application.Features.Commands;

public record class DeleteServiceCommand(long id) : IRequest<Result<Service>> { }
