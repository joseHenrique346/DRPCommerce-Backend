using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

public sealed class ArchitectureCompletionTests
{
    [Theory]
    [InlineData("StoreCommerce.Application")]
    [InlineData("DropCommerce.Application")]
    [InlineData("SharedCommerce.Application")]
    public void Each_concrete_handler_owns_one_request(string assemblyName)
    {
        var handlers = Assembly.Load(assemblyName).GetTypes()
            .Where(Type => !Type.IsAbstract && !Type.ContainsGenericParameters)
            .Select(Type => new
            {
                Handler = Type,
                Requests = Type.GetInterfaces().Where(Interface => Interface.IsGenericType &&
                    (Interface.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
                     Interface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))).ToArray()
            })
            .Where(Item => Item.Requests.Length > 0).ToList();
        Assert.NotEmpty(handlers);

        foreach (var handler in handlers)
            Assert.True(handler.Requests.Length == 1, handler.Handler.FullName);
    }

    [Theory]
    [InlineData("StoreCommerce.Api")]
    [InlineData("DropCommerce.Api")]
    public void Controllers_do_not_depend_on_contexts_repositories_or_business_services(string assemblyName)
    {
        var controllers = Assembly.Load(assemblyName).GetTypes()
            .Where(Type => !Type.IsAbstract && typeof(ControllerBase).IsAssignableFrom(Type)).ToList();
        Assert.NotEmpty(controllers);

        foreach (var controller in controllers)
        {
            var dependencies = controller.GetConstructors().SelectMany(Constructor => Constructor.GetParameters());
            Assert.DoesNotContain(dependencies, Parameter =>
                Parameter.ParameterType.Name.EndsWith("DbContext", StringComparison.Ordinal) ||
                Parameter.ParameterType.Name.EndsWith("Repository", StringComparison.Ordinal) ||
                Parameter.ParameterType.Name.EndsWith("Service", StringComparison.Ordinal) &&
                    Parameter.ParameterType != typeof(StoreCommerce.Security.Services.JwtSessionService) ||
                Parameter.ParameterType.Name is "IUnitOfWork" or "IIdentityUnitOfWork");
        }
    }

    [Fact]
    public void Writers_do_not_depend_on_transaction_owners_or_request_dispatchers()
    {
        var writers = Assembly.Load("StoreCommerce.Application").GetTypes()
            .Where(Type => Type.Name.EndsWith("Writer", StringComparison.Ordinal)).ToList();
        Assert.NotEmpty(writers);

        foreach (var writer in writers)
        {
            Assert.DoesNotContain(writer.GetConstructors().SelectMany(Constructor => Constructor.GetParameters()), Parameter =>
                Parameter.ParameterType.Name is "IUnitOfWork" or "IIdentityUnitOfWork" or "ISender" or "IMediator" or "CatalogWriteContext");
        }
    }

    [Fact]
    public void Removed_catalog_writers_and_provider_orchestrators_cannot_be_reintroduced()
    {
        var types = Assembly.Load("StoreCommerce.Application").GetTypes()
            .Concat(Assembly.Load("StoreCommerce.Infrastructure.Data").GetTypes());
        var removed = new[] { "ProductCatalogWriter", "ProductVariantWriter", "MercadoPagoAuthorizationService", "MercadoPagoNotifications" };
        Assert.DoesNotContain(types, Type => removed.Contains(Type.Name));
    }
}
