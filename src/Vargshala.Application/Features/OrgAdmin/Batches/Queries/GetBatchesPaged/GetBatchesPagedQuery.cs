using MediatR;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Queries.GetBatchesPaged;

public record GetBatchesPagedQuery(
    PagedRequest? Request = null,
    Guid? BranchId = null,
    Guid? ClassId = null,
    Guid? SubjectId = null,
    bool? IsActive = null) : IRequest<ApiResponse<PagedResponse<BatchDto>>>;
