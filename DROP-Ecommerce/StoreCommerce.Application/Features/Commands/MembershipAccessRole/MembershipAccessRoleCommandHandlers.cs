using MediatR;
using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateMembershipAccessRoleCommandHandler(IMembershipAccessRoleRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateMembershipAccessRoleCommand, Result<MembershipAccessRoleResponse>>
{
    public async Task<Result<MembershipAccessRoleResponse>> Handle(CreateMembershipAccessRoleCommand request, CancellationToken cancellationToken)
    {
        var entity = MembershipAccessRole.Create(request.enterpriseId, request.enterpriseMembershipId, request.accessRoleId);
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result<MembershipAccessRoleResponse>.Success(new(entity.EnterpriseId, entity.EnterpriseMembershipId, entity.AccessRoleId));
    }
}

public class DeleteMembershipAccessRoleCommandHandler(IMembershipAccessRoleRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteMembershipAccessRoleCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteMembershipAccessRoleCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByKeyAsync(request.enterpriseMembershipId, request.accessRoleId, cancellationToken);
        if (entity is null)
            return Result<bool>.NotFound("MembershipAccessRole.NotFound", "Relacionamento nao encontrado.");
        repository.Delete(entity);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}

public class GetByKeyMembershipAccessRoleQueryHandler(IMembershipAccessRoleRepository repository)
    : IRequestHandler<GetByKeyMembershipAccessRoleQuery, Result<MembershipAccessRoleResponse>>
{
    public async Task<Result<MembershipAccessRoleResponse>> Handle(GetByKeyMembershipAccessRoleQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByKeyAsync(request.enterpriseMembershipId, request.accessRoleId, cancellationToken);
        return entity is null
            ? Result<MembershipAccessRoleResponse>.NotFound("MembershipAccessRole.NotFound", "Relacionamento nao encontrado.")
            : Result<MembershipAccessRoleResponse>.Success(new(entity.EnterpriseId, entity.EnterpriseMembershipId, entity.AccessRoleId));
    }
}

public class GetAllMembershipAccessRoleQueryHandler(IMembershipAccessRoleRepository repository)
    : IRequestHandler<GetAllMembershipAccessRoleQuery, Result<List<MembershipAccessRoleResponse>>>
{
    public async Task<Result<List<MembershipAccessRoleResponse>>> Handle(GetAllMembershipAccessRoleQuery request, CancellationToken cancellationToken)
    {
        var entities = await repository.GetAllAsync(cancellationToken);
        return Result<List<MembershipAccessRoleResponse>>.Success(entities
            .Select(entity => new MembershipAccessRoleResponse(entity.EnterpriseId, entity.EnterpriseMembershipId, entity.AccessRoleId)).ToList());
    }
}
