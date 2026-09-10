using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.OrgAdmin.ClassSessions.Infrastructure;
using Vargshala.Contracts.ClassSessions;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Vargshala.Application.Features.OrgAdmin.ClassSessions.Commands.CreateClassSession;

public record CreateClassSessionCommand(CreateClassSessionRequest Request) : IRequest<ApiResponse<ClassSessionDto>>;

public class CreateClassSessionCommandHandler : IRequestHandler<CreateClassSessionCommand, ApiResponse<ClassSessionDto>>
{
    private readonly IClassSessionRepository _repo;
    private readonly IVargshalaDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateClassSessionCommandHandler(
        IClassSessionRepository repo,
        IVargshalaDbContext db,
        ICurrentUser currentUser)
    {
        _repo = repo;
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<ClassSessionDto>> Handle(CreateClassSessionCommand cmd, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<ClassSessionDto>.FailureResponse("No active organization context found.");
        }

        var req = cmd.Request;

        var batch = await _db.Batches
            .Include(b => b.Class)
                .ThenInclude(c => c!.Branch)
            .FirstOrDefaultAsync(b => b.Id == req.BatchId && !b.IsDeleted, cancellationToken);

        if (batch == null || batch.Class?.Branch?.OrganizationId != orgId.Value)
        {
            return ApiResponse<ClassSessionDto>.FailureResponse("Batch not found or unauthorized.");
        }

        if (req.TeacherId.HasValue && req.TeacherId.Value != Guid.Empty)
        {
            var teacherExists = await _db.Teachers
                .AnyAsync(t => t.Id == req.TeacherId.Value && !t.IsDeleted, cancellationToken);
            if (!teacherExists)
            {
                return ApiResponse<ClassSessionDto>.FailureResponse("Assigned teacher not found.");
            }
        }

        var session = new ClassSession
        {
            BatchId = req.BatchId,
            BatchScheduleId = req.BatchScheduleId,
            TeacherId = req.TeacherId == Guid.Empty ? null : req.TeacherId,
            SessionDate = req.SessionDate,
            StartTime = req.StartTime,
            EndTime = req.EndTime,
            Topic = req.Topic?.Trim(),
            Notes = req.Notes?.Trim(),
            Status = ClassSessionStatusNames.Scheduled,
            CreatedBy = _currentUser.UserId,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(session, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        var created = await _repo.GetByIdAsync(session.Id, cancellationToken);
        return ApiResponse<ClassSessionDto>.SuccessResponse(created!.ToDto(), "Class session created successfully.");
    }
}
