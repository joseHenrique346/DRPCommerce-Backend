using MediatR;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public record class DeleteListEnterpriseCommand(List<long> ids) : IRequest<Result<bool>> { }
