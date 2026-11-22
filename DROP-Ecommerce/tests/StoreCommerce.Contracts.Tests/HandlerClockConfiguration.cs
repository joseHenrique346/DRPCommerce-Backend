using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace StoreCommerce.Contracts.Tests;

internal static class HandlerClockConfiguration
{
    public static void UseHandlerClock(this IServiceCollection services, TimeProvider clock, Assembly assembly)
    {
        foreach (var handler in assembly.GetTypes().Where(Type => Type.IsSealed &&
            Type.Namespace?.EndsWith(".Features.Commands", StringComparison.Ordinal) == true))
        {
            if (!handler.GetConstructors().Any(Constructor => Constructor.GetParameters().Any(Parameter =>
                Parameter.ParameterType == typeof(TimeProvider) && Parameter.Name == "clock")))
                continue;

            foreach (var contract in handler.GetInterfaces().Where(Interface => Interface.IsGenericType &&
                Interface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
                services.AddTransient(contract, Provider => ActivatorUtilities.CreateInstance(Provider, handler, clock));
        }
    }
}
