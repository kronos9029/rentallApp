using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RentalApp.Application.Features.Availability;
using RentalApp.Application.Features.Auth;
using RentalApp.Application.Features.Booking;
using RentalApp.Application.Features.Pricing;
using RentalApp.Infrastructure.Communication.Email;
using RentalApp.Domain.Entities;
using RentalApp.Infrastructure.Booking;
using RentalApp.Infrastructure.Persistence;
using RentalApp.Infrastructure.Persistence.Configuration;
using RentalApp.Infrastructure.Persistence.Repositories;
using RentalApp.Infrastructure.Security.Authentication;

namespace RentalApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var databaseOptions = configuration.GetSection(MySqlDatabaseOptions.SectionName).Get<MySqlDatabaseOptions>()
            ?? throw new InvalidOperationException("Database:MySql configuration was not configured.");
        var connectionString = MySqlConnectionStringFactory.Build(databaseOptions);
        var serverVersion = ServerVersion.AutoDetect(connectionString);

        services.AddDbContext<RentalAppDbContext>(options =>
        {
            options.UseMySql(
                connectionString,
                serverVersion,
                mysql => mysql.MigrationsAssembly(typeof(RentalAppDbContext).Assembly.FullName));

            if (environment.IsDevelopment())
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddMemoryCache();
        services.Configure<MySqlDatabaseOptions>(configuration.GetSection(MySqlDatabaseOptions.SectionName));
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<BookingInventoryProvisioner>();
        services.AddScoped<DeadlockRetryExecutor>();
        services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
        services.AddScoped<IUserAuthService, DbUserAuthService>();
        services.AddScoped<IPasswordResetNotificationService, SmtpPasswordResetNotificationService>();
        services.AddScoped<IAvailabilityService, AvailabilityReadService>();
        services.AddScoped<IHoldLifecycleService, HoldLifecycleService>();
        services.AddScoped<IHoldService, HoldService>();
        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<IPriceCalculationService, PricingRuleReadService>();
        return services;
    }
}
