using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Stockpilot.Application;

public static class AddApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {

        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Register FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}

