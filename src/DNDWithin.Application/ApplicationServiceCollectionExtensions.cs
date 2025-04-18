﻿using DNDWithin.Application.Database;
using DNDWithin.Application.HostedServices;
using DNDWithin.Application.Repositories;
using DNDWithin.Application.Repositories.Implementation;
using DNDWithin.Application.Services;
using DNDWithin.Application.Services.Implementation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using EmailService = DNDWithin.Application.HostedServices.EmailService;

namespace DNDWithin.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        #region Repositories

        services.AddSingleton<IAccountRepository, AccountRepository>();
        services.AddSingleton<IGlobalSettingsRepository, GlobalSettingsRepository>();
        services.AddSingleton<IEmailRepository, EmailRepository>();
        services.AddSingleton<ICharacterRepository, CharacterRepository>();

        #endregion

        #region Services

        services.AddSingleton<IAccountService, AccountService>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IGlobalSettingsService, GlobalSettingsService>();
        services.AddSingleton<IEmailService, Services.Implementation.EmailService>();
        services.AddSingleton<ICharacterService, CharacterService>();

        #endregion

        #region Validators

        services.AddValidatorsFromAssemblyContaining<IApplicationMarker>(ServiceLifetime.Singleton); // set to singleton as it'll be one.

        #endregion

        #region Other

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        #endregion

        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDbConnectionFactory>(_ => new NpgsqlConnectionFactory(connectionString));
        services.AddSingleton<DbInitializer>();
        services.AddSingleton<IEmailService, Services.Implementation.EmailService>();

        services.AddHostedService<EmailService>();

        return services;
    }
}