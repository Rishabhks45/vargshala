using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.ClassSessions.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.ClassSessions.Commands.DeleteClassSession;

public record DeleteClassSessionCommand(Guid Id) : IRequest<ApiResponse<bool>>;

public class DeleteClassSessionCommandHandler : IRequestHandler<DeleteClassSessionCommand, ApiResponse<bool>>
{
    private readonly IClassSessionRepository _repo;
    private readonly ICurrentUser _currentUser;

    public DeleteClassSessionCommandHandler(
        IClassSessionRepository repo,
        ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteClassSessionCommand cmd, CancellationToken cancellationToken)
    {
        var session = await _repo.GetByIdForUpdateAsync(cmd.Id, cancellationToken);
        if (session == null)
        {
            return ApiResponse<bool>.FailureResponse("Class session not found.");
        }

        session.IsDeleted = true;
        session.DeletedBy = _currentUser.UserId;
        session.DeletedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(session, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Class session deleted successfully.");
    }
}
