using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using FestGuide.DataAccess.Abstractions;
using FestGuide.DataAccess.Repositories;

namespace FestGuide.DataAccess;

/// <summary>
/// Extension methods for registering data access services.
/// </summary>
public static class DataAccessServiceExtensions
{
    /// <summary>
    /// Adds data access services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddDataAccessServices(this IServiceCollection services, string connectionString)
    {
        // Register IDbConnection factory
        services.AddScoped<IDbConnection>(_ => new SqlConnection(connectionString));

        // Register repositories
        services.AddScoped<IUserRepository, SqlServerUserRepository>();
        services.AddScoped<IRefreshTokenRepository, SqlServerRefreshTokenRepository>();
        services.AddScoped<IEmailVerificationTokenRepository, SqlServerEmailVerificationTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, SqlServerPasswordResetTokenRepository>();

        return services;
    }
}
