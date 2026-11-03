using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StockSync.Application.Features.Auth;

namespace StockSync.Application;

public static class ApplicationServiceRegistration
{
    private const string HandlerTypeName = "Handler";

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var handlers = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsNested: true, Name: HandlerTypeName });

        foreach (var handler in handlers)
        {
            services.AddScoped(handler);
        }

        services.AddScoped<AuthSessionFactory>();
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        return services;
    }
}
