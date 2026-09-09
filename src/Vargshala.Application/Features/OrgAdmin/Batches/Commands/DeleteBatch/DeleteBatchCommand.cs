using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Commands.DeleteBatch;

public record DeleteBatchCommand(Guid Id) : IRequest<ApiResponse<bool>>;
