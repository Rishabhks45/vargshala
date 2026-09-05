using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Teachers.Helpers;

public class EmployeeCodeGenerator : IEmployeeCodeGenerator
{
    private readonly IVargshalaDbContext _db;
    private readonly ICurrentUser _currentUser;

    public EmployeeCodeGenerator(IVargshalaDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Generates the next unique employee code (format: EMP-YY00001, e.g. EMP-2600001) for the current year,
    /// strictly verified against the database on behalf of the tenant's OrganizationId.
    /// </summary>
    public async Task<string> GenerateNextCodeAsync(CancellationToken cancellationToken = default)
    {
        var targetOrgId = _currentUser.OrganizationId;
        var now = DateTime.UtcNow;
        var yearShort = (now.Year % 100).ToString("D2"); // e.g. "26" for 2026
        var prefix = $"EMP-{yearShort}";                  // e.g. "EMP-26"

        // Fetch all existing employee codes matching this year's prefix strictly for current organization
        var query = _db.Teachers
            .AsNoTracking()
            .Where(t => !t.IsDeleted && t.EmployeeCode != null && t.EmployeeCode.StartsWith(prefix));

        if (targetOrgId.HasValue && targetOrgId.Value != Guid.Empty)
        {
            query = query.Where(t => t.User != null && t.User.OrganizationId == targetOrgId.Value);
        }

        var existingCodes = await query
            .Select(t => t.EmployeeCode!)
            .ToListAsync(cancellationToken);

        int maxSeq = 0;

        foreach (var code in existingCodes)
        {
            var trimmed = code.Trim();
            var digitsOnly = new string(trimmed.Where(char.IsDigit).ToArray());

            // Check if digits start with 2-digit year (e.g. "26") and have at least 5 sequence digits (e.g. 2600001 -> length 7)
            if (digitsOnly.StartsWith(yearShort) && digitsOnly.Length >= 7)
            {
                var suffix = digitsOnly.Substring(yearShort.Length);
                if (int.TryParse(suffix, out int seq) && seq > maxSeq)
                {
                    maxSeq = seq;
                }
            }
        }

        int nextSeq = maxSeq + 1;
        var employeeCode = $"EMP-{yearShort}{nextSeq:D5}"; // e.g. "EMP-2600001"

        // Ensure collision-free uniqueness in DB strictly for this organization
        while (await _db.Teachers.AnyAsync(t => !t.IsDeleted &&
            (!targetOrgId.HasValue || (t.User != null && t.User.OrganizationId == targetOrgId.Value)) &&
            t.EmployeeCode == employeeCode, cancellationToken))
        {
            nextSeq++;
            employeeCode = $"EMP-{yearShort}{nextSeq:D5}";
        }

        return employeeCode;
    }

    public async Task<bool> IsCodeTakenAsync(string employeeCode, Guid? excludeTeacherId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(employeeCode))
        {
            return false;
        }

        var targetOrgId = _currentUser.OrganizationId;
        var normalized = employeeCode.Trim();
        var query = _db.Teachers.Where(t => t.EmployeeCode == normalized && !t.IsDeleted);

        if (targetOrgId.HasValue && targetOrgId.Value != Guid.Empty)
        {
            query = query.Where(t => t.User != null && t.User.OrganizationId == targetOrgId.Value);
        }

        if (excludeTeacherId.HasValue)
        {
            query = query.Where(t => t.Id != excludeTeacherId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
