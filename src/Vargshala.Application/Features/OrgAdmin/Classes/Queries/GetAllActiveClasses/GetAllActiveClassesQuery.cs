using MediatR;
using Vargshala.Contracts.Classes;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Classes.Queries.GetAllActiveClasses;

public record GetAllActiveClassesQuery(Guid? BranchId = null) : IRequest<ApiResponse<List<ClassLookupDto>>>;
