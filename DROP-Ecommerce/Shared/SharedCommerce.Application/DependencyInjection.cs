using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SharedCommerce.Application.Validation;

namespace SharedCommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedApplication(this IServiceCollection services, params Assembly[] moduleAssemblies)
    {
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssemblies(
            [.. moduleAssemblies, typeof(DependencyInjection).Assembly]));

        services.AddScoped<AppearanceSettingsValidator>();
        services.AddScoped<AppearanceResourceValidator>();
        services.AddScoped<AppearanceMigration>();
        services.AddScoped<ThemeDefinitionValidator>();

        return services;
    }
}
