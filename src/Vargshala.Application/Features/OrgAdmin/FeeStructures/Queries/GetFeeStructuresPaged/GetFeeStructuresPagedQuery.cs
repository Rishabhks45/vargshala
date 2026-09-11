using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.FeeStructures;

namespace Vargshala.Application.Features.OrgAdmin.FeeStructures.Queries.GetFeeStructuresPaged;

public record GetFeeStructuresPagedQuery(
    PagedRequest Request,
    Guid? BranchId = null,
    Guid? ClassId = null,
    string? Session = null,
    bool? IsActive = null) : IRequest<ApiResponse<PagedResponse<FeeStructureDto>>>;
