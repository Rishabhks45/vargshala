using MediatR;
using Vargshala.Application.Features.OrgAdmin.ClassSessions.Infrastructure;
using Vargshala.Contracts.ClassSessions;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.ClassSessions.Queries.GetClassSessionById;

public record GetClassSessionByIdQuery(Guid Id) : IRequest<ApiResponse<ClassSessionDto>>;

public class GetClassSessionByIdQueryHandler : IRequestHandler<GetClassSessionByIdQuery, ApiResponse<ClassSessionDto>>
{
    private readonly IClassSessionRepository _repo;

    public GetClassSessionByIdQueryHandler(IClassSessionRepository repo)
    {
        _repo = repo;
    }

    public async Task<ApiResponse<ClassSessionDto>> Handle(GetClassSessionByIdQuery query, CancellationToken cancellationToken)
    {
        var session = await _repo.GetByIdAsync(query.Id, cancellationToken);
        if (session == null)
        {
            return ApiResponse<ClassSessionDto>.FailureResponse("Class session not found.");
        }

        return ApiResponse<ClassSessionDto>.SuccessResponse(session.ToDto());
    }
}
