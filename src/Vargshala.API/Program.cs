using Scalar.AspNetCore;
using Serilog;
using Vargshala.API.Extensions;
using Vargshala.API.Middleware;
using Vargshala.Application.DependencyInjection;
using Vargshala.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerServices();

// Application & Infrastructure DI
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? new[] { "https://localhost:7001" })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Vargshala API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Vargshala API Documentation";
        options.DisplayRequestDuration();
        options.EnablePersistAuthorization();
    });

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Vargshala API Reference")
               .WithTheme(ScalarTheme.DeepSpace)
               .WithOpenApiRoutePattern("/swagger/v1/swagger.json");
    });

    // Auto-redirect root "/" to "/swagger"
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowBlazor");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
