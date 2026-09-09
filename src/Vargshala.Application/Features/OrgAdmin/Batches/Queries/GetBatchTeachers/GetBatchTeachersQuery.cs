using MediatR;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Queries.GetBatchTeachers;

public record GetBatchTeachersQuery(Guid BatchId) : IRequest<ApiResponse<List<BatchTeacherDto>>>;
