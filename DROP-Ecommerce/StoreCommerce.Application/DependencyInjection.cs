using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using StoreCommerce.Application.Features;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddStoreApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
            assembly,
            typeof(Commerce.Appearance.Application.Features.GetAppearanceStatusQueryHandler).Assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddScoped<IdentityHandlerContext>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        return services;
    }
}
