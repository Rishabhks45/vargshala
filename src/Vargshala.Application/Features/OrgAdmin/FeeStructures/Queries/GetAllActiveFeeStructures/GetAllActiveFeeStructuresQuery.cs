using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.FeeStructures;

namespace Vargshala.Application.Features.OrgAdmin.FeeStructures.Queries.GetAllActiveFeeStructures;

public record GetAllActiveFeeStructuresQuery(Guid? BranchId = null, Guid? ClassId = null) : IRequest<ApiResponse<List<FeeStructureLookupDto>>>;
