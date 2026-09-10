using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.ClassSessions.Infrastructure;
using Vargshala.Contracts.ClassSessions;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.ClassSessions.Commands.UpdateClassSession;

public record UpdateClassSessionCommand(Guid Id, UpdateClassSessionRequest Request) : IRequest<ApiResponse<ClassSessionDto>>;

public class UpdateClassSessionCommandHandler : IRequestHandler<UpdateClassSessionCommand, ApiResponse<ClassSessionDto>>
{
    private readonly IClassSessionRepository _repo;
    private readonly ICurrentUser _currentUser;

    public UpdateClassSessionCommandHandler(
        IClassSessionRepository repo,
        ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<ClassSessionDto>> Handle(UpdateClassSessionCommand cmd, CancellationToken cancellationToken)
    {
        var session = await _repo.GetByIdForUpdateAsync(cmd.Id, cancellationToken);
        if (session == null)
        {
            return ApiResponse<ClassSessionDto>.FailureResponse("Class session not found.");
        }

        var req = cmd.Request;
        session.TeacherId = req.TeacherId == Guid.Empty ? null : req.TeacherId;
        session.SessionDate = req.SessionDate;
        session.StartTime = req.StartTime;
        session.EndTime = req.EndTime;
        session.Topic = req.Topic?.Trim();
        session.Notes = req.Notes?.Trim();
        session.Status = string.IsNullOrWhiteSpace(req.Status) ? ClassSessionStatusNames.Scheduled : req.Status;
        session.UpdatedBy = _currentUser.UserId;
        session.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(session, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        var updated = await _repo.GetByIdAsync(session.Id, cancellationToken);
        return ApiResponse<ClassSessionDto>.SuccessResponse(updated!.ToDto(), "Class session updated successfully.");
    }
}
