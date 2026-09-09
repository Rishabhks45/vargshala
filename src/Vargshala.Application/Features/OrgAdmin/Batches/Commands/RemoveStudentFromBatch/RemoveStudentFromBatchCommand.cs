using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Commands.RemoveStudentFromBatch;

public record RemoveStudentFromBatchCommand(Guid BatchId, Guid StudentId) : IRequest<ApiResponse<bool>>;
