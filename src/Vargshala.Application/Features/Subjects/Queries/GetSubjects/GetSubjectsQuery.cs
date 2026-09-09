using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subjects;

namespace Vargshala.Application.Features.Subjects.Queries.GetSubjects;

public record GetSubjectsQuery(PagedRequest? Request = null, bool? IsActive = null)
    : IRequest<ApiResponse<PagedResponse<SubjectDto>>>;
