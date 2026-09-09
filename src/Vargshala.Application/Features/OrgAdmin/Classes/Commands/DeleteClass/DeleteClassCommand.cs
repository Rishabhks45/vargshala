using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Classes.Commands.DeleteClass;

public record DeleteClassCommand(Guid Id) : IRequest<ApiResponse<bool>>;
