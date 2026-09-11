using MediatR;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Queries.GetAllActiveBatches;

public record GetAllActiveBatchesQuery(Guid? ClassId = null, Guid? BranchId = null) : IRequest<ApiResponse<List<BatchDto>>>;
