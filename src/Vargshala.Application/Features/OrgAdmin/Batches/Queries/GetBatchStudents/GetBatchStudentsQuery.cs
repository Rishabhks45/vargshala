using MediatR;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Queries.GetBatchStudents;

public record GetBatchStudentsQuery(Guid BatchId) : IRequest<ApiResponse<List<BatchStudentDto>>>;
