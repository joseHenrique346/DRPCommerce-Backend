using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CreateCategoryCommandHandler(IMediator mediator) : IRequestHandler<CreateCategoryCommand, Result<Category>>
{
    public async Task<Result<Category>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateListCategoryCommand([request]), cancellationToken);

        return result.IsSuccess ? Result<Category>.Success(result.Content![0]) : Result<Category>.Failure(result.Errors);
    }
}
