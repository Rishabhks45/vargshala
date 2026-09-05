using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Vargshala.Application.Behaviors;

namespace Vargshala.Application.DependencyInjection;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationServiceRegistration).Assembly;

        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // MediatR Pipeline Behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        // Helpers & Code Generators
        services.AddScoped<Vargshala.Application.Features.OrgAdmin.Students.Helpers.IStudentCodeGenerator, Vargshala.Application.Features.OrgAdmin.Students.Helpers.StudentCodeGenerator>();
        services.AddScoped<Vargshala.Application.Features.OrgAdmin.Teachers.Helpers.IEmployeeCodeGenerator, Vargshala.Application.Features.OrgAdmin.Teachers.Helpers.EmployeeCodeGenerator>();

        return services;
    }
}
