using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Subjects.Commands.DeleteSubject;

public record DeleteSubjectCommand(Guid Id) : IRequest<ApiResponse<bool>>;
