using MediatR;
using Vargshala.Application.Features.OrgAdmin.Batches.Infrastructure;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Queries.GetBatchStudents;

public class GetBatchStudentsQueryHandler : IRequestHandler<GetBatchStudentsQuery, ApiResponse<List<BatchStudentDto>>>
{
    private readonly IBatchRepository _batchRepository;

    public GetBatchStudentsQueryHandler(IBatchRepository batchRepository)
    {
        _batchRepository = batchRepository;
    }

    public async Task<ApiResponse<List<BatchStudentDto>>> Handle(GetBatchStudentsQuery query, CancellationToken cancellationToken)
    {
        var students = await _batchRepository.GetStudentsByBatchIdAsync(query.BatchId, cancellationToken);
        var dtos = students.Select(s => s.ToDto()).ToList();
        return ApiResponse<List<BatchStudentDto>>.SuccessResponse(dtos);
    }
}
