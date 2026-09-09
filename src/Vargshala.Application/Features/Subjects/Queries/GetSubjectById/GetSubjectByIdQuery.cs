using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subjects;

namespace Vargshala.Application.Features.Subjects.Queries.GetSubjectById;

public record GetSubjectByIdQuery(Guid Id) : IRequest<ApiResponse<SubjectDto>>;
