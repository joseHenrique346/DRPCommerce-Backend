using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Identity;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateUserCommandHandler(
    IUserRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateUserCommand, Result<User>>
{
    public async Task<Result<User>> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var entity = (await repository.GetListByListIdAsync([request.id], ct)).SingleOrDefault();

        if (entity is null)
            return Result<User>.NotFound("User.NotFound", $"User com id {request.id} não encontrado.");

        entity.UpdateDetails(request.fullName, UserEmail.Create(request.email), request.isActive);
        repository.UpdateRange([entity]);
        await unitOfWork.CommitAsync(ct);

        return Result<User>.Success(entity);
    }
}
