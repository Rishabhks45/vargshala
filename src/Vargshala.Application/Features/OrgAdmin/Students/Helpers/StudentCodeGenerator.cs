using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Students.Helpers;

public class StudentCodeGenerator : IStudentCodeGenerator
{
    private readonly IVargshalaDbContext _db;
    private readonly ICurrentUser _currentUser;

    public StudentCodeGenerator(IVargshalaDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Generates the next unique student code (format: STU-YY00001, e.g. STU-2600001)
    /// and roll number (format: YY00001, e.g. 2600001) for the current year,
    /// strictly verified against the database on behalf of the tenant's OrganizationId.
    /// </summary>
    public async Task<StudentCodeAndRollResult> GenerateNextCodeAndRollAsync(CancellationToken cancellationToken = default)
    {
        var targetOrgId = _currentUser.OrganizationId;
        var now = DateTime.UtcNow;
        var yearShort = (now.Year % 100).ToString("D2"); // e.g. "26" for 2026
        var prefix = $"STU-{yearShort}";                  // e.g. "STU-26"

        // Fetch all existing student codes matching this year's prefix strictly for the current organization
        var query = _db.Students
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.StudentCode != null && s.StudentCode.StartsWith(prefix));

        if (targetOrgId.HasValue && targetOrgId.Value != Guid.Empty)
        {
            query = query.Where(s => s.User != null && s.User.OrganizationId == targetOrgId.Value);
        }

        var existingCodes = await query
            .Select(s => s.StudentCode!)
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
        var rollNumber = $"{yearShort}{nextSeq:D5}";       // e.g. "2600001"
        var studentCode = $"STU-{rollNumber}";             // e.g. "STU-2600001"

        // Ensure collision-free uniqueness in DB strictly for this organization
        while (await _db.Students.AnyAsync(s => !s.IsDeleted &&
            (!targetOrgId.HasValue || (s.User != null && s.User.OrganizationId == targetOrgId.Value)) &&
            (s.StudentCode == studentCode || s.RollNumber == rollNumber), cancellationToken))
        {
            nextSeq++;
            rollNumber = $"{yearShort}{nextSeq:D5}";
            studentCode = $"STU-{rollNumber}";
        }

        return new StudentCodeAndRollResult(studentCode, rollNumber, nextSeq);
    }

    public async Task<string> GenerateNextCodeAsync(CancellationToken cancellationToken = default)
    {
        var result = await GenerateNextCodeAndRollAsync(cancellationToken);
        return result.StudentCode;
    }

    public async Task<bool> IsCodeTakenAsync(string studentCode, Guid? excludeStudentId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentCode))
        {
            return false;
        }

        var targetOrgId = _currentUser.OrganizationId;
        var normalized = studentCode.Trim();
        var query = _db.Students.Where(s => s.StudentCode == normalized && !s.IsDeleted);

        if (targetOrgId.HasValue && targetOrgId.Value != Guid.Empty)
        {
            query = query.Where(s => s.User != null && s.User.OrganizationId == targetOrgId.Value);
        }

        if (excludeStudentId.HasValue)
        {
            query = query.Where(s => s.Id != excludeStudentId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
