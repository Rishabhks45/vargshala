using MediatR;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Commands.UpdateBatch;

public record UpdateBatchCommand(UpdateBatchRequest Request) : IRequest<ApiResponse<BatchDto>>;
