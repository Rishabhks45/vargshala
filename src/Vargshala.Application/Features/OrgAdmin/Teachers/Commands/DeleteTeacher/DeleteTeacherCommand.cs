using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Teachers.Commands.DeleteTeacher;

public record DeleteTeacherCommand(Guid Id) : IRequest<ApiResponse<bool>>;
