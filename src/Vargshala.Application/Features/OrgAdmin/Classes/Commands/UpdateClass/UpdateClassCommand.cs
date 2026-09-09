using MediatR;
using Vargshala.Contracts.Classes;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Classes.Commands.UpdateClass;

public record UpdateClassCommand(UpdateClassRequest Request) : IRequest<ApiResponse<ClassDto>>;
