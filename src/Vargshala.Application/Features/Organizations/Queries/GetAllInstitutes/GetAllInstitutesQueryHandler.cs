using MediatR;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.Organizations.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Organizations;

namespace Vargshala.Application.Features.Organizations.Queries.GetAllInstitutes;

public class GetAllInstitutesQueryHandler : IRequestHandler<GetAllInstitutesQuery, ApiResponse<List<InstituteSummaryDto>>>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IVargshalaDbContext _db;

    public GetAllInstitutesQueryHandler(IOrganizationRepository organizationRepository, IVargshalaDbContext db)
    {
        _organizationRepository = organizationRepository;
        _db = db;
    }

    public async Task<ApiResponse<List<InstituteSummaryDto>>> Handle(
        GetAllInstitutesQuery request,
        CancellationToken cancellationToken)
    {
        var organizations = await _organizationRepository.GetAllAsync(cancellationToken);

        // Fetch users grouped by organization for accurate counts
        var orgUsers = await _db.Users
            .AsNoTracking()
            .Where(u => u.OrganizationId.HasValue && !u.IsDeleted)
            .Select(u => new { u.OrganizationId, u.Role, u.FirstName, u.LastName, u.Email })
            .ToListAsync(cancellationToken);

        var list = new List<InstituteSummaryDto>();

        foreach (var org in organizations)
        {
            var users = orgUsers.Where(u => u.OrganizationId == org.Id).ToList();
            var admin = users.FirstOrDefault(u => u.Role == UserRole.OrganizationAdmin);

            var studentCount = users.Count(u => u.Role == UserRole.Student);
            var teacherCount = users.Count(u => u.Role == UserRole.Teacher);

            // Assign plan based on code or size for display
            var plan = org.Code.Equals("VARGSHALA", StringComparison.OrdinalIgnoreCase) 
                ? "Enterprise" 
                : (studentCount > 100 ? "Pro Institute" : "Standard");

            list.Add(new InstituteSummaryDto
            {
                Id = org.Id,
                Name = org.Name,
                Code = org.Code,
                OwnerName = admin != null ? $"{admin.FirstName} {admin.LastName}".Trim() : "Administrator",
                Email = org.Email ?? admin?.Email,
                Mobile = org.Mobile,
                Address = org.Address,
                City = org.City ?? "Not Specified",
                State = org.State ?? "India",
                Pincode = org.Pincode,
                LogoUrl = org.LogoUrl,
                AcademicSession = org.AcademicSession,
                StudentCount = studentCount,
                TeacherCount = teacherCount,
                TotalUsersCount = users.Count,
                Plan = plan,
                IsActive = org.IsActive,
                CreatedAt = org.CreatedAt
            });
        }

        return ApiResponse<List<InstituteSummaryDto>>.SuccessResponse(list);
    }
}
