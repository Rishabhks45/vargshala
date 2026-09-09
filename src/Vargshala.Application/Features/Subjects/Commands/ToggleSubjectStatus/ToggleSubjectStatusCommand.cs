using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Subjects.Commands.ToggleSubjectStatus;

public record ToggleSubjectStatusCommand(Guid Id) : IRequest<ApiResponse<bool>>;
