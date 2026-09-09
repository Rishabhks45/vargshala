using MediatR;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Queries.GetBatchById;

public record GetBatchByIdQuery(Guid Id) : IRequest<ApiResponse<BatchDetailDto>>;
