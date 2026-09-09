using MediatR;
using Vargshala.Contracts.Classes;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Classes.Queries.GetClassById;

public record GetClassByIdQuery(Guid Id) : IRequest<ApiResponse<ClassDto>>;
