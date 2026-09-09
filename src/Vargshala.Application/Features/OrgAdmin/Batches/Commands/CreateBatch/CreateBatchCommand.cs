using MediatR;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Commands.CreateBatch;

public record CreateBatchCommand(CreateBatchRequest Request) : IRequest<ApiResponse<BatchDto>>;
