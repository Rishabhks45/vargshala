using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Settings;
using Vargshala.Infrastructure.Authentication;
using Vargshala.Infrastructure.Persistence;
using Vargshala.Infrastructure.Services;

namespace Vargshala.Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<VargshalaDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(
                    typeof(VargshalaDbContext).Assembly.FullName)));

        services.AddScoped<IVargshalaDbContext>(provider =>
            provider.GetRequiredService<VargshalaDbContext>());

        // JWT Options
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // Encryption Settings
        services.Configure<EncryptionSettings>(configuration.GetSection(EncryptionSettings.SectionName));

        // Authentication & Encryption services
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        // Repositories
        services.AddScoped<Vargshala.Application.Features.Organizations.Infrastructure.IOrganizationRepository, Vargshala.Infrastructure.Persistence.Repositories.OrganizationRepository>();
        services.AddScoped<Vargshala.Application.Features.Users.Infrastructure.IUserRepository, Vargshala.Infrastructure.Persistence.Repositories.UserRepository>();
        services.AddScoped<Vargshala.Application.Features.Authentication.Infrastructure.IAuthRepository, Vargshala.Infrastructure.Persistence.Repositories.AuthRepository>();

        // HttpContextAccessor (needed by CurrentUser)
        services.AddHttpContextAccessor();

        return services;
    }
}
