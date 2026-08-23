using System.Reflection;
using Evently.Common.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Evently.Common.Application;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplication(this IServiceCollection services,
        Assembly[] assemblies)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(assemblies);

            config.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
        });
        
        services.AddValidatorsFromAssemblies(assemblies);
        
        return services;
    }    
}
