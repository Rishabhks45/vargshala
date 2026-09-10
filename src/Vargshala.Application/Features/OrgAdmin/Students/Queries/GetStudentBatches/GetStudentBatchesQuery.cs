using MediatR;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Students.Queries.GetStudentBatches;

public record GetStudentBatchesQuery(Guid StudentId) : IRequest<ApiResponse<List<StudentBatchEnrollmentDto>>>;
