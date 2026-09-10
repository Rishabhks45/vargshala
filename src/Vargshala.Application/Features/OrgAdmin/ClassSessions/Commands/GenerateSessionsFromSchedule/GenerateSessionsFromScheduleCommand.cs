using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.OrgAdmin.ClassSessions.Infrastructure;
using Vargshala.Contracts.ClassSessions;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using DayOfWeek = Vargshala.Contracts.Common.DayOfWeek;

namespace Vargshala.Application.Features.OrgAdmin.ClassSessions.Commands.GenerateSessionsFromSchedule;

public record GenerateSessionsFromScheduleCommand(GenerateSessionsFromScheduleRequest Request) : IRequest<ApiResponse<int>>;

public class GenerateSessionsFromScheduleCommandHandler : IRequestHandler<GenerateSessionsFromScheduleCommand, ApiResponse<int>>
{
    private readonly IClassSessionRepository _repo;
    private readonly IVargshalaDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GenerateSessionsFromScheduleCommandHandler(
        IClassSessionRepository repo,
        IVargshalaDbContext db,
        ICurrentUser currentUser)
    {
        _repo = repo;
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<int>> Handle(GenerateSessionsFromScheduleCommand cmd, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<int>.FailureResponse("No active organization context found.");
        }

        var req = cmd.Request;
        if (req.StartDate > req.EndDate)
        {
            return ApiResponse<int>.FailureResponse("Start date cannot be after End date.");
        }

        var batch = await _db.Batches
            .Include(b => b.Class)
                .ThenInclude(c => c!.Branch)
            .Include(b => b.BatchTeachers)
                .ThenInclude(bt => bt.Teacher)
            .FirstOrDefaultAsync(b => b.Id == req.BatchId && !b.IsDeleted, cancellationToken);

        if (batch == null || batch.Class?.Branch?.OrganizationId != orgId.Value)
        {
            return ApiResponse<int>.FailureResponse("Batch not found or unauthorized.");
        }

        var schedules = await _repo.GetSchedulesForBatchAsync(req.BatchId, cancellationToken);
        if (schedules.Count == 0)
        {
            // If no recurring weekly schedules yet, check if batch has start/end time fallback
            if (batch.StartTime.HasValue && batch.EndTime.HasValue)
            {
                // Create a default weekday schedule for each Monday-Friday
                schedules = new List<BatchSchedule>
                {
                    new() { BatchId = batch.Id, DayOfWeek = DayOfWeek.Monday, StartTime = batch.StartTime.Value, EndTime = batch.EndTime.Value },
                    new() { BatchId = batch.Id, DayOfWeek = DayOfWeek.Tuesday, StartTime = batch.StartTime.Value, EndTime = batch.EndTime.Value },
                    new() { BatchId = batch.Id, DayOfWeek = DayOfWeek.Wednesday, StartTime = batch.StartTime.Value, EndTime = batch.EndTime.Value },
                    new() { BatchId = batch.Id, DayOfWeek = DayOfWeek.Thursday, StartTime = batch.StartTime.Value, EndTime = batch.EndTime.Value },
                    new() { BatchId = batch.Id, DayOfWeek = DayOfWeek.Friday, StartTime = batch.StartTime.Value, EndTime = batch.EndTime.Value }
                };
            }
            else
            {
                return ApiResponse<int>.FailureResponse("No recurring timetable schedules defined for this batch.");
            }
        }

        // Determine default teacher
        Guid? defaultTeacherId = req.TeacherId;
        if (!defaultTeacherId.HasValue || defaultTeacherId.Value == Guid.Empty)
        {
            defaultTeacherId = batch.BatchTeachers.FirstOrDefault(bt => bt.IsActive)?.TeacherId;
        }

        var createdSessions = new List<ClassSession>();
        var currentDate = req.StartDate;

        while (currentDate <= req.EndDate)
        {
            // Map DayOfWeek (Monday=1, Sunday=7)
            int isoDay = (int)currentDate.DayOfWeek;
            if (isoDay == 0) isoDay = 7; // Sunday

            var matchingSchedules = schedules.Where(s => (int)s.DayOfWeek == isoDay).ToList();

            foreach (var sched in matchingSchedules)
            {
                var exists = await _repo.ExistsAsync(req.BatchId, sched.Id == Guid.Empty ? null : sched.Id, currentDate, cancellationToken);
                if (!exists)
                {
                    createdSessions.Add(new ClassSession
                    {
                        BatchId = req.BatchId,
                        BatchScheduleId = sched.Id == Guid.Empty ? null : sched.Id,
                        TeacherId = defaultTeacherId,
                        SessionDate = currentDate,
                        StartTime = sched.StartTime,
                        EndTime = sched.EndTime,
                        Topic = $"{batch.Name} Class",
                        Status = ClassSessionStatusNames.Scheduled,
                        CreatedBy = _currentUser.UserId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            currentDate = currentDate.AddDays(1);
        }

        if (createdSessions.Count > 0)
        {
            await _repo.AddRangeAsync(createdSessions, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse<int>.SuccessResponse(createdSessions.Count, $"Successfully generated {createdSessions.Count} class session(s).");
    }
}
