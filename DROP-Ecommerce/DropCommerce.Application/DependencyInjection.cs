using DropCommerce.Application.Features;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DropCommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddDropApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        return services;
    }
}
