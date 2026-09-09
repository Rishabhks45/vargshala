using MediatR;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Commands.EnrollStudentToBatch;

public record EnrollStudentToBatchCommand(Guid BatchId, EnrollStudentToBatchRequest Request) : IRequest<ApiResponse<BatchStudentDto>>;
