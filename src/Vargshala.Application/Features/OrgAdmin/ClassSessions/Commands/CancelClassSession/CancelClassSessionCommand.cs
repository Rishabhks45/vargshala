using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.ClassSessions.Infrastructure;
using Vargshala.Contracts.ClassSessions;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.ClassSessions.Commands.CancelClassSession;

public record CancelClassSessionCommand(Guid Id, CancelClassSessionRequest Request) : IRequest<ApiResponse<bool>>;

public class CancelClassSessionCommandHandler : IRequestHandler<CancelClassSessionCommand, ApiResponse<bool>>
{
    private readonly IClassSessionRepository _repo;
    private readonly ICurrentUser _currentUser;

    public CancelClassSessionCommandHandler(
        IClassSessionRepository repo,
        ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(CancelClassSessionCommand cmd, CancellationToken cancellationToken)
    {
        var session = await _repo.GetByIdForUpdateAsync(cmd.Id, cancellationToken);
        if (session == null)
        {
            return ApiResponse<bool>.FailureResponse("Class session not found.");
        }

        session.Status = ClassSessionStatusNames.Cancelled;
        if (!string.IsNullOrWhiteSpace(cmd.Request.Reason))
        {
            session.Notes = string.IsNullOrWhiteSpace(session.Notes)
                ? $"Cancellation Reason: {cmd.Request.Reason.Trim()}"
                : $"{session.Notes}\nCancellation Reason: {cmd.Request.Reason.Trim()}";
        }

        session.UpdatedBy = _currentUser.UserId;
        session.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(session, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Class session cancelled successfully.");
    }
}
