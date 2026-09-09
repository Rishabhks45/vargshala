using MediatR;
using Vargshala.Application.Features.OrgAdmin.Classes.Infrastructure;
using Vargshala.Contracts.Classes;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Classes.Queries.GetClassById;

public class GetClassByIdQueryHandler : IRequestHandler<GetClassByIdQuery, ApiResponse<ClassDto>>
{
    private readonly IClassRepository _classRepository;

    public GetClassByIdQueryHandler(IClassRepository classRepository)
    {
        _classRepository = classRepository;
    }

    public async Task<ApiResponse<ClassDto>> Handle(GetClassByIdQuery query, CancellationToken cancellationToken)
    {
        var entity = await _classRepository.GetByIdAsync(query.Id, cancellationToken);
        if (entity == null)
        {
            return ApiResponse<ClassDto>.FailureResponse("Class not found.");
        }

        return ApiResponse<ClassDto>.SuccessResponse(entity.ToDto());
    }
}
