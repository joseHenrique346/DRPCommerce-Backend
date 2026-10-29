using MediatR;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed class DeleteCategoryCommandHandler(IMediator mediator) : IRequestHandler<DeleteCategoryCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        return mediator.Send(new DeleteListCategoryCommand([request.id], request.moveProductsToCategoryId), cancellationToken);
    }
}
