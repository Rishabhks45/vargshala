using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Students.Commands.DeleteStudent;

public record DeleteStudentCommand(Guid Id) : IRequest<ApiResponse<bool>>;
