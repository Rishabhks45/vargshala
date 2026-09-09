using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subjects;

namespace Vargshala.Application.Features.Subjects.Queries.GetAllActiveSubjects;

public record GetAllActiveSubjectsQuery : IRequest<ApiResponse<List<SubjectDto>>>;
