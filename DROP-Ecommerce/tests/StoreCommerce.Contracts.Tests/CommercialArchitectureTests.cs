using System.Reflection;
using System.Text.Json;
using DropCommerce.Application.Contracts;
using MediatR;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

public sealed class CommercialArchitectureTests
{
    [Fact]
    public void Commercial_workflows_do_not_use_services_between_handlers_and_repositories()
    {
        var store = Assembly.Load("StoreCommerce.Application");
        var drop = Assembly.Load("DropCommerce.Application");
        var removedTypes = new[]
        {
            "PlatformBillingService", "PlatformBillingNotifications", "DropContractPublicationService",
            "DropStockTransferService", "DropSalePaymentService", "DropCatalogService", "DropQueueService",
            "DropCheckoutService", "DropStockSagaService", "DropStorefrontService", "AccessContractPolicyService"
        };
        var types = store.GetTypes().Concat(drop.GetTypes()).ToArray();
        Assert.DoesNotContain(types, Type => removedTypes.Contains(Type.Name));

        var handlers = new[]
        {
            store.GetType("StoreCommerce.Application.Features.Commands.StartPlatformBillingCommandHandler", true)!,
            store.GetType("StoreCommerce.Application.Features.Commands.ReconcilePlatformSubscriptionCommandHandler", true)!,
            store.GetType("StoreCommerce.Application.Features.Commands.ReconcileDropSalePaymentCommandHandler", true)!,
            drop.GetType("DropCommerce.Application.Features.Commands.CreateDropCatalogProductCommandHandler", true)!,
            drop.GetType("DropCommerce.Application.Features.Commands.ReserveDropPurchaseCommandHandler", true)!,
            drop.GetType("DropCommerce.Application.Features.Commands.ApplyDropPaymentReceiptCommandHandler", true)!
        };

        foreach (var handler in handlers)
        {
            var parameters = Assert.Single(handler.GetConstructors()).GetParameters();
            Assert.Contains(parameters, Parameter => Parameter.ParameterType.IsInterface &&
                Parameter.ParameterType.Name.EndsWith("Repository", StringComparison.Ordinal));
            Assert.DoesNotContain(parameters, Parameter => removedTypes.Contains(Parameter.ParameterType.Name));
        }
    }

    [Theory]
    [InlineData("StoreCommerce.Application")]
    [InlineData("DropCommerce.Application")]
    public void Application_has_no_dependency_on_persistence_or_http_controllers(string assemblyName)
    {
        var assembly = Assembly.Load(assemblyName);

        Assert.DoesNotContain(assembly.GetReferencedAssemblies(), Reference =>
            Reference.Name!.Contains("Infrastructure") || Reference.Name.EndsWith(".Api") ||
            Reference.Name.StartsWith("Microsoft.EntityFrameworkCore"));
    }

    [Fact]
    public void Commercial_controllers_enter_through_mediator_without_concrete_services_or_contexts()
    {
        var store = Assembly.Load("StoreCommerce.Api");
        var drop = Assembly.Load("DropCommerce.Api");
        var controllers = new[]
        {
            store.GetType("StoreCommerce.Api.Controllers.PlatformBillingController", true)!,
            store.GetType("StoreCommerce.Api.Controllers.PlatformBillingWebhookController", true)!,
            store.GetType("StoreCommerce.Api.Controllers.DropStockTransferController", true)!,
            store.GetType("StoreCommerce.Api.Controllers.InternalDropStockTransferController", true)!,
            store.GetType("StoreCommerce.Api.Controllers.InternalDropSalePaymentController", true)!,
            store.GetType("StoreCommerce.Api.Controllers.InternalDropContractController", true)!,
            store.GetType("StoreCommerce.Api.Controllers.DropContractSyncController", true)!,
            drop.GetType("DropCommerce.Api.Controllers.DropCatalogController", true)!,
            drop.GetType("DropCommerce.Api.Controllers.DropPurchaseController", true)!,
            drop.GetType("DropCommerce.Api.Controllers.StorefrontDropPurchaseController", true)!,
            drop.GetType("DropCommerce.Api.Controllers.StorefrontQueueEntryController", true)!,
            drop.GetType("DropCommerce.Api.Controllers.StorefrontDropEventController", true)!,
            drop.GetType("DropCommerce.Api.Controllers.StorefrontDropRegistrationController", true)!
        };

        foreach (var controller in controllers)
        {
            var constructor = Assert.Single(controller.GetConstructors());
            var dependency = Assert.Single(constructor.GetParameters());
            Assert.Equal(typeof(IMediator), dependency.ParameterType);
        }
    }

    [Theory]
    [InlineData("StoreCommerce.Application")]
    [InlineData("DropCommerce.Application")]
    public void Every_commercial_request_has_exactly_one_concrete_handler(string assemblyName)
    {
        var assembly = Assembly.Load(assemblyName);
        var requests = assembly.GetTypes().Where(Type => Type.IsSealed &&
            Type.Namespace == assemblyName + ".Features.Commands" &&
            (Type.Name.Contains("Drop") || Type.Name.Contains("PlatformBilling") || Type.Name.Contains("EnterpriseTeamSelection")) &&
            Type.GetInterfaces().Any(Interface => Interface.IsGenericType &&
                Interface.GetGenericTypeDefinition() == typeof(IRequest<>))).ToList();
        Assert.NotEmpty(requests);

        foreach (var request in requests)
        {
            var response = request.GetInterfaces().Single(Interface => Interface.IsGenericType &&
                Interface.GetGenericTypeDefinition() == typeof(IRequest<>)).GenericTypeArguments[0];
            var handler = typeof(IRequestHandler<,>).MakeGenericType(request, response);
            Assert.Single(assembly.GetTypes(), Type => !Type.IsAbstract && handler.IsAssignableFrom(Type));
        }
    }

    [Fact]
    public void Named_drop_responses_preserve_the_public_json_fields()
    {
        var usage = JsonSerializer.SerializeToElement(new DropCatalogUsageResponse(1, true, 50, 1, true, 1, 0, 0, 0),
            JsonSerializerOptions.Web);
        Assert.Equal(new[]
        {
            "allowsDrop", "awaitingStockTransfer", "blockedParticipations", "canCreate", "catalogTotal",
            "contractRevision", "productLimit", "publishedPrincipalEvents", "transferringStock"
        }, usage.EnumerateObject().Select(Property => Property.Name).Order().ToArray());

        var queue = JsonSerializer.SerializeToElement(new DropQueueStatusResponse(1, 1, 1, null), JsonSerializerOptions.Web);
        Assert.Equal(JsonValueKind.Null, queue.GetProperty("session").ValueKind);
        Assert.Equal(1, queue.GetProperty("queueEntryStatusId").GetInt64());
    }
}
