using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Domain.Entities;
using Vargshala.Contracts.Common;

namespace Vargshala.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VargshalaDbContext>();
        var encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();
        var encryptionSettings = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Vargshala.Application.Settings.EncryptionSettings>>().Value;
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<VargshalaDbContext>>();

        try
        {
            // 1. Seed / Update SuperAdmin (Platform Level - No Organization)
            const string superAdminEmail = "rishabh.sharma@vargshala.com";
            var superAdmin = await context.Users
                .FirstOrDefaultAsync(u => u.Email == superAdminEmail && u.Role == UserRole.SuperAdmin);

            var encryptedPassword = encryptionService.Encrypt("Admin@123", encryptionSettings.MasterKey);

            if (superAdmin is null)
            {
                superAdmin = new User
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    OrganizationId = null,
                    FirstName = "Rishabh",
                    LastName = "Sharma",
                    Email = superAdminEmail,
                    Mobile = "+919876543210",
                    PasswordHash = encryptedPassword,
                    Role = UserRole.SuperAdmin,
                    EmailVerified = true,
                    MobileVerified = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(superAdmin);
                logger.LogInformation("Seeded SuperAdmin user: {Email}", superAdminEmail);
            }
            else
            {
                superAdmin.PasswordHash = encryptedPassword;
                superAdmin.IsActive = true;
            }

            // 2. Seed Default Organization
            const string defaultOrgCode = "VARGSHALA";
            var defaultOrg = await context.Organizations
                .FirstOrDefaultAsync(o => o.Code == defaultOrgCode);

            if (defaultOrg is null)
            {
                defaultOrg = new Organization
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Vargshala Institute",
                    Code = defaultOrgCode,
                    Email = "contact@vargshala.com",
                    Mobile = "+919876543210",
                    Address = "Connaught Place",
                    City = "New Delhi",
                    State = "Delhi",
                    Pincode = "110001",
                    AcademicSession = "2026-2027",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Organizations.Add(defaultOrg);
                logger.LogInformation("Seeded Default Organization: {OrgName} ({OrgCode})", defaultOrg.Name, defaultOrg.Code);
            }

            // 3. Seed / Update OrganizationAdmin for Default Organization
            const string orgAdminEmail = "rishabh.admin@vargshala.com";
            var orgAdmin = await context.Users
                .FirstOrDefaultAsync(u => u.Email == orgAdminEmail && u.OrganizationId == defaultOrg.Id);

            if (orgAdmin is null)
            {
                orgAdmin = new User
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    OrganizationId = defaultOrg.Id,
                    FirstName = "Rishabh",
                    LastName = "Sharma",
                    Email = orgAdminEmail,
                    Mobile = "+919876543210",
                    PasswordHash = encryptedPassword,
                    Role = UserRole.OrganizationAdmin,
                    EmailVerified = true,
                    MobileVerified = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(orgAdmin);
                logger.LogInformation("Seeded OrganizationAdmin user: {Email}", orgAdminEmail);
            }
            else
            {
                orgAdmin.PasswordHash = encryptedPassword;
                orgAdmin.IsActive = true;
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding initial data.");
            throw;
        }
    }
}
