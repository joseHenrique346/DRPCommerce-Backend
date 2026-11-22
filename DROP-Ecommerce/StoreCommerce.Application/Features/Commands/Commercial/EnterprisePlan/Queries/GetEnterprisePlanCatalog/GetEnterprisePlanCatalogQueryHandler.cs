using MediatR;
using StoreCommerce.Application.Contracts;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetEnterprisePlanCatalogQueryHandler
    : IRequestHandler<GetEnterprisePlanCatalogQuery, Result<EnterprisePlanCatalogResponse>>
{
    public Task<Result<EnterprisePlanCatalogResponse>> Handle(
        GetEnterprisePlanCatalogQuery request, CancellationToken cancellationToken)
    {
        var plans = BaseStaticEntity.GetAll<EnterprisePlan>()
            .OrderBy(Plan => Plan.MonthlyPrice)
            .Select(EnterpriseContractResponseMapper.Plan)
            .ToList();
        var addOns = BaseStaticEntity.GetAll<EnterpriseAddOn>()
            .OrderBy(AddOn => AddOn.Id)
            .Select(EnterpriseContractResponseMapper.AddOn)
            .ToList();
        var catalog = new EnterprisePlanCatalogResponse(
            EnterpriseCommercialCatalog.Version,
            EnterpriseCommercialCatalog.Currency,
            MaximumActiveAddOns: 1,
            plans,
            addOns);

        return Task.FromResult(Result<EnterprisePlanCatalogResponse>.Success(catalog));
    }
}
