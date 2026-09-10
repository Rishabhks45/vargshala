using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Batches.Infrastructure;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Students.Queries.GetStudentBatches;

public class GetStudentBatchesQueryHandler : IRequestHandler<GetStudentBatchesQuery, ApiResponse<List<StudentBatchEnrollmentDto>>>
{
    private readonly IBatchRepository _batchRepository;
    private readonly ICurrentUser _currentUser;

    public GetStudentBatchesQueryHandler(
        IBatchRepository batchRepository,
        ICurrentUser currentUser)
    {
        _batchRepository = batchRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<List<StudentBatchEnrollmentDto>>> Handle(
        GetStudentBatchesQuery query,
        CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<List<StudentBatchEnrollmentDto>>.FailureResponse("No active organization context found.");
        }

        var batchStudents = await _batchRepository.GetBatchesByStudentIdAsync(query.StudentId, cancellationToken);

        var dtos = batchStudents
            .Where(bs => bs.Batch?.Class?.Branch?.OrganizationId == orgId.Value)
            .Select(bs => new StudentBatchEnrollmentDto
            {
                Id = bs.Id,
                BatchId = bs.BatchId,
                BatchName = bs.Batch?.Name ?? string.Empty,
                BatchCode = bs.Batch?.Code ?? string.Empty,
                ClassId = bs.Batch?.ClassId ?? Guid.Empty,
                ClassName = bs.Batch?.Class?.Name ?? string.Empty,
                SubjectId = bs.Batch?.SubjectId ?? Guid.Empty,
                SubjectName = bs.Batch?.Subject?.Name ?? string.Empty,
                BranchId = bs.Batch?.Class?.BranchId ?? Guid.Empty,
                BranchName = bs.Batch?.Class?.Branch?.Name ?? string.Empty,
                IsPrimary = bs.IsPrimary,
                EnrollmentType = bs.EnrollmentType,
                JoinedAt = bs.JoinedAt,
                LeftAt = bs.LeftAt,
                IsActive = bs.IsActive
            })
            .ToList();

        return ApiResponse<List<StudentBatchEnrollmentDto>>.SuccessResponse(dtos);
    }
}
