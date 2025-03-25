using DNDWithin.Application.Database;
using DNDWithin.Application.Repositories;
using DNDWithin.Application.Repositories.Implementation;
using DNDWithin.Application.Services;
using DNDWithin.Application.Services.Implementation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DNDWithin.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IAccountRepository, AccountRepository>();
        services.AddSingleton<IAccountService, AccountService>();
        services.AddValidatorsFromAssemblyContaining<IApplicationMarker>(ServiceLifetime.Singleton); // set to singleton as it'll be one.
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDbConnectionFactory>(_ => new NpgsqlConnectionFactory(connectionString));
        services.AddSingleton<DbInitializer>();
        return services;
    }
}