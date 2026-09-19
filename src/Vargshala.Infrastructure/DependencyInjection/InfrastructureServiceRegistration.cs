using Vargshala.Application.Features.OrgAdmin.Classes.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.Fees.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.Batches.Infrastructure;
using Vargshala.Application.Features.Subjects.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Email;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Abstractions.Storage;
using Vargshala.Application.Settings;
using Vargshala.Infrastructure.Authentication;
using Vargshala.Infrastructure.Persistence;
using Vargshala.Infrastructure.Services;
using Vargshala.Infrastructure.Settings;
using Vargshala.Application.Features.Organizations.Infrastructure;
using Vargshala.Application.Features.Users.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.Students.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.Teachers.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.Branches.Infrastructure;
using Vargshala.Application.Features.Authentication.Infrastructure;
using Vargshala.Application.Features.Coupons.Infrastructure;
using Vargshala.Application.Features.Emails.Infrastructure;
using Vargshala.Application.Features.Messages.Infrastructure;
using Vargshala.Infrastructure.Persistence.Repositories;

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
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(VargshalaDbContext).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                }));

        services.AddScoped<IVargshalaDbContext>(provider =>
            provider.GetRequiredService<VargshalaDbContext>());

        // JWT Options
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // Encryption Settings
        services.Configure<EncryptionSettings>(configuration.GetSection(EncryptionSettings.SectionName));

        // Supabase Storage
        services.Configure<SupabaseStorageOptions>(configuration.GetSection(SupabaseStorageOptions.SectionName));
        services.AddHttpClient<IStorageService, SupabaseStorageService>();

        // Authentication, Security & Encryption services
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<Vargshala.Application.Abstractions.Security.IBranchAuthorizationService, Vargshala.Infrastructure.Services.Security.BranchAuthorizationService>();

        // Repositories
        services.AddScoped<IOrganizationRepository,OrganizationRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IStudentRepository,StudentRepository>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IClassRepository, ClassRepository>();
        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<Vargshala.Application.Features.OrgAdmin.ClassSessions.Infrastructure.IClassSessionRepository, ClassSessionRepository>();
        services.AddScoped<Vargshala.Application.Features.OrgAdmin.Attendances.Infrastructure.IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<Vargshala.Application.Features.BranchAdmin.Dashboard.Infrastructure.IBranchDashboardRepository, BranchDashboardRepository>();

        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<IFeeStructureRepository, FeeStructureRepository>();
        services.AddScoped<IFeeRepository, FeeRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<Vargshala.Application.Features.Messages.Security.IConversationAuthorizationService, Vargshala.Infrastructure.Services.Messages.ConversationAuthorizationService>();

        // QuestPDF PDF Receipt Generator
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        services.AddScoped<Vargshala.Application.Abstractions.Pdf.IFeeReceiptPdfGenerator, Vargshala.Infrastructure.Services.Receipts.QuestPdfFeeReceiptGenerator>();

        // HttpContextAccessor (needed by CurrentUser)
        services.AddHttpContextAccessor();

        // SendGrid Email Settings & HTTP Client
        services.Configure<SendGridOptions>(configuration.GetSection(SendGridOptions.SectionName));
        services.AddHttpClient<IEmailService, SendGridEmailService>(client =>
        {
            client.BaseAddress = new Uri("https://api.sendgrid.com/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        return services;
    }
}
