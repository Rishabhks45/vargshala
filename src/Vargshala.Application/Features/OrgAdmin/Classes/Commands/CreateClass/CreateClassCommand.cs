using MediatR;
using Vargshala.Contracts.Classes;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Classes.Commands.CreateClass;

public record CreateClassCommand(CreateClassRequest Request) : IRequest<ApiResponse<ClassDto>>;
