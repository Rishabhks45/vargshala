using MediatR;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Commands.AssignTeacherToBatch;

public record AssignTeacherToBatchCommand(Guid BatchId, AssignTeacherToBatchRequest Request) : IRequest<ApiResponse<BatchTeacherDto>>;
