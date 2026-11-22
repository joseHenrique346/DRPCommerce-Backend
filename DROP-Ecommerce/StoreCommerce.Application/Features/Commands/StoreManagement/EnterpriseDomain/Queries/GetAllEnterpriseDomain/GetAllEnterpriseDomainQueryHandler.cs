using StoreCommerce.Domain.Interfaces.StoreManagement;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetAllEnterpriseDomainQueryHandler(IEnterpriseDomainRepository repository)
    : BaseGetAllHandler<GetAllEnterpriseDomainQuery, EnterpriseDomain>(repository)
;
