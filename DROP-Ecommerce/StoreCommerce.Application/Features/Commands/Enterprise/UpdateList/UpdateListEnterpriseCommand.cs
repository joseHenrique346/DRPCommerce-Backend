using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateListEnterpriseCommand(List<UpdateEnterpriseCommand> commands) : IRequest<Result<List<Enterprise>>> { }
