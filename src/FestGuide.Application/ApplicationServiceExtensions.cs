using FluentValidation;
using FestGuide.Application.Services;
using FestGuide.Application.Validators;
using FestGuide.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FestGuide.Application;

/// <summary>
/// Extension methods for registering application services.
/// </summary>
public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Adds application services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserService, UserService>();

        // Date/Time provider
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        // Validators
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        return services;
    }
}
