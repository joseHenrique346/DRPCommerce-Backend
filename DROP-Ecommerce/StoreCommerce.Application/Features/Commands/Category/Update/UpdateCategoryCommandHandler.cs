using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateCategoryCommandHandler(IMediator mediator) : IRequestHandler<UpdateCategoryCommand, Result<Category>>
{
    public async Task<Result<Category>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateListCategoryCommand([request]), cancellationToken);

        return result.IsSuccess ? Result<Category>.Success(result.Content![0]) : Result<Category>.Failure(result.Errors);
    }
}
