using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Commands.RemoveTeacherFromBatch;

public record RemoveTeacherFromBatchCommand(Guid BatchId, Guid TeacherId) : IRequest<ApiResponse<bool>>;
