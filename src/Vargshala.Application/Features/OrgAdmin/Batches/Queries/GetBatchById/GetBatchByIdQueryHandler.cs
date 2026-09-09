using MediatR;
using Vargshala.Application.Features.OrgAdmin.Batches.Infrastructure;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Queries.GetBatchById;

public class GetBatchByIdQueryHandler : IRequestHandler<GetBatchByIdQuery, ApiResponse<BatchDetailDto>>
{
    private readonly IBatchRepository _batchRepository;

    public GetBatchByIdQueryHandler(IBatchRepository batchRepository)
    {
        _batchRepository = batchRepository;
    }

    public async Task<ApiResponse<BatchDetailDto>> Handle(GetBatchByIdQuery query, CancellationToken cancellationToken)
    {
        var batch = await _batchRepository.GetDetailByIdAsync(query.Id, cancellationToken);
        if (batch == null)
        {
            return ApiResponse<BatchDetailDto>.FailureResponse("Batch not found.");
        }

        var detail = new BatchDetailDto
        {
            Batch = batch.ToDto(),
            Teachers = batch.BatchTeachers?.Where(bt => bt.IsActive).Select(bt => bt.ToDto()).ToList() ?? new(),
            Students = batch.BatchStudents?.Where(bs => bs.IsActive).Select(bs => bs.ToDto()).ToList() ?? new()
        };

        return ApiResponse<BatchDetailDto>.SuccessResponse(detail);
    }
}
