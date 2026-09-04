using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Vargshala.Contracts.Students;
using Vargshala.Web.Auth;
using Vargshala.Web.Components;
using Vargshala.Web.Services;



namespace Vargshala.Web;

public class WebUIStartup { }

public static class StartupExtensions
{
    #region Service Registration (AddServices)
    public static void AddServices(this WebApplicationBuilder builder)
    {
        #region Razor Components & MVC
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();
        #endregion

        #region FluentValidation
        builder.Services.AddValidatorsFromAssemblyContaining<StudentDtoValidator>();
        #endregion

        #region HTTP Clients & Token Interceptor
        builder.Services.AddTransient<JwtTokenHandler>();

        var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7288";

        // Anonymous client for auth endpoints (refresh, login)
        builder.Services.AddHttpClient("VargshalaApi.Anonymous", client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        });

        // Authenticated client with automatic token interception & 401 refresh
        builder.Services.AddHttpClient("VargshalaApi", client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        })
        .AddHttpMessageHandler<JwtTokenHandler>();
        #endregion

        #region Authentication & Cookie Security
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "Vargshala.AuthCookie";
                options.LoginPath = "/login";
                options.LogoutPath = "/account/logout";
                options.AccessDeniedPath = "/login";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
            });

        builder.Services.AddAuthorization();
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
        builder.Services.AddScoped<TokenValidator>();
        #endregion

        #region Application Scoped Services
        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IInstituteService, InstituteService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        #endregion
    }
    #endregion

    #region Middleware Pipeline (UseServices)
    public static void UseServices(this WebApplication app)
    {
        #region Error Handling & Security
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();
        #endregion

        #region Authentication & Antiforgery
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();
        #endregion

        #region Endpoints & Razor Components
        app.MapStaticAssets();
        app.MapControllers();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        #endregion
    }
    #endregion
}
