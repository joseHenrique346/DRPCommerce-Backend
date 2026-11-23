using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Identity;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CreateUserCommandHandler(
    IUserRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateUserCommand, Result<User>>
{
    public async Task<Result<User>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var entity = User.Create(request.fullName, UserEmail.Create(request.email), request.isActive);
        await repository.AddRangeAsync([entity], ct);
        await unitOfWork.CommitAsync(ct);

        return Result<User>.Success(entity);
    }
}
