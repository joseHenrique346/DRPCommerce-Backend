using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

public sealed class CommercialCatalogTests
{
    [Fact]
    public async Task Catalog_exposes_five_plans_and_five_fixed_add_ons()
    {
        var result = await new GetEnterprisePlanCatalogQueryHandler()
            .Handle(new GetEnterprisePlanCatalogQuery(), default);
        var catalog = Assert.IsType<EnterprisePlanCatalogResponse>(result.Content);

        Assert.Equal("BRL", catalog.Currency);
        Assert.Equal(1, catalog.CatalogVersion);
        Assert.Equal(1, catalog.MaximumActiveAddOns);
        Assert.Equal(new[] { "free", "initial", "pro", "drpdrop", "drpcommerce" },
            catalog.Plans.Select(Plan => Plan.Code));
        Assert.Equal(new[] { 0m, 49m, 99m, 149m, 199m },
            catalog.Plans.Select(Plan => Plan.MonthlyPrice));
        Assert.Equal(new[] { 9.90m, 29.90m, 9.90m, 19.90m, 14.90m },
            catalog.AddOns.Select(AddOn => AddOn.MonthlyPrice));
    }

    [Theory]
    [InlineData(1, 50, 0, 0, 1, 0)]
    [InlineData(4, 100, 0, 1, 3, 0)]
    [InlineData(2, 500, 0, 3, 10, 0)]
    [InlineData(3, 2000, 100, 5, 20, 10)]
    [InlineData(5, 0, 100, 5, 0, 10)]
    public void Plans_match_the_approved_limits(long planId, int products, int drops, int employees,
        int storeThemes, int dropThemes)
    {
        var rights = EnterpriseEntitlements.Resolve(planId, null);

        Assert.Equal(products, rights.StoreProductLimit);
        Assert.Equal(drops, rights.DropProductLimit);
        Assert.Equal(employees, rights.EmployeeLimit);
        Assert.Equal(storeThemes, rights.StoreThemeLimit);
        Assert.Equal(dropThemes, rights.DropThemeLimit);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 3)]
    [InlineData(4, 2)]
    [InlineData(4, 5)]
    [InlineData(2, 5)]
    [InlineData(3, 3)]
    [InlineData(5, 4)]
    [InlineData(5, 3)]
    public void Incompatible_add_on_never_grants_a_module(long planId, long addOnId)
    {
        Assert.Throws<ArgumentException>(() => EnterpriseEntitlements.Resolve(planId, addOnId));
    }

    [Fact]
    public void Add_on_increases_only_its_resource()
    {
        var rights = EnterpriseEntitlements.Resolve(EnterprisePlan.Pro.Id, EnterpriseAddOn.LargeTeam.Id);

        Assert.Equal(8, rights.EmployeeLimit);
        Assert.Equal(500, rights.StoreProductLimit);
        Assert.False(rights.AllowsDrop);
        Assert.Equal(0, rights.DropProductLimit);
    }

    [Fact]
    public void Unknown_ids_fail_instead_of_falling_back_to_free()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EnterpriseEntitlements.Resolve(99, null));
        Assert.Throws<ArgumentOutOfRangeException>(() => EnterpriseEntitlements.Resolve(2, 99));
    }

    [Fact]
    public void Changing_only_the_add_on_changes_contract_revision_once()
    {
        var enterprise = NewEnterprise();
        enterprise.ChangeContract(EnterprisePlan.Pro.Id, null);
        var revision = enterprise.ContractRevision;

        enterprise.ChangeContract(EnterprisePlan.Pro.Id, EnterpriseAddOn.LargeTeam.Id);

        Assert.Equal(revision + 1, enterprise.ContractRevision);
        Assert.Equal(EnterprisePlan.Pro.Id, enterprise.PlanId);
        Assert.Equal(EnterpriseAddOn.LargeTeam.Id, enterprise.AddOnId);

        enterprise.ChangeContract(EnterprisePlan.Pro.Id, EnterpriseAddOn.LargeTeam.Id);

        Assert.Equal(revision + 1, enterprise.ContractRevision);
    }

    [Fact]
    public void Invalid_change_does_not_partially_mutate_the_enterprise()
    {
        var enterprise = NewEnterprise();

        Assert.Throws<ArgumentException>(() => enterprise.ChangeContract(1, 1));
        Assert.Equal(EnterprisePlan.Free.Id, enterprise.PlanId);
        Assert.Null(enterprise.AddOnId);
        Assert.Equal(1, enterprise.ContractRevision);
    }

    [Fact]
    public void Choosing_an_offer_freezes_server_prices_and_grants_no_access()
    {
        var enterprise = NewEnterprise();
        var change = EnterpriseContractChange.Request(10, 20, "choice", 3, 5, 1, DateTime.UtcNow);
        var response = EnterpriseContractResponseMapper.Change(change);

        Assert.Equal(199m, change.PlanMonthlyPrice);
        Assert.Equal(14.90m, change.AddOnMonthlyPrice);
        Assert.Equal(213.90m, response.TotalMonthlyPrice);
        Assert.False(response.GrantsAccess);
        Assert.Equal(EnterprisePlan.Free.Id, enterprise.PlanId);
        Assert.Equal(1, enterprise.ContractRevision);
    }

    private static Enterprise NewEnterprise()
    {
        return Enterprise.Create("Loja", "Loja Teste", EnterpriseEmail.Create("shop@example.invalid"),
            EnterprisePhone.Create("11999999999"), "Rua 1", "São Paulo", 1, "01001000", "BR", true);
    }
}
