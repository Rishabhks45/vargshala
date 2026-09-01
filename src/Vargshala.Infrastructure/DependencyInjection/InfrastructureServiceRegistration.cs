using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Infrastructure.Authentication;
using Vargshala.Infrastructure.Persistence;

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

        // Authentication services
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        // HttpContextAccessor (needed by CurrentUser)
        services.AddHttpContextAccessor();

        return services;
    }
}
