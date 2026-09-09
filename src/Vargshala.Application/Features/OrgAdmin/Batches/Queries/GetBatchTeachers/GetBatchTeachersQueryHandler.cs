using MediatR;
using Vargshala.Application.Features.OrgAdmin.Batches.Infrastructure;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Queries.GetBatchTeachers;

public class GetBatchTeachersQueryHandler : IRequestHandler<GetBatchTeachersQuery, ApiResponse<List<BatchTeacherDto>>>
{
    private readonly IBatchRepository _batchRepository;

    public GetBatchTeachersQueryHandler(IBatchRepository batchRepository)
    {
        _batchRepository = batchRepository;
    }

    public async Task<ApiResponse<List<BatchTeacherDto>>> Handle(GetBatchTeachersQuery query, CancellationToken cancellationToken)
    {
        var teachers = await _batchRepository.GetTeachersByBatchIdAsync(query.BatchId, cancellationToken);
        var dtos = teachers.Select(t => t.ToDto()).ToList();
        return ApiResponse<List<BatchTeacherDto>>.SuccessResponse(dtos);
    }
}
