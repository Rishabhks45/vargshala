using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.ClassSessions.Infrastructure;
using Vargshala.Contracts.ClassSessions;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.ClassSessions.Commands.CompleteClassSession;

public record CompleteClassSessionCommand(Guid Id, CompleteClassSessionRequest Request) : IRequest<ApiResponse<bool>>;

public class CompleteClassSessionCommandHandler : IRequestHandler<CompleteClassSessionCommand, ApiResponse<bool>>
{
    private readonly IClassSessionRepository _repo;
    private readonly ICurrentUser _currentUser;

    public CompleteClassSessionCommandHandler(
        IClassSessionRepository repo,
        ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(CompleteClassSessionCommand cmd, CancellationToken cancellationToken)
    {
        var session = await _repo.GetByIdForUpdateAsync(cmd.Id, cancellationToken);
        if (session == null)
        {
            return ApiResponse<bool>.FailureResponse("Class session not found.");
        }

        session.Status = ClassSessionStatusNames.Completed;
        if (!string.IsNullOrWhiteSpace(cmd.Request.Topic))
        {
            session.Topic = cmd.Request.Topic.Trim();
        }
        if (!string.IsNullOrWhiteSpace(cmd.Request.Notes))
        {
            session.Notes = cmd.Request.Notes.Trim();
        }

        session.UpdatedBy = _currentUser.UserId;
        session.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(session, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Class session marked as completed.");
    }
}
